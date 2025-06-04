using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorWarning : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float radius;
    public int subdivisions;
    public float duration;
    private float progress;

    void DrawCircle()
    {
        float angleStep = 2f * Mathf.PI * progress / duration / subdivisions;
        lineRenderer.positionCount = subdivisions + 1;

        lineRenderer.SetPosition(0, Vector3.zero);

        for (int i = 0; i < subdivisions; i++)
        {
            float xPosition = radius * Mathf.Cos(angleStep * i);
            float yPosition = radius * Mathf.Sin(angleStep * i);

            Vector3 pointInCircle = new Vector3(xPosition, yPosition, 0f);
            lineRenderer.SetPosition(i + 1, pointInCircle);
        }
    }

    private void Start()
    {
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        progress += Time.deltaTime;
        DrawCircle();
    }
}
