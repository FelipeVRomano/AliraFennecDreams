using System.Collections;
using FennecDreams.Player.Ability;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FennecDreams.Player
{
  public class FDMoveScript : MonoBehaviour
  { 
      
    #region MyRegion
    [Header("Player Configuração")]
    [SerializeField] float[] _speedValues;
    [SerializeField] float _playerSpeed;
    
    [Range(0,1)]
    [SerializeField] float decreaseJumpPercentage = 0.75f;
    [Header("Cooldown para dar o double jump depois do jump normal.")]
    [SerializeField] float _doubleJumpCoolDown;
    [SerializeField] float _jumpForce;
    [SerializeField] float _jumpForceBase;
    [SerializeField] float _jumpForceSad;
    
    [SerializeField] float _doubleJumpForceBase;
    [SerializeField] float _doubleJumpForce;
    [SerializeField] float _doubleJumpForceSad;
    
    [SerializeField] float gravity = 20;
    [HideInInspector] public float _gravityBase;
    [SerializeField] float[] _gravityValues;
    [SerializeField] float groundedPullMagnitude = 5f;
    [SerializeField] private float groundRememberedTime = 0.2f;
    
    [Header("Colisão com chão")]
    [SerializeField] bool isOnSlope = false;
    [SerializeField] float slideVelocity;
    [SerializeField] float slopeForceDown;
    [SerializeField] float _damping;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask GroundLayers;
    
    [Header("Componentes Extras")]
    [SerializeField] UnityEngine.Camera mainCamera;
    [SerializeField] Animator _animator;
    [SerializeField] GameObject _doubleJumpVFX;
    [HideInInspector] public FDPlayerStatus _playerStatus;
    public InputM inputController;
    public bool isOnMovingPlatform;
    
    #endregion
    
    #region Private Variables
    float groundRememberTime;
    float _fallVelocity;  
    float _lastPlayerSpeed;
    float horizontalMove, verticalMove;
    
    Vector3 hitNormal;
    Vector3 camForward, camRight;
    Vector3 playerInput, movePlayer, dashPlayer, addForcePlayer;
    
    FDDashScript _pDash;
    FDPlayerStateManager _stateManager;
    CharacterController playerMov; 
    
    bool wasGroundedLastFrame;
    bool _isJumping;
    bool canDoubleJump;
    bool _canJump = true;
    bool _canMove = true;
    bool _cooldownDoubleJump;
    bool _ignoreGravity;
    bool _setStateMove;
    bool _stopJumping;
    #endregion
    
    private void OnEnable()
    {
        inputController.Enable();
    }
    private void OnDisable()
    {
        inputController.Disable();
    }
    void Awake()
    { 
      inputController = new InputM();
      playerMov = GetComponent<CharacterController>();
      _pDash = GetComponent<FDDashScript>();
      _playerStatus = GetComponent<FDPlayerStatus>();
      _lastPlayerSpeed = _playerSpeed;
      _stateManager = GetComponent<FDPlayerStateManager>();
      _setStateMove = true;
    }

    void Update()
    {
        GetInput();
        Moviment();
        RotatePlayerCamDirection();
        SetGravity();
        JumpMove();
        SlideDown();
        playerMov.Move((movePlayer + dashPlayer + addForcePlayer) * Time.unscaledDeltaTime);
    }

    public void GetDashValue(Vector3 dashValue)
    {
        dashPlayer = dashValue;
    }

    void GetInput()
    {
        if(!_canMove)
            return;
        horizontalMove = inputController.Player.Horizontal.ReadValue<float>();
        verticalMove = inputController.Player.Vertical.ReadValue<float>();
    } 
    
    public void GetAddForce(Vector3 addForcePlayerTemp)
    {
        addForcePlayer = addForcePlayerTemp;
    }
    
    void CamDirection()
    {
        camForward = mainCamera.transform.forward;
        camRight = mainCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }
    
    void Moviment()
    {
        if(!_canMove)
        {
           horizontalMove = 0;
           verticalMove = 0;
           movePlayer = Vector3.zero;
        }
        
        if(isGrounded())
        {   
            FDPlayerStateManager.Process.ProcessState tempState = _stateManager.GetCurrentState();
            if(tempState != FDPlayerStateManager.Process.ProcessState.Anticipating &&
            tempState != FDPlayerStateManager.Process.ProcessState.Attacking &&
            tempState != FDPlayerStateManager.Process.ProcessState.FinishingAttack &&
            !_pDash.DashingCheck())
            if(_setStateMove)
            {
                SetMoveState();
            }
        }
        
        playerInput = new Vector3(horizontalMove, 0, verticalMove);
        playerInput = Vector3.ClampMagnitude(playerInput, 1);
        CamDirection();
        movePlayer = playerInput.x * camRight + playerInput.z * camForward;
        movePlayer = movePlayer * _playerSpeed;  
    }
    
    void SetMoveState()
    {
        if (_stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.Moving)
            _stateManager.SetNextStage(FDPlayerStateManager.Process.Command.Move);
        _animator.SetBool("Land", true);
        SetStateMove(false);
     }

    public void PlayerSpeed(float happyValue)
    {
        if(happyValue == 2) _lastPlayerSpeed = _speedValues[0];
        else if(happyValue == 0) _lastPlayerSpeed = _speedValues[1];
        else if(happyValue == 1) _lastPlayerSpeed = _speedValues[2];

        if(!FDTurtleAbility._usingAbility)
            _playerSpeed = _lastPlayerSpeed;     
    }

    public void SetOriginalPlayerSpeed()
    {
        _playerSpeed = _lastPlayerSpeed;
    }

    public void ChangePlayerSpeed(float value)
    {
        _playerSpeed = value;
    }

    public void GravityValue(float sadValue)
    {
            if (sadValue == 2)
            {
                _gravityBase = _gravityValues[0];
                _jumpForce = _jumpForceSad;
                _doubleJumpForceBase = _doubleJumpForceSad;
            }
            else if (sadValue == 0)
            {
                _gravityBase = _gravityValues[1];
                _jumpForce = _jumpForceBase;
                _doubleJumpForceBase = _doubleJumpForce;
            }
            else if (sadValue == 1)
            {
                _gravityBase = _gravityValues[2];
                _jumpForce = _jumpForceBase;
                _doubleJumpForceBase = _doubleJumpForce;
            }

            if(gravity == 0) gravity = _gravityBase;
    }

    public void Jump(InputAction.CallbackContext valueJ)
    {
            if(_stopJumping || isOnSlope) return;
            if(!_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Moving) && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.IsOnAir)
            && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Pausing)) return;
            if(canDoubleJump && !_isJumping)
            {
                if (_stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.Jumping)
                    _stateManager.SetNextStage(FDPlayerStateManager.Process.Command.Jump);
                _isJumping = true;
                _doubleJumpVFX.SetActive(true);
            }
            if(!_canJump)
                return;
            if(groundRememberTime > 0 && !_isJumping)
            {
                if (_stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.Jumping)
                        _stateManager.SetNextStage(FDPlayerStateManager.Process.Command.Jump);
                _isJumping = true;
            }      
    }
    
    void SetGravity()
    {
       if(CharacterGrounded())
        {
            if(!wasGroundedLastFrame && playerMov.isGrounded && !_isJumping) _fallVelocity = 0;
            groundRememberTime = groundRememberedTime;
            movePlayer.y += -groundedPullMagnitude;
            gravity = _gravityBase;
            canDoubleJump = false;
            _cooldownDoubleJump = false;
        }
       else
        {
            groundRememberTime -= Time.deltaTime;
            if(!_setStateMove)
                SetStateMove(true);
            if(!_ignoreGravity)
            {
                if(!_cooldownDoubleJump && !canDoubleJump && groundRememberTime < 0)
                {
                    canDoubleJump = true;
                    _cooldownDoubleJump = true;
                }
                movePlayer.y += _fallVelocity + gravity * Time.unscaledDeltaTime;
                _fallVelocity = movePlayer.y;
            }
            else{
                _fallVelocity = 0;
                movePlayer.y = 0;
            }
        }
        wasGroundedLastFrame = CharacterGrounded();
    }

    void JumpMove()
    {
        if(_isJumping)
        {
            if(!canDoubleJump)
            {
                if(_playerStatus.PredominantEmotion == FDPlayerStatus.Emotions.Sadness)
                {
                    _fallVelocity = _jumpForce * decreaseJumpPercentage;
                }
                else{
                    _fallVelocity = _jumpForce;
                }
                StartCoroutine(cooldownDoubleJump());
            } else{
                if(_playerStatus.PredominantEmotion == FDPlayerStatus.Emotions.Sadness)
                {
                    _fallVelocity = _doubleJumpForceBase * decreaseJumpPercentage;
                    canDoubleJump = false;
                }
                else{
                    _fallVelocity = _doubleJumpForceBase;
                    canDoubleJump = false;
                }
            }
            movePlayer.y = _fallVelocity;
            _isJumping = false;
        }
        else{
                if(!isGrounded())
                {
                    if( _stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.SpecialAttackUsing &&
                    _stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.Anticipating &&
                    _stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.Attacking &&
                    _stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.FinishingAttack)
                    {
                        if(!_pDash.DashingCheck())
                        {
                            if(_stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.IsOnAir)
                                _stateManager.SetNextStage(FDPlayerStateManager.Process.Command.OnAir);
                        }
                    }
                }
        }
    }

    IEnumerator cooldownDoubleJump()
    {
         _cooldownDoubleJump = true;
        yield return new WaitForSecondsRealtime(_doubleJumpCoolDown);
        _cooldownDoubleJump = false;
        _doubleJumpVFX.SetActive(false);
        canDoubleJump = true;
    }

    void RotatePlayerCamDirection()
    {
         if (playerInput.magnitude > 0.01f && _canMove)
        {
          var rotationTemp = Quaternion.LookRotation(movePlayer);
          transform.rotation = Quaternion.Slerp (transform.rotation, new Quaternion(0,rotationTemp.y,0, rotationTemp.w), 
              Time.unscaledDeltaTime * _damping);
        }
    }

    public void SetGravityValue(float x)
    {
        gravity = x;
    }
    
    public void SlideDown()
    {
        if(!isGrounded()){ 
            isOnSlope = false;
            return;
        }
        
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= playerMov.slopeLimit;

        if(isOnSlope)
        {
            movePlayer.x += ((1f- hitNormal.y) * hitNormal.x) * slideVelocity;
            movePlayer.z += ((1f- hitNormal.y) * hitNormal.z) * slideVelocity;
            movePlayer.y += slopeForceDown;
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    public void SetStateMove(bool stateMoveValue)
    {
        _setStateMove = stateMoveValue;
    }
    
    public void SetCanMove(bool canMove)
    {
        _canMove = canMove;
    }
    
    public void SetMoviment(bool canMoveValue)
    {
        _canMove = canMoveValue;
    }
    
    public void SetStopJumping(bool value)
    {
        _stopJumping = value;
    }
    
    public void SetJump(bool value)
    {
        _canJump = value;
    }

    public void SetAllMovements(bool canAllMoviments)
    {
        if (!canAllMoviments)
        {
            horizontalMove = 0;
            verticalMove = 0;
        }
        
        _canJump = canAllMoviments;
        _canMove = canAllMoviments;
        _pDash.SetStopDash(!canAllMoviments);
    }

    public void SetIgnoreGravity(bool ignoreGravityValue)
    {
        _ignoreGravity = ignoreGravityValue;
    }

    #region Bool Functions
    public bool ReturnIfMoving()
    {
        if(playerInput.magnitude < 0.01f) return false;
        else return true;
    }
    
    public bool isGrounded()
    {
        if(JumpingOn()) return false;

        if (CharacterGrounded()) return true;

        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        return Physics.CheckSphere(spherePosition, groundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    public bool JumpingOn()
    {
        if(_fallVelocity > 0) return true;
        else{
            return false;
        }
    }
    
    public bool CharacterGrounded()
    {
        if(JumpingOn()) return false;

        if (!playerMov.isGrounded && isOnMovingPlatform) return true;

        return playerMov.isGrounded;
    }
    
    #endregion
    
  }   
}
