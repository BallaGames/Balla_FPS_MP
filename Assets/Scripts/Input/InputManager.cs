using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Input Keys
    public const string MOVE_ACTION = "Move";
    public const string LOOK_ACTION = "Look";
    public const string PRIMARY_ACTION = "PrimaryAct";
    public const string SECONDARY_ACTION = "SecondaryAct";
    public const string PING_ACTION = "Ping";
    public const string DANGER_PING_ACTION = "Ping";
    public const string JUMP_ACTION = "Jump";
    public const string SPRINT_ACTION = "Sprint";
    public const string CROUCHTOGGLE_ACTION = "CrouchToggle";
    public const string CROUCHHOLD_ACTION = "CrouchHold";
    public const string PAUSE_ACTION = "Pause";
    public const string MELEE_ACTION = "Melee";
    public const string INTERACT_ACTION = "Interact";
    public const string EMOTE_ACTION = "Emote";
    public const string INSPECT_ACTION = "Inspect";
    public const string CYCLE_ACTION = "CycleWeapon";
    public const string SWITCH_ACTION = "Switch";
    public const string QUICK_CHAT_ACTION = "QuickChat";
    public const string MAP_ACTION = "Map";
    public const string INVENTORY_ACTION = "Inventory";
    public const string GRENADE_ACTION = "Grenade";
    public const string GRENADE_WHEEL_ACTION = "GrenadeWheel";
    public const string HEAL_WHEEL_ACTION = "HealWheel";
    public const string HEAL_ACTION = "Heal";
    public const string HOLSTER_ACTION = "Holster";
    public const string SET_WEAPON_ACTION = "SetWeapon";
    public const string RELOAD_ACTION = "Reload";
    
    #endregion
    public PlayerInput input;
    InputActionMap gameplayMap, UIMap;
    public bool additionalLogging = true;
    public static bool IsGamepad;
    public static bool cursorFree = false;

    public static float AimSensitivity = 0.01f;
    public float aimSens;

    #region Input Values
    public static Vector2 MoveInput, LookInput;
    public static bool PrimaryInput, SecondaryInput,
        InteractInput, PingInput, DangerPingInput,
        JumpInput, SprintInput,
        CrouchToggleInput, CrouchHoldInput,
        PauseInput, MeleeInput,
        EmoteInput, InspectInput,
        QuickChatInput, InventoryInput,
        MapInput, SwitchInput,
        HolsterInput, HealthUseInput,
        HealthWheelInput, GrenadeUseInput,
        GrenadeWheelInput, ReloadInput;
    public static float CycleInput, SetWeaponInput;
    #endregion
    #region Input Events

    #region Performed
    public static Action OnPrimaryPerform,
        OnSecondaryPerform,
        OnPingPerform,
        OnDangerPingPerform,
        OnJumpPerform,
        OnSprintPerform,
        OnInteractPerform,
        OnCrouchTogglePerform,
        OnCrouchHoldPerform,
        OnPausePerform,
        OnMeleePerform,
        OnEmotePerform,
        OnInspectPerform,
        OnQuickChatPerform,
        OnInventoryPerform,
        OnMovePerform,
        OnLookPerform,
        OnCyclePerform,
        OnMapPerform,
        OnHolsterPerform,
        OnGrenadePerform,
        OnGrenadeWheelPerform,
        OnHealPerform,
        OnHealWheelPerform,
        OnSetWeaponPerform,
        OnReloadPerform;
    #endregion Perform

    #region Cancel
    public static Action OnPrimaryCancel,
        OnSecondaryCancel,
        OnPingCancel,
        OnDangerPingCancel,
        OnJumpCancel,
        OnSprintCancel,
        OnInteractCancel,
        OnCrouchToggleCancel,
        OnCrouchHoldCancel,
        OnPauseCancel,
        OnMeleeCancel,
        OnEmoteCancel,
        OnInspectCancel,
        OnQuickChatCancel,
        OnInventoryCancel,
        OnMoveCancel,
        OnLookCancel,
        OnCycleCancel,
        OnMapCancel, 
        OnHolsterCancel,
        OnGrenadeCancel,
        OnGrenadeWheelCancel,
        OnHealCancel,
        OnHealWheelCancel,
        OnSetWeaponCancel,
        OnReloadCancel;
    #endregion Cancel

    #endregion Input Events
    #region methods
    private void Awake()
    {
        gameplayMap = input.actions.FindActionMap("Player");
        UIMap = input.actions.FindActionMap("UI");

        Debug.Log(gameplayMap == null);
        Debug.Log(UIMap == null);

        ZeroInputs();

        SetupInputs(true);

        input.onControlsChanged += OnControlsChanged;
        input.onDeviceLost += OnDeviceLost;
        input.onDeviceRegained += OnDeviceRegain;

        OnControlsChanged(input);

        OnPausePerform += TryPause;

    }

    private void TryPause()
    {
        cursorFree = !cursorFree;
        Cursor.lockState = cursorFree ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnValidate()
    {
        AimSensitivity = aimSens * 0.005f;
    }

    void SetupInputs(bool sub)
    {
        SubscribeAction(gameplayMap.FindAction(MOVE_ACTION), OnMove, sub);
        SubscribeAction(gameplayMap.FindAction(LOOK_ACTION), OnLook, sub);
        SubscribeAction(gameplayMap.FindAction(PRIMARY_ACTION), OnPrimaryAtk, sub);
        SubscribeAction(gameplayMap.FindAction(SECONDARY_ACTION), OnSecondaryAtk, sub);
        SubscribeAction(gameplayMap.FindAction(JUMP_ACTION), OnJump, sub);
        SubscribeAction(gameplayMap.FindAction(SPRINT_ACTION), OnSprint, sub);
        SubscribeAction(gameplayMap.FindAction(INTERACT_ACTION), OnInteract, sub);
        SubscribeAction(gameplayMap.FindAction(INSPECT_ACTION), OnInspect, sub);
        SubscribeAction(gameplayMap.FindAction(CYCLE_ACTION), OnCycleWeapon, sub);
        SubscribeAction(gameplayMap.FindAction(CROUCHHOLD_ACTION), OnCrouchHold, sub);
        SubscribeAction(gameplayMap.FindAction(CROUCHTOGGLE_ACTION), OnCrouchToggle, sub);
        SubscribeAction(gameplayMap.FindAction(MELEE_ACTION), OnMelee, sub);
        SubscribeAction(gameplayMap.FindAction(PAUSE_ACTION), OnPause, sub);
        //SubscribeAction(gameplayMap.FindAction(QUICK_CHAT_ACTION), OnQuickChat, sub);
        SubscribeAction(gameplayMap.FindAction(INVENTORY_ACTION), OnInventory, sub);
        //SubscribeAction(gameplayMap.FindAction(EMOTE_ACTION), OnEmote, sub);
        SubscribeAction(gameplayMap.FindAction(PING_ACTION), OnPing, sub);
        SubscribeAction(gameplayMap.FindAction(DANGER_PING_ACTION), OnDangerPing, sub);
        SubscribeAction(gameplayMap.FindAction(MAP_ACTION), OnMap, sub);
        SubscribeAction(gameplayMap.FindAction(HOLSTER_ACTION), OnHolster, sub);
        SubscribeAction(gameplayMap.FindAction(HEAL_ACTION), OnHeal, sub);
        SubscribeAction(gameplayMap.FindAction(GRENADE_ACTION), OnGrenade, sub);
        SubscribeAction(gameplayMap.FindAction(HEAL_WHEEL_ACTION), OnHealWheel, sub);
        SubscribeAction(gameplayMap.FindAction(GRENADE_WHEEL_ACTION), OnGrenadeWheel, sub);
        SubscribeAction(gameplayMap.FindAction(CYCLE_ACTION), OnCycleWeapon, sub);
        SubscribeAction(gameplayMap.FindAction(RELOAD_ACTION), OnReload, sub);

    }

    //we may want some logic that handles players attempting to swap devices.
    private void OnDeviceRegain(PlayerInput obj)
    {

    }
    private void OnDeviceLost(PlayerInput obj)
    {

    }
    public void ZeroInputs()
    {
        PrimaryInput = SecondaryInput = PingInput = JumpInput = SprintInput = CrouchToggleInput = CrouchHoldInput = PauseInput = MeleeInput = EmoteInput = InspectInput = QuickChatInput = InventoryInput = false;
        MoveInput = LookInput = Vector2.zero;
        CycleInput = 0;
    }
    private void OnControlsChanged(PlayerInput obj)
    {
        ZeroInputs();
        IsGamepad = obj.currentControlScheme == "Gamepad";
    }
    private void OnDestroy()
    {
        SetupInputs(false);
    }
    public void SubscribeAction(InputAction action, Action<InputAction.CallbackContext> callback, bool sub)
    {
        if (sub)
        {
            action.performed += callback;
            action.canceled += callback;
        }
        else
        {
            action.performed -= callback;
            action.canceled -= callback;
        }
    }
    public void GetInput<T>(ref T value, InputAction.CallbackContext ctx, Action perform, Action cancel) where T : unmanaged
    {
        value = ctx.ReadValue<T>();
        if (ctx.performed)
        {
            perform?.Invoke();
        }
        if (ctx.canceled)
        {

            cancel?.Invoke();
        }

    }
    public void GetInput(ref bool value, InputAction.CallbackContext ctx, Action perform, Action cancel)
    {
        value = ctx.ReadValueAsButton();
        if (ctx.performed)
        {
            perform?.Invoke();
        }
        if (ctx.canceled)
        {

            cancel?.Invoke();
        }

    }
    #endregion
    #region Input 
    public void OnMap(InputAction.CallbackContext ctx)
    {
        GetInput(ref MapInput, ctx, OnMapPerform, OnMapCancel);
    }
    public void OnMove(InputAction.CallbackContext ctx)
    {
        GetInput(ref MoveInput, ctx, OnMovePerform, OnMoveCancel);
    }
    public void OnLook(InputAction.CallbackContext ctx)
    {
        GetInput(ref LookInput, ctx, OnLookPerform, OnLookCancel);
    }
    public void OnJump(InputAction.CallbackContext ctx)
    {
        GetInput(ref JumpInput, ctx, OnJumpPerform, OnJumpCancel);
    }
    public void OnPrimaryAtk(InputAction.CallbackContext ctx)
    {
        GetInput(ref PrimaryInput, ctx, OnPrimaryPerform, OnPrimaryCancel);
    }
    public void OnSecondaryAtk(InputAction.CallbackContext ctx)
    {
        GetInput(ref SecondaryInput, ctx, OnSecondaryPerform, OnSecondaryCancel);
    }
    public void OnPing(InputAction.CallbackContext ctx)
    {
        GetInput(ref PingInput, ctx, OnPingPerform, OnPingCancel);
    }
    public void OnDangerPing(InputAction.CallbackContext ctx)
    {
        GetInput(ref DangerPingInput, ctx, OnDangerPingPerform, OnDangerPingCancel);
    }
    public void OnSprint(InputAction.CallbackContext ctx)
    {
       GetInput(ref SprintInput, ctx, OnSprintPerform, OnSprintCancel);
    }
    public void OnCrouchToggle(InputAction.CallbackContext ctx)
    {
        GetInput(ref CrouchToggleInput, ctx, OnCrouchTogglePerform, OnCrouchToggleCancel);
    }
    public void OnCrouchHold(InputAction.CallbackContext ctx)
    {
        GetInput(ref CrouchHoldInput, ctx, OnCrouchHoldPerform, OnCrouchHoldCancel);
    }
    public void OnPause(InputAction.CallbackContext ctx)
    {
        GetInput(ref PauseInput, ctx, OnPausePerform, OnPauseCancel);
    }
    public void OnMelee(InputAction.CallbackContext ctx)
    {
        GetInput(ref MeleeInput, ctx, OnMeleePerform, OnMeleeCancel);
    }
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        GetInput(ref InteractInput, ctx, OnInteractPerform, OnInteractCancel);
    }
    public void OnEmote(InputAction.CallbackContext ctx)
    {
        GetInput(ref EmoteInput, ctx, OnEmotePerform, OnEmoteCancel);
    }
    public void OnInspect(InputAction.CallbackContext ctx)
    {
        GetInput(ref InspectInput, ctx, OnInspectPerform, OnInspectCancel);
    }
    public void OnQuickChat(InputAction.CallbackContext ctx)
    {
        GetInput(ref QuickChatInput, ctx, OnQuickChatPerform, OnQuickChatCancel);
    }
    public void OnInventory(InputAction.CallbackContext ctx)
    {
        GetInput(ref InventoryInput, ctx, OnInventoryPerform, OnInventoryCancel);
    }
    public void OnCycleWeapon(InputAction.CallbackContext ctx)
    {
        GetInput(ref CycleInput, ctx, OnCyclePerform, OnCycleCancel);
    }
    public void OnGrenade(InputAction.CallbackContext ctx)
    {
        GetInput(ref GrenadeUseInput, ctx, OnGrenadePerform, OnGrenadeCancel);
    }
    public void OnHolster(InputAction.CallbackContext ctx)
    {
        GetInput(ref HolsterInput, ctx, OnHolsterPerform, OnHolsterCancel);
    }
    public void OnHeal(InputAction.CallbackContext ctx)
    {
        GetInput(ref HealthUseInput, ctx, OnHealPerform, OnHealCancel);
    }
    public void OnGrenadeWheel(InputAction.CallbackContext ctx)
    {
        GetInput(ref GrenadeWheelInput, ctx, OnGrenadeWheelPerform, OnGrenadeWheelCancel);
    }
    public void OnHealWheel(InputAction.CallbackContext ctx)
    {
        GetInput(ref HealthWheelInput, ctx, OnHealWheelPerform, OnHealWheelCancel);
    }
    public void OnReload(InputAction.CallbackContext ctx)
    {
        GetInput(ref ReloadInput, ctx, OnReloadPerform, OnReloadCancel);
    }
    #endregion
}
