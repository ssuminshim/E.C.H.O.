using UnityEngine;
using BackEnd;

public class BackendManager : MonoBehaviour
{
    public static BackendManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            // 내가 첫 번째 인스턴스라면
            Instance = this;
            DontDestroyOnLoad(gameObject); // 나를 파괴하지 않음
        }
        else
        {
            // 이미 인스턴스가 있다면 (중복)
            Destroy(gameObject); // 새로 생긴 나 자신을 파괴
            return; // 중복 객체는 초기화를 실행하면 안 됨
        }
        
        // 뒤끝 서버 초기화 (첫 번째 인스턴스만 실행함)
        BackendSetup();
    }

    private void BackendSetup()
    {
        // 뒤끝 초기화
        var bro = Backend.Initialize();

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            // 초기화 성공일 경우 statusCode 204 Success
            Debug.Log("초기화 성공 : " + bro);
        }
        else
        {
            // 초기화 실패일 경우 statusCode 400대 에러
            Debug.LogError("초기화 실패 : " + bro);
        }
    }

}