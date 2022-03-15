using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMain : MonoBehaviour
{
    public static string id = ""; // 用户名 cache

    void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);

        PanelManager.Init();
        BattleManager.Init();

        //PanelManager.CreatePanel<LoginPanel>();

        // 用于单机测试

        GameMain.id = "cat";
        TankInfo tankInfo = new TankInfo();
        tankInfo.camp = 1;
        tankInfo.id = GameMain.id;
        tankInfo.hp = 30;
        tankInfo.x = 262;
        tankInfo.y = -8;
        tankInfo.z = 342;
        BattleManager.GenerateTank(tankInfo);
        PanelManager.CreatePanel<BattlePanel>();
        PanelManager.CreatePanel<AimPanel>();

        TankInfo tankInfo2 = new TankInfo();
        tankInfo2.camp = 2;
        tankInfo2.id = "dog";
        tankInfo2.hp = 100;
        tankInfo2.z = 30;
        tankInfo2.y = 5;
        tankInfo2.ey = 130;
        BattleManager.GenerateTank(tankInfo2);
    }

    void Update()
    {
        NetManager.Update();
    }

    void OnConnectClose(string err)
    {
        Debug.Log("断开连接");
    }

    void OnMsgKick(MsgBase msgBase)
    {
        PanelManager.CreatePanel<TipPanel>("被踢下线");
    }
}
