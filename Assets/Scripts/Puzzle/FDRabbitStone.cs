using System.Collections.Generic;
using UnityEngine;
using FennecDreams.Player;
using UnityEngine.Events;

namespace FennecDreams.Puzzles
{
    public class FDRabbitStone : MonoBehaviour
    {
        public enum rockState{
            WaitingForPlayer,
            Inactive, 
            Moving
        }
        
        [SerializeField] rockState _actualState;
        public List<Transform> positionsToRun;
        [SerializeField] FDPuzzleRoute[] _puzzleRoute;
        
        #region Editor Variables
        [SerializeField] float _speed;
        [SerializeField] float _speedRotation;
        
        [SerializeField] Transform playerPosition;
        
        [Header("Qual vai ser a distância minima do Player para o desafio ser iniciado?")]
        [SerializeField] float _distStartPuzzle;
        [Header("Essa variavel indica a distancia minima que o Player pode ficar sem que posição seja descartada")]
        [SerializeField] float _minDist;
        [Header("Essa variavel indica a distancia minima para a pedra voltar a fugir do Player")]
        [SerializeField] float _minDistRunPuzzle;
        [Header("Essas variaveis aumentam o quanto vai verificar antes de escolher uma posição para ir")]
        [SerializeField] float _addDistanceX;
        [SerializeField] float _addDistanceZ;
        
        [Header("Quantas vezes vai checar se pode ir ao ponto antes de não respeitar o player?")]
        [SerializeField] private int _checkLimit;
        public Transform[] xValue;
        
        [Header("Quanto de tristeza vai alterar ao perder?")]
        [SerializeField] private float _sadValueLost;
        [Header("Quanto de tristeza vai alterar ao ganhar?")]
        [SerializeField] private float _sadValueWon;
        [Header("Quanto de alegria vai alterar ao perder?")]
        [SerializeField] private float _happyValueLost;
        [Header("Quanto de alegria vai alterar ao ganhar?")]
        [SerializeField] private float _happyValueWon;
        [Header("Quanto de raiva vai alterar ao perder?")]
        [SerializeField] private float _angryValueLost;
        [Header("Quanto de raiva vai alterar ao ganhar?")]
        [SerializeField] private float _angryValueWon;
        
        [Header("Efeito de raio que sai quando a pedra do coelho se movimenta")]
        [SerializeField] GameObject _trailEffect;
        [Header("Efeito de feedback de quando a pedra é ativada")]
        [SerializeField] GameObject _activeStoneVFX;
        
        [Header("Quantos segundos para o player ter alterações nas emoções?")]
        [SerializeField] float _defeatCooldown;
        public UnityEvent OnFinishPuzzle;
        #endregion
        
        #region Private variables
        Quaternion _quaternionTarget;
        Transform _targetPos;
        CharacterController _controller;
        FDPlayerStatus _playerStatus;
        
        float _speedBase;
        float _addDistanceXBase;
        float _addDistanceZBase;
        float _cooldownDefeatBase;
        
        int _checkPlayer;
        int _indexPos;
        int _indexRoute;
        int _tryLimit;
        
        bool _choosedPos;
        bool _canRotate;
        #endregion
        
        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            playerPosition = GameObject.Find("CharacterFinal").GetComponent<Transform>();
            _playerStatus = GameObject.Find("CharacterFinal").GetComponent<FDPlayerStatus>();
            _targetPos = positionsToRun[0];
            _speedBase = _speed;
            ChangeState(rockState.WaitingForPlayer);
        }
        
        void FixedUpdate()
        {
            StateController();
        }
        
        void StateController()
        {
            switch(_actualState)
            {
                case rockState.WaitingForPlayer:
                CheckIfPlayerIsNear();
                if (_canRotate)
                    RotateRabbit(playerPosition.position);
                _canRotate = true;
                if(_trailEffect.activeSelf == true) 
                    _trailEffect.SetActive(false);
                break;
                
                case rockState.Inactive:
                Desactive();
                return;
                
                case rockState.Moving: 
                if(_trailEffect.activeSelf == false) 
                    _trailEffect.SetActive(true);
                CheckIfIsPosition();
                CheckWhileMoving();
                MoveStoneToPosition();
                AddCooldown();
                if(!_canRotate)
                    RotateRabbit(_targetPos.position);
                _canRotate = false;
                return;
            }
        }

        void RotateRabbit(Vector3 target)
        {
            Vector3 targetRotation = target - transform.position;
            if(targetRotation != Vector3.zero)
                _quaternionTarget = Quaternion.LookRotation(targetRotation);
            transform.rotation = Quaternion.Slerp (transform.rotation, new Quaternion(0,_quaternionTarget.y ,0, _quaternionTarget.w), 
                Time.deltaTime * _speedRotation);
        }
        
        void AddCooldown()
        {
            _cooldownDefeatBase += Time.deltaTime;

            if(_cooldownDefeatBase > _defeatCooldown)
            {
                  _playerStatus.changeEmotions(_happyValueLost, _sadValueLost, _angryValueLost);
                _cooldownDefeatBase = 0;
            }
        }

        void CheckIfPlayerIsNear()
        {
            float distPlayer = Vector3.Distance(transform.position, playerPosition.position);

            if(_checkPlayer == 0)
            {
                if(distPlayer < _distStartPuzzle)
                {
                    _addDistanceXBase = _addDistanceX;
                    _addDistanceZBase = _addDistanceZ;
                    ChangeState(rockState.Moving);
                } 
            }else if(distPlayer < _minDistRunPuzzle){
                    _addDistanceXBase = _addDistanceX;
                    _addDistanceZBase = _addDistanceZ;
                    ChangeState(rockState.Moving);
            }
        }
        
        public void Desactive()
        {
            _playerStatus.changeEmotions(_happyValueWon, _sadValueWon, _angryValueWon);
            _controller.gameObject.SetActive(false);
            _trailEffect.SetActive(false);
            _activeStoneVFX.SetActive(false);
            OnFinishPuzzle.Invoke();
        }
        
        void CheckIfIsPosition()
        {
            float distToTarget = Vector3.Distance(transform.position, _targetPos.position);
            if(distToTarget < 0.1)
            {
                for(int i = 0; i < _puzzleRoute.Length; i++)
                {
                    if(_targetPos == positionsToRun[i])
                    {
                        _indexPos = i;
                    }
                }
                if(_choosedPos)
                {
                _indexRoute = 0;
                _choosedPos = false;
                }
            }else{
                _choosedPos = true;
            }
            float distToPlayer = Vector3.Distance(transform.position, playerPosition.position);

            if(distToPlayer > _minDistRunPuzzle && distToTarget < 0.1f)
            { 
                _checkPlayer = 1;
                ChangeState(rockState.WaitingForPlayer);
            }
        }
        void CheckWhileMoving()
        {
            float zOffset = transform.position.z - _puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.z;
            float xOffset = transform.position.x - _puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.x;
            
            bool m_cantPassX = false;
            bool m_cantPassZ = false;
            
            if(Vector3.Distance(_puzzleRoute[_indexPos].positionsToRun[_indexRoute].position, playerPosition.position) < _minDist)
            {
                m_cantPassX = true;
                m_cantPassZ = true;
            }
            
            if(zOffset < 0 && xOffset >= 0)
            {
                xValue[0].position = new Vector3(_puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.x - _addDistanceX, 47, transform.position.z - _addDistanceZ);
                xValue[1].position = new Vector3(transform.position.x + _addDistanceX, 47, _puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.z + _addDistanceZ);
                if(playerPosition.position.x >= xValue[0].position.x && xValue[1].position.x >= playerPosition.position.x) m_cantPassX = true;
                if(playerPosition.position.z >= xValue[0].position.z && xValue[1].position.z >= playerPosition.position.z) m_cantPassZ = true;
            }
            else if(zOffset >= 0 && xOffset < 0)
            {
                xValue[0].position = new Vector3(transform.position.x - _addDistanceX, 47, _puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.z - _addDistanceZ);
                xValue[1].position = new Vector3(_puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.x + _addDistanceX , 47, transform.position.z + _addDistanceZ);
                 if(playerPosition.position.x >= xValue[0].position.x && xValue[1].position.x >= playerPosition.position.x) m_cantPassX = true;
                if(playerPosition.position.z >= xValue[0].position.z && xValue[1].position.z >= playerPosition.position.z) m_cantPassZ = true;
            }
            else if(zOffset < 0 && xOffset < 0)
            {
                xValue[0].position = new Vector3(transform.position.x - _addDistanceX, 47, transform.position.z - _addDistanceZ );
                xValue[1].position = new Vector3(_puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.x + _addDistanceX , 47, _puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.z + _addDistanceZ);
                 if(playerPosition.position.x >= xValue[0].position.x && xValue[1].position.x >= playerPosition.position.x) m_cantPassX = true;
                if(playerPosition.position.z >= xValue[0].position.z && xValue[1].position.z >= playerPosition.position.z) m_cantPassZ = true;
            }
            else if(zOffset >= 0 && xOffset >= 0)
            {
                xValue[0].position = new Vector3(_puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.x - _addDistanceX, 47, _puzzleRoute[_indexPos].positionsToRun[_indexRoute].position.z - _addDistanceZ);
                xValue[1].position = new Vector3(transform.position.x + _addDistanceX, 47, transform.position.z + _addDistanceZ);
                if(playerPosition.position.x >= xValue[0].position.x && xValue[1].position.x >= playerPosition.position.x) m_cantPassX = true;
                if(playerPosition.position.z >= xValue[0].position.z && xValue[1].position.z >= playerPosition.position.z) m_cantPassZ = true;
            }

            if(m_cantPassX && m_cantPassZ){
                _indexRoute++;
                if(_indexRoute > _puzzleRoute[_indexPos].positionsToRun.Count - 1) _indexRoute = 0; 
                if(_tryLimit < _checkLimit + 1) _tryLimit++;
                if(_tryLimit > _checkLimit) 
                {
                _targetPos = _puzzleRoute[_indexPos].positionsToRun[_indexRoute];
                _indexRoute = 0;
                }
            }
            else{
                _targetPos = _puzzleRoute[_indexPos].positionsToRun[_indexRoute];
                _indexRoute = 0;
                _tryLimit = 0;
            }
        }
        
        public void ChangeState(rockState stateName)
        {
            _actualState = stateName;
        }
        
        void MoveStoneToPosition()
        {
            Vector3 dir = _targetPos.position - transform.position;
            Vector3 movement = dir.normalized * _speed * Time.deltaTime;
            if (movement.magnitude > dir.magnitude) movement = dir;
            _controller.Move(movement);
        }

        private float Vector2DDistance (Vector3 v1, Vector3 v2)
        {
            float xDiff = v1.x - v2.x;
            float zDiff = v1.z - v2.z;
            return Mathf.Sqrt((xDiff * xDiff) + (zDiff * zDiff));
        }

        void OnTriggerEnter(Collider hit)
        {
            if(hit.CompareTag("Player")) 
            {
                ChangeState(rockState.Inactive);
            }
        }
    }
}

