using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P2Keyboard : MonoBehaviour
{
    public PlayerMovement target;

    // Update is called once per frame
    void Update()
    {
        int rtnValue = 0;
        if (Input.GetAxisRaw("2P-Vertical") > 0) rtnValue += 1;
        if (Input.GetAxisRaw("2P-Vertical") < 0) rtnValue += 2;
        if (Input.GetAxisRaw("2P-Horizontal") < 0) rtnValue += 4;
        if (Input.GetAxisRaw("2P-Horizontal") > 0) rtnValue += 8;
        if (Input.GetButton("2P-Attack")) rtnValue += 16;
        if (Input.GetButton("2P-Guard")) rtnValue += 32;
        if (Input.GetButton("2P-Skill1")) rtnValue += 64;
        if (Input.GetButton("2P-Skill2")) rtnValue += 128;
        if (Input.GetButton("2P-DashL")) rtnValue += 256;
        if (Input.GetButton("2P-DashR")) rtnValue += 512;

        target.inputFlags = rtnValue;
    }
}
