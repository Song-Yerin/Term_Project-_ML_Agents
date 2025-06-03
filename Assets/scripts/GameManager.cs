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
        if (against2p) p2HP -= amount / (p2Script.isGuarding ? 5 : 1);
        else p1HP -= amount / (p1Script.isGuarding ? 5 : 1);
    }
}
