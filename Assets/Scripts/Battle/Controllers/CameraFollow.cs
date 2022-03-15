using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float Pitch { get; private set; }
    public float Yaw { get; private set; }

    public const float MOUSE_SENSITIVITY = 5;
    public const float ROTATE_SPEED = 80;
    public const float Y_SPEED = 5;

    private Transform target; // this

    public Vector3 disVec = new Vector3(0, 15, -22); // 相机与目标的连线矢量
    // disVec 使用柱坐标系，(x, y, z) = (角度, 高度, 半径)

    public readonly Vector3 OFFSET = new Vector3(0, 8f, 0); // 让相机盯着模型中心以上 高度 + 8 的位置，更舒服
    public const float SPEED = 25f; // 相机移动速度
    public const float MIN_DISTANCE_Z = -35f; 
    public const float MAX_DISTANCE_Z = -10f;
    public const float ZOOM_SPEED = 2f;

    public Camera camera; // 不使用 MonoBehaivour 的 camera 成员

    void Start()
    {
        // 设置为玩家主相机
        camera = Camera.main;

        // 相机初始位置设置为坦克位置 偏后上方
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 initPos = pos - 30 * forward + Vector3.up * 10;

        camera.transform.position = initPos;
    }

    // 调整距离
    void Zoom()
    {
        float axis = Input.GetAxis("Mouse ScrollWheel");
        disVec.z += axis * ZOOM_SPEED;
        disVec.z = Mathf.Clamp(disVec.z, MIN_DISTANCE_Z, MAX_DISTANCE_Z);
    }

    // 调整角度
    void Rotate()
    {
        if (!Input.GetMouseButton(1)) // 锁定角度
        {
            return;
        }
        float axis = Input.GetAxis("Mouse X");
        disVec.x += 2 * axis;
        disVec.x = Mathf.Clamp(disVec.x, -20, 20);
    }

    //所有组件update之后发生
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        // 相机目标位置
        Vector3 targetPos = pos;
        targetPos = pos + forward * disVec.z + right * disVec.x;
        targetPos.y += disVec.y;
        // 相机位置
        Vector3 cameraPos = camera.transform.position; // get position
        cameraPos = Vector3.MoveTowards(cameraPos, targetPos, Time.deltaTime * SPEED); // 跟随目标
        camera.transform.position = cameraPos; // set position
        // 对准目标
        Camera.main.transform.LookAt(pos + OFFSET);

        // 接收用户鼠标输入：调整距离和角度
        Zoom();
        Rotate();
    }
}

