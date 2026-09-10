using UnityEngine;

public class FragCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Trap"))
        {
            // Destroy the player object when it collides with this object
            Debug.Log("Player collided with FragCheck. Destroying player object.");
        }
    }
}
