using UnityEngine;

public class DoubleJumpUpgrade : MonoBehaviour
{
    public AudioClip PickupSFX;
    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.UnlockDoubleJump();
            if (PickupSFX != null)
                AudioSource.PlayClipAtPoint(PickupSFX, transform.position);
            Destroy(gameObject);
        }
    }
}