using KinematicCharacterController;
using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMotor : MonoBehaviour, ICharacterController
{
    public KinematicCharacterMotor Motor;
    PlayerCamera playerCam;
    public enum CharacterState
    {
        Grounded = 0,
        Air = 1,
        Slide = 2,
        Ladder = 4,
        Wallrun = 8,
        Zipline = 16,
        Downed = 32,
        Dead = 64,
    }
    public CharacterState characterState;
    bool Sprinting => InputManager.MoveInput.y > .7f && characterState == CharacterState.Grounded && InputManager.SprintInput && !crouching;
    float aimPitch, aimYaw, wallrunYaw;
    internal float aimPitchDelta, aimYawDelta;

    [Header("References")]
    public Transform aimRotate;

    [Header("Aim")]
    public bool invertX;
    public bool invertY;
    [Range(0, 1)]
    public float jumpVelocityCancel = 0.5f;

    [Header("Ground Movement")]
    public float groundMoveSpeed;
    public float groundMoveSharpness;
    public float additionalSprintSpeed;

    [Header("Sliding")]
    public float slideDrag;
    public float slideStartBoost;
    public float slideSteerForce;
    bool slideLaunching;
    [Header("Air Movement")]
    public float airMoveForce;
    public float airDrag = 0.1f;
    public float jumpVelocity = 2f;

    [Header("Ladder Movement")]
    public float ladderSpeed;
    public float ladderSlideSpeed;

    [Header("Downed Movement")]
    public float downedMoveSpeed;
    public float downedRotateMult;

    [Header("Wallrunning")]
    public float wallrunSpeed;
    public float wallCastDistance;
    public float wallrunIdleDecay = 2;
    public float maxWallrunTime;
    public AnimationCurve wallrunGravityCurve;
    public float wallrunAccel;
    public float wallrunTurnSpeed;
    public float wallrunLockoutTime;
    public float wallStickForce;
    public float wallrunCamRoll;
    float wallrunLock;
    Vector3 wallNormal;
    float wallrunSide;
    float wallrunTime;
    float targetAngle;
    


    [Header("Misc")]
    public Vector3 gravity;
#if UNITY_EDITOR
    public Color debugColour;
#endif
    bool jumpRequested;
    bool lastGrounded;

    bool crouching;
    public Vector2 crouchHeight;
    public Vector2 slideHeight;
    public Vector2 standHeight;
    public float crouchSpeed;
    public float slowWalkSpeedMult = 0.5f;
    public float minSlideSpeed;
    public float slideStopSpeed;
    Vector2 currHeight, targHeight;
    private void Awake()
    {
        Motor.CharacterController = this;
        playerCam = GetComponent<PlayerCamera>();

        InputManager.OnLookPerform += Input_OnLookPerform;
        InputManager.OnJumpPerform += Input_OnJumpPerform;

        InputManager.OnSprintPerform += TrySprint;
        InputManager.OnSprintCancel += TrySprint;

        InputManager.OnCrouchTogglePerform += ToggleCrouch;
        InputManager.OnCrouchHoldPerform += HoldCrouch;
        InputManager.OnCrouchHoldCancel += HoldCrouch;

    }

    private void HoldCrouch()
    {
        OnCrouched();
        crouching = InputManager.CrouchHoldInput;
    }

    private void ToggleCrouch()
    {
        //Will always result in being true if we try to press either crouch button.
        OnCrouched();
        crouching = InputManager.CrouchHoldInput || !crouching;
    }

    void OnCrouched()
    {
        if(Sprinting && Vector3.Dot(Motor.Velocity, Motor.CharacterForward) > minSlideSpeed)
        {
            Debug.Log("started slide!");
            StartSlide();
        }
    }

    void TrySprint()
    {

    }

    private void Input_OnJumpPerform() => jumpRequested = true;

    private void Input_OnLookPerform()
    {
        float prevPitch = aimPitch;
        float prevYaw = aimYaw;
        aimPitch = Mathf.Clamp(aimPitch + ((invertY ? 1 : -1) * InputManager.LookInput.y * InputManager.AimSensitivity), -89, 89);
        switch (characterState)
        {
            case CharacterState.Ladder:
                break;
            case CharacterState.Wallrun:
                wallrunYaw = Mathf.Clamp((wallrunYaw + ((invertX ? -1 : 1) * InputManager.LookInput.x * InputManager.AimSensitivity)) % 360, -89, 89);
                break;
            case CharacterState.Dead:
                break;
            default:
                aimYaw = (aimYaw + ((invertX ? -1 : 1) * InputManager.LookInput.x * InputManager.AimSensitivity)) % 360;
                break;
        }
        aimPitchDelta = prevPitch - aimPitch;
        aimYawDelta = prevYaw - aimYaw;
    }

    private void LateUpdate()
    {
        playerCam.UpdateCam(aimYaw + wallrunYaw, aimPitch, characterState == CharacterState.Wallrun ? wallrunCamRoll * wallrunSide : 0);
    }

    public void SetInput()
    {

    }

    public void AfterCharacterUpdate(float deltaTime)
    {
        
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {

    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = Quaternion.Euler(0, aimYaw, 0);
    }
    void CheckCharacterState(ref Vector3 currentVelocity, float deltaTime)
    {
        //basically, we want to ONLY be able to cancel any of the other states when we meet their cancel condition; usually the character will reset itself back to the air state to be checked on the next frame.
        if (!(characterState == CharacterState.Grounded || characterState == CharacterState.Air))
            return;
        Debug.Log("checking character state");
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            characterState = CharacterState.Grounded;
        }
        else
        {
            characterState = CharacterState.Air;
        }
    }
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        CheckCharacterState(ref currentVelocity, deltaTime);
        if(wallrunLock > 0)
            wallrunLock -= deltaTime;
        if (characterState == CharacterState.Grounded)
        {
            targHeight = (crouching ? crouchHeight : standHeight);
        }
        else
        {
            targHeight = (characterState == CharacterState.Slide ? slideHeight : standHeight);
        }
        currHeight = Vector2.MoveTowards(currHeight, targHeight, deltaTime * crouchSpeed);


        aimRotate.localPosition = Vector3.MoveTowards(aimRotate.localPosition, Motor.CharacterUp * targHeight.y, crouchSpeed * deltaTime);
        Motor.SetCapsuleDimensions(Motor.Capsule.radius, currHeight.x, 0);


        switch (characterState)
        {
            case CharacterState.Grounded:
                GroundMovement(ref currentVelocity, deltaTime);
                break;
            case CharacterState.Air:
                currentVelocity += gravity * deltaTime;
                AirMovement(ref currentVelocity, deltaTime);
                break;
            case CharacterState.Slide:
                SlideMovement(ref currentVelocity, deltaTime);
                break;
            case CharacterState.Ladder:
                break;
            case CharacterState.Wallrun:
                WallrunMovement(ref currentVelocity, deltaTime);
                break;
            case CharacterState.Zipline:
                break;
            case CharacterState.Downed:
                break;
            case CharacterState.Dead:
                break;
            default:
                break;
        }
        jumpRequested = false;
    }

    #region Movement
    void GroundMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        float currVelocityMag = currentVelocity.magnitude;
        Vector3 groundNormal = Motor.GroundingStatus.GroundNormal;
        
        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, groundNormal) * currVelocityMag;

        Vector3 targetVelocity = Vector3.ClampMagnitude((Motor.CharacterRight * InputManager.MoveInput.x) + (Motor.CharacterForward * InputManager.MoveInput.y), 1) * groundMoveSpeed;
        if (Sprinting)
        {
            targetVelocity += additionalSprintSpeed * InputManager.MoveInput.y * Motor.CharacterForward;
        }
        
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-groundMoveSharpness * deltaTime));

        Debug.DrawRay(transform.position, currentVelocity, Color.yellow, deltaTime);
        Debug.DrawRay(transform.position, Motor.CharacterForward, Color.green, deltaTime);
        Debug.DrawRay(transform.position, Motor.CharacterRight, Color.red, deltaTime);

        if (jumpRequested)
        {
            TryJump(ref currentVelocity);
        }
    }
    void AirMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity += (airMoveForce * deltaTime * Vector3.ClampMagnitude((Motor.CharacterRight * InputManager.MoveInput.x) + (Motor.CharacterForward * InputManager.MoveInput.y), 1)) - (airDrag * Time.deltaTime * currentVelocity);
        if (wallrunLock <= 0 && TryStartWallrun())
        {
            StartWallrun();
            return;
        }
    }

    private void StartSlide()
    {
        characterState = CharacterState.Slide;
        slideLaunching = true;
    }

    void SlideMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        if (slideLaunching)
        {
            currentVelocity += Vector3.Cross(Motor.CharacterRight, Motor.GroundingStatus.GroundNormal) * slideStartBoost;
            slideLaunching = false;
        }
        if (!Motor.GroundingStatus.IsStableOnGround || Motor.Velocity.magnitude <= slideStopSpeed)
        {
            characterState = CharacterState.Air;
            return;
        }

        //Apply gravity. On a flat surface, this won't apply.
        currentVelocity += Vector3.ProjectOnPlane(gravity, Motor.GroundingStatus.GroundNormal);
        //Apply steer and drag
        currentVelocity += (InputManager.MoveInput.x * slideSteerForce * Motor.CharacterRight) - (deltaTime * slideDrag * currentVelocity);

    }
    void LadderMovement(ref Vector3 currentVelocity, float deltaTime)
    {

    }
    void WallrunMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        bool didHit = WallrunCast(wallrunSide, out RaycastHit hit);
        //check if we hit the ground as well
        if (!Motor.GroundingStatus.IsStableOnGround && wallrunTime < maxWallrunTime && didHit)
        {
            wallNormal = hit.normal;
            //Lose wallrun time twice as fast if not moving
            wallrunTime += deltaTime * (Mathf.Lerp(1, wallrunIdleDecay, InputManager.MoveInput.y));
            Vector3 forward = Vector3.Cross(wallNormal * -wallrunSide, Motor.CharacterUp);
            Debug.DrawRay(transform.position, forward * wallrunSpeed, Color.yellow);
            targetAngle = Vector3.SignedAngle(Vector3.forward, forward, Motor.CharacterUp);
            //aimYaw = Mathf.MoveTowardsAngle(aimYaw, Vector3.SignedAngle(Motor.CharacterForward, forward, Motor.CharacterUp), wallrunTurnSpeed * deltaTime);
            currentVelocity = Vector3.Lerp(currentVelocity, InputManager.MoveInput.y * wallrunSpeed * forward, wallrunAccel * Time.deltaTime) + (gravity * wallrunGravityCurve.Evaluate(Mathf.InverseLerp(0, maxWallrunTime, wallrunTime))) + -wallNormal * wallStickForce;
            Debug.DrawRay(transform.position, Motor.CharacterRight * wallrunSide, Color.green);
            aimYaw = Mathf.LerpAngle(aimYaw, targetAngle, 1 - Mathf.Exp( -wallrunTurnSpeed * deltaTime));

            if (jumpRequested)
            {
                TryJump(ref currentVelocity);
                wallrunLock *= 0.5f;
            }

        }
        else
        {
            Debug.Log($"broke wallrun conditions: time - {wallrunTime >= maxWallrunTime} /// Missed wall - {!didHit}");
            Debug.DrawRay(transform.position, Motor.CharacterRight * wallrunSide, Color.red);
            CancelWallrun();
        }
    }
    void CancelWallrun()
    {
        aimYaw = transform.eulerAngles.y + wallrunYaw;
        wallrunYaw = 0;
        wallrunLock = wallrunLockoutTime;
        characterState = CharacterState.Air;
    }
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = debugColour;
        Vector3 size = new(.5f, 1, 0.5f);
        Gizmos.DrawWireCube(transform.position + Motor.CharacterRight, size + Vector3.up);
        float lerp = Mathf.InverseLerp(0, maxWallrunTime, wallrunTime);
        Gizmos.DrawCube(transform.position + Motor.CharacterRight - (Vector3.up * (1 - lerp)), size + Vector3.Lerp(-Vector3.up, Vector3.up, lerp));
    }
#endif
    void ZiplineMovement(ref Vector3 currentVelocity, float deltaTime)
    {

    }
    void TryJump(ref Vector3 currentVelocity)
    {
        //For now, just jump.
        Motor.ForceUnground();
        currentVelocity += (Motor.CharacterUp * jumpVelocity) - (Vector3.Project(currentVelocity, Motor.CharacterUp) * jumpVelocityCancel);
        if(characterState == CharacterState.Wallrun)
        {
            currentVelocity += jumpVelocity * wallNormal;
            CancelWallrun();
        }
        jumpRequested = false;
    }
    
    bool TryStartWallrun()
    {

        if (WallrunCast(-1, out RaycastHit hit) || WallrunCast(1, out hit))
        {
            Debug.DrawLine(transform.position, hit.point, Color.green, Time.fixedDeltaTime);
            wallNormal = hit.normal;
            //Must be on a mostly vertical surface.
            if (math.abs(Vector3.Dot(wallNormal, Motor.CharacterRight)) > 0.9f)
            {
                wallrunSide = -math.sign(Vector3.Dot(hit.normal, Motor.CharacterRight));
                return true;
            }
        }
        Debug.DrawRay(transform.position, wallCastDistance * Motor.CharacterRight, Color.red, Time.fixedDeltaTime);
        Debug.DrawRay(transform.position, wallCastDistance * -Motor.CharacterRight, Color.red, Time.fixedDeltaTime);

        return false;
    }
    bool WallrunCast(float side, out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, Motor.CharacterRight * side, out hit, wallCastDistance);
    }
    void StartWallrun()
    {
        Debug.Log("Started wall run");
        characterState = CharacterState.Wallrun;
        wallrunTime = 0;
        //wallrunYaw = Mathf.Clamp(aimYaw, -90, 90);
    }
    #endregion
}
