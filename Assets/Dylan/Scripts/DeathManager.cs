using UnityEngine;

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
    public CameraFollow GameCamera; // drag the camera holding CameraFollow.cs here

    [Header("--- STAMINA UI ---")]
    public StaminaBarFollow StaminaBarPosition; // drag the Canvas (or bar object) holding StaminaBarFollow.cs here
    public StaminaBarUI StaminaBarDisplay;      // drag the StaminaBar object holding StaminaBarUI.cs here

    [Header("--- CORPSE CLEANUP ---")]
    public int MaxCorpses = 10; // total corpses allowed in the scene (active + retired); oldest is deleted past this

    [Header("--- ABILITY UNLOCKS (persist across every future spawn) ---")]
    public bool DoubleJumpUnlocked = false; // lives here, not on PlayerController, so it survives Destroy

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

    public void RegisterPlayer(GameObject player)
    {
        currentPlayer = player;

        // Apply any persistent unlocks to this new player instance
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null && DoubleJumpUnlocked)
            controller.HasDoubleJumpUnlocked = true;

        // Retarget the camera to follow whichever player is currently alive
        if (GameCamera != null)
            GameCamera.SetTarget(player.transform);

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
        if (PlayerVariants != null && PlayerVariants.Length > 0)
        {
            PlayerVariant variant = PlayerVariants[deathCount % PlayerVariants.Length];

            SpriteRenderer sr = newPlayer.GetComponent<SpriteRenderer>();
            if (sr != null && variant.BodySprite != null)
                sr.sprite = variant.BodySprite;

            newPlayer.transform.localScale = variant.Scale;

            PlayerController controller = newPlayer.GetComponent<PlayerController>();
            if (controller != null)
                controller.SetVariantDeathAssets(variant.DeathPoseSprite, variant.SkeletonSprite);
        }
    }
}