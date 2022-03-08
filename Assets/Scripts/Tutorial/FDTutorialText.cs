using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class FDTutorialText
{
    [Tooltip("If true, gameObject will be changed after use")]
    public bool changeGameObjects;
    [Tooltip("If true, textObj will be changed after use")]
    public bool changeTextObj;
    
    public GameObject[] activeGameObjects;
    public GameObject[] disableGameObjects;
    public Text textThisGameObject;
    public string tutorialText;
}
