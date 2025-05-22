using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Animator animator;

    private Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        movement = new Vector3(h, 0f, v).normalized;

        UpdateAnimation(h, v);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateAnimation(float h, float v)
    {
        bool up = false, down = false, left = false, right = false;

        if (h == 0 && v == 0)
        {
            // 아무 것도 누르지 않았을 때 → 모든 방향 false
        }
        else
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
            {
                if (h > 0) right = true;
                else left = true;
            }
            else
            {
                if (v > 0) up = true;
                else down = true;
            }
        }

        // Animator Bool 파라미터 설정
        animator.SetBool("Move_Up", up);
        animator.SetBool("Move_Down", down);
        animator.SetBool("Move_Left", left);
        animator.SetBool("Move_Right", right);
    }
}
