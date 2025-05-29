using UnityEngine;
using System.Collections;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Animator animator;

    private Vector3 movement;
    private bool canDodge = true;

    public AudioClip attackSoundClip;    // (Z) 사운드
    public AudioClip skillSoundClip;     // (C) 사운드
    public AudioClip projectileSoundClip; // (V) 사운드
    public AudioClip fbx; // C 발생 사운드
    public AudioClip projectile; // V 발생 사운드
    private AudioSource audioSource;

    public GameObject attackHitbox; // RightHand 등


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
            // �ƹ� �͵� ������ �ʾ��� �� �� ��� ���� false
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

        // Animator Bool �Ķ���� ����
        animator.SetBool("Move_Up", up);
        animator.SetBool("Move_Down", down);
        animator.SetBool("Move_Left", left);
        animator.SetBool("Move_Right", right);
    }

    void HandleActions()
    {
        // 평타 (Z키)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetBool("isAttacking", true);
            Debug.Log("평타 시작!");
            audioSource.PlayOneShot(attackSoundClip);
            StartCoroutine(ActivateHitboxTemporarily()); // 히트박스 켜기
            StartCoroutine(ResetState("isAttacking", attackDuration));
        }

        else
        {
            animator.SetBool("isAttacking", false);
        }

        // 가드 (X키)
        if (Input.GetKey(KeyCode.X))
        {
            animator.SetBool("isGuarding", true);
            Debug.Log("가드 중!");
        }
        else
        {
            animator.SetBool("isGuarding", false);
        }

        // 좌우 회피 (Q/E키)
        if (Input.GetKeyDown(KeyCode.Q) && canDodge)
        {
            StartCoroutine(Dodge(Vector3.left));
            CreateDashEffect();  // Q 회피 시 잔상 생성
        }
        else if (Input.GetKeyDown(KeyCode.E) && canDodge)
        {
            StartCoroutine(Dodge(Vector3.right));
            CreateDashEffect();  // E 회피 시 잔상 생성
        }

        // 스킬 (C키)
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetBool("isSkill", true);
            Debug.Log("스킬 시작!");
            audioSource.PlayOneShot(skillSoundClip);
            StartCoroutine(DelayedSkillEffect());
            StartCoroutine(ResetState("isSkill", skillDuration));
        }
        else
        {
            animator.SetBool("isSkill", false);
        }

        // 투사체 스킬 (V)
        if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetBool("isSkill2", true);
            audioSource.PlayOneShot(projectileSoundClip);
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
        Debug.Log("회피 완료!");

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    System.Collections.IEnumerator DelayedSkillEffect()
    {
        yield return new WaitForSeconds(effectDelay);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;  //  플레이어의 오른쪽 방향 (x축)

        // Y축 기준 +90도 회전된 방향
        Vector3 rotatedDirection = Quaternion.Euler(0, -90, 0) * forward;

        // 추가 이동 거리
        float forwardOffset = 1.0f;  // 앞으로 나갈 거리
        float rightOffset = 1.0f;    // 오른쪽으로 이동 (음수면 왼쪽)

        // 위치 계산
        Vector3 effectPosition = rb.position
                                + rotatedDirection * 1.0f
                                + forward * forwardOffset
                                + right * rightOffset;

        effectPosition.y += effectOffsetY- 3.0f;

        Quaternion effectRotation = Quaternion.LookRotation(rotatedDirection, Vector3.up);

        audioSource.PlayOneShot(fbx);

        Instantiate(skillEffectPrefab, effectPosition, effectRotation);
    }

    System.Collections.IEnumerator ResetState(string stateName, float duration)
    {
        yield return new WaitForSeconds(duration);
        animator.SetBool(stateName, false);
        Debug.Log($"{stateName} 종료");
    }

    void ShootProjectile()
    {
        // 플레이어 위치 + 약간 앞쪽(또는 원하는 방향) 위치 계산
        Vector3 spawnPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;

        // 오브젝트 생성
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(transform.forward));


        // Rigidbody 이동 처리
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;  // 필요시
            rb.velocity = transform.forward * projectileSpeed;  // 앞 방향으로 이동
        }
        else
        {
            Debug.LogWarning("Projectile에 Rigidbody가 없습니다.");
        }

        audioSource.PlayOneShot(projectile);

    } 
    void CreateDashEffect()
    {
        Vector3 effectPosition = rb.position + Vector3.up * effectOffsetY;
        Instantiate(dashEffectPrefab, effectPosition, Quaternion.identity);
        Debug.Log("대시 잔상 이펙트 생성");
    }

    IEnumerator ActivateHitboxTemporarily()
    {
        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(attackDuration); // 예: 0.5초
        attackHitbox.SetActive(false);
    }
}
