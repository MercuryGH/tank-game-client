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

        PanelManager.CreatePanel<LoginPanel>();

        // 用于单机测试
        //SinglePlayTest();
    }

    private void TestJson()
    {
        MsgRegister msg = new MsgRegister();
        msg.id = "Mercury";
        msg.pw = "114514";

        BaseMsg.Encode(msg);
    }

    private void SinglePlayTest()
    {
        GameMain.id = "cat";
        TankInfo tankInfo = new TankInfo();
        tankInfo.team = 1;
        tankInfo.id = GameMain.id;
        tankInfo.hp = 30;
        tankInfo.x = 262;
        tankInfo.y = -8;
        tankInfo.z = 342;
        BattleManager.GenerateTank(tankInfo);
        PanelManager.CreatePanel<BattlePanel>();
        PanelManager.CreatePanel<AimPanel>();

        TankInfo tankInfo2 = new TankInfo();
        tankInfo2.team = 2;
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
        PanelManager.CreatePanel<TipPanel>("与服务器断开连接");
    }

    void OnMsgKick(BaseMsg baseMsg)
    {
        MsgKick msgKick = (MsgKick)baseMsg;

        string showText = "";
        if (msgKick.reason == 0)
        {
            showText = "因他人登录同一账号，您已被迫下线";
        }
        else if (msgKick.reason == 1)
        {
            showText = "服务端无法解析您的行为，您已被迫下线";
        }
        else if (msgKick.reason == 2)
        {
            showText = "您发送的数据过长，服务端无法处理，您已被迫下线";
        }
        else if (msgKick.reason == 3)
        {
            showText = "【心跳失常】与服务器断开连接";
        }
        PanelManager.CreatePanel<TipPanel>(showText);
    }
}
