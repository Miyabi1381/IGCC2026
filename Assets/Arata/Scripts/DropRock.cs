using UnityEngine;

public class DropRock : MonoBehaviour
{
    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // 最初は重力を無効にしておく
    }

    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // プレイヤーがトリガーに入ったらオブジェクトを落とす
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 1; // 重力を有効にする

            Debug.Log("Playerが岩に当たった");
        }
    }
}