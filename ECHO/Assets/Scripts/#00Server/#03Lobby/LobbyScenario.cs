using BackEnd;
using UnityEngine;

public class LobbyScenario : MonoBehaviour
{   
    private void Start()
    {
        BackendGameData.Instance.GameDataLoad();
    }
}
