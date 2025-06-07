using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int p1HP = 100;
    public int p2HP = 100;
    public GameObject p1Obj;
    public GameObject p2Obj;
    public GameObject p1WinText;
    public GameObject p2WinText;
    public DelayBarControl p1DelayDisp;
    public DelayBarControl p2DelayDisp;
    private PlayerMovement p1Script;
    private PlayerMovement p2Script;
    private int winner = 0;

    // Start is called before the first frame update
    void Start()
    {
        p1HP = 100;
        p2HP = 100;
        p1Script = p1Obj.GetComponent<PlayerMovement>();
        p2Script = p2Obj.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (winner)
        {
            case 1:
                p1WinText.SetActive(true);
                break;
            case 2:
                p2WinText.SetActive(true);
                break;
        }
    }

    public void Damage(bool against2p, int amount)
    {
        if (against2p)
        {
            p2HP -= amount / (p2Script.isGuarding ? 5 : 1);
            if (p2HP <= 0)
            {
                p1Script.Die();
                p2Script.Die();
                winner = 1;
            }
        }
        else
        {
            p1HP -= amount / (p1Script.isGuarding ? 5 : 1);
            if (p1HP <= 0)
            {
                p1Script.Die();
                p2Script.Die();
                winner = 2;
            }
        }


    }

    public void FloorHandling(bool against2p, Vector3 fxPos, int damage)
    {
        Vector2 fxPos2 = new(fxPos.x, fxPos.z);
        Vector2 topdownPos;
        if (against2p)
            topdownPos = new(p2Obj.transform.position.x, p2Obj.transform.position.z);
        else
            topdownPos = new(p1Obj.transform.position.x, p1Obj.transform.position.z);
        if (Vector2.Distance(topdownPos, fxPos2) <= 3.0f) Damage(against2p, damage);
    }

    public void DelayDisplay(bool amI1p, float duration)
    {
        if (amI1p)
        {
            p1DelayDisp.startTime = Time.time;
            p1DelayDisp.endTime = Time.time + duration;
            p1DelayDisp.active = true;
        }
        else
        {
            p2DelayDisp.startTime = Time.time;
            p2DelayDisp.endTime = Time.time + duration;
            p2DelayDisp.active = true;
        }
    }

}
