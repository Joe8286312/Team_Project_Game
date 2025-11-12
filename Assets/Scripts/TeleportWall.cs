using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportWall : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    Debug.Log("触发器已触发，尝试传送！");
        //    Transform player = other.transform;
        //    Debug.Log("玩家位置：" + player.position);

        //    // 取反x轴，将玩家传送到墙另一侧
        //    Vector3 newPos = player.position;
        //    newPos.x = -newPos.x; // x轴取反

        //    player.position = newPos;
        //    Debug.Log("新的玩家位置：" + newPos);

        //    // 玩家朝向旋转180度
        //    Vector3 playerEuler = player.eulerAngles;
        //    playerEuler.y += 180f;
        //    player.rotation = Quaternion.Euler(playerEuler);

        //    // 如果你的相机是玩家的子物体，它会跟着一同旋转，不需额外处理
        //}
    }
}
