using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{
    // TODO: modify this interval
    public const float SYNC_INTERVAL = 0.05f; // 两次进行位置同步最短的时间间隔

    public Photographer photographer;

    public BaseTank aimTank; // 自动瞄准的坦克

    private const float SEARCH_INTERVAL = 1f;       // 搜索时间间隔
    private const float MAX_SEARCH_DISTANCE = 150f; // 最大搜索距离
    private const float PREVENT_INTERVAL = 2f;      // 手动操作炮塔后的保护时间
    private const float MAX_AIM_DISTANCE = 200f;

    private float lastSendSyncTime = 0;  // 上一次发送同步信息的时间
    private float lastSearchTime = 0;    // 上一次搜索时间
    private float freezeAutoAimTime = 0; // 禁止自动瞄准到某个时间

    public override void Init(string skinPath)
    {
        base.Init(skinPath);
        photographer = GameObject.Find("Photographer").GetComponent<Photographer>(); // 如此暴力的反射找全局GameObject下的Component
        photographer.InitCamera(cameraFocus);
    }

    new void Update()
    {
        base.Update();
        MoveUpdate();
        TurretUpdate();
        FireUpdate();
        SyncUpdate();
        SearchAutoAimedTargetUpdate();
        AutoAimUpdate();
    }

    // 移动控制
    public void MoveUpdate()
    {
        if (IsDie())
        {
            return;
        }

        // 旋转
        float x = Input.GetAxis("Horizontal");
        transform.Rotate(0, x * steer * Time.deltaTime, 0);

        // 前进后退
        float y = Input.GetAxis("Vertical");
        Vector3 s = y * transform.forward * speed * Time.deltaTime;
        transform.transform.position += s;

        // 轮子旋转，履带滚动
        WheelUpdate(y);
    }

    // 炮塔控制
    public void TurretUpdate()
    {
        if (IsDie())
        {
            return;
        }

        int axis = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            axis = -1;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            axis = 1;
        }
        if (axis == 0)
        {
            return;
        }

        // 旋转
        Vector3 lea = turret.localEulerAngles;
        lea.y += axis * Time.deltaTime * turretSpeed;
        turret.localEulerAngles = lea;

        // 玩家手动旋转了炮塔，短时间内进入禁止自动瞄准的状态
        freezeAutoAimTime = Time.time + PREVENT_INTERVAL;
        aimTank = null;
    }

    // 开炮
    public void FireUpdate()
    {
        if (IsDie())
        {
            return;
        }
        // 按下空格或鼠标左键，才开炮
        if (!Input.GetKey(KeyCode.Space) && !Input.GetMouseButton(0))
        {
            return;
        }
        // cd
        if (Time.time - lastFireTime < fireCd)
        {
            return;
        }

        Bullet bullet = Fire();

        // 发送同步协议
        MsgFire msg = new MsgFire
        {
            x = bullet.transform.position.x,
            y = bullet.transform.position.y,
            z = bullet.transform.position.z,
            ex = bullet.transform.eulerAngles.x,
            ey = bullet.transform.eulerAngles.y,
            ez = bullet.transform.eulerAngles.z
        };
        NetManager.Send(msg);
    }

    // 发送同步信息
    public void SyncUpdate()
    {
        // 时间间隔判断
        if (Time.time - lastSendSyncTime < SYNC_INTERVAL)
        {
            return;
        }
        lastSendSyncTime = Time.time;
        // 发送同步协议
        MsgSyncTank msg = new MsgSyncTank
        {
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            ex = transform.eulerAngles.x,
            ey = transform.eulerAngles.y,
            ez = transform.eulerAngles.z,
            turretY = turret.localEulerAngles.y,
            gunX = gun.localEulerAngles.x
        };
        NetManager.Send(msg);
    }

    // 从炮管发射射线，获取第一个相交点
    // 若相交于无穷远点，则取长度为MAX_AIM_DISTANCE的射线上的点
    public Vector3 getAimedDirection()
    {
        // 碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        // 沿着炮管方向的射线
        Vector3 pos = firePoint.position;
        Ray ray = new Ray(pos, firePoint.forward);

        // 射线检测
        int layerMask = ~(1 << LayerMask.NameToLayer("Bullet")); // 小于 Bullet 层的所有层。Layer可以在Project Settings中自行设置
        if (Physics.Raycast(ray, out RaycastHit hit, MAX_AIM_DISTANCE, layerMask))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(MAX_AIM_DISTANCE);
        }
        return hitPoint;
    }

    // 搜寻自动瞄准目标
    public void SearchAutoAimedTargetUpdate()
    {
        // 时间间隔判断
        if (Time.time - lastSearchTime < SEARCH_INTERVAL)
        {
            return;
        }
        lastSearchTime = Time.time;

        // 搜索
        aimTank = null;
        foreach (BaseTank tank in BattleManager.tanks.Values)
        {
            // 同个阵营，或者是尸体
            if (tank.team == team || tank == this || tank.IsDie())
            {
                continue;
            }

            // 相对位置（z）
            Vector3 p = firePoint.InverseTransformPoint(tank.transform.position);
            if (p.z <= 0 || p.z > MAX_SEARCH_DISTANCE)
            {
                continue;
            }
            // 相对位置，45°俯仰角限制
            if (Mathf.Abs(p.x) > p.z)
            {
                continue;
            }

            // 是否切换目标
            if (aimTank != null)
            {
                float d1 = Vector3.Distance(tank.transform.position, transform.position);
                float d2 = Vector3.Distance(aimTank.transform.position, transform.position);
                if (d1 > d2)
                {
                    continue;
                }
            }
            aimTank = tank;
        }
    }

    // 自动旋转炮管炮塔
    public void AutoAimUpdate()
    {
        // 保护时间
        if (Time.time < freezeAutoAimTime)
        {
            return;
        }

        Vector3 p;
        if (aimTank == null)
        {
            // 回正
            p = firePoint.InverseTransformPoint(transform.position + transform.forward * 100 + transform.up * 5);
        }
        else
        {
            // 相对位置。加上 (0,5,0) 是高度修正
            p = firePoint.InverseTransformPoint(aimTank.transform.position + new Vector3(0, 5f, 0));
        }
        // 旋转炮塔
        float axis = Mathf.Clamp(p.x, -1, 1);
        Vector3 le = turret.localEulerAngles;
        le.y += axis * Time.deltaTime * turretSpeed;
        turret.localEulerAngles = le;

        // 旋转炮管
        axis = Mathf.Clamp(p.y, -1, 1);
        le = gun.localEulerAngles;
        le.x -= axis * Time.deltaTime * gunSpeed;
        if (le.x > 180) { le.x = 360 - le.x; }
        le.x = Mathf.Clamp(le.x, minGunAngle, maxGunAngle);
        gun.localEulerAngles = le;
    }

}
