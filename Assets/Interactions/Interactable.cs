using Aarthificial.Safekeeper;
using Aarthificial.Safekeeper.Attributes;
using Aarthificial.Safekeeper.Stores;
using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Blackboards;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Cinemachine;
using Framework;
using Player;
using Saves;
using System;
using Typewriter;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;
using View.Overlay;

namespace Interactions {
  public class Interactable : MonoBehaviour, ISaveStore {
    private struct Interaction {
      public InteractionWaypoint Waypoint;
      public readonly bool IsActive;
      public bool IsReady;

      public Interaction(InteractionWaypoint waypoint) {
        Waypoint = waypoint;
        IsActive = true;
        IsReady = false;
      }
    }

    public event Action StateChanged;

    [NonSerialized] public PlayerType PlayerType;
    [NonSerialized] public bool IsHovered;
    [NonSerialized] public bool IsInDialogue;

    public InteractionWaypoint[] Waypoints;
    public InteractionGizmo Gizmo;
    public TypewriterEvent Event;
    public Blackboard Blackboard = new();
    public InteractionContext Context;

    [SerializeField]
    [FormerlySerializedAs("Item")]
    [EntryFilter(BaseType = typeof(ItemEntry), AllowEmpty = true)]
    private EntryReference _initialItem;
    [ObjectLocation] [SerializeField] private SaveLocation _id;
    [SerializeField] private CinemachineVirtualCamera _cameraTemplate;
    [SerializeField] private float _orthoSize = 4;
    [SerializeField] private InteractionArea _area;

    private PlayerManager _players;
    private PlayerLookup<Interaction> _interactions;
    private CinemachineVirtualCamera _camera;
    private Camera _mainCamera;
    private EntryReference _initiator;
    private EntryReference _listener;

    protected void Awake() {
      _players = FindObjectOfType<PlayerManager>();
      Context.Interaction = Blackboard;
      Context.Setup(this);
      Context.Set(Facts.IsLTPresent, 0);
      Context.Set(Facts.IsRTPresent, 0);
      Context.Set(Facts.InitialEvent, Event.EventReference);

      _mainCamera = OverlayManager.Camera;
      _camera = Instantiate(_cameraTemplate);
      var cameraTarget = new GameObject("Camera Target").transform;
      cameraTarget.SetParent(transform);
      var position = Vector3.zero;
      foreach (var waypoint in Waypoints) {
        position += waypoint.Position;
      }
      position /= Waypoints.Length;
      position.y += 1;
      cameraTarget.position = position;

      _camera.Follow = cameraTarget;
      _camera.Priority = 100;
      _camera.m_Lens.OrthographicSize = _orthoSize;
      _camera.gameObject.SetActive(false);
    }

    private void OnEnable() {
      SaveStoreRegistry.Register(this);
    }

    private void OnDisable() {
      SaveStoreRegistry.Unregister(this);
    }

    public void OnLoad(SaveControllerBase save) {
      Context.Global = ((SaveController)save).GlobalData.Blackboard;
      if (!save.Data.Read(_id, Blackboard) && _initialItem.HasValue) {
        Context.Set(_initialItem, 1);
      }
    }

    public void OnSave(SaveControllerBase save) {
      save.Data.Write(_id, Blackboard);
    }

    private void Start() {
      OnStateChanged();
    }

    private void Update() {
      UpdateInteraction(PlayerType.LT);
      UpdateInteraction(PlayerType.RT);
      UpdatePlayerType();
      UpdateBlackboard();
      UpdateGizmo();
    }

    private void UpdateBlackboard() {
      Context.Set(Facts.IsLTPresent, IsPresent(_players.LT));
      Context.Set(Facts.IsRTPresent, IsPresent(_players.RT));
      Context.Set(Facts.Initiator, _initiator);
      Context.Set(Facts.Listener, _listener);
    }

    private void UpdateInteraction(PlayerType type) {
      var interaction = _interactions[type];
      if (!interaction.IsActive) {
        return;
      }

      interaction.IsReady = _area.IsPlayerInside[type];
      _interactions[type] = interaction;
    }

    public void Interact(PlayerController player) {
      if (_interactions[player.Type].IsReady) {
        player.EnterDialogueSound.Play();
        Event.Invoke(Context);
      } else if (!_interactions[player.Type].IsActive) {
        player.InteractSound.Play();
        player.InteractState.Enter(this);
      }
    }

    public void OnHoverEnter() {
      IsHovered = true;
      OnStateChanged();
    }

    public void OnHoverExit() {
      IsHovered = false;
      OnStateChanged();
    }

    public void OnFocusEnter(PlayerController player) {
      var otherInteraction = _interactions[player.Other.Type];
      var otherWaypoint = otherInteraction.IsActive
        ? otherInteraction.Waypoint
        : null;

      // relocate the other player if necessary
      var closestWaypoint = FindClosestWaypoint(player.transform.position);
      if (otherInteraction.IsActive && otherWaypoint == closestWaypoint) {
        var position = player.transform.position;
        var closestDistance = float.MaxValue;
        InteractionWaypoint betterWaypoint = null;

        var otherClosestDistance = float.MaxValue;
        var replacementWaypoint = Waypoints[0] == closestWaypoint
          ? Waypoints[1]
          : Waypoints[0];

        foreach (var waypoint in Waypoints) {
          if (waypoint == closestWaypoint) {
            continue;
          }

          var distance = Vector3.Distance(position, waypoint.Position);
          var dot = Vector3.Dot(
            position - closestWaypoint.Position,
            waypoint.Position - closestWaypoint.Position
          );

          if (distance < closestDistance && dot > 0) {
            betterWaypoint = waypoint;
            closestDistance = distance;
          }

          var otherDistance = Vector3.Distance(
            closestWaypoint.Position,
            waypoint.Position
          );
          if (otherDistance < otherClosestDistance) {
            replacementWaypoint = waypoint;
            otherClosestDistance = otherDistance;
          }
        }

        if (betterWaypoint != null) {
          closestWaypoint = betterWaypoint;
        } else {
          otherInteraction.Waypoint = replacementWaypoint;
          _interactions[player.Type.Other()] = otherInteraction;
        }
      }

      _interactions[player.Type] = new Interaction(closestWaypoint);
      if (!otherInteraction.IsActive) {
        _initiator = player.Fact;
      } else {
        _listener = player.Fact;
      }

      UpdateBlackboard();
    }

    public void OnFocusExit(PlayerController player) {
      _interactions[player.Type] = default;
      _listener = default;
      _initiator = _interactions[player.Other.Type].IsActive
        ? player.Other.Fact
        : default;
    }

    public void OnDialogueEnter() {
      IsInDialogue = true;
      _camera.gameObject.SetActive(true);
      OnStateChanged();
    }

    public void OnDialogueExit() {
      IsInDialogue = false;
      _camera.gameObject.SetActive(false);
      App.Game.Story.AutoSave();
      OnStateChanged();
    }

    public void UseItem(EntryReference item) {
      if (item.HasValue) {
        Blackboard.Set(item, 1);
      }
    }

    public void SetEvent(EntryReference entry) {
      Event.EventReference = entry;
      Blackboard.Set(Facts.InitialEvent, entry);
    }

    public Vector3 GetPosition(PlayerController player) {
      return _interactions[player.Type].Waypoint.Position;
    }

    public Quaternion GetRotation(PlayerController player) {
      return _interactions[player.Type].Waypoint.transform.rotation;
    }

    public bool IsReady(PlayerController player) {
      return _interactions[player.Type].IsReady;
    }

    private void UpdatePlayerType() {
      var playerType =
        (_interactions.LT.IsActive ? PlayerType.LT : PlayerType.None)
        | (_interactions.RT.IsActive ? PlayerType.RT : PlayerType.None);

      if (PlayerType != playerType) {
        PlayerType = playerType;
        OnStateChanged();
      }
    }

    private void OnStateChanged() {
      StateChanged?.Invoke();
    }

    private InteractionWaypoint FindClosestWaypoint(
      Vector3 position,
      InteractionWaypoint omit = null
    ) {
      var closestDistance = float.MaxValue;
      var closestWaypoint = Waypoints[0] == omit ? Waypoints[1] : Waypoints[0];

      foreach (var waypoint in Waypoints) {
        if (waypoint == omit) {
          continue;
        }

        var distance = Vector3.Distance(position, waypoint.Position);

        if (distance < closestDistance) {
          closestWaypoint = waypoint;
          closestDistance = distance;
        }
      }

      return closestWaypoint;
    }

    private int IsPresent(PlayerController player) {
      if (_interactions[player.Type].IsActive) {
        return 1;
      }

      if (!player.InteractState.IsActive) {
        return 0;
      }

      return -1;
    }

    private void UpdateGizmo() {
      var ltPosition =
        (Vector2)_mainCamera.WorldToScreenPoint(_players.LT.transform.position);
      var rtPosition =
        (Vector2)_mainCamera.WorldToScreenPoint(_players.RT.transform.position);

      Gizmo.Direction = (rtPosition - ltPosition).normalized;
      Gizmo.PlayerType = PlayerType;

      // The dialogue view will take care of the gizmo
      if (IsInDialogue) {
        return;
      }

      if (_players.DialogueState.IsActive) {
        Gizmo.IsExpanded = false;
        Gizmo.IsHovered = false;
        Gizmo.IsFocused = false;
        Gizmo.IsDisabled = true;
        Gizmo.Icon = InteractionGizmo.DialogueIcon;
        return;
      }

      var isFocused = _interactions.LT.IsActive || _interactions.RT.IsActive;
      var isInteracting = _interactions.LT.IsReady || _interactions.RT.IsReady;
      var hasDialogue = Context.HasMatchingRule(Event.EventReference);

      Gizmo.IsDisabled = false;
      Gizmo.IsExpanded = isInteracting && hasDialogue;
      Gizmo.IsHovered = IsHovered;
      Gizmo.IsFocused = isFocused;
      Gizmo.Icon = InteractionGizmo.DialogueIcon;
    }
  }
}
