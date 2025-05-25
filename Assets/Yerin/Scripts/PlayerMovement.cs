using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dodgeDistance = 3f;
    public float dodgeCooldown = 1f;

    public GameObject skillEffectPrefab;
    public float effectDelay = 0.5f;
    public float effectOffsetY = 0.1f;
    public float attackDuration = 0.5f;   // ��Ÿ ���� �ð�
    public float skillDuration = 1f;      // ��ų ���� �ð�

    public GameObject projectilePrefab;  // VŰ ����ü ������
    public float projectileSpeed = 10f;  // ����ü �ӵ�
    public GameObject dashEffectPrefab;  // ��� �ܻ� ����Ʈ ������

    private Rigidbody rb;
    private Animator animator;
    private Vector3 movement;
    private bool canDodge = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Vertical"); 
        float v = Input.GetAxisRaw("Horizontal");

        movement = new Vector3(h, 0f, v).normalized;

        UpdateAnimation(h, v);
        HandleActions();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateAnimation(float h, float v)
    {
        bool up = false, down = false, left = false, right = false;

        if (h != 0 || v != 0)
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

        animator.SetBool("Move_Up", up);
        animator.SetBool("Move_Down", down);
        animator.SetBool("Move_Left", left);
        animator.SetBool("Move_Right", right);
    }

    void HandleActions()
    {
        // ��Ÿ (ZŰ)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetBool("isAttacking", true);
            Debug.Log("��Ÿ ����!");
            StartCoroutine(ResetState("isAttacking", attackDuration));
        }

        else
        {
            animator.SetBool("isAttacking", false);
        }

        // ���� (XŰ)
        if (Input.GetKey(KeyCode.X))
        {
            animator.SetBool("isGuarding", true);
            Debug.Log("���� ��!");
        }
        else
        {
            animator.SetBool("isGuarding", false);
        }

        // �¿� ȸ�� (Q/EŰ)
        if (Input.GetKeyDown(KeyCode.Q) && canDodge)
        {
            StartCoroutine(Dodge(Vector3.left));
            CreateDashEffect();  // Q ȸ�� �� �ܻ� ����
        }
        else if (Input.GetKeyDown(KeyCode.E) && canDodge)
        {
            StartCoroutine(Dodge(Vector3.right));
            CreateDashEffect();  // E ȸ�� �� �ܻ� ����
        }

        // ��ų (CŰ)
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetBool("isSkill", true);
            Debug.Log("��ų ����!");
            StartCoroutine(DelayedSkillEffect());
            StartCoroutine(ResetState("isSkill", skillDuration));
        }
        else
        {
            animator.SetBool("isSkill", false);
        }

        // ����ü ��ų (V)
        if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetBool("isSkill2", true);
            ShootProjectile();
        }
        else
        {
            animator.SetBool("isSkill2", false);
        }
    }

    System.Collections.IEnumerator Dodge(Vector3 direction)
    {
        canDodge = false;
        Vector3 dodgeTarget = rb.position + direction * dodgeDistance;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            rb.MovePosition(Vector3.Lerp(rb.position, dodgeTarget, elapsed / duration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(dodgeTarget);
        Debug.Log("ȸ�� �Ϸ�!");

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    System.Collections.IEnumerator DelayedSkillEffect()
    {
        yield return new WaitForSeconds(effectDelay);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;  //  �÷��̾��� ������ ���� (x��)

        // Y�� ���� +90�� ȸ���� ����
        Vector3 rotatedDirection = Quaternion.Euler(0, -90, 0) * forward;

        // �߰� �̵� �Ÿ�
        float forwardOffset = 1.0f;  // ������ ���� �Ÿ�
        float rightOffset = 1.0f;    // ���������� �̵� (������ ����)

        // ��ġ ���
        Vector3 effectPosition = rb.position
                                + rotatedDirection * 1.0f
                                + forward * forwardOffset
                                + right * rightOffset;

        effectPosition.y += effectOffsetY- 3.0f;

        Quaternion effectRotation = Quaternion.LookRotation(rotatedDirection, Vector3.up);

        Instantiate(skillEffectPrefab, effectPosition, effectRotation);
    }

    System.Collections.IEnumerator ResetState(string stateName, float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool(stateName, false);
        Debug.Log($"{stateName} ����");
    }

    void ShootProjectile()
    {
        // �÷��̾� ��ġ + �ణ ����(�Ǵ� ���ϴ� ����) ��ġ ���
        Vector3 spawnPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

        // ������Ʈ ����
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(transform.forward));

        // Rigidbody �̵� ó��
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;  // �ʿ��
            rb.velocity = transform.forward * projectileSpeed;  // �� �������� �̵�
        }
        else
        {
            Debug.LogWarning("Projectile�� Rigidbody�� �����ϴ�.");
        }

    } 
    void CreateDashEffect()
    {
        Vector3 effectPosition = rb.position + Vector3.up * effectOffsetY;
        Instantiate(dashEffectPrefab, effectPosition, Quaternion.identity);
        Debug.Log("��� �ܻ� ����Ʈ ����");
    }
}
