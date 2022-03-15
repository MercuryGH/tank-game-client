using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncTank : BaseTank
{
    // 上次收到的位置、旋转、炮塔、炮管信息
    private Vector3 lastPos;
    private Vector3 lastRot;
    private float lastTurretY;
    private float lastGunX;
    // 上次收到同步数据的时刻
    private float lastSyncTime;

    // 由上次同步数据，预测的位置、旋转、炮塔、炮管信息
    private Vector3 forecastPos;
    private Vector3 forecastRot;
    private float forecastTurretY;
    private float forecastGunX;

    public override void Init(string skinPath)
    {
        base.Init(skinPath);

        // SyncTank 不受物理运动影响，节省算力也避免bug
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;
        rigidBody.useGravity = false;

        // 初始化预测信息
        lastPos = transform.position;
        lastRot = transform.eulerAngles;
        lastTurretY = turret.localEulerAngles.y;
        lastSyncTime = Time.time;
        lastGunX = gun.localEulerAngles.x;

        forecastPos = transform.position;
        forecastRot = transform.eulerAngles;
        forecastTurretY = turret.localEulerAngles.y;
        forecastGunX = gun.localEulerAngles.x;
    }

    new void Update()
    {
        base.Update();
        ForecastUpdate();
    }

    // 位置同步，使用预测算法
    public void SyncPos(MsgSyncTank msg)
    {
        // 位置
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);

        // 线性插值法，这里认为加速度恒定
        forecastPos = pos + (pos - lastPos);
        forecastRot = rot + (rot - lastRot);
        forecastTurretY = msg.turretY + (msg.turretY - lastTurretY);
        forecastGunX = msg.gunX + (msg.gunX - lastGunX);

        // 更新
        lastPos = pos;
        lastRot = rot;
        lastTurretY = turret.localEulerAngles.y;
        lastGunX = turret.localEulerAngles.x;
        lastSyncTime = Time.time;
    }

    /**
     * 在帧率 > 同步帧率的情况下，使用ForecastUpdate让SyncTank移动到预测的位置。
     *
     * 算法为简单线性插值，即假设匀速直线运动
     * forecast 字段直到下次位置同步才会更新
     */
    public void ForecastUpdate()
    {
        // 经过时间（归一化）
        float t = (Time.time - lastSyncTime) / CtrlTank.SYNC_INTERVAL;
        t = Mathf.Clamp(t, 0f, 1f);

        // 位置
        Vector3 pos = transform.position;
        pos = Vector3.Lerp(pos, forecastPos, t);
        transform.position = pos;

        // 旋转
        Quaternion quat = transform.rotation;
        Quaternion forcastQuat = Quaternion.Euler(forecastRot);
        quat = Quaternion.Lerp(quat, forcastQuat, t);
        transform.rotation = quat;

        // 轮子旋转，履带滚动
        float axis = transform.InverseTransformPoint(forecastPos).z;
        axis = Mathf.Clamp(axis * 1024, -1f, 1f);
        WheelUpdate(axis);

        // 炮管
        Vector3 lea = turret.localEulerAngles;
        lea.y = Mathf.LerpAngle(lea.y, forecastTurretY, t);
        turret.localEulerAngles = lea;

        // 炮塔
        lea = gun.localEulerAngles;
        lea.x = Mathf.LerpAngle(lea.x, forecastGunX, t);
        gun.localEulerAngles = lea;
    }

    // 开火
    public void SyncFire(MsgFire msg)
    {
        Bullet bullet = Fire();

        // 更新坐标
        Vector3 pos = new Vector3(msg.x, msg.y, msg.z);
        Vector3 rot = new Vector3(msg.ex, msg.ey, msg.ez);
        bullet.transform.position = pos;
        bullet.transform.eulerAngles = rot;
    }
}
