using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Photographer : MonoBehaviour
{
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }

    public const float MOUSE_SENSITIVITY = 5;
    public const float ROTATE_SPEED = 80;
    public const float Y_SPEED = 5;

    private Transform _target;
    private Transform _camera;
    [SerializeField] private AnimationCurve _armLengthCurve;

    private void Awake()
    {
        _camera = transform.GetChild(0);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void InitCamera(Transform target)
    {
        _target = target;
        transform.position = target.position;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateRotation();
        UpdatePosition();
        UpdateArmLength();
    }


    private void UpdateRotation()
    {
        Yaw += Input.GetAxis("Mouse X") * MOUSE_SENSITIVITY;
        Yaw += Input.GetAxis("Camera Rate X") * ROTATE_SPEED * Time.deltaTime;
        Pitch += Input.GetAxis("Mouse Y") * MOUSE_SENSITIVITY;
        Pitch += Input.GetAxis("Camera Rate Y") * ROTATE_SPEED * Time.deltaTime;
        Pitch = Mathf.Clamp(Pitch, -90, 90);

        transform.rotation = Quaternion.Euler(Pitch, Yaw, 0);
    }

    private void UpdatePosition()
    {
        Vector3 position = _target.position;
        float newY = Mathf.Lerp(transform.position.y, position.y, Time.deltaTime * Y_SPEED);
        transform.position = new Vector3(position.x, newY, position.z);
    }

    private void UpdateArmLength()
    {
        _camera.localPosition = new Vector3(0, 0, _armLengthCurve.Evaluate(Pitch) * -1);
    }
}
