using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FDTutorialManager : MonoBehaviour
{
    public FDTutorialText[] _tutorialText;
    public UnityEvent _onStartCollision;
    public UnityEvent _onEndCollision;

    [SerializeField] bool _needInput;
    [SerializeField] Text _uiTextEvent;
    
    bool _executed;
    int _indexText;

    void OnTriggerEnter(Collider other)
    {
        if (_executed || !other.CompareTag("Player"))
            return;

        _onStartCollision.Invoke();
        _executed = true;   
    }
    
    public void ReceiveInput()
    {
        if (_executed && _needInput)
        {
            _indexText++;
            if (_indexText >= _tutorialText.Length)
            {
                _onEndCollision.Invoke();
                _needInput = false;
            }
            else
            {
                if (!_tutorialText[_indexText].changeTextObj)
                    _uiTextEvent.text = _tutorialText[_indexText].tutorialText;
                else
                {
                    _uiTextEvent.text = "";
                    _tutorialText[_indexText].textThisGameObject.text = _tutorialText[_indexText].tutorialText;
                }

                if (_tutorialText[_indexText].changeGameObjects)
                {
                    if (_tutorialText[_indexText].activeGameObjects.Length > 0)
                    {
                        for (int i = 0; i < _tutorialText[_indexText].activeGameObjects.Length; i++)
                        {
                            _tutorialText[_indexText].activeGameObjects[i].SetActive(true);
                        }
                    }
                    if (_tutorialText[_indexText].disableGameObjects.Length > 0)
                    {
                        for (int i = 0; i < _tutorialText[_indexText].disableGameObjects.Length; i++)
                        {
                            _tutorialText[_indexText].disableGameObjects[i].SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
