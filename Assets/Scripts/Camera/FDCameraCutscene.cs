using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FDCameraCutscene : MonoBehaviour
{
    public UnityEvent OnCutscenePuzzleStart;
    public UnityEvent OnCutscenePuzzleEnd;
    
    public bool blockCutscenes;
    public static bool isOnCutscene;
    
    [SerializeField] Transform _cameraCutscenePos;
    [SerializeField] List<Transform> _cameraCutscenePoint = new List<Transform>();
    [SerializeField] Vector3 _cameraRotation;
    [SerializeField] float _timeCutscenePuzzle = 5;
    
    int _indexCutscene;
    float _cutsceneTimeBase;
    Transform _parentTrans;
    bool _cutsceneBool;
    bool _cutsceneActivated;
    
    public void InvokeCutscenePuzzles()
    {
        if(!blockCutscenes)
            StartCoroutine(Cutscene());
    }

    public void SetOnCutscene(bool bValue)
    {
        isOnCutscene = bValue;
    }

    void Update()
    {
        CutsceneGuideInvoked();    
    }
    
    IEnumerator Cutscene()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OnCutscenePuzzleStart?.Invoke();
        gameObject.transform.position = _cameraCutscenePos.position;
        gameObject.transform.GetComponent<Camera>().fieldOfView = 60; 
        gameObject.transform.eulerAngles = _cameraRotation;
        
        yield return new WaitForSecondsRealtime(_timeCutscenePuzzle);
        gameObject.transform.GetComponent<Camera>().fieldOfView = 40;
        OnCutscenePuzzleEnd?.Invoke();
    }

    public void InvokeCutsceneSpiritualGuide()
    {
        if (blockCutscenes) return;
        if(!_cutsceneBool)
            StartCoroutine(CutsceneSpiritualGuide());
        _cutsceneBool = true;
    }
    
    void CutsceneGuideInvoked()
    {
        if (!_cutsceneActivated)
            return;

        _cutsceneTimeBase += Time.deltaTime;

        if (_cutsceneTimeBase > _timeCutscenePuzzle)
        {
            _cutsceneTimeBase = 0;
            _indexCutscene++;

            if(_indexCutscene >= _cameraCutscenePoint.Count)
            {
                _indexCutscene = 0;
                _cutsceneActivated = false;
                return;
            }
            gameObject.transform.position = _cameraCutscenePoint[_indexCutscene].position;
            gameObject.transform.eulerAngles = _cameraCutscenePoint[_indexCutscene].eulerAngles;
        }
    }
    
    IEnumerator CutsceneSpiritualGuide()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        OnCutscenePuzzleStart?.Invoke();
        gameObject.transform.GetComponent<Camera>().fieldOfView = 60;

        _cutsceneActivated = true;
        
        gameObject.transform.position = _cameraCutscenePoint[_indexCutscene].position;
        gameObject.transform.eulerAngles = _cameraCutscenePoint[_indexCutscene].eulerAngles;
        while (_cutsceneActivated)
            yield return null;

        gameObject.transform.GetComponent<Camera>().fieldOfView = 40;
        OnCutscenePuzzleEnd?.Invoke();
    }

}

