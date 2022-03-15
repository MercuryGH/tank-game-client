using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
    // {玩家id: 坦克对象} 键值对
    public static Dictionary<string, BaseTank> tanks = new Dictionary<string, BaseTank>();

    public static void Init()
    {
        //添加监听
        NetManager.AddMsgListener("MsgEnterBattle", OnMsgEnterBattle);
        NetManager.AddMsgListener("MsgBattleResult", OnMsgBattleResult);
        NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);

        NetManager.AddMsgListener("MsgSyncTank", OnMsgSyncTank);
        NetManager.AddMsgListener("MsgFire", OnMsgFire);
        NetManager.AddMsgListener("MsgHit", OnHitMsg);
    }

    public static void AddTank(string id, BaseTank tank)
    {
        tanks[id] = tank;
    }

    public static void RemoveTank(string id)
    {
        tanks.Remove(id);
    }

    public static BaseTank GetTank(string id)
    {
        if (tanks.ContainsKey(id))
        {
            return tanks[id];
        }
        return null;
    }

    // 获取当前玩家控制的坦克
    public static BaseTank GetCtrlTank()
    {
        return GetTank(GameMain.id);
    }

    // 重置场景
    public static void Reset()
    {
        // 场景
        foreach (BaseTank tank in tanks.Values)
        {
            MonoBehaviour.Destroy(tank.gameObject);
        }
        // 坦克列表
        tanks.Clear();
    }

    public static void EnterBattle(MsgEnterBattle msg)
    {
        // 重置场景
        Reset();

        // 关闭非游戏场景界面
        PanelManager.RemovePanel("RoomPanel");  //可以放到房间系统的监听中
        PanelManager.RemovePanel("ResultPanel");
        PanelManager.RemovePanel("KillPanel");
        PanelManager.RemovePanel("BattlePanel");
        PanelManager.RemovePanel("AimPanel");

        // 产生坦克
        for (int i = 0; i < msg.tanks.Length; i++)
        {
            GenerateTank(msg.tanks[i]);
        }
        // 打开游戏场景界面
        PanelManager.CreatePanel<BattlePanel>();
        PanelManager.CreatePanel<AimPanel>();
    }

    public static void GenerateTank(TankInfo tankInfo)
    {
        // 产生名称为 "Tank_${玩家id}" 的 GameObject
        string objName = "Tank_" + tankInfo.id; 
        GameObject tankObj = new GameObject(objName);

        // AddComponent
        BaseTank tank = null;
        
        // 是自己的坦克，就挂载控制脚本，并添加第三人称相机
        if (tankInfo.id == GameMain.id)
        {
            tank = tankObj.AddComponent<CtrlTank>();
            tankObj.AddComponent<CameraFollow>();
        }
        else
        {
            tank = tankObj.AddComponent<SyncTank>();
        }
        
        // 属性
        tank.team = tankInfo.camp;
        tank.id = tankInfo.id;
        tank.hp = tankInfo.hp;

        // 获取几何坐标
        Vector3 pos = new Vector3(tankInfo.x, tankInfo.y, tankInfo.z);
        Vector3 rot = new Vector3(tankInfo.ex, tankInfo.ey, tankInfo.ez);
        tank.transform.position = pos;
        tank.transform.eulerAngles = rot;

        // 根据坦克所属阵营分配不同坦克模型
        if (tankInfo.camp == 1)
        {
            tank.Init("tankPrefab");
        }
        else
        {
            tank.Init("tankPrefab2");
        }

        // 加入哈希表
        AddTank(tankInfo.id, tank);
    }

    public static void OnMsgEnterBattle(MsgBase msgBase)
    {
        MsgEnterBattle msg = (MsgEnterBattle)msgBase;
        EnterBattle(msg);
    }

    public static void OnMsgBattleResult(MsgBase msgBase)
    {
        MsgBattleResult msg = (MsgBattleResult)msgBase;

        //判断显示胜利还是失败
        bool isWin = false;
        BaseTank tank = GetCtrlTank();
        if (tank != null && tank.team == msg.winCamp)
        {
            isWin = true;
        }

        PanelManager.CreatePanel<ResultPanel>(isWin);
        PanelManager.RemovePanel("AimPanel");
    }

    public static void OnMsgLeaveBattle(MsgBase msgBase)
    {
        MsgLeaveBattle msg = (MsgLeaveBattle)msgBase;

        // 查找坦克后删除
        BaseTank tank = GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        RemoveTank(msg.id);
        Object.Destroy(tank.gameObject);
    }

    // 同步位置
    public static void OnMsgSyncTank(MsgBase msgBase)
    {
        MsgSyncTank msg = (MsgSyncTank)msgBase;
        // 不同步自己
        if (msg.id == GameMain.id)
        {
            return;
        }

        // 查找坦克后同步
        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        tank.SyncPos(msg);
    }

    // 同步开火
    public static void OnMsgFire(MsgBase msgBase)
    {
        MsgFire msg = (MsgFire)msgBase;
        if (msg.id == GameMain.id)
        {
            return;
        }

        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        tank.SyncFire(msg);
    }

    // 收到击中协议
    public static void OnHitMsg(MsgBase msgBase)
    {
        MsgHit msg = (MsgHit)msgBase;
        // 查找坦克
        BaseTank hitTank = GetTank(msg.targetId);
        if (hitTank == null)
        {
            return;
        }
        if (hitTank.IsDie()) // 鞭尸不用管
        {
            return;
        }

        // 被击中
        hitTank.Attacked(msg.damage);

        // 击杀提示
        if (hitTank.IsDie() && msg.id == GameMain.id)
        {
            PanelManager.CreatePanel<KillPanel>();
        }
    }
}
