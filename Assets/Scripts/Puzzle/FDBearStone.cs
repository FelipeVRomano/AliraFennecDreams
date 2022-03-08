using UnityEngine;

namespace FennecDreams.Puzzles
{
    public class FDBearStone : MonoBehaviour
    {
        private FDBearStoneManage _fdStoneManager;
        private FDBearStone _fdBearStone;
        private float _speedRock;
        private float _maxPosY;
        private float _minPosY;
        private float _cooldownRock;
        private float _cooldownBase;
        private bool _fallingBack;
        private bool _canActivate;
        private float _initialPos;
        private int _useIndex;
        
        void Start()
        {
            _initialPos = this.transform.position.y;
            _fdStoneManager = transform.parent.GetComponent<FDBearStoneManage>();
            _fdBearStone = GetComponent<FDBearStone>();
        }
        void OnEnable()
        {
            _fallingBack = false;
            _cooldownBase = 0;
            if(_useIndex > 0)
            {
                transform.position = new Vector3(transform.position.x, _initialPos, transform.position.z);
            }
            else{
                _useIndex++;
            }
        }

        void Update()
        {
            if(!_canActivate) return;
            if(_fallingBack)
            FallingBack();
            else FirstStep();
        }

        void FirstStep()
        {
            if (transform.position.y <= _maxPosY)
            {
                transform.Translate(Vector3.up * _speedRock * Time.deltaTime);
            }
            else
            {
                _cooldownBase += Time.deltaTime;
            }
            if(_cooldownBase >= _cooldownRock)
                _fallingBack = true;
        }
        
        void FallingBack()
        {
            transform.Translate(Vector3.down * _speedRock * Time.deltaTime);

            if (transform.position.y <= _initialPos)
            {
                _fdStoneManager.DesactiveStone(_fdBearStone);

            }
        }

        public void SetAtributes(float speed, float heightPosYmax, float cooldown)
        {
            _speedRock = speed;
            _maxPosY = heightPosYmax;
            _cooldownRock = cooldown;
            _canActivate = true;
        }

        public void PlayerHitStone()
        {
            _fdStoneManager.AddStone(1);
            _fdStoneManager.DesactiveStone(_fdBearStone);
        }
    }
}