using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using UnityEngine.UI;

public class DelayBarControl : MonoBehaviour
{
    public float startTime = 0f;
    public float endTime = 0f;
    public bool active = false;
    public Image fillImage;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!active) fillImage.fillAmount = 1;
        else
        {
            float duration = endTime - startTime;
            if (Time.time > endTime)
            {
                fillImage.fillAmount = 1;
                active = false;
            }
            else
            {
                fillImage.fillAmount = (Time.time - startTime) / duration;
            }
        }
    }
}
