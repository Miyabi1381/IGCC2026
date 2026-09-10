using System.Collections;
using UnityEngine;

public class DropObject : MonoBehaviour
{
    // 当たり判定を2つ使います、IsTrigger付きがプレイヤーが通ったかのフラグで、なしが地面との衝突判定です
    // use two colliders: one with IsTrigger for detecting the player, and another without it for ground collision detection.

    private Rigidbody2D rb;
    [SerializeField] private float GravityScale = 1f; // Gravity scale for the falling rock 重力

    [Header("Telegraph Settings")]
    [SerializeField] private float shakeDuration = 0.35f; // shake duration in seconds 揺れる時間
    [SerializeField] private float shakeMagnitude = 0.06f; // Range of fluctuation  揺れ幅

    private bool isTriggered = false;
    private Vector3 originalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        originalPos = transform.position;
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Player is detected and the drop sequence hasn't been triggered
        if (other.gameObject.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(DropSequence());
        }
    }

    // Coroutine to handle the drop sequence
    // 落下するときの処理
    private IEnumerator DropSequence()
    {
        float elapsed = 0f;

        // 指定された時間だけ振動させる
        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position = originalPos + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 振動が終わったら元の位置に戻す
        transform.position = originalPos;

        // 落下開始
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = GravityScale;

        Debug.Log("Drop");
    }

    // 地面に当たったかの判定
    // Collision detection with the ground
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // 落下後、地面に当たったら落下を止める
            rb.bodyType = RigidbodyType2D.Kinematic;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            Debug.Log("Hit Ground");
        }
    }
}