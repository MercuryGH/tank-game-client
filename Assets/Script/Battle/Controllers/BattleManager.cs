using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager
{
    //战场中的坦克
    public static Dictionary<string, BaseTank> tanks = new Dictionary<string, BaseTank>();

    //初始化
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

    //添加坦克
    public static void AddTank(string id, BaseTank tank)
    {
        tanks[id] = tank;
    }

    //删除坦克
    public static void RemoveTank(string id)
    {
        tanks.Remove(id);
    }

    //获取坦克
    public static BaseTank GetTank(string id)
    {
        if (tanks.ContainsKey(id))
        {
            return tanks[id];
        }
        return null;
    }

    //获取玩家控制的坦克
    public static BaseTank GetCtrlTank()
    {
        return GetTank(GameMain.id);
    }

    //重置战场
    public static void Reset()
    {
        //场景
        foreach (BaseTank tank in tanks.Values)
        {
            MonoBehaviour.Destroy(tank.gameObject);
        }
        //列表
        tanks.Clear();
    }

    //开始战斗
    public static void EnterBattle(MsgEnterBattle msg)
    {
        //重置
        BattleManager.Reset();
        //关闭界面
        PanelManager.RemovePanel("RoomPanel");//可以放到房间系统的监听中
        PanelManager.RemovePanel("ResultPanel");
        PanelManager.RemovePanel("KillPanel");
        PanelManager.RemovePanel("BattlePanel");
        PanelManager.RemovePanel("AimPanel");
        //产生坦克
        for (int i = 0; i < msg.tanks.Length; i++)
        {
            GenerateTank(msg.tanks[i]);
        }
        //打开界面
        PanelManager.CreatePanel<BattlePanel>();
        PanelManager.CreatePanel<AimPanel>();
    }

    //产生坦克
    public static void GenerateTank(TankInfo tankInfo)
    {
        //GameObject
        string objName = "Tank_" + tankInfo.id;
        GameObject tankObj = new GameObject(objName);
        //AddComponent
        BaseTank tank = null;
        if (tankInfo.id == GameMain.id)
        {
            tank = tankObj.AddComponent<CtrlTank>();
        }
        else
        {
            tank = tankObj.AddComponent<SyncTank>();
        }
        //camera
        if (tankInfo.id == GameMain.id)
        {
            CameraFollow cf = tankObj.AddComponent<CameraFollow>();
        }
        //属性
        tank.team = tankInfo.team;
        tank.id = tankInfo.id;
        tank.hp = tankInfo.hp;
        //pos rotation
        Vector3 pos = new Vector3(tankInfo.x, tankInfo.y, tankInfo.z);
        Vector3 rot = new Vector3(tankInfo.ex, tankInfo.ey, tankInfo.ez);
        tank.transform.position = pos;
        tank.transform.eulerAngles = rot;
        //init
        if (tankInfo.team == 1)
        {
            tank.Init("tankPrefab");
        }
        else
        {
            tank.Init("tankPrefab2");
        }
        //列表
        AddTank(tankInfo.id, tank);
    }

    //收到进入战斗协议
    public static void OnMsgEnterBattle(MsgBase msgBase)
    {
        MsgEnterBattle msg = (MsgEnterBattle)msgBase;
        EnterBattle(msg);
    }

    //收到战斗结束协议
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
        //显示界面
        PanelManager.CreatePanel<ResultPanel>(isWin);
        //关闭界面
        PanelManager.RemovePanel("AimPanel");
    }

    //收到玩家退出协议
    public static void OnMsgLeaveBattle(MsgBase msgBase)
    {
        MsgLeaveBattle msg = (MsgLeaveBattle)msgBase;
        //查找坦克
        BaseTank tank = GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        //删除坦克
        RemoveTank(msg.id);
        MonoBehaviour.Destroy(tank.gameObject);
    }

    //收到同步协议
    public static void OnMsgSyncTank(MsgBase msgBase)
    {
        MsgSyncTank msg = (MsgSyncTank)msgBase;
        //不同步自己
        if (msg.id == GameMain.id)
        {
            return;
        }
        //查找坦克
        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        //移动同步
        tank.SyncPos(msg);
    }

    //收到开火协议
    public static void OnMsgFire(MsgBase msgBase)
    {
        MsgFire msg = (MsgFire)msgBase;
        //不同步自己
        if (msg.id == GameMain.id)
        {
            return;
        }
        //查找坦克
        SyncTank tank = (SyncTank)GetTank(msg.id);
        if (tank == null)
        {
            return;
        }
        //开火
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
