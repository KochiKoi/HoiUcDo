using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public bool Throwable;
    public bool Explosion;
    private bool hasLanded;
    private Rigidbody2D rb;

    void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.isKinematic = false;
        }
    }


    //trigger to celebrate Ground
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasLanded && other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Debug.Log("Trigger on Ground-Stopp!!");

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            hasLanded = true;
        }
    }
    public void ResetLanding()
    {
        hasLanded = false;
        Debug.Log("Reset Landing");
    }


    public void ActivateExplosion()
    {
        if (!Explosion) return;

        // Thêm hiệu ứng nổ ở đây nếu có
        Debug.Log("💥 Boom");

        // Ví dụ: phá huỷ vật thể sau khi nổ
        Destroy(gameObject);
    }
}


