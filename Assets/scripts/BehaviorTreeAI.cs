using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.Mathematics;
using UnityEngine;

public class BehaviorTreeAI : MonoBehaviour
{
    public PlayerMovement target;
    private PlayerMovement targetScript;
    private bool amI1p;
    public GameManager gameManager;
    public GameObject opponent;
    private PlayerMovement opponentScript;

    public int rawSkillFactor = 0; //스킬 지르기 보정치
    private float rawSkillDeniedTime = 0.0f;
    public int closeInFactor = 0; //이동 시 접근 보정치
    private float closeInDeniedTime = 0.0f;

    private float guardTime = 0.0f;
    private int guardHP = 100;
    private float moveTime = 0.0f;
    private Vector2Int moveDir = Vector2Int.zero;
    private float evadeTime = 0.0f;
    private Vector2Int evadeDir = Vector2Int.zero;

    //접근인지 아닌지는 8방향을 각각 따짐
    //거리 변화의 XY 부분을 각각 sign만 보고 합침
    //0이면 무효, 아니라면 sign에 따라 접근/후퇴 결정

    private readonly Vector2Int[] directions = {
        new(1, 0),
        new(1, 1),
        new(0, 1),
        new(-1, 1),
        new(-1, 0),
        new(-1, -1),
        new(0, -1),
        new(1, -1)
    };

    private bool[] dirSafety = {
        true,
        true,
        true,
        true,
        true,
        true,
        true,
        true
    };


    // Start is called before the first frame update
    void Start()
    {
        opponentScript = opponent.GetComponent<PlayerMovement>();
        targetScript = target.GetComponent<PlayerMovement>();
        amI1p = target.amI1p;
    }

    // Update is called once per frame
    void Update()
    {
        rawSkillDeniedTime += Time.deltaTime;
        closeInDeniedTime += Time.deltaTime;

        target.inputFlags = 0;
        if (target.actable)
        {
            for (int i = 0; i < 8; i++) dirSafety[i] = true;
            if (guardTime > 0) //가드 플래그
            {
                guardTime -= Time.deltaTime;
                int currentHP = amI1p ? gameManager.GetComponent<GameManager>().p1HP : gameManager.GetComponent<GameManager>().p2HP;
                if (!(guardTime <= 0 || guardHP > currentHP))
                {
                    target.inputFlags = 32;
                    return;
                }
            }
            if (evadeTime > 0) // 회피 플래그
            {
                evadeTime -= Time.deltaTime;
                if (evadeDir.x == 1) target.inputFlags += 4;
                if (evadeDir.x == -1) target.inputFlags += 8;
                if (evadeDir.y == 1) target.inputFlags += 2;
                if (evadeDir.y == -1) target.inputFlags += 1;
                if (Vector3.Distance(transform.position, Vector3.zero) >= 9.99f)
                    evadeTime = 0;
                return;
            }

            GameObject floor = GameObject.FindWithTag(amI1p ? "2P-Warning" : "1P-Warning");
            if (floor != null)
                if (FloorDefenseBT(floor)) return;
            GameObject proj = GameObject.FindWithTag(amI1p ? "2P-Projectile" : "1P-Projectile");
            if (proj != null)
                if (ProjectileDefenseBT(proj)) return;
            OffenseBT();
        }
    }

    void OffenseBT()
    {
        float attackRange = 2.5f;
        Collider[] hits = Physics.OverlapSphere(transform.position + Vector3.right * (amI1p ? -1 : 1) + Vector3.up * 2.0f, attackRange);
        bool atkRange = false;
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag(amI1p ? "2P" : "1P"))
            {
                atkRange = true;
                break;
            }
        }
        bool ahead = amI1p ? opponent.transform.position.x <= target.transform.position.x : opponent.transform.position.x >= target.transform.position.x;
        bool hpAdvantage = amI1p ? gameManager.p1HP > gameManager.p2HP : gameManager.p1HP < gameManager.p2HP;
        if (atkRange)
        {
            int hitEm = UnityEngine.Random.Range(0, 100);
            int threshold = (hpAdvantage ? 50 : 10) + (!opponentScript.actable ? 20 : 0);

            if (hitEm < threshold)
            {
                target.inputFlags = 16;
            }
            else
            {
                Vector2 topdownPos = new(target.transform.position.x, target.transform.position.z);
                Vector2 BackDashCompo = targetScript.dodgeDistance * Vector2.right * (amI1p ? 1 : -1);
                topdownPos += BackDashCompo;
                float dist = Vector2.Distance(topdownPos, Vector2.zero);
                if (dist > 10f) topdownPos *= 10f / dist;
                
                bool BackDash = Math.Abs(topdownPos.x - target.transform.position.x) >= targetScript.dodgeDistance * 0.8f;

                if (BackDash)
                {
                    target.inputFlags = amI1p ? 256 : 512;
                }
                else
                {
                    target.inputFlags = target.transform.position.z > 0 ? 1 : 2;
                    moveDir.y = target.transform.position.z > 0 ? -1 : 1;
                    moveTime = 0.5f;
                }
            }
            return;
        }
        if (!opponentScript.actable && hpAdvantage) //딜레이 캐치 시도
        {
            if (Vector3.Distance(target.transform.position, opponent.transform.position) < targetScript.dodgeDistance) //걸어서 접근할까?
            {
                moveDir.x = Math.Sign(target.transform.position.x - opponent.transform.position.x);
                moveDir.y = Math.Sign(target.transform.position.z - opponent.transform.position.z);
                if (moveDir.x == 1) target.inputFlags += 4;
                if (moveDir.x == -1) target.inputFlags += 8;
                if (moveDir.y == 1) target.inputFlags += 2;
                if (moveDir.y == -1) target.inputFlags += 1;
                return;
            }
            hits = Physics.OverlapSphere(transform.position + Vector3.right * (amI1p ? -1 : 1) + Vector3.up * 2.0f + (amI1p ? Vector3.left : Vector3.right) * targetScript.dodgeDistance, attackRange);
            bool DashAtkRange = false;
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag(amI1p ? "2P" : "1P"))
                {
                    DashAtkRange = true;
                    break;
                }
            }
            if (DashAtkRange) //점멸하고 평타를 칠까?
            {
                target.inputFlags = amI1p ? 512 : 256;
                return;
            }
            Vector3 floorPos = target.transform.position + Vector3.up * 1.0f + (amI1p ? -1 : 1) * 4f * Vector3.right;
            Vector2 floorPos2 = new(floorPos.x, floorPos.z);
            Vector2 topdownPos = new(opponent.transform.position.x, opponent.transform.position.z);
            if (Vector2.Distance(topdownPos, floorPos2) <= 3.0f)
            {
                target.inputFlags = 64;
                return;
            }

            if (Math.Abs(target.transform.position.z - opponent.transform.position.z) <= 0.5f && ahead)
            {
                target.inputFlags = 128;
                return;
            }
        }

        int randy = UnityEngine.Random.Range(0, 100);

        if (randy < (int)(rawSkillDeniedTime / 2.0f) + rawSkillFactor) //지르기
        {
            Vector3 floorPos = target.transform.position + Vector3.up * 1.0f + (amI1p ? -1 : 1) * 4f * Vector3.right;
            Vector2 floorPos2 = new(floorPos.x, floorPos.z);
            Vector2 topdownPos = new(opponent.transform.position.x, opponent.transform.position.z);
            if (Math.Abs(target.transform.position.z - opponent.transform.position.z) <= 0.5f && ahead) //장풍이 맞을 위치라면 장풍
            {
                target.inputFlags = 128;
                rawSkillDeniedTime = 0;
                Debug.Log("방금 껀 지르기임");
                return;
            }
            else if (Vector2.Distance(topdownPos, floorPos2) <= 3.0f) //장판이 맞을 거리면 장판
            {
                target.inputFlags = 64;
                rawSkillDeniedTime = 0;
                Debug.Log("방금 껀 지르기임");
                return;
            }
            else
            {
                int[] possible = { 64, 128, 256, 512 };
                target.inputFlags = possible[UnityEngine.Random.Range(0,3)];
                rawSkillDeniedTime = 0;
                Debug.Log("방금 껀 지르기임");
                return;
            }
        }

        if (moveTime > 0) // 일반 이동 플래그
        {
            moveTime -= Time.deltaTime;
            if (moveDir.x == 1) target.inputFlags += 4;
            if (moveDir.x == -1) target.inputFlags += 8;
            if (moveDir.y == 1) target.inputFlags += 2;
            if (moveDir.y == -1) target.inputFlags += 1;
            if (Vector3.Distance(transform.position, Vector3.zero) >= 9.99f)
                moveTime = 0;
            return;
        }

        List<Vector2Int> approach = new();
        List<Vector2Int> backaway = new();
        int xApproachDir = Math.Sign(target.transform.position.x - opponent.transform.position.x);
        int yApproachDir = Math.Sign(target.transform.position.z - opponent.transform.position.z);
        for (int i = 0; i < 8; i++) //접근 여부는 단순히 xy축별로 접근인지 후퇴인지 따짐 (이때 x 접근 y 후퇴 같은은 경우 양쪽 다로 쳐줌)
        {
            if (!dirSafety[i]) continue;
            int a = 0;
            a += directions[i].x * xApproachDir;
            a += directions[i].y * yApproachDir;
            if (a >= 0) approach.Add(directions[i]);
            if (a <= 0) backaway.Add(directions[i]);
        }

        if (approach.Count == 0 && backaway.Count == 0) return; //모든 방향이 위험하면 가만히 있기

        randy = UnityEngine.Random.Range(0, 100);
        if (randy < (gameManager.p1HP - gameManager.p2HP) * (amI1p ? 1 : -1) + (int)(closeInDeniedTime / 1.0f) + closeInFactor * (ahead ? 1 : 3)) //무빙으로 근접하기
        {
            closeInDeniedTime = 0;
            moveDir = directions[UnityEngine.Random.Range(0, approach.Count - 1)];
            moveTime = UnityEngine.Random.Range(0.5f, 1.0f);
        }
        else //안하기로 했으면 뒤로 빠지기기
        {
            moveDir = directions[UnityEngine.Random.Range(0, backaway.Count - 1)];
            moveTime = UnityEngine.Random.Range(0.5f, 1.0f);
        }
    }
    bool FloorDefenseBT(GameObject floor)
    {
        Vector2 fxPos2 = new(floor.transform.position.x, floor.transform.position.z);
        Vector2 topdownPos = new(target.transform.position.x, target.transform.position.z);
        if (Vector2.Distance(topdownPos, fxPos2) <= 3.0f) // 범위 안
        {
            int xdir = Math.Sign(target.transform.position.x - floor.transform.position.x);
            int ydir = Math.Sign(target.transform.position.z - floor.transform.position.z);
            float remain = floor.GetComponent<FloorWarning>().GetRemainingTime() + 0.1f;

            Vector2 xOnly = new(xdir * remain * target.moveSpeed, 0f);
            Vector2 yOnly = new(0f, ydir * remain * target.moveSpeed);
            Vector2 xyBoth = new(xdir * remain * target.moveSpeed, ydir * remain * target.moveSpeed);
            if (Vector2.Distance(topdownPos + xyBoth, fxPos2) <= 3.0f && Vector2.Distance(topdownPos + xyBoth, Vector2.zero) <= 10f)
            {
                evadeDir.x = xdir;
                evadeDir.y = ydir;
                evadeTime = remain;
            }
            else if (Vector2.Distance(topdownPos + xOnly, fxPos2) <= 3.0f && Vector2.Distance(topdownPos + xOnly, Vector2.zero) <= 10f)
            {
                evadeDir.x = xdir;
                evadeTime = remain;
            }
            else if (Vector2.Distance(topdownPos + yOnly, fxPos2) <= 3.0f && Vector2.Distance(topdownPos + yOnly, Vector2.zero) <= 10f)
            {
                evadeDir.y = ydir;
                evadeTime = remain;
            }
            else
            {
                Vector2 LDashCompo = targetScript.dodgeDistance * Vector2.right;
                Vector2 RDashCompo = targetScript.dodgeDistance * Vector2.left;
                bool LDash = Vector2.Distance(topdownPos + LDashCompo, fxPos2) <= 3.0f && Vector2.Distance(topdownPos + LDashCompo, fxPos2) <= 10f;
                bool RDash = Vector2.Distance(topdownPos + RDashCompo, fxPos2) <= 3.0f && Vector2.Distance(topdownPos + RDashCompo, fxPos2) <= 10f;

                if (LDash && RDash)
                {
                    if (amI1p ? gameManager.p1HP >= gameManager.p2HP : gameManager.p1HP <= gameManager.p2HP)
                    {
                        target.inputFlags = amI1p ? 512 : 256;
                    }
                    else
                    {
                        target.inputFlags = amI1p ? 256 : 512;
                    }
                }
                else if (LDash)
                {
                    target.inputFlags = 256;
                }
                else if (RDash)
                {
                    target.inputFlags = 512;
                }
                else
                {
                    target.inputFlags = 32;
                    guardTime = 60f;
                }
            }
        }
        else
        {
            if (Vector2.Distance(topdownPos, fxPos2) >= 4f) return false;
            for (int i = 0; i < 8; i++)
            {
                if (Vector2.Angle(directions[i], fxPos2) < Math.PI / 8)
                {
                    dirSafety[i] = false;
                    dirSafety[(i + 1) % 8] = false;
                    dirSafety[(i + 7) % 8] = false;
                    break;
                }
            }
            return false;
        }
        return true;
    }
    bool ProjectileDefenseBT(GameObject proj)
    {
        bool ahead = amI1p ? proj.transform.position.x <= target.transform.position.x : proj.transform.position.x >= target.transform.position.x;
        if (Math.Abs(proj.transform.position.z - target.transform.position.z) <= 1 && ahead) // 범위 안
        {
            if (Vector3.Distance(proj.transform.position, target.transform.position) <= 1.5f)
            {
                target.inputFlags = 32;
                guardTime = 60f;
            }
            else
            {
                target.inputFlags = 128;
                return true;
                /*
                int yDir = Math.Sign(proj.transform.position.z - target.transform.position.z);
                evadeDir.y = yDir < 0 ? -1 : 1;
                evadeTime = 60f;

                if (evadeDir.y == 1) target.inputFlags += 2;
                if (evadeDir.y == -1) target.inputFlags += 1;
                if (Vector3.Distance(transform.position, Vector3.zero) >= 9.99f)
                    evadeTime = 0;
                */
            }
        }
        else
        {
            int yDir = Math.Sign(proj.transform.position.z - target.transform.position.z);
            if (ahead)
            {
                for (int i = 0; i < 8; i++)
                    if (directions[i].y == yDir) dirSafety[i] = false;
            }
            else if (Vector3.Distance(proj.transform.position, target.transform.position) <= 1.5f)
            {
                for (int i = 0; i < 8; i++)
                    if (directions[i].y == yDir && directions[i].x == (amI1p ? 1 : -1)) dirSafety[i] = false;
            }

            return false;
        }
        return true;
    }
}
