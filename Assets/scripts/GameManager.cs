using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int p1HP = 100;
    public int p2HP = 100;
    public GameObject p1Obj;
    public GameObject p2Obj;
    private PlayerMovement p1Script;
    private PlayerMovement p2Script;

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

            }
        }
        else
        {
            p1HP -= amount / (p1Script.isGuarding ? 5 : 1);
            if (p1HP <= 0 )
            {
                p1Script.Die();
                p2Script.Die();
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
}
