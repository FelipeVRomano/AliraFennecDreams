using FennecDreams.CharactersConfig;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FennecDreams.Player
{
    public class FDDashScript : MonoBehaviour
    {
        [SerializeField] private bool _canDash;
        [SerializeField] private GameObject _vfx;

        [Header("Configurações do Dash")]
        [SerializeField] private float _dashTime = 0.5f;
        [SerializeField]float _dashSpeed = 50f;
        [SerializeField]float _dashSpeedExtra = 75f;
        [SerializeField]float _dashSpeedSad = 25f;
        [SerializeField]float _dashCooldown = 1.5f;

        #region Private Variables
        float _dashSpeedBase;
        float _dashCooldownBase;
        float _saveDashSpeed;
        float _dashCount;
        
        bool _cooldownDash;
        bool _stopDash;
        bool _dashing;
        bool _dashOnAir;
        
        FDMoveScript _moveScript;
        FDPlayerStateManager _stateManager;
        FDMultipleAttack _multipleAttack;
        FDMultipleAnticipation _multipleAnticipation;
        #endregion
        
        void Awake()
        {
            _dashSpeedBase = _dashSpeed;
        }
        
        void Start()
        {
            _canDash = true;
            _moveScript = GetComponent<FDMoveScript>();
            _stateManager = GetComponent<FDPlayerStateManager>();
            _multipleAnticipation = GetComponent<FDMultipleAnticipation>();
            _multipleAttack = GetComponent<FDMultipleAttack>();
        }

        void Update()
        {
            Dashing();
            ResetDash();
        }

        public void DashSpeed(float _happyValue)
        {
            if(_happyValue == 2) _saveDashSpeed = _dashSpeedSad;
            else if(_happyValue == 0){ _saveDashSpeed = _dashSpeedBase;
            }else if(_happyValue == 1) _saveDashSpeed = _dashSpeedExtra;
            
            _dashSpeed = _saveDashSpeed;
        }
        
        public void GetSpeedDash()
        {
            _dashSpeed = _saveDashSpeed;
        }

        public void Dash(InputAction.CallbackContext valueJ)
        {
            if(_stopDash) return;
            
            if(valueJ.ReadValue<float>() <= 0)
                return;
            
            if(!_canDash)
                return;
            
            if(!_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Pausing) && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.FinishingAttack) 
               && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Jumping) && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Attacking)
               && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Moving) && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.IsOnAir) 
               && !_stateManager.ReturnIfProcessIsTrue(FDPlayerStateManager.Process.ProcessState.Anticipating)) return;

            if(_stateManager.GetCurrentState() != FDPlayerStateManager.Process.ProcessState.Dashing)
                _stateManager.SetNextStage(FDPlayerStateManager.Process.Command.Dash);
            
            _dashing = true;
            _vfx.SetActive(true);
            
            if(!_moveScript.isGrounded()) _dashOnAir = true;

            _multipleAnticipation.CancelAnticipate();
            _multipleAttack.CancelAttack();   
        }

        private void Dashing()
        {
            if(!_dashing) return;
            _canDash = false;

            Vector3 dashDirection = new Vector3();
            dashDirection = transform.forward;
            _moveScript.SetJump(false);
            _moveScript.SetMoviment(false);
            _moveScript.SetIgnoreGravity(true);

            if(!_stopDash)
            {
                _moveScript.GetDashValue(dashDirection * _dashSpeed);
            }
            else{
                _dashCount = _dashTime;
            }
            _dashCount += Time.unscaledDeltaTime;

            if(_dashCount >= _dashTime)
            {
                FinishDash();
            }
        }
        
        public void SetCanDash(bool b)
        {
            _canDash = b;
        }

        public void SetStopDash(bool b)
        {
            _stopDash = b;
        }
        
        void ResetDash()
        {
            if(_dashing)
                return;
            
            if(_cooldownDash)
            {
                _dashCooldownBase += Time.unscaledDeltaTime;

                if(!_moveScript.isGrounded() && !_dashOnAir)
                {
                    _dashCooldownBase = 0;
                    _cooldownDash = false;
                }
                if(_dashCooldownBase > _dashCooldown)
                {
                    _dashCooldownBase = 0;
                    _cooldownDash = false;
                }
                return;
            }

            if(_moveScript.isGrounded()) _dashOnAir = false;

            if(_dashOnAir) return;
        
            _canDash = true;
        }
        
        void FinishDash()
        {
            _moveScript.GetDashValue(Vector3.zero);
            _moveScript.SetJump(true);
            
            if(!_stopDash)
                _moveScript.SetMoviment(true);
            _moveScript.SetStateMove(true);
            
            _moveScript.SetIgnoreGravity(false);
            _dashing = false;
            _cooldownDash = true;
            _dashCount = 0;
            _vfx.SetActive(false);
        }

        public bool DashingCheck()
        {
            return _dashing;
        }
        
    }
}
