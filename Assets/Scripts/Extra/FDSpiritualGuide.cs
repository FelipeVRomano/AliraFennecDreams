using System.Collections.Generic;
using UnityEngine;

namespace FennecDreams.Guide
{
    public class FDSpiritualGuide : MonoBehaviour
    {
        enum StateSpirit{
            idling, 
            moving,
        }
        
        [SerializeField] private StateSpirit _currentStateSpirit;

        public bool _canFinishExecution;
        public List<FDSpiritGuideColliders> ListOfPointLists;
        
        [SerializeField] private int _minPlayerDist;
        [SerializeField] private float _spiritSpeed;
        [SerializeField] private int _indexPath;
        [SerializeField] private float _speedRotation;
        
        private Transform _playerPos;
        private Quaternion _quaternionTarget;
        
        void Start()
        {
            _playerPos = GameObject.Find("CharacterFinal").GetComponent<Transform>();
            ChangeState(StateSpirit.idling);
        }
        
        void FixedUpdate()
        {
            Move();
            if(_canFinishExecution) return;
            CheckIfPlayerIsNear();
            RotateSpirit();
        }

        void RotateSpirit()
        {
            if(_currentStateSpirit == StateSpirit.idling)
            {
                Vector3 targetRotation = _playerPos.position - transform.position;
                if(_indexPath + 1 <= ListOfPointLists.Count)
                    _quaternionTarget = Quaternion.LookRotation(-targetRotation);
            }else{
                Vector3 targetRotation = ListOfPointLists[_indexPath].gameObject.transform.position - transform.position;
                if(targetRotation != Vector3.zero)
                    _quaternionTarget = Quaternion.LookRotation(-targetRotation);
            }
            transform.rotation = Quaternion.Slerp (transform.rotation, new Quaternion(0,_quaternionTarget.y,0, _quaternionTarget.w), Time.deltaTime * _speedRotation);
        }

        void CheckIfPlayerIsNear()
        {
            bool distPlayer = ListOfPointLists[_indexPath].CanGoNextPoint;
            if(distPlayer && _currentStateSpirit == StateSpirit.idling && _indexPath + 1 < ListOfPointLists.Count)
            {
                ChangeState(StateSpirit.moving);
                _indexPath += 1;
            }
        }

        void ChangeState(StateSpirit spiritState)
        {
            _currentStateSpirit = spiritState;
        }

        void Move()
        {
            if (_indexPath + 1 == ListOfPointLists.Count && _currentStateSpirit == StateSpirit.idling)
            {
                if (!_canFinishExecution)
                {
                    float distPlayer = Vector3.Distance(ListOfPointLists[_indexPath].gameObject.transform.position, _playerPos.position);
                    if (distPlayer < _minPlayerDist)
                    {
                        _canFinishExecution = true;
                    }
                }
                return;
            }

            if(_currentStateSpirit != StateSpirit.moving)
                return;
            if(transform.position == ListOfPointLists[_indexPath].gameObject.transform.position) ChangeState(StateSpirit.idling);
                transform.position = Vector3.MoveTowards(transform.position, 
                    ListOfPointLists[_indexPath].gameObject.transform.position, _spiritSpeed * Time.deltaTime);
        }
    }
}
