using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float radius = 2f;
    public float force = 10f;
    public LayerMask targetLayer;
    public bool destroyTargets = true;
    public float lifetime = 0.5f;

    void Start()
    {

        Explode();
        Destroy(gameObject, lifetime);

        void Explode()
        {
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, targetLayer);

            foreach (Collider2D col in targets)
            {
                if (destroyTargets)
                {
                    if (col.CompareTag("Breakable"))
                    {
                        Destroy(col.gameObject);
                    }
                }
                //Add Thrust(Can be remove if unnecessary)
                Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = col.transform.position - transform.position;
                    rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
                }
            }
        }
    }
}

