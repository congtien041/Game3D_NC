using System;
using System.Collections;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public Rigidbody rb;
    [System.Serializable]
    public class CamPosition
    {
        public GameObject PositionTarget;
        public GameObject LookAtTarget;
        [Range(0, 1)]
        public float Lerp;
        public bool IsFixed; //Fix for hood and inside like positions.
    }

    [System.Serializable]
    public class CameraProperties
    {
        public CamPosition[] CamPositions;
    }

    public CameraProperties CamSetup = new CameraProperties();
    private Vector3 localVelocity = Vector3.zero;

    private HDCarController rootScript;

    private GameObject DP, LAT; //DP: Desired Position, LAT:Look at target.

    private int CamPosIndex;

    void Start()
    {
        rootScript = rb.transform.root.GetComponent<HDCarController>();
        CamPosIndex = rootScript.SelectedCamPositionIndex;

        DP = CamSetup.CamPositions[CamPosIndex].PositionTarget;
        LAT = CamSetup.CamPositions[CamPosIndex].LookAtTarget;
    }

    void Update()
    {
        if (CamPosIndex != rootScript.SelectedCamPositionIndex)
        {
            CamPosIndex = rootScript.SelectedCamPositionIndex;
        }
        
        DP = CamSetup.CamPositions[CamPosIndex].PositionTarget;
        LAT = CamSetup.CamPositions[CamPosIndex].LookAtTarget;
    }

    Vector3 lastVelocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;
    private void FixedUpdate()
    {
        float lerp = CamSetup.CamPositions[CamPosIndex].Lerp;
        acceleration = (rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = rb.linearVelocity;
        if (CamSetup.CamPositions[CamPosIndex].IsFixed)
        {
            Vector3 desiredPos = CamSetup.CamPositions[CamPosIndex].PositionTarget.transform.position;
            Vector3 campos = transform.position;

            transform.position = Vector3.Lerp(transform.position, desiredPos, lerp);
            transform.rotation = rb.rotation;
            transform.LookAt(LAT.transform.position);
        }
        else
        {
            float MV = 6 + Mathf.Clamp(Mathf.Log(rb.linearVelocity.magnitude / 10), 0, 4);

            Vector3 tp = rb.transform.position + new Vector3(0, 2, 0);

            Vector3 avrgVector = rb.linearVelocity + rb.transform.forward * rb.linearVelocity.magnitude / 2;

            if (avrgVector.magnitude > rb.transform.forward.magnitude)
            {
                DP.transform.position = tp - avrgVector.normalized * MV;
            }
            else
            {
                DP.transform.position = tp - rb.transform.forward * MV;
            }

            transform.position = Vector3.Lerp(transform.position, DP.transform.position, lerp);
            transform.LookAt(LAT.transform.position);
        }
    }
}
