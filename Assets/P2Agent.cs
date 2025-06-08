using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class P2Agent : Agent
{
    public GameManager gameManager;
    public PlayerMovement myScript;
    public PlayerMovement enemyScript;

    private int prevMyHP;
    private int prevEnemyHP;

    readonly float[] cdTime = { 0, 0, 0, 0.2f, 2f, 2f, 1f, 1f };
    float[] cdRemain = new float[8];

    public bool isP2 = false;

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

        // prevHP 초기화 (분기 적용)
        prevMyHP = isP2 ? gm.p2HP : gm.p1HP;
        prevEnemyHP = isP2 ? gm.p1HP : gm.p2HP;

        for (int i = 0; i < 8; i++) cdRemain[i] = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 rel = enemyScript.transform.position - myScript.transform.position;
        sensor.AddObservation(rel.x / 10f);
        sensor.AddObservation(rel.z / 10f);

        // isP2에 따른 체력 관측
        float myHP = isP2 ? gameManager.p2HP : gameManager.p1HP;
        float enemyHP = isP2 ? gameManager.p1HP : gameManager.p2HP;

        sensor.AddObservation(myHP / 100f);
        sensor.AddObservation(enemyHP / 100f);

        float dist = Vector3.Distance(myScript.transform.position, enemyScript.transform.position) / 10f;
        sensor.AddObservation(dist);

        Vector3 dir = (enemyScript.transform.position - myScript.transform.position).normalized;
        sensor.AddObservation(Vector3.Dot(myScript.transform.forward, dir));

        sensor.AddObservation(Mathf.Clamp01(cdRemain[4] / cdTime[4]));
        sensor.AddObservation(Mathf.Clamp01(cdRemain[5] / cdTime[5]));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        for (int i = 0; i < 8; i++) if (cdRemain[i] > 0) cdRemain[i] -= Time.deltaTime;

        int a = actions.DiscreteActions[0];
        myScript.inputFlags = 0;

        bool triedSkill = (a == 4 || a == 5);
        bool skillReady = cdRemain[a] <= 0f;
        bool skillIssued = false;

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

        int myHP = isP2 ? gameManager.p2HP : gameManager.p1HP;
        int enemyHP = isP2 ? gameManager.p1HP : gameManager.p2HP;
        int myLoss = prevMyHP - myHP;
        int enemyLoss = prevEnemyHP - enemyHP;

        float r = 0f;
        r += enemyLoss * 0.3f - myLoss * 0.2f;

        float d = Vector3.Distance(myScript.transform.position, enemyScript.transform.position);
        r += Mathf.Clamp01(2f - d) * 0.7f;

        Vector3 dir = (enemyScript.transform.position - myScript.transform.position).normalized;
        r += Mathf.Max(0, Vector3.Dot(myScript.transform.forward, dir)) * 0.3f;

        if (triedSkill && !skillReady) r -= 0.3f;
        if (triedSkill && enemyLoss == 0) r -= 0.4f;
        r -= 0.001f;

        if (enemyHP <= 0) r += 5f;

        AddReward(r);

        prevMyHP = myHP;
        prevEnemyHP = enemyHP;

        if (myHP <= 0 || enemyHP <= 0) EndEpisode();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
    }
}
