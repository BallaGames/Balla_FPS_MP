using KinematicCharacterController;
using System;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(PlayerCamera))]
public class PlayerMotor : NetworkBehaviour, ICharacterController
{
    public bool sprintRestricted;

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
        Vault = 128,
    }
    public CharacterState characterState;
    bool Sprinting => InputManager.MoveInput.y > .7f && characterState == CharacterState.Grounded && InputManager.SprintInput && !crouching && !sprintRestricted;
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
    public int coyoteFrames;
    int coyoteFramesConsumed;

    [Header("Ladder Movement")]
    public float ladderSpeed;
    public float ladderSlideSpeed;

    [Header("Downed Movement")]
    public float downedMoveSpeed;
    public float downedRotateMult;

    [Header("Wallrunning")]
    public bool canWallrun;
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

    [Header("Mantle")]
    public bool canMantle;
    public float mantleSpeed;
    public AnimationCurve lateralMantleCurve;
    public AnimationCurve verticalMantleCurve;
    public float mantleDistance;
    public float mantleMaxHeight;
    public LayerMask mantleMask;
    bool mantling;
    float mantleProgress, mantleRate;
    Vector3 mantleStart, mantleEnd;

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
    public float slideLandStartSpeed;
    Vector2 currHeight, targHeight;

    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
        if (IsOwner)
        {
            InputManager.OnLookPerform += Input_OnLookPerform;
            InputManager.OnJumpPerform += Input_OnJumpPerform;

            InputManager.OnSprintPerform += TrySprint;
            InputManager.OnSprintCancel += TrySprint;

            InputManager.OnCrouchTogglePerform += ToggleCrouch;
            InputManager.OnCrouchHoldPerform += HoldCrouch;
            InputManager.OnCrouchHoldCancel += HoldCrouch;
        }
    }



    private void Awake()
    {
        Motor.CharacterController = this;
        playerCam = GetComponent<PlayerCamera>();
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
            case CharacterState.Vault:
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
        if(IsOwner)
            playerCam.UpdateCam(aimYaw + wallrunYaw, aimPitch, characterState == CharacterState.Wallrun ? wallrunCamRoll * wallrunSide : 0);
    }

    #region misc
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

    #endregion misc
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        currentRotation = Quaternion.Euler(0, aimYaw, 0);
    }
    void CheckCharacterState(ref Vector3 currentVelocity, float deltaTime)
    {
        //basically, we want to ONLY be able to cancel any of the other states when we meet their cancel condition; usually the character will reset itself back to the air state to be checked on the next frame.
        if (!(characterState == CharacterState.Grounded || characterState == CharacterState.Air))
            return;
        if (Motor.GroundingStatus.IsStableOnGround)
        {
            if(!lastGrounded && crouching && Motor.Velocity.magnitude > groundMoveSpeed * 1.1f)
            {
                //Skip grounding and slide instead if we land stable on something and are going fast enough to slide.
                characterState = CharacterState.Slide;
                return;
            }
            characterState = CharacterState.Grounded;
            coyoteFramesConsumed = 0;
        }
        else
        {
            characterState = CharacterState.Air;
        }
        lastGrounded = Motor.GroundingStatus.IsStableOnGround;
    }
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (!IsOwner)
            return;


        CheckCharacterState(ref currentVelocity, deltaTime);
        if(wallrunLock > 0)
            wallrunLock -= deltaTime;
        if (characterState == CharacterState.Grounded || characterState == CharacterState.Slide)
        {
            targHeight = ((crouching | characterState==CharacterState.Slide) ? crouchHeight : standHeight);
        }
        else
        {
            targHeight = (characterState == CharacterState.Slide ? slideHeight : standHeight);
        }
        currHeight = Vector2.MoveTowards(currHeight, targHeight, deltaTime * crouchSpeed);


        aimRotate.localPosition = Vector3.MoveTowards(aimRotate.localPosition, Motor.CharacterUp * targHeight.y, crouchSpeed * deltaTime);
        Motor.SetCapsuleDimensions(Motor.Capsule.radius, currHeight.x, (standHeight.x - currHeight.x) * -0.5f);


        switch (characterState)
        {
            case CharacterState.Grounded:
                GroundMovement(ref currentVelocity, deltaTime);
                break;
            case CharacterState.Air:
                currentVelocity += gravity * deltaTime;
                AirMovement(ref currentVelocity, deltaTime);
                CheckWallrun();
                //Only check vault if we are attempting to move forwards OR trying to "jump"
                if(InputManager.MoveInput.y > 0.3f || InputManager.JumpInput)
                    CheckVault();
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
            case CharacterState.Vault:
                VaultMovement(ref currentVelocity, deltaTime);
                break;
            default:
                break;
        }
        jumpRequested = false;
    }

    #region Movement
    void CheckVault()
    {
        if (wallrunLock > 0)
            return;


        //Checks forwards to make sure there's something to mantle to. Uses a capsule cast to ensure we can climb the surface.
        if(Physics.CapsuleCast(transform.position - (0.45f * Motor.Capsule.height * transform.up),
            transform.position + (0.45f * Motor.Capsule.height * transform.up),
            Motor.Capsule.radius * 0.9f,
            Motor.CharacterForward, out RaycastHit mHit, mantleDistance, mantleMask, QueryTriggerInteraction.Ignore) 
            && Mathf.Abs(Vector3.Dot(transform.up, mHit.normal)) < 0.1f && Mathf.Abs(Vector3.Dot(transform.forward, mHit.normal)) >= 0.65f) 
        {
            Vector3 point = new(mHit.point.x, transform.position.y, mHit.point.z);
            //If this downward ray hits then we're able to mantle onto the surface in front of us if there's enough upward space.
            if(Physics.SphereCast(point - (mHit.normal * Motor.Capsule.radius)
                + (transform.up * mantleMaxHeight), 
                Motor.Capsule.radius, -transform.up, out RaycastHit downHit, mantleMaxHeight, mantleMask))
            {
                //If this one does NOT hit, then all is gucki
                if(!Physics.Raycast(downHit.point, transform.up, out RaycastHit upHit, mantleMaxHeight, mantleMask))
                {
                    mantleStart = transform.position;
                    mantleEnd = downHit.point + (0.5f * Motor.Capsule.height * transform.up);
                    StartMantle();
                }
            }
        }
        else
        {
            //didnt hit anything for mantle. Cannot mantle.
        }
    }
    void StartMantle()
    {
        characterState = CharacterState.Vault;
        mantleRate = mantleSpeed / Vector3.Distance(mantleStart, mantleEnd);
        Motor.SetCapsuleCollisionsActivation(false);
        mantleProgress = 0;
        mantling = true;
    }
    void VaultMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        if (!mantling)
        {
            EndMantle();
            return;
        }
        float latLerp = lateralMantleCurve.Evaluate(mantleProgress);
        Vector3 position = new(Mathf.LerpUnclamped(mantleStart.x, mantleEnd.x, latLerp),
            Mathf.LerpUnclamped(mantleStart.y, mantleEnd.y, verticalMantleCurve.Evaluate(mantleProgress)), 
            Mathf.LerpUnclamped(mantleStart.z, mantleEnd.z, latLerp));
        currentVelocity = Motor.GetVelocityForMovePosition(transform.position, position, deltaTime);
        mantleProgress += mantleRate * deltaTime;
        if (jumpRequested)
        {
            TryJump(ref currentVelocity);
        }
        if (mantleProgress >= 1)
            EndMantle();
            
    }

    void EndMantle()
    {
        mantling = false;
        CancelWallrun();
        Motor.SetCapsuleCollisionsActivation(true);
    }

    void GroundMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        float currVelocityMag = currentVelocity.magnitude;
        Vector3 groundNormal = Motor.GroundingStatus.GroundNormal;
        
        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, groundNormal) * currVelocityMag;

        Vector3 targetVelocity = Vector3.ClampMagnitude((Motor.CharacterRight * InputManager.MoveInput.x) + (Motor.CharacterForward * InputManager.MoveInput.y), 1) 
            * groundMoveSpeed * (crouching ? slowWalkSpeedMult : 1);

        if (Sprinting)
        {
            targetVelocity += additionalSprintSpeed * InputManager.MoveInput.y * Motor.CharacterForward;
        }
        
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1f - Mathf.Exp(-groundMoveSharpness * deltaTime));

        Debug.DrawRay(transform.position, currentVelocity, Color.yellow, deltaTime);
        Debug.DrawRay(transform.position, Motor.CharacterForward, Color.green, deltaTime);
        Debug.DrawRay(transform.position, Motor.CharacterRight, Color.red, deltaTime);

        CheckJump(ref currentVelocity);
    }
    void CheckJump(ref Vector3 currentVelocity)
    {
        if (jumpRequested)
        {
            TryJump(ref currentVelocity);
        }
    }
    void AirMovement(ref Vector3 currentVelocity, float deltaTime)
    {
        currentVelocity += (airMoveForce * deltaTime * Vector3.ClampMagnitude((Motor.CharacterRight * InputManager.MoveInput.x) + (Motor.CharacterForward * InputManager.MoveInput.y), 1)) - (airDrag * Time.deltaTime * currentVelocity);
        if(coyoteFramesConsumed < coyoteFrames)
        {
            CheckJump(ref currentVelocity);
            coyoteFramesConsumed++;
        }
    }
    void CheckWallrun()
    {
        if (canWallrun && wallrunLock <= 0 && TryStartWallrun())
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
        if (!crouching || !Motor.GroundingStatus.IsStableOnGround || Motor.Velocity.magnitude <= slideStopSpeed)
        {
            characterState = CharacterState.Air;
            return;
        }
        CheckJump(ref currentVelocity);
        //Apply gravity. On a flat surface, this won't apply.
        currentVelocity += Vector3.ProjectOnPlane(gravity, Motor.GroundingStatus.GroundNormal) * deltaTime;
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
        else if(characterState == CharacterState.Vault)
        {
            currentVelocity = jumpVelocity * (-transform.forward + transform.up).normalized;
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
