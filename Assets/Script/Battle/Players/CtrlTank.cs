using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlTank : BaseTank
{
    public const float SYNC_INTERVAL = 0.05f; // 同步帧率

    public BaseTank aimTank; // 自动瞄准的坦克

    private const float SEARCH_INTERVAL = 1f;       // 搜索时间间隔
    private const float MAX_SEARCH_DISTANCE = 150f; // 最大搜索距离
    private const float PREVENT_INTERVAL = 2f;      // 手动操作炮塔后的保护时间

    private float lastSendSyncTime = 0;  // 上一次发送同步信息的时间
    private float lastSearchTime = 0;    // 上一次搜索时间
    private float preventSearchTime = 0; // 禁止自动瞄准到某个时间

    new void Update()
    {
        base.Update();
        //移动控制
        MoveUpdate();
        //炮塔控制
        TurretUpdate();
        //开炮
        FireUpdate();
        //发送同步信息
        SyncUpdate();
        //自动搜寻目标
        SearchUpdate();
        //自动瞄准
        AutoAimUpdate();
    }

    //移动控制
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

    //炮塔控制
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
        Vector3 le = turret.localEulerAngles;
        le.y += axis * Time.deltaTime * turretSpeed;
        turret.localEulerAngles = le;

        // 保护时间
        preventSearchTime = Time.time + PREVENT_INTERVAL;
        aimTank = null;
    }

    //开炮
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
        MsgFire msg = new MsgFire();
        msg.x = bullet.transform.position.x;
        msg.y = bullet.transform.position.y;
        msg.z = bullet.transform.position.z;
        msg.ex = bullet.transform.eulerAngles.x;
        msg.ey = bullet.transform.eulerAngles.y;
        msg.ez = bullet.transform.eulerAngles.z;
        NetManager.Send(msg);
    }

    //发送同步信息
    public void SyncUpdate()
    {
        //时间间隔判断
        if (Time.time - lastSendSyncTime < SYNC_INTERVAL)
        {
            return;
        }
        lastSendSyncTime = Time.time;
        //发送同步协议
        MsgSyncTank msg = new MsgSyncTank();
        msg.x = transform.position.x;
        msg.y = transform.position.y;
        msg.z = transform.position.z;
        msg.ex = transform.eulerAngles.x;
        msg.ey = transform.eulerAngles.y;
        msg.ez = transform.eulerAngles.z;
        msg.turretY = turret.localEulerAngles.y;
        msg.gunX = gun.localEulerAngles.x;
        NetManager.Send(msg);
    }


    //计算爆炸位置
    public Vector3 ForecastExplodePoint()
    {
        //碰撞信息和碰撞点
        Vector3 hitPoint = Vector3.zero;
        RaycastHit hit;
        //沿着炮管方向的射线
        Vector3 pos = firePoint.position;
        Ray ray = new Ray(pos, firePoint.forward);
        //射线检测
        int layerMask = ~(1 << LayerMask.NameToLayer("Bullet"));
        if (Physics.Raycast(ray, out hit, 200.0f, layerMask))
        {
            hitPoint = hit.point;
        }
        else
        {
            hitPoint = ray.GetPoint(200);
        }
        return hitPoint;
    }

    //搜寻自动瞄准目标
    public void SearchUpdate()
    {
        //时间间隔判断
        if (Time.time - lastSearchTime < SEARCH_INTERVAL)
        {
            return;
        }
        lastSearchTime = Time.time;
        //搜索
        aimTank = null;
        foreach (BaseTank tank in BattleManager.tanks.Values)
        {
            //同个阵营
            if (tank.team == team)
            {
                continue;
            }
            //自己
            if (tank == this)
            {
                continue;
            }
            //已经死亡
            if (tank.IsDie())
            {
                continue;
            }
            //相对位置（z）
            Vector3 p = firePoint.InverseTransformPoint(tank.transform.position);
            if (p.z <= 0 || p.z > MAX_SEARCH_DISTANCE)
            {
                continue;
            }
            //相对位置，45°角度限制
            if (Mathf.Abs(p.x) > p.z)
            {
                continue;
            }
            //是否切换目标
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

    //自动旋转炮管炮塔
    public void AutoAimUpdate()
    {
        //保护时间
        if (Time.time < preventSearchTime)
        {
            return;
        }
        Vector3 p;
        if (aimTank == null)
        {
            //回正
            p = firePoint.InverseTransformPoint(transform.position + transform.forward * 100 + transform.up * 5);
        }
        else
        {
            //相对位置
            p = firePoint.InverseTransformPoint(aimTank.transform.position + new Vector3(0, 5f, 0));
        }
        //旋转炮塔
        float axis = Mathf.Clamp(p.x, -1, 1);
        Vector3 le = turret.localEulerAngles;
        le.y += axis * Time.deltaTime * turretSpeed;
        turret.localEulerAngles = le;
        //旋转炮管
        axis = Mathf.Clamp(p.y, -1, 1);
        le = gun.localEulerAngles;
        le.x -= axis * Time.deltaTime * gunSpeed;
        if (le.x > 180) { le.x = 360 - le.x; }
        le.x = Mathf.Clamp(le.x, minGunAngle, maxGunAngle);
        gun.localEulerAngles = le;
    }

}
