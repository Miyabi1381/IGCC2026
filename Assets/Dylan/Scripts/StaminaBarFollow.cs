using UnityEngine;

public class StaminaBarFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 1.2f, 0f); // tune to sit above the head

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Position-only sync — deliberately ignores player's rotation and scale,
        // so the bar never flips or resizes when the character does
        transform.position = player.position + offset;
    }
}