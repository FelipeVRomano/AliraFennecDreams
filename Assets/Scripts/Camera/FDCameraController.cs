using UnityEngine;
using Cinemachine;

public class FDCameraController : MonoBehaviour
{
		[Header("Cinemachine/Camera Config")]
        [SerializeField] CinemachineVirtualCamera _cinemachine;
        public GameObject CinemachineCameraTarget;
		public float TopClamp = 70.0f;
		public float BottomClamp = -30.0f;
		public float CameraAngleOverride = 0.0f;
		public bool LockCameraPosition = false;
		[SerializeField] float _cinemachineTargetYaw;
        [SerializeField] Vector2 _cameraLook;
        public float mouseSensitivity = 50f;

        #region MyRegion
        InputM _inputM;
        PauseMenu _pauseMenu;
        
        float _cinemachineTargetPitch;
        float _mouseX;
        float _mouseY;
        const float _threshold = 0.01f;
        float _startingIntensity;
        float _shakeTimer;
        float _shakeTimerTotal;
        float _startingFrequency;
        
        bool _doCameraMoviment;
        
        Vector3 _currentRotation;
        #endregion
        
        void Awake()
        {
            _inputM = new InputM();
        }
        
        private void OnEnable()
        {
            _inputM.Enable();
        }
        
        private void OnDisable()
        {
            _inputM.Disable();
        }

        void Start()
        {
            if(PauseMenu.Instance == null) return;
				PauseMenu.Instance.onChangeSensibility += ChangeSensitivity;
            _pauseMenu = GameObject.Find("PauseController").GetComponent<PauseMenu>();
            _pauseMenu.SetInvokeSensitivity();
        }
        
        void Update()
        {
            DoCameraShake();
        }
        
        void LateUpdate()
		{
            LookInput();
			CameraRotation();
		}

        public void LookInput()
		{
			_mouseX = _inputM.Player.MouseX.ReadValue<float>();
            _mouseY = _inputM.Player.MouseY.ReadValue<float>();
		}
        
    	void CameraRotation()
		{
            _cameraLook = new Vector2(_mouseX, -_mouseY);
			if (_cameraLook.sqrMagnitude >= _threshold && !LockCameraPosition)
			{
				_cinemachineTargetYaw += mouseSensitivity * _cameraLook.x * Time.deltaTime;
				_cinemachineTargetPitch += mouseSensitivity * _cameraLook.y * Time.deltaTime;
			}
			_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
		    CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

        public void DoCameraMoviment(bool value)
        {
            LockCameraPosition = !value;
            _cinemachine.enabled = value;
        }

        public void SetLockCamera(bool value)
        {
            LockCameraPosition = value;
        }

        public void ChangeSensitivity(float value)
        {
            mouseSensitivity = value;
        }

        public void CameraShake(float intensitiy, float frequency, float time)
        {
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = _cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensitiy;
            _startingIntensity = intensitiy;

            cinemachineBasicMultiChannelPerlin.m_FrequencyGain = frequency;
            _startingFrequency = frequency;

            _shakeTimerTotal = time;
            _shakeTimer = time;
        }

        void DoCameraShake()
        {
            if(_shakeTimer > 0) {
                _shakeTimer -= Time.deltaTime;
                if(_shakeTimer <= 0.0f){
                    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = _cinemachine.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                    cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(_startingIntensity, 0.5f, 1 -(_shakeTimer/ _shakeTimerTotal));
                    cinemachineBasicMultiChannelPerlin.m_FrequencyGain = Mathf.Lerp(_startingFrequency, 0.3f, 1 -(_shakeTimer/ _shakeTimerTotal));
                }
            }
        }
}
