using UnityEngine;

public class Rockfall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object has the layer "Ground"
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            // Destroy this rock object
            Destroy(gameObject);
        }
    }
}
