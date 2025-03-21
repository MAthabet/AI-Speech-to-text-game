using UnityEngine;

public class chikenLogic : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;
    [SerializeField] private float power = 3.0f;
    [SerializeField] private float MaxHP = 100.0f;
    [SerializeField] GameObject egg;
    private float HP;
    private int direction = 1; // 1 for right, -1 for left
    [SerializeField] float eggSpawnInterval = 2.0f;
    float nextEggTime;

    void Start()
    {
        HP = MaxHP;
        nextEggTime = eggSpawnInterval;
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
        if (Time.time >= nextEggTime)
        {
            spawnEgg();
            Debug.Log("Egg Spawned");
            nextEggTime = Time.time + eggSpawnInterval;
        }
    }
    void spawnEgg()
    {
        GameObject newEgg = Instantiate(egg, transform.position, Quaternion.identity);
        newEgg.GetComponent<bulletLogic>().direction = -1;
        newEgg.GetComponent<bulletLogic>().speed = power;
        newEgg.GetComponent<bulletLogic>().damage = power;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Wall" || collision.gameObject.tag == "Chicken")
        {
            direction *= -1;
        }
        else if(collision.gameObject.tag == "Bullet")
        {
            HP -= collision.gameObject.GetComponent<bulletLogic>().damage;
            Destroy(collision.gameObject);
            if(HP <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
