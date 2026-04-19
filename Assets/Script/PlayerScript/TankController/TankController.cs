using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour
{
    [Header("Tank Movement")]
    public float tankspeed = 5f;
    private Rigidbody2D rb;
    private float moveInput;

    [Header("TankShooting")]
    public GameObject bulletPrefab;
    public Transform ShootPoint;
    public float bulletForce = 10f;
    public float fireDelay = 0.5f;
    private float nextFireTime = 0f;
    public bool isControlled; // using when switched Player to Tank(if using),
    // If you using cutscene instead of switched case, unnecessary to use

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
    
        TankMove();
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireDelay;
        }
    }



    private void TankMove()
    {

        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * tankspeed, rb.velocity.y);
    }

    void Shoot()
    {
        Debug.Log("Shoot!!!!!");

        if (bulletPrefab && ShootPoint)
        {
            GameObject bullet = Instantiate(bulletPrefab, ShootPoint.position, ShootPoint.rotation);
            Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();
            if (rbBullet != null)
            {
                rbBullet.AddForce(ShootPoint.right * bulletForce, ForceMode2D.Impulse);
            }
        }
    }

}
