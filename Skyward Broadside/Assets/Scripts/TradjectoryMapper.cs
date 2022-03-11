using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TradjectoryMapper : MonoBehaviourPunCallbacks
{
    BasicCannonController basicCannonController;
    LineRenderer lineRenderer;

    public int noOfPoints = 50;
    public float pointInterval = 0.1f;

    public LayerMask obstructions;

    // Start is called before the first frame update
    void Start()
    {
        basicCannonController = GetComponent<BasicCannonController>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine)
        {
            drawLine();
        }
    }

    //inspired by https://www.youtube.com/watch?v=RnEO3MRPr5Y&ab_channel=AdamKonig
    void drawLine()
    {
        lineRenderer.positionCount = noOfPoints;
        List<Vector3> points = new List<Vector3>();
        Vector3 startPos = basicCannonController.shotOrigin.position;
        Vector3 velocity = basicCannonController.shotOrigin.forward * basicCannonController.power;
        for (float t = 0; t < noOfPoints; t += pointInterval)
        {
            Vector3 newPoint = startPos + (t * velocity);
            newPoint.y = startPos.y + ((velocity.y * t) + (Physics.gravity.y / 2f * t * t));
            points.Add(newPoint);

            if (Physics.OverlapSphere(newPoint, 2, obstructions).Length > 0)
            {
                lineRenderer.positionCount = points.Count;
                break;
            }
        }
        lineRenderer.SetPositions(points.ToArray());
    }
}
