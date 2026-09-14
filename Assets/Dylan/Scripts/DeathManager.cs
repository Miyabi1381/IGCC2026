using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;

    [System.Serializable]
    public class PlayerVariant
    {
        public string Name; // just for readability in the Inspector list
        public Sprite BodySprite;
        public Sprite DeathPoseSprite;
        public Sprite SkeletonSprite;
        public Vector3 Scale = Vector3.one; // per-variant fix for mismatched sprite sizes
    }

    [Header("--- PLAYER SETUP ---")]
    public GameObject PlayerPrefab;         // ONE base prefab, shared physics/logic
    public PlayerVariant[] PlayerVariants;  // each entry bundles body/death/skeleton sprites + scale
    public Transform RespawnPoint;

    [Header("--- CAMERA ---")]
    public CinemachineCamera GameCamera;

    [Header("--- STAMINA UI ---")]
    public StaminaBarFollow StaminaBarPosition; // drag the Canvas (or bar object) holding StaminaBarFollow.cs here
    public StaminaBarUI StaminaBarDisplay;      // drag the StaminaBar object holding StaminaBarUI.cs here

    [Header("--- CORPSE CLEANUP ---")]
    public int MaxCorpses = 10; // total corpses allowed in the scene (active + retired); oldest is deleted past this

    [Header("--- ABILITY UNLOCKS (persist across every future spawn) ---")]
    public bool DoubleJumpUnlocked = false; // lives here, not on PlayerController, so it survives Destroy

    [Header("--- OFFERINGS ---")]
    public int CurrentOfferings = 0;       // what the current player is carrying right now
    private List<Sprite> CollectedOfferingSprites = new List<Sprite>();
    public GameObject OfferingPickupPrefab; // spawned as scattered drops when the player dies
    public float DropScatterRadius = 0.6f;  // how far apart dropped offerings scatter from the death point
    public LayerMask SolidLayer;            // Ground + Wall (+ Corpse if desired) — dropped offerings won't spawn inside these
    public float OfferingCheckRadius = 0.15f; // roughly the offering sprite's own radius, for the overlap check

    private int deathCount = 0;
    private GameObject currentPlayer;
    private GameObject activeCorpse; // the most recently died player — the only collidable corpse
    private System.Collections.Generic.List<GameObject> corpseHistory = new System.Collections.Generic.List<GameObject>(); // oldest first

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnNextPlayer(); // spawns the very first player too, so it gets a variant + death/skeleton sprites like everyone else
    }

    // Runtime lookup so prefabs (like StageTrigger) never need a direct serialized scene
    // reference to the camera's confiner — they just ask DeathManager for it when needed.
    public CinemachineConfiner2D GetCameraConfiner()
    {
        return GameCamera != null ? GameCamera.GetComponent<CinemachineConfiner2D>() : null;
    }

    public void RegisterPlayer(GameObject player)
    {
        currentPlayer = player;

        // Apply any persistent unlocks to this new player instance
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null && DoubleJumpUnlocked)
            controller.HasDoubleJumpUnlocked = true;

        // Retarget the Cinemachine camera to follow whichever player is currently alive
        if (GameCamera != null)
            GameCamera.Follow = player.transform; // convenience alias for Target.TrackingTarget

        // Retarget the stamina bar's position-follow and its data source to the new player
        if (StaminaBarPosition != null)
            StaminaBarPosition.SetPlayer(player.transform);

        if (StaminaBarDisplay != null && controller != null)
            StaminaBarDisplay.SetPlayer(controller);
    }

    // Called by the double-jump pickup. Applies immediately to the current player too,
    // not just future ones, in case the pickup is found mid-run rather than after a death.
    public void UnlockDoubleJumpPermanently()
    {
        DoubleJumpUnlocked = true;

        if (currentPlayer != null)
        {
            PlayerController controller = currentPlayer.GetComponent<PlayerController>();
            if (controller != null)
                controller.HasDoubleJumpUnlocked = true;
        }
    }

    // Called by an OfferingPickup when the current player collects it.
    public void CollectOffering(Sprite offeringSprite)
    {
        CurrentOfferings++;
        CollectedOfferingSprites.Add(offeringSprite);
    }

    // Called by PlayerDied() — scatters the current offering count as pickups near the death spot,
    // then clears the carried count, since dying costs you whatever you were holding.
    private void DropOfferingsAtDeath(Vector3 position)
    {
        if (OfferingPickupPrefab == null || CollectedOfferingSprites.Count <= 0)
            return;

        foreach (Sprite offeringSprite in CollectedOfferingSprites)
        {
            Vector3 spawnPos = FindValidScatterPosition(position);

            GameObject newOffering = Instantiate(
                OfferingPickupPrefab,
                spawnPos,
                Quaternion.identity
            );

            OfferingPickup pickup = newOffering.GetComponent<OfferingPickup>();

            if (pickup != null)
                pickup.SetOfferingSprite(offeringSprite);
        }

        CollectedOfferingSprites.Clear();
        CurrentOfferings = 0;
    }

    // Tries several random offsets around the death point, only accepting one that doesn't overlap
    // solid geometry (walls/ground) — so offerings never spawn stuck inside a wall or off a ledge
    // into unreachable space. Falls back to the exact death position if nothing valid is found.
    private Vector3 FindValidScatterPosition(Vector3 origin)
    {
        const int maxAttempts = 8;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * DropScatterRadius;
            Vector3 candidate = origin + new Vector3(randomOffset.x, randomOffset.y, 0f);

            bool overlapsSolid = Physics2D.OverlapCircle(candidate, OfferingCheckRadius, SolidLayer);
            if (!overlapsSolid)
                return candidate;
        }

        return origin; // every attempt failed — just drop it exactly where the player died
    }

    // Called by PlayerController.Die() right as a player becomes a corpse.
    public void PlayerDied(GameObject corpse)
    {
        // Retire whatever was previously the active (collidable) corpse before this new one takes over.
        if (activeCorpse != null)
        {
            PlayerController prevController = activeCorpse.GetComponent<PlayerController>();
            if (prevController != null)
                prevController.RetireCorpse();
        }

        activeCorpse = corpse; // this new corpse is now the only collidable one
        corpseHistory.Add(corpse);

        TrimOldestCorpses();
        DropOfferingsAtDeath(corpse.transform.position); // scatter carried offerings at the death spot

        deathCount++;
        SpawnNextPlayer();
    }

    // Deletes the oldest corpses once the total exceeds MaxCorpses, keeping the scene from
    // accumulating an unbounded number of dead player objects over a long play session.
    private void TrimOldestCorpses()
    {
        if (MaxCorpses <= 0) return; // 0 or negative = no cap, skip cleanup entirely

        while (corpseHistory.Count > MaxCorpses)
        {
            GameObject oldest = corpseHistory[0];
            corpseHistory.RemoveAt(0);

            if (oldest != null)
                Destroy(oldest);
        }
    }

    private void SpawnNextPlayer()
    {
        if (PlayerPrefab == null)
        {
            Debug.LogWarning("DeathManager: No PlayerPrefab assigned.");
            return;
        }

        if (RespawnPoint == null)
        {
            Debug.LogWarning("DeathManager: No RespawnPoint assigned.");
            return;
        }

        GameObject newPlayer = Instantiate(PlayerPrefab, RespawnPoint.position, Quaternion.identity);
        currentPlayer = newPlayer;

        // Apply the next variant's full look — body sprite, matching death/skeleton sprites, and scale —
        // all bundled together so they can never drift out of sync or need manual per-death fixing.
        PlayerController controller = newPlayer.GetComponent<PlayerController>();

        if (PlayerVariants != null && PlayerVariants.Length > 0)
        {
            PlayerVariant variant = PlayerVariants[deathCount % PlayerVariants.Length];

            SpriteRenderer sr = newPlayer.GetComponent<SpriteRenderer>();
            if (sr != null && variant.BodySprite != null)
                sr.sprite = variant.BodySprite;

            newPlayer.transform.localScale = variant.Scale;

            if (controller != null)
                controller.SetVariantDeathAssets(variant.DeathPoseSprite, variant.SkeletonSprite);
        }

        // Register immediately rather than waiting for newPlayer's own Start() — Start() is deferred
        // until just before next Update, which leaves Cinemachine's Follow null for a frame otherwise.
        RegisterPlayer(newPlayer);
    }
}