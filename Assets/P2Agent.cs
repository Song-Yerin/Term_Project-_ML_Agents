using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class P2Agent : Agent
{
    public GameManager gameManager;
    public PlayerMovement myScript;     // 2P
    public PlayerMovement enemyScript;  // 1P

    private int prevMyHP;
    private int prevEnemyHP;

    public override void OnEpisodeBegin()
    {
        // 초기화
        gameManager.p1HP = 100;
        gameManager.p2HP = 100;

        prevMyHP = 100;
        prevEnemyHP = 100;

        // 위치 초기화
        myScript.transform.position = new Vector3(5f, 0f, 0f);
        enemyScript.transform.position = new Vector3(-5f, 0f, 0f);

        myScript.actable = true;
        enemyScript.actable = true;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 relPos = enemyScript.transform.position - myScript.transform.position;
        sensor.AddObservation(relPos / 10f);  // 상대 상대적 위치 정규화

        sensor.AddObservation(gameManager.p1HP / 100f);
        sensor.AddObservation(gameManager.p2HP / 100f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        myScript.inputFlags = 0;

        // 액션 -> inputFlags로 처리
        switch (action)
        {
            case 0: break; // 가만히
            case 1: myScript.inputFlags = 4; break; // 왼쪽
            case 2: myScript.inputFlags = 8; break; // 오른쪽
            case 3: myScript.inputFlags = 16; break; // 평타
            case 4: myScript.inputFlags = 64; break; // 장판
            case 5: myScript.inputFlags = 128; break; // 장풍
        }

        // 보상 계산
        int nowMyHP = gameManager.p2HP;
        int nowEnemyHP = gameManager.p1HP;

        int myLoss = prevMyHP - nowMyHP;
        int enemyLoss = prevEnemyHP - nowEnemyHP;

        float reward = (enemyLoss - myLoss) / 10f;  // HP 10씩 줄 때마다 보상
        AddReward(reward);

        prevMyHP = nowMyHP;
        prevEnemyHP = nowEnemyHP;

        if (nowMyHP <= 0 || nowEnemyHP <= 0)
        {
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;  // 무조건 가만히 있음 (나중에 조정 가능)
    }
}
