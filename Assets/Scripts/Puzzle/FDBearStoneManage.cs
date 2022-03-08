using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FennecDreams.Player.Ability;
using FennecDreams.Player;

namespace FennecDreams.Puzzles{
    public class FDBearStoneManage : MonoBehaviour
    {
        #region Editor Variables
        
        [Header("Coloque nesse objeto abaixo, as pedras que vão participar do desafio")]
        public List<FDBearStone> _stoneLists;
        [SerializeField] List<FDBearStone> _stoneManagerList;
        [Header("Quanto tempo demora para uma pedra aparecer?")]
        [SerializeField] float _timerAtivarPedra;
        [Header("Quanto tempo a pedra fica parada exposta antes de descer")]
        [SerializeField] float _cooldownPedraDesce;
        [Header("Velocidade da subida e descida das pedras")]
        [SerializeField] float _stoneSpeed;
        [Header("Coloque nesse objeto abaixo, um transform que guie a altura máxima que as pedras podem chegar")]
        [SerializeField] Transform _alturaMaximaDasPedras;
        [Header("Quanto tempo o jogador terá para ganhar o desafio?")]
        [SerializeField] float timeLimit;
        [Header("Qual segundo que o evento para indicar que o tempo esta acabando?")]
        [SerializeField] float _eventLostTime = 50f;
        public UnityEvent OnLosingPuzzle;
        [Header("Quantas pedras o jogador deve acertar para ganhar o desafio?")]
        [SerializeField] int _stoneNumber;
        [Header("Qual a distância minima para ativar o desafio?")]
        [SerializeField] int _minDistAttack;
        [Header("Quanto de tristeza vai alterar ao perder?")]
        [SerializeField] float _sadValueLost;
        [Header("Quanto de tristeza vai alterar ao ganhar?")]
        [SerializeField] float _sadValueWon;
        [Header("Quanto de alegria vai alterar ao perder?")]
        [SerializeField] float _happyValueLost;
        [Header("Quanto de alegria vai alterar ao ganhar?")]
        [SerializeField] float _happyValueWon;
        [Header("Quanto de raiva vai alterar ao perder?")]
        [SerializeField] float _angryValueLost;
        [Header("Quanto de raiva vai alterar ao ganhar?")]
        [SerializeField] float _angryValueWon;
        
        [SerializeField] InputText _inputText;
        [SerializeField] Renderer _meshBearMaterial;
        [SerializeField] GameObject _activeStoneVFX;
        #endregion

        #region Private variables
        float _minColor = 0f;
        float _maxColor = 1f;
        float _actualColor;
        float _timeLimitBase;
        int _stoneNumberBase;
        float _stoneTimerActive;
        bool _defeatPuzzle;
        bool _stopPuzzle;
        bool _canExecute;
        bool _lostPuzzle;
        bool _colliding;
        
        List<FDBearStone> _activeStones;
        FDBearAbility _fdBearAbility;
        FDPlayerStatus _fdPlayerStatus;
        Transform _playerPos;
        FDInteractEvent _interactEvent;
        #endregion
        
        void Start()
        {
            _fdBearAbility = GameObject.Find("CharacterFinal").GetComponent<FDBearAbility>();
            _playerPos = GameObject.Find("CharacterFinal").GetComponent<Transform>();
            _fdPlayerStatus = GameObject.Find("CharacterFinal").GetComponent<FDPlayerStatus>();
            for(int i = 0; i < _stoneLists.Count; i++)
                _stoneManagerList.Add(_stoneLists[i]);
            
            _activeStones = new List<FDBearStone>();
            _interactEvent = FindObjectOfType<FDInteractEvent>();
            _interactEvent.InteractEvent += CheckIfPlayerAttack;
            _actualColor = _minColor;
            _meshBearMaterial.material.SetColor("_EmissionColor", new Color(_minColor,0,0,1));
        }

        void Update()
        {
            if(!_canExecute)
            {
                if (_colliding)
                {
                    float distPlayer = Vector3.Distance(transform.position, _playerPos.position);
                    if (distPlayer < _minDistAttack)
                    {
                        _inputText.Ativa();
                    }
                    else
                    {
                        _inputText.Desativa();
                    }
                }
                return;
            }

            if(_stopPuzzle) return;
            CheckIfEnded();
            if(_stoneTimerActive < _timerAtivarPedra)
            {
                _stoneTimerActive += Time.deltaTime;
                return;
            }

            RandomIndex();
        }

        public void CheckIfPlayerAttack()
        {
            float distPlayer = Vector3.Distance(transform.position, _playerPos.position);
            if(distPlayer < _minDistAttack)
            {
                 _canExecute = true;
                _inputText.Desativa();
                _activeStoneVFX.SetActive(true);
                
                if (_lostPuzzle) {
                     _stopPuzzle = false;
                     _lostPuzzle = false;
                 } 
            }
        }

        public void CheatWinPuzzle()
        {
            _canExecute = true;
            _stoneNumberBase = _stoneNumber;
        }

        void CheckIfEnded()
        {
            if(_stoneNumberBase >= _stoneNumber)
            {
               _defeatPuzzle = true;
                _inputText.Desativa();
                _inputText.DesativaCol();
                ResetBearStone();
            } 
            _timeLimitBase += Time.deltaTime;

            if(_timeLimitBase >= _eventLostTime)
            {
                OnLosingPuzzle.Invoke();
            }
            if(_timeLimitBase >= timeLimit) 
            {
                ResetBearStone();
            }
        }
        
        void RandomIndex()
        {
            _inputText.Desativa();
            _inputText.DesativaCol();
            
            if (_stoneManagerList.Count <= 0) return;
            
            int index = Random.Range(0, _stoneManagerList.Count);
            _activeStones.Add(_stoneManagerList[index]);
            _stoneManagerList.Remove(_stoneManagerList[index]);
            _activeStones[_activeStones.Count - 1].gameObject.SetActive(true);
            _activeStones[_activeStones.Count - 1].SetAtributes(_stoneSpeed, _alturaMaximaDasPedras.position.y, _cooldownPedraDesce);
            _stoneTimerActive = 0;
        }

        public void DesactiveStone(FDBearStone thisStone)
        {
            _actualColor += _maxColor;
            _meshBearMaterial.material.SetColor("_EmissionColor", new Color(_actualColor,0,0,1));
            _activeStones.Remove(thisStone);
            _stoneManagerList.Add(thisStone);
            thisStone.gameObject.SetActive(false);
        }
        
        public void ResetBearStone()
        {
            _stopPuzzle = true;

            for (int i = 0; i < _stoneLists.Count; i++)
                _stoneLists[i].gameObject.SetActive(false);
            
            if(!_defeatPuzzle)
            {
                RestartPuzzle();
            }
            else{
                _activeStoneVFX.SetActive(false);
                _fdPlayerStatus.changeEmotions(_happyValueWon, _sadValueWon, _angryValueWon);
            }
        }

        void RestartPuzzle()
        {   
            _activeStoneVFX.SetActive(false);
            _stoneManagerList.Clear();
            _fdPlayerStatus.changeEmotions(_happyValueLost, _sadValueLost, _angryValueLost);
            _meshBearMaterial.material.SetColor("_EmissionColor", new Color(_minColor,0,0,1));
            _actualColor = _minColor;
            _activeStones.Clear();
            
            for (int i = 0; i < _stoneLists.Count; i++)
            {
                _stoneManagerList.Add(_stoneLists[i]);
            }

            _stoneTimerActive = 0;
            _stoneNumberBase = 0;
            _timeLimitBase = 0;          
            SetStopPuzzle(false);
            _lostPuzzle = true;          
        }

        public void SetStopPuzzle(bool value)
        {
            _stopPuzzle = value;
            
        }
        
        public void AddStone(int x)
        {
            _stoneNumberBase += x;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                _colliding = true;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                _colliding = false;
        }
    }
}
