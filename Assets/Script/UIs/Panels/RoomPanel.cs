﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    private Button startButton;
    private Button closeButton;

    private Transform content1; //蓝色阵营
    private Transform content2; //红色阵营
    private GameObject playerObj; //玩家信息物体

    public override void OnInit()
    {
        skinPath = "RoomPanel";
        layer = PanelManager.Layer.CommonPanel;
    }

    public override void OnShow(params object[] args)
    {
        //寻找组件
        startButton = skin.transform.Find("CtrlPanel/StartButton").GetComponent<Button>();
        closeButton = skin.transform.Find("CtrlPanel/CloseButton").GetComponent<Button>();
        content1 = skin.transform.Find("ListPanel/Scroll View1/Viewport/Content");
        content2 = skin.transform.Find("ListPanel/Scroll View2/Viewport/Content");
        playerObj = skin.transform.Find("Player").gameObject;
        //不激活玩家信息
        playerObj.SetActive(false);
        //按钮事件
        startButton.onClick.AddListener(OnStartClick);
        closeButton.onClick.AddListener(OnCloseClick);
        //协议监听
        NetManager.AddMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.AddMsgListener("MsgStartBattle", OnMsgStartBattle);
        //发送查询
        MsgGetRoomInfo msg = new MsgGetRoomInfo();
        NetManager.Send(msg);
    }

    public override void OnClose()
    {
        //协议监听
        NetManager.RemoveMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.RemoveMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.RemoveMsgListener("MsgStartBattle", OnMsgStartBattle);
    }

    public void OnMsgGetRoomInfo(MsgBase msgBase)
    {
        MsgGetRoomInfo msg = (MsgGetRoomInfo)msgBase;
        //清除玩家列表
        for (int i = content1.childCount - 1; i >= 0; i--)
        {
            GameObject o = content1.GetChild(i).gameObject;
            Destroy(o);
        }
        for (int i = content2.childCount - 1; i >= 0; i--)
        {
            GameObject o = content2.GetChild(i).gameObject;
            Destroy(o);
        }
        //重新生成列表
        if (msg.players == null)
        {
            return;
        }
        for (int i = 0; i < msg.players.Length; i++)
        {
            GeneratePlayerInfo(msg.players[i]);
        }
    }

    public void GeneratePlayerInfo(PlayerInfo playerInfo)
    {
        //创建物体
        GameObject o = Instantiate(playerObj);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;
        //设置阵营 1-红 2-蓝
        if (playerInfo.camp == 1)
        {
            o.transform.SetParent(content2);
        }
        else
        {
            o.transform.SetParent(content1);
        }
        //获取组件
        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Image ownerImage = trans.Find("OwnerImage").GetComponent<Image>();
        Text scoreText = trans.Find("ScoreText").GetComponent<Text>();
        //填充信息
        idText.text = playerInfo.id;
        if (playerInfo.isOwner == 1)
        {
            ownerImage.gameObject.SetActive(true);
        }
        else
        {
            ownerImage.gameObject.SetActive(false);
        }
        scoreText.text = playerInfo.win + "胜 " + playerInfo.lost + "负";
    }

    //点击退出按钮
    public void OnCloseClick()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);
    }

    //收到退出房间协议
    public void OnMsgLeaveRoom(MsgBase msgBase)
    {
        MsgLeaveRoom msg = (MsgLeaveRoom)msgBase;
        //成功退出房间
        if (msg.result == 0)
        {
            //PanelManager.Open<TipPanel>("退出房间");
            PanelManager.CreatePanel<RoomListPanel>();
            Close();
        }
        //退出房间失败
        else
        {
            PanelManager.CreatePanel<TipPanel>("退出房间失败");
        }
    }

    //点击开战按钮
    public void OnStartClick()
    {
        MsgStartBattle msg = new MsgStartBattle();
        NetManager.Send(msg);
    }

    //收到开战返回
    public void OnMsgStartBattle(MsgBase msgBase)
    {
        MsgStartBattle msg = (MsgStartBattle)msgBase;
        //开战
        if (msg.result == 0)
        {
            Close();
        }
        //开战失败
        else
        {
            PanelManager.CreatePanel<TipPanel>("开战失败！两队至少都需要一名玩家，只有队长可以开始战斗！");
        }
    }

}
