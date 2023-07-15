using Aarthificial.Typewriter.Blackboards;
using Items;
using Player;
using UnityEngine;
using Utils;
using View.Dialogue;

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
    public Item Item;

    private PlayerLookup<Interaction> _interactions;

    [SerializeField]
    private float _radius = 0.3f;

    private DialogueButton _button;

    private void Update() {
      UpdateInteraction(PlayerType.LT);
      UpdateInteraction(PlayerType.RT);
      UpdateState();

      Blackboard.Set(InteractionContext.IsLTPresent, IsPresent(Players.LT));
      Blackboard.Set(InteractionContext.IsRTPresent, IsPresent(Players.RT));
      Blackboard.Set(InteractionContext.Initiator, Initiator);
      Blackboard.Set(InteractionContext.Listener, Listener);

      UpdateButton();
    }

    private void UpdateInteraction(PlayerType type) {
      var interaction = _interactions[type];
      if (!interaction.IsActive) {
        return;
      }

      var distance = Vector3.Distance(
        interaction.Player.transform.position,
        transform.position
      );

      interaction.IsReady = distance < _radius;
      _interactions[type] = interaction;
    }

    public override void Interact(PlayerController player) {
      player.InteractState.Enter(this);
    }

    public void OnFocusEnter(PlayerController player) {
      var otherInteraction = _interactions[player.Other.Type];
      var otherWaypoint = otherInteraction.IsActive
        ? otherInteraction.Waypoint
        : null;

      // relocate the other player if necessary
      var closestWaypoint = FindClosestWaypoint(player.transform.position);
      if (otherInteraction.IsActive && otherWaypoint == closestWaypoint) {
        otherInteraction.Waypoint = FindClosestWaypoint(
          otherInteraction.Player.transform.position,
          closestWaypoint
        );
        _interactions[player.Type.Other()] = otherInteraction;
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

    public Vector3 GetPosition(PlayerController player) {
      return _interactions[player.Type].Waypoint.Position;
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

    private void UpdateButton() {
      if (HasDialogue && _button == null) {
        _button = Players.Manager.BorrowButton();
        _button.SetInteraction(this);
        _button.Clicked += HandleButtonClicked;
      }

      if (!HasDialogue && _button != null) {
        _button.Clicked -= HandleButtonClicked;
        _button.SetInteraction(null);
        Players.Manager.ReleaseButton(_button);
        _button = null;
      }
    }

    private void HandleButtonClicked() {
      Players.Manager.DialogueState.Enter(this);
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
  }
}
