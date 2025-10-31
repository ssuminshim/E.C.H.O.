using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [SerializeField]
    Transform player;
    private void FixedUpdate()
    {
        Vector3 targetPos = new Vector3(player.position.x, player.position.y, player.position.z);

        //targetPos.x
        // 만약 x < 0 
        // 카메라에 그만큼 +

        float xOffset = 0;
        float yOffset = 0;


        if (player.position.x < 0.1)
        {
            xOffset = -player.position.x;
        }
        if (player.position.x > 100)
        {
            xOffset = -player.position.x - 100;
        }
        if (player.position.y < 0.1)
        {
            yOffset = -player.position.y;
        }
        if (player.position.x > 100)
        {
            yOffset = -player.position.y - 100;
        }
        else
        {
            xOffset = 0;
            yOffset = 0;
        }
    }
}
