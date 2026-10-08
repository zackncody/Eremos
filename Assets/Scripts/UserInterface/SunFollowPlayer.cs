using UnityEngine;

public class SunFollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 5f, 0f);

    void LateUpdate()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            else return;
        }

        transform.position = player.position + offset;
    }
}