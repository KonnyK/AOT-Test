using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    [SerializeField] private Transform Target;
    [SerializeField] private float MouseSensivity = 1;
    [SerializeField] private float CameraHeight = 10;
    [SerializeField] private float CameraDistance = 50;
    [SerializeField] private bool InvertXAxis = false;
    [SerializeField] private bool InvertYAxis = false;
    private float VerticalAngle = 0; //angle between original offset and current position
    private float OffsetAngle; //vertical angle of the offset vector
    void Update()
    {
        if (Input.mousePresent) 
        {
            transform.localPosition = -Vector3.forward * CameraDistance;
            transform.parent.position = Target.position + CameraHeight*Vector3.up;
            //Vector2 ScreenCenter = new Vector2(Screen.width, Screen.height)/2;
            //Vector2 Rot = MouseSensivity * new Vector2(ScreenCenter.x -Input.mousePosition.x, Input.mousePosition.y - ScreenCenter.y);
            Vector2 Rot = MouseSensivity * new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            if (InvertXAxis) Rot.x *= -1;
            if (!InvertYAxis) Rot.y *= -1;
            //Rotation von Offset um Rot.x um Y-Achse und Rot.y um X-Achse
            transform.parent.Rotate(Vector3.up, Rot.x, Space.World);
            VerticalAngle = Mathf.Clamp(VerticalAngle + Rot.y, -90-OffsetAngle, 90-OffsetAngle);
            if (Mathf.Abs(VerticalAngle) < 90) transform.parent.Rotate(transform.parent.right, Rot.y, Space.World);
            /*
            Vector3 newOffset;
            newOffset.x = Mathf.Cos(Rot.x) * Offset.x - Mathf.Sin(Rot.x)*Mathf.Cos(Rot.y) * Offset.z + Mathf.Sin(Rot.x)*Mathf.Sin(Rot.y) * Offset.y;
            newOffset.z = Mathf.Sin(Rot.x) * Offset.x + Mathf.Cos(Rot.x)*Mathf.Cos(Rot.y) * Offset.z - Mathf.Cos(Rot.x)*Mathf.Sin(Rot.y) * Offset.y;
            newOffset.y = Mathf.Sin(Rot.y) * Offset.z + Mathf.Cos(Rot.y) * Offset.y;
            Vector3 LookVector = Target.position+Vector3.up-transform.position;
            if (Mathf.Abs(Vector3.SignedAngle(Vector3.up, LookVector, transform.right)) > 0.1f) Offset = newOffset;
            else Debug.Log("Warning");
            */
            Cursor.lockState = CursorLockMode.Locked;
            //Cursor.lockState = CursorLockMode.Confined;
        }
        //transform.position = Target.position + Offset;
        //transform.rotation = Quaternion.LookRotation(Target.position+Vector3.up-transform.position, Target.up);
        transform.rotation = Quaternion.LookRotation(transform.parent.position-transform.position, Vector3.up);
    }
}

