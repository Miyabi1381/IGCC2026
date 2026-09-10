using UnityEngine;
using UnityEngine.InputSystem; 

public class FragCheck : MonoBehaviour
{
    void Start()
    {
    }

    void Update()
    {
        // Keyboard.currentがnullでないか確認し、スペースキーが押された瞬間を検知
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            DropObject[] dropObjects = FindObjectsByType<DropObject>(FindObjectsSortMode.None);
            Minecart[] minecarts = FindObjectsByType<Minecart>(FindObjectsSortMode.None);
            foreach (DropObject dropObject in dropObjects)
            {
                dropObject.ResetDropObject();
            }
            foreach (Minecart minecart in minecarts)
            {
                minecart.ResetMinecart();
            }
        }
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