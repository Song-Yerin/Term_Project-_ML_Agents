using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P1Keyboard : MonoBehaviour
{
    public PlayerMovement target;

    // Update is called once per frame
    void Update()
    {
        int rtnValue = 0;
        if (Input.GetAxisRaw("1P-Vertical") > 0) rtnValue += 1;
        if (Input.GetAxisRaw("1P-Vertical") < 0) rtnValue += 2;
        if (Input.GetAxisRaw("1P-Horizontal") < 0) rtnValue += 4;
        if (Input.GetAxisRaw("1P-Horizontal") > 0) rtnValue += 8;
        if (Input.GetButton("1P-Attack")) rtnValue += 16;
        if (Input.GetButton("1P-Guard")) rtnValue += 32;
        if (Input.GetButton("1P-Skill1")) rtnValue += 64;
        if (Input.GetButton("1P-Skill2")) rtnValue += 128;
        if (Input.GetButton("1P-DashL")) rtnValue += 256;
        if (Input.GetButton("1P-DashR")) rtnValue += 512;

        target.inputFlags = rtnValue;
    }
}
