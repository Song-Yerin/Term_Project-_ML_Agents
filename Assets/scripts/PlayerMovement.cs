using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public GameManager gameManager;
    public bool amI1p = true;

    public float moveSpeed = 5f;

    public GameObject dashEffectPrefab;  // 대시 잔상 이펙트 프리팹
    public float dodgeDistance = 3f;
    public float dodgeDelay = 0.1f;

    public int attackDamage = 10; // 평타 데미지
    public float attackDelay = 0.5f; // 평타 후딜레이

    public GameObject skillEffectPrefab;
    public int skillDamage = 15; // 장판 데미지
    public float skillDelay = 1f; // 장판 후딜레이
    public float effectDelay = 0f;  // 장판 발동시간?
    public float effectOffsetY = 0.1f;

    public GameObject projectilePrefab;  // V키 투사체 프리팹
    public int projectileDamage = 15; // 장풍 데미지
    public float projectileSpeed = 10f;  // 투사체 속도
    public float projectileDelay = 1.5f;      // 장풍 후딜레이



    private Rigidbody rb;
    private Animator animator;
    private Vector3 movement;

    public AudioClip attackSoundClip;    // (Z) 사운드
    public AudioClip skillSoundClip;     // (C) 사운드
    public AudioClip projectileSoundClip; // (V) 사운드
    public AudioClip fbx; // C 발생 사운드
    public AudioClip projectile; // V 발생 사운드
    private AudioSource audioSource;

    public bool actable = true;
    public bool isGuarding = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        actable = true;
        isGuarding = false;
    }

    void Update()
    {
        float v = Input.GetAxisRaw("Vertical") * -1;
        float h = Input.GetAxisRaw("Horizontal");

        movement = actable && !isGuarding ? new Vector3(h, 0f, v).normalized : Vector3.zero;

        UpdateAnimation(h * (amI1p ? -1 : 1), v * (amI1p ? -1 : 1));
        HandleActions();
    }

    void RangeLimiting() //아레나 밖으로 나가지 않게 위치 조정 (이동할 때 마다 호출출)
    {
        Vector2 topdownPos = new(rb.position.x, rb.position.z);
        float dist = Vector2.Distance(topdownPos, Vector2.zero);

        if (dist > 10f)
        {
            topdownPos *= 10f / dist;
            rb.MovePosition(new Vector3(topdownPos.x, rb.position.y, topdownPos.y));
            //transform.position = new Vector3(topdownPos.x, transform.position.y, topdownPos.y);
        }
    }

    void FixedUpdate()
    {
        if (actable && !isGuarding) rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        RangeLimiting();
    }

    void UpdateAnimation(float h, float v)
    {
        bool up = false, down = false, left = false, right = false;

        if (actable && !isGuarding)
        {
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

        }
        animator.SetBool("Move_Left", left);
        animator.SetBool("Move_Right", right);
        animator.SetBool("Move_Up", up);
        animator.SetBool("Move_Down", down);
    }

    void HandleActions()
    {
        // 가드 (X키)
        if (Input.GetKey(KeyCode.X) && actable)
        {
            animator.SetBool("isGuarding", true);
            Debug.Log("가드 중!");
            isGuarding = true;
            return;
        }
        else
        {
            animator.SetBool("isGuarding", false);
            isGuarding = false;
        }

        if (!actable || isGuarding)
        {
            animator.SetBool("isGuarding", false);
            animator.SetBool("isAttacking", false);
            animator.SetBool("isSkill", false);
            animator.SetBool("isSkill2", false);
            return;
        }

        // 평타 (Z키)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetBool("isAttacking", true);
            TryDamageEnemy();
            Debug.Log("평타 시작!");
            audioSource.PlayOneShot(attackSoundClip);
            StartCoroutine(SetPostDelay(attackDelay));
        }
        else
        {
            animator.SetBool("isAttacking", false);
        }

        // 좌우 회피 (Q/E키)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(Dodge(Vector3.right));
            CreateDashEffect();  // Q 회피 시 잔상 생성
            StartCoroutine(SetPostDelay(dodgeDelay));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(Dodge(Vector3.left));
            CreateDashEffect();  // E 회피 시 잔상 생성
            StartCoroutine(SetPostDelay(dodgeDelay));
        }

        // 스킬 (C키)
        if (Input.GetKeyDown(KeyCode.C))
        {
            animator.SetBool("isSkill", true);
            Debug.Log("스킬 시작!");
            audioSource.PlayOneShot(skillSoundClip);
            StartCoroutine(SetPostDelay(skillDelay));
            StartCoroutine(DelayedSkillEffect(transform.position));
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
            StartCoroutine(SetPostDelay(projectileDelay));
        }
        else
        {
            animator.SetBool("isSkill2", false);
        }
    }

    System.Collections.IEnumerator Dodge(Vector3 direction)
    {
        Vector3 dodgeTarget = rb.position + direction * dodgeDistance;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            rb.MovePosition(Vector3.Lerp(rb.position, dodgeTarget, elapsed / duration));
            RangeLimiting();
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(dodgeTarget);
        Debug.Log("회피 완료!");
    }

    System.Collections.IEnumerator DelayedSkillEffect(Vector3 origin)
    {
        yield return new WaitForSeconds(effectDelay);

        Debug.Log("장판 이펙트 생성");

        Vector3 forward = Vector3.right * (amI1p ? -1 : 1);

        // Y축 기준 +90도 회전된 방향
        Vector3 rotatedDirection = Vector3.up;

        // 추가 이동 거리
        float forwardOffset = 4f;  // 앞으로 나갈 거리

        // 위치 계산
        Vector3 effectPosition = origin
                                + rotatedDirection * 1.0f
                                + forward * forwardOffset;

        effectPosition.y = 0;

        Quaternion effectRotation = Quaternion.LookRotation(Vector3.right * (amI1p ? -1 : 1), Vector3.up);

        audioSource.PlayOneShot(fbx);

        Instantiate(skillEffectPrefab, effectPosition, effectRotation);

        yield return new WaitForSeconds(0.2f);
        gameManager.GetComponent<GameManager>().FloorHandling(amI1p, effectPosition, skillDamage);
    }

    System.Collections.IEnumerator SetPostDelay(float duration)
    {
        actable = false;
        yield return new WaitForSeconds(duration);
        actable = true;
    }

    void ShootProjectile()
    {
        // 플레이어 위치 + 약간 앞쪽(또는 원하는 방향) 위치 계산
        Vector3 spawnPos = transform.position + Vector3.right * (amI1p ? -1 : 1) + Vector3.up * 0.5f;

        // 오브젝트 생성
        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(Vector3.right * (amI1p ? -1 : 1)));


        // Rigidbody 이동 처리
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        Projectile script = proj.GetComponent<Projectile>();
        if (rb != null)
        {
            rb.useGravity = false;  // 필요시
            rb.velocity = (amI1p ? -1 : 1) * projectileSpeed * Vector3.right;  // 앞 방향으로 이동

            script.ownerIs1p = amI1p;
            script.damage = projectileDamage;
            proj.tag = amI1p ? "1P-Projectile" : "2P-Projectile";
            script.gameManager = gameManager;
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
    // 공격 범위 안의 적 찾기 (예시: 평타용)
    void TryDamageEnemy()
    {
        float attackRange = 2.5f;
        Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.right * (amI1p ? -1 : 1) + Vector3.up * 2.0f, attackRange);

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(amI1p ? "2P" : "1P"))
            {
                gameManager.GetComponent<GameManager>().Damage(amI1p, attackDamage);
                break;
            }  
        }
    }
}

