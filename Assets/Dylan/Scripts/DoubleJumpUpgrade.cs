using UnityEngine;

public class DoubleJumpUpgrade : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.UnlockDoubleJump();
            // TODO: play pickup VFX/SFX here
            Destroy(gameObject);
        }
    }
}