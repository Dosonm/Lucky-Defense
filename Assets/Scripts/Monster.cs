using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Monster : MonoBehaviour
{
    public float speed = 3f;

    public Rigidbody2D rb;
    public SpriteRenderer sr;

    public int order;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start() 
    {
        order = GetOrderPositionY();
        sr.sortingOrder = order;
    }

    public void Initialize(Vector2 startDirection)
    {
        rb.linearVelocity = startDirection.normalized * speed;
    }


    private static int nextOrderPositionY = 0;
    public int GetOrderPositionY()
    {
        int order = nextOrderPositionY;
        nextOrderPositionY = (nextOrderPositionY + 1) % 100;
        return order;
    }
}