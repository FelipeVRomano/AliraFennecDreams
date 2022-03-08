using UnityEngine;

namespace FennecDreams.Player
{
    public class FDPlayerStatus : MonoBehaviour
    {
        public enum Emotions{
            Happiness,
            Sadness, 
            Rage,
            None          
        }
        
        public Emotions PredominantEmotion;
        
        [Range (1,100)]
        [SerializeField] private float _happyValue;
        [Range (1,100)]
        [SerializeField] private float _sadValue;
        [Range (1,100)]
        [SerializeField] private float _rageValue;
        
        #region Orb effects
        [SerializeField] GameObject _orbHappyEffect;
        [SerializeField] GameObject _orbSadEffect;
        [SerializeField] GameObject _orbAngryEffect;
        [SerializeField] GameObject _orbNeutralEffect;
        [SerializeField] Renderer _orbColor;
        [SerializeField] [ColorUsage(true,true)]Color _sadColor;
        [SerializeField] [ColorUsage(true, true)] Color _angryColor;
        [SerializeField] [ColorUsage(true, true)] Color _happyColor;
        [SerializeField] FDMultipleMaterialShader _materialShader;
        [SerializeField] float _orbEffectSpeed;
        [SerializeField] private int _transictionOrbValue;
        #endregion
        
        #region Buff Config
        
        [Header("Buff config")]
        [Range(1,100)]
        [SerializeField] private float buffValue = 85f;
        [Header("Qual o valor minimo para o fim do buff")]
        [Range(1,100)]
        [SerializeField] private float _endBuffValue = 50f;
        [Range(0,10)]
        [SerializeField] private float _decreaseBuffValue = 3f;
        #endregion
        
        #region Private Variables

        FDMoveScript _moveScript;
        FDDashScript _dashScript;
        
        bool _buffHappy, _buffSad, _buffRage;
        #endregion
        
        void Start()
        {
            _moveScript = GetComponent<FDMoveScript>();
            _dashScript = GetComponent<FDDashScript>();
            PredominantEmotion = Emotions.None;
            RedefineEmotionsValue(_happyValue, _sadValue, _rageValue);
        }
        
        void Update()
        {
            DecreaseBuff();
        }
        
        void DecreaseBuff()
        {
            if(_buffHappy) {
                _happyValue -= _decreaseBuffValue * Time.deltaTime;
                
                if(_happyValue <= _endBuffValue) {
                    _buffHappy = false;
                    PredominantEmotion = Emotions.None;
                    DefineDominantEmotion();
                }
            } 
            if(_buffRage) {
                _rageValue -= _decreaseBuffValue * Time.deltaTime;
                
                if(_rageValue <= _endBuffValue){
                     _buffRage = false;
                     PredominantEmotion = Emotions.None;
                    DefineDominantEmotion();
                }
            }
            if(_buffSad) {
                _sadValue -= _decreaseBuffValue * Time.deltaTime;
                
                if(_sadValue <= _endBuffValue){
                     _buffSad = false;
                     PredominantEmotion = Emotions.None;
                     DefineDominantEmotion();
                }
            }
        }

        void ChangeEmotionImpact()
        {
            int emotionIndex = 0;
            if(PredominantEmotion == Emotions.Happiness) emotionIndex = 1;
            else if(PredominantEmotion == Emotions.Sadness) emotionIndex = 2;

            _moveScript.PlayerSpeed(emotionIndex);
            _dashScript.DashSpeed(emotionIndex);
            _moveScript.GravityValue(emotionIndex);
        }

        public void ChangeEmotions(float changeHappy, float changeSad, float changeRage)
        {
            if(_happyValue < _transictionOrbValue && _rageValue < _transictionOrbValue)
                _sadValue += changeSad;
            _sadValue = Mathf.Clamp(_sadValue, 1, 100);
            
            if(_sadValue < _transictionOrbValue && _happyValue < _transictionOrbValue)
                _rageValue += changeRage;
            _rageValue = Mathf.Clamp(_rageValue, 1, 100);
            
            if(_sadValue < _transictionOrbValue && _rageValue < _transictionOrbValue)
                _happyValue += changeHappy;
            _happyValue = Mathf.Clamp(_happyValue, 1, 100);
            
            float dominantEmotion = Mathf.Max(changeHappy, changeSad, changeRage);
            if (dominantEmotion == changeSad) _materialShader.InterpolateMaterials(_orbEffectSpeed, _sadColor);
            else if (dominantEmotion == changeRage) _materialShader.InterpolateMaterials(_orbEffectSpeed, _angryColor);
            else if (dominantEmotion == changeHappy) _materialShader.InterpolateMaterials(_orbEffectSpeed, _happyColor);

            DefineDominantEmotion();
        }

        public void RedefineEmotionsValue(float newChangeHappy, float newChangeSad, float newChangeRage)
        {
            if(_happyValue < _transictionOrbValue && _rageValue < _transictionOrbValue)
                _sadValue = newChangeSad;
            else{
                if(_sadValue > _transictionOrbValue)
                 _sadValue = _transictionOrbValue - 1;
            }
            if(_sadValue < _transictionOrbValue && _happyValue < _transictionOrbValue)
                _rageValue = newChangeRage; 
            else{
                if (_rageValue > _transictionOrbValue)
                    _rageValue = _transictionOrbValue - 1;
            }
            if(_sadValue < _transictionOrbValue && _rageValue < _transictionOrbValue)
                _happyValue = newChangeHappy;
            else{
                if (_happyValue > _transictionOrbValue)
                    _happyValue = _transictionOrbValue - 1;
            }
            DefineDominantEmotion();
        }

        public void DefineDominantEmotion()
        {           
            if (_sadValue >= buffValue)
            {
                if(PredominantEmotion == Emotions.None)
                {
                    PredominantEmotion = Emotions.Sadness;
                    _buffSad = true;
                    _orbSadEffect.SetActive(true);
                    _orbNeutralEffect.SetActive(false);
                    _orbHappyEffect.SetActive(false);
                    _orbAngryEffect.SetActive(false);
                }
            }
            else if(_rageValue >= buffValue)
            {
                if(PredominantEmotion == Emotions.None)
                {
                     PredominantEmotion = Emotions.Rage;
                     _buffRage = true;
                    _orbAngryEffect.SetActive(true);
                    _orbNeutralEffect.SetActive(false);
                    _orbHappyEffect.SetActive(false);
                    _orbSadEffect.SetActive(false);
                }
            } else if(_happyValue >= buffValue)
            {
                if(PredominantEmotion == Emotions.None)
                {
                     PredominantEmotion = Emotions.Happiness;
                     _buffHappy = true;
                    _orbHappyEffect.SetActive(true);
                    _orbNeutralEffect.SetActive(false);
                    _orbAngryEffect.SetActive(false);
                    _orbSadEffect.SetActive(false);
                }
            } else{
                if(!_buffHappy && !_buffRage && !_buffSad) {
                    _orbHappyEffect.SetActive(false);
                    _orbAngryEffect.SetActive(false);
                    _orbSadEffect.SetActive(false);
                    _orbNeutralEffect.SetActive(true); 

                    if(_happyValue > _transictionOrbValue)
                    {
                        _orbColor.material.SetColor("Color_DF812E5B", _happyColor);
                    }
                    else if(_rageValue > _transictionOrbValue)
                    {
                        _orbColor.material.SetColor("Color_DF812E5B", _angryColor);
                    }
                    else if(_sadValue > _transictionOrbValue)
                    {
                        _orbColor.material.SetColor("Color_DF812E5B", _sadColor);
                    }
                }               
            }
            
            ChangeEmotionImpact();
        }

        public float HappyValue()
        {
            return _happyValue;
        }

        public float SadValue()
        {
            return _sadValue;
        }

        public float RageValue()
        {
            return _rageValue;
        }
    }
}

