using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using Cinemachine;
using Framework;
using Items;
using Player;
using Typewriter;
using UnityEngine;
using Utils;
using View.Overlay;

namespace Interactions {
  public class Conversation : Interactable {
    public struct Interaction {
      public InteractionWaypoint Waypoint;
      public PlayerController Player;
      public bool IsActive;
      public bool IsReady;

      public Interaction(
        PlayerController player,
        InteractionWaypoint waypoint
      ) {
        Player = player;
        Waypoint = waypoint;
        IsActive = true;
        IsReady = false;
      }
    }

    public InteractionWaypoint[] Waypoints;
    [EntryFilter(BaseType = typeof(ItemEntry), AllowEmpty = true)]
    public EntryReference Item;

    private PlayerLookup<Interaction> _interactions;

    [SerializeField] private CinemachineVirtualCamera _cameraTemplate;
    [SerializeField] private float _orthoSize = 4;
    [SerializeField] private InteractionArea _area;
    [SerializeField] private Transform _itemTransform;

    private CinemachineVirtualCamera _camera;
    private Item _itemInstance;
    private Camera _mainCamera;

    protected override void Awake() {
      base.Awake();
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

      Blackboard.Set(InteractionContext.AvailableItem, Item);
      TryInstantiateItem();
    }

    private void Update() {
      UpdateInteraction(PlayerType.LT);
      UpdateInteraction(PlayerType.RT);
      UpdateState();

      Blackboard.Set(InteractionContext.IsLTPresent, IsPresent(Players.LT));
      Blackboard.Set(InteractionContext.IsRTPresent, IsPresent(Players.RT));
      Blackboard.Set(InteractionContext.Initiator, Initiator);
      Blackboard.Set(InteractionContext.Listener, Listener);

      UpdateGizmo();
    }

    private void UpdateInteraction(PlayerType type) {
      var interaction = _interactions[type];
      if (!interaction.IsActive) {
        return;
      }

      interaction.IsReady = _area.IsPlayerInside[type];
      _interactions[type] = interaction;
    }

    public override void Interact(PlayerController player) {
      if (_interactions[player.Type].IsReady) {
        player.EnterDialogueSound.Play();
        Event.Invoke(Context);
      } else if (!_interactions[player.Type].IsActive) {
        player.InteractSound.Play();
        player.InteractState.Enter(this);
      }
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

      _interactions[player.Type] = new Interaction(player, closestWaypoint);
      if (!otherInteraction.IsActive) {
        Initiator = player.Fact;
      } else {
        Listener = player.Fact;
      }
    }

    public void OnFocusExit(PlayerController player) {
      _interactions[player.Type] = default;
      Listener = default;
      Initiator = _interactions[player.Other.Type].IsActive
        ? player.Other.Fact
        : default;
    }

    public override void OnDialogueEnter() {
      base.OnDialogueEnter();
      _camera.gameObject.SetActive(true);
    }

    public override void OnDialogueExit() {
      base.OnDialogueExit();
      _camera.gameObject.SetActive(false);
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

    private void UpdateState() {
      var playerType =
        (_interactions.LT.IsActive ? PlayerType.LT : PlayerType.None)
        | (_interactions.RT.IsActive ? PlayerType.RT : PlayerType.None);
      var isFocused = _interactions.LT.IsActive || _interactions.RT.IsActive;
      var isInteracting = _interactions.LT.IsReady || _interactions.RT.IsReady;
      var hasDialogue = IsInteracting
        && Context.HasMatchingRule(Event.EventReference);

      if (IsFocused != isFocused
        || IsInteracting != isInteracting
        || PlayerType != playerType
        || hasDialogue != HasDialogue) {
        IsFocused = isFocused;
        IsInteracting = isInteracting;
        PlayerType = playerType;
        HasDialogue = hasDialogue;
        OnStateChanged();
      }
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

    public override void UseItem(EntryReference item) {
      base.UseItem(item);
      Item = item;
      TryInstantiateItem();
    }

    public override EntryReference PickUpItem() {
      base.PickUpItem();
      if (_itemInstance != null) {
        Destroy(_itemInstance.gameObject);
        _itemInstance = null;
      }

      return Item;
    }

    private void TryInstantiateItem() {
      if (Item.TryGetEntry(out ItemEntry itemEntry)) {
        _itemInstance = Instantiate(itemEntry.Prefab, _itemTransform);
      }
    }

    private void UpdateGizmo() {
      var ltPosition =
        (Vector2)_mainCamera.WorldToScreenPoint(Players.LT.transform.position);
      var rtPosition =
        (Vector2)_mainCamera.WorldToScreenPoint(Players.RT.transform.position);

      Gizmo.Direction = (rtPosition - ltPosition).normalized;
      Gizmo.PlayerType = PlayerType;

      // The dialogue view will take care of the gizmo
      if (IsDialogue) {
        return;
      }

      if (Players.DialogueState.IsActive) {
        Gizmo.IsExpanded = false;
        Gizmo.IsHovered = false;
        Gizmo.IsFocused = false;
        Gizmo.IsDisabled = true;
        Gizmo.Icon = InteractionGizmo.DialogueIcon;
        return;
      }
      Gizmo.IsDisabled = false;

      Gizmo.IsExpanded = IsInteracting && HasDialogue;
      Gizmo.IsHovered = IsHovered;
      Gizmo.IsFocused = IsFocused;
      Gizmo.Icon = InteractionGizmo.DialogueIcon;
    }
  }
}
