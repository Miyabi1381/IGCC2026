using UnityEngine;

public class OfferingPickup : MonoBehaviour
{
    public SpriteRenderer SpriteRenderer;

    private Sprite offeringSprite;

    void Awake()
    {
        // Automatically find the SpriteRenderer if one wasn't assigned
        if (SpriteRenderer == null)
            SpriteRenderer = GetComponent<SpriteRenderer>();

        // Remember what this pickup looked like originally
        if (SpriteRenderer != null)
            offeringSprite = SpriteRenderer.sprite;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (DeathManager.Instance != null)
            DeathManager.Instance.CollectOffering(offeringSprite);

        // TODO: play pickup VFX/SFX here

        Destroy(gameObject);
    }

    // Used by DeathManager when spawning a dropped offering
    public void SetOfferingSprite(Sprite newSprite)
    {
        offeringSprite = newSprite;

        if (SpriteRenderer != null && newSprite != null)
            SpriteRenderer.sprite = newSprite;
    }
}