using UnityEngine;

public class StaminaBarFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 1.2f, 0f); 

    void LateUpdate()
    {
        transform.position = player.position + offset;
    }
}