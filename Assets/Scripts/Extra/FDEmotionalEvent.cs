using UnityEngine;
using UnityEngine.Events;

namespace FennecDreams.Emotion
{
    public class FDEmotionalEvent : MonoBehaviour
    {
        [Header("Efeito Emoções")]
        [SerializeField] private float _sadValue;
        [SerializeField] private float _happyValue;
        [SerializeField] private float _rageValue;
        
        [Header("Evento Config")]
        [SerializeField] private int _loopEvent;
        [SerializeField] private float _eventCooldown;
        [SerializeField] private bool _needInput;

        public UnityEvent OnExitCollisionEventEmotional;
        public UnityEvent OnCollisionEventEmotional;
        public UnityEvent OnInteractionEvent;

        #region Private Variables
        bool _playerIsColliding;
        bool _playerCanBeAffected;
        float _eventCooldownBase;
        float _loopEventBase;
        
        FDInteractEvent _interactEvent;
        BoxCollider _boxCollider;
        Player.FDPlayerStatus _playerStatus;
        #endregion

        void Start()
        {
            _boxCollider = GetComponent<BoxCollider>();

            _interactEvent = FindObjectOfType<FDInteractEvent>();
            _interactEvent.InteractEvent += CallEmotionalEvent;
        }
        
        void Update()
        {
            CheckInteraction();
            
            if(!_playerCanBeAffected || _loopEventBase >= _loopEvent)
                return;
            
            _eventCooldownBase += Time.deltaTime;
            if (_eventCooldownBase <= _eventCooldown)
                return;

            EmotionalEvent();
        }

        void CheckInteraction()
        {
            if(_needInput)
            {
                if(!_boxCollider.enabled) 
                {
                    _playerCanBeAffected = true;
                }
            }
        }

        void EmotionalEvent()
        {
            OnInteractionEvent?.Invoke();
            _loopEventBase++;
            _playerStatus.changeEmotions(_happyValue, _sadValue, _rageValue);

            _eventCooldownBase = 0f;
        }
        
        public void CallEmotionalEvent()
        {
            if(_playerIsColliding)
                _boxCollider.enabled = false;
        }

        private void OnTriggerEnter(Collider hit)
        {
            if (!hit.CompareTag("Player")) return;
            
            if (_playerStatus == null)
            {
                _playerStatus = hit.GetComponent<Player.FDPlayerStatus>();
            }

            if(_loopEventBase >= _loopEvent)
                if(!_needInput) _boxCollider.enabled = false;

            if (_needInput)
                OnCollisionEventEmotional?.Invoke();

            _playerIsColliding = true;
        }

        private void OnTriggerStay(Collider hit)
        {
            if (hit.CompareTag("Player"))
            {
                if (!_needInput)
                {
                    _playerCanBeAffected = true;
                }
            }
        }

        private void OnTriggerExit(Collider hit)
        {
            if(hit.CompareTag("Player"))
            {
                _playerCanBeAffected = false;

                if (_needInput)
                    OnExitCollisionEventEmotional?.Invoke();

                _playerIsColliding = false;
            }
        }
    }
}
