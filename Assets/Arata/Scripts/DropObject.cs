using System.Collections;
using UnityEngine;

public class DropObject : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float GravityScale = 1f; // Gravity scale for the falling rock 重力

    [Header("Vibration settings 揺れの設定")]
    [SerializeField] private float shakeDuration = 0.35f; // shake duration in seconds 揺れる時間
    [SerializeField] private float shakeMagnitude = 0.06f; // Range of fluctuation  揺れ幅

    private bool isTriggered = false;
    private bool hasStopped = false;
    private Vector3 originalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        originalPos = transform.position;

        // 最初は子オブジェクトをオフにしておく
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーが検出されたが、ドロップシーケンスがトリガーされていない場合
        if (other.gameObject.CompareTag("Player") && !isTriggered)
        {
            // 重複判定を防ぐため、触れた瞬間にフラグを立てる
            isTriggered = true;
            hasStopped = false; // フラグをリセット
            StartCoroutine(DropSequence());
        }
    }

    // ドロップシーケンスの処理
    // Drop sequence processing
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

        // 落下開始のタイミングで子オブジェクトをオンにする
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }

        // 落下開始
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = GravityScale;

        Debug.Log("Drop");

        // 少し待機
        yield return new WaitForSeconds(0.1f);

        // 停止処理が呼ばれておらず、かつ一定以上の速度で動いている間は待機
        while (!hasStopped && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            yield return null;
        }

        // 速度がほぼゼロになった場合、停止処理を呼ぶ
        if (!hasStopped)
        {
            StopAndDeactivate();
        }
    }

    // 停止と子オブジェクトの無効化処理
    // Stop and deactivate process
    private void StopAndDeactivate()
    {
        // 既に停止済みならスキップ
        if (hasStopped) return;
        hasStopped = true;

        // 落下を完全に止める
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        // 子オブジェクトを無効化する
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        Debug.Log("Stopped and Deactivated");
    }

    // 地面に当たったかの判定
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Groundレイヤーに当たった場合は即座に停止処理を呼ぶ
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            StopAndDeactivate();
        }
    }

    // リセット処理
    // Reset processing
    public void ResetDropObject()
    {
        isTriggered = false;
        hasStopped = false; 

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // 落下中の勢いもリセットする
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = originalPos;

        // 子オブジェクトを無効にする
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}