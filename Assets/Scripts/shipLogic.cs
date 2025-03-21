using UnityEngine;
public class shipLogic : MonoBehaviour
{
    public enum ShipState
    {
        Attack,
        Defense
    }

    private Transform currentTarget;
    public ShipState currentState;
    private bool shieldActive = false;
    public GameObject bulletPrefab; // Assign in inspector
    public float shootingForce = 10f;
    public float followSpeed = 5f;
    public float HP = 100f;
    GameObject shield;
    float nextShoot = 0;
    public float shootInterval = 0.5f;

    void Start()
    {
        currentState = ShipState.Attack;
        shield = GetComponentInChildren<GameObject>();
        shield.active = false;
    }

    void Update()
    {
        if (currentState == ShipState.Attack && currentTarget != null)
        {
            FollowTarget(currentTarget);
        }
        if (Time.time >= nextShoot)
        {
            shoot();
            nextShoot = Time.time + shootInterval;
        }
    }
    void shoot()
    {
        GameObject newEgg = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        newEgg.GetComponent<bulletLogic>().direction = 1;
        newEgg.GetComponent<bulletLogic>().speed = shootingForce;
        newEgg.GetComponent<bulletLogic>().damage = shootingForce;
    }

    public void Shoot()
    {
        if (currentState == ShipState.Attack && bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.AddForce(transform.up * shootingForce, ForceMode2D.Impulse);
            }
        }
    }
    void setState(ShipState a)
    {
        if (a == ShipState.Attack   )
        {
            currentState = ShipState.Attack;
            ActivateShield();
        }
        else if (a == ShipState.Defense)
        {
            currentState = ShipState.Defense;
            ActivateShield();
        }
    }
    public void ActivateShield()
    {
        if (currentState == ShipState.Defense)
        {
            shieldActive = true;
            shield.active = true;
        }
        else
        {
            shieldActive = false;
            shield.active = false;
        }
    }

        public void FollowTarget(Transform target)
    {
        if (target != null && currentState == ShipState.Attack)
        {
            currentTarget = target;
            float xDirection = (target.position.x - transform.position.x);
            Vector3 newPosition = transform.position;
            newPosition.x += Mathf.Sign(xDirection) * followSpeed * Time.deltaTime;
            transform.position = newPosition;
        }
        else
        {
            currentTarget = null;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            if (shieldActive)
            {
                Destroy(collision.gameObject);
            }
            else
            {
                Destroy(collision.gameObject);
                // Add damage logic here
                HP -= collision.gameObject.GetComponent<bulletLogic>().damage;
                if (HP <= 0)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

}
