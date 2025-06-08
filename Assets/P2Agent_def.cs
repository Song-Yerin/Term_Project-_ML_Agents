using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

//수비 보상함수
public class P2Agentdef : Agent
{
    
        public GameManager gameManager;
        public PlayerMovement myScript;     // 2P
        public PlayerMovement enemyScript;  // 1P
    
        private int prevMyHP;
        private int prevEnemyHP;
    
        readonly float[] cdTime = { 0, 0, 0, 0.2f, 2f, 2f, 1f, 1f }; // index=action
        float[] cdRemain = new float[8];

        //카운터어택 변수들
        private bool enemyWasAttacking = false;
        private bool enemyWasSkilling = false;
        private float enemyActionEndTime = 0f;
        private float counterWindowTime = 0.5f;

        public override void OnEpisodeBegin()
        {
            GameManager gm = FindAnyObjectByType<GameManager>();
            gm.p1HP = 100;
            gm.p2HP = 100;
            gm.p1Obj.transform.position = new Vector3(2, 0, 0);
            gm.p2Obj.transform.position = new Vector3(-2, 0, 0);
            gm.p1WinText.SetActive(false);
            gm.p2WinText.SetActive(false);
            gm.p1Obj.GetComponent<PlayerMovement>().Revive();
            gm.p2Obj.GetComponent<PlayerMovement>().Revive();

            enemyWasAttacking = false;
            enemyWasSkilling = false;
            enemyActionEndTime = 0f;
        
            prevMyHP = prevEnemyHP = 100;
            for (int i = 0; i < 8; i++) cdRemain[i] = 0f;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            /* 1) 상대 위치 : Vector2 대신 각각의 축을 따로 넣기 */
            Vector3 rel = enemyScript.transform.position - myScript.transform.position;
            sensor.AddObservation(rel.x / 10f);   // X
            sensor.AddObservation(rel.z / 10f);   // Z
    
            /* 2) 체력 */
            sensor.AddObservation(gameManager.p2HP / 100f);  // 내 HP
            sensor.AddObservation(gameManager.p1HP / 100f);  // 적 HP
    
            /* 3) 거리 */
            float dist = Vector3.Distance(myScript.transform.position, enemyScript.transform.position) / 10f;
            sensor.AddObservation(dist);
    
            /* 4) 정면 여부 (Dot) */
            Vector3 dir = (enemyScript.transform.position - myScript.transform.position).normalized;
            sensor.AddObservation(Vector3.Dot(myScript.transform.forward, dir)); // -1 ~ 1
    
            /* 5) 스킬 쿨타임 비율 (Clamp01로 안전하게) */
            sensor.AddObservation(Mathf.Clamp01(cdRemain[4] / cdTime[4]));  // 장판
            sensor.AddObservation(Mathf.Clamp01(cdRemain[5] / cdTime[5]));  // 장풍
        }

    public override void OnActionReceived(ActionBuffers actions)
    {
        /* 쿨타임 감소 */
        for (int i = 0; i < 8; i++) if (cdRemain[i] > 0) cdRemain[i] -= Time.deltaTime;

        int a = actions.DiscreteActions[0];
        myScript.inputFlags = 0;

        bool triedSkill = (a == 4 || a == 5);
        bool skillReady = cdRemain[a] <= 0f;
        bool skillIssued = false;
        int totalGames =0;
        int p2Wins =0;
        int p1Wins =0;
        /* 행동 매핑 (쿨 중이면 무시) */
        switch (a)
        {
            case 1: myScript.inputFlags = 4; break;
            case 2: myScript.inputFlags = 8; break;
            case 3: myScript.inputFlags = 16; skillIssued = true; break;
            case 4:
            case 5:
                if (skillReady)
                {
                    myScript.inputFlags = (a == 4 ? 64 : 128);
                    cdRemain[a] = cdTime[a];
                    skillIssued = true;
                }
                break;
            case 6: myScript.inputFlags = 256; break;
            case 7: myScript.inputFlags = 512; break;
        }

        float counterReward = CalculateCounterAttackReward(a); //case에 따라 카운터 보상 계산
        CheckEnemyActionState();
        
        int myHP = gameManager.p2HP;
        int enemyHP = gameManager.p1HP;
        int myLoss = prevMyHP - myHP;
        int enemyLoss = prevEnemyHP - enemyHP;
        float r = 0f;

        r += enemyLoss * 0.4f - myLoss * 2.0f;  // 공격보다 방어 중시
        
        //거리둘때 보상
        float d = Vector3.Distance(myScript.transform.position, enemyScript.transform.position);
        r += Mathf.Clamp01((d - 1.5f) / 2f) * 0.6f;

        //가드 성공보상
        if (myScript.isGuarding && IsEnemyAttacking()) r += 0.4f;

        // 체력차이에 따른 보상
        float healthAdvantage = (myHP - enemyHP) / 100f;
        r += healthAdvantage * 0.1f;
        
        r += counterReward; //카운터 보상

        if (triedSkill && !skillReady) r -= 0.2f;
        if (triedSkill && enemyLoss == 0) r -= 0.3f;
        r -= 0.001f;

        if (enemyHP <= 0) 
        {
            totalGames++;
            p2Wins++;
            r += 10f;
        }
        if (myHP <= 0) 
        {
            totalGames++;   
            p1Wins++;
            r -= 5.0f; // 패배 패널티 추가
        }

        AddReward(r);

        prevMyHP = myHP;
        prevEnemyHP = enemyHP;

        if (myHP <= 0 || enemyHP <= 0) 
        {
            if (totalGames % 10 == 0)
            {
                float p2WinRate = (float)p2Wins / totalGames * 100f;
                float p1WinRate = (float)p1Wins / totalGames * 100f;
                Debug.Log($"P2(수비): {p2Wins}승 ({p2WinRate:F1}%)");
                Debug.Log($"P1(공격): {p1Wins}승 ({p1WinRate:F1}%)");
            }
            EndEpisode();
        }
    }

    private float CalculateCounterAttackReward(int myAction)
    {
        float reward = 0f;
        bool isMyAttack = (myAction == 3 || myAction == 4 || myAction == 5);
        
        if (!isMyAttack) return 0f;  // 공격이 아니면 카운터 불가

        // 1. 적의 공격직후 반격 (클래식 카운터)
        float timeSinceEnemyAction = Time.time - enemyActionEndTime;
        if (timeSinceEnemyAction <= counterWindowTime && 
            (enemyWasAttacking || enemyWasSkilling))
        {
            reward += 2f;  // 큰 카운터 보상
            Debug.Log("Perfect Counter Attack!");
        }
        
        // 2. 적이 현재 공격 모션 중일 때 내가 더 빠른 공격
        if (IsEnemyAttacking() && myAction == 3)  // 평타는 빠름
        {
            reward += 0.5f;
            Debug.Log("Interrupt Counter!");
        }
        
        // 3. 적이 스킬 시전 중일 때 방해
        if (IsEnemySkilling())
        {
            reward += 1.0f;
            Debug.Log("Skill Interrupt!");
        }
        
        // 4. 거리별 카운터 보너스
        float distance = Vector3.Distance(myScript.transform.position, enemyScript.transform.position);
        if (reward > 0 && distance <= 3f)  // 근거리 카운터
        {
            reward += 0.2f;
        }
        
        return reward;
    }

    private void CheckEnemyActionState()
    {
        bool currentlyAttacking = IsEnemyAttacking();
        bool currentlySkilling = IsEnemySkilling();
        
        // 적이 공격을 끝냈을 때 타이밍 기록
        if (enemyWasAttacking && !currentlyAttacking)
        {
            enemyActionEndTime = Time.time;
        }
        if (enemyWasSkilling && !currentlySkilling)
        {
            enemyActionEndTime = Time.time;
        }
        
        // 상태 업데이트
        enemyWasAttacking = currentlyAttacking;
        enemyWasSkilling = currentlySkilling;
    }

    private bool IsEnemyAttacking()
    {
        return !enemyScript.actable && 
               (enemyScript.floorActivated == 0 && enemyScript.projectileActivated == 0);
    }
    
    private bool IsEnemySkilling()
    {
        return enemyScript.floorActivated > 0 || enemyScript.projectileActivated > 0;
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;  // 기본: 아무 행동 안 함
    }
}