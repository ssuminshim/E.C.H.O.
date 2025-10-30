using UnityEngine;
using BackEnd;

public class BackendManager : MonoBehaviour
{
    private void Awake()
    {
        // Update() 메소드의 Backend.AsyncPoll(); 호출을 위해 오브젝트를 파괴하지 않는다.
        DontDestroyOnLoad(gameObject);

        // 뒤끝 서버 초기화
        BackendSetup();
    }

    private void Update()
    {
        // 서버의 비동기 메소드 호출(콜백 함수 풀링)을 위해 작성
        // if ( Backend.IsInitialized )
        // {
        //     Backend.AsyncPoll();
        // }
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
