using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 30f;

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }
}
