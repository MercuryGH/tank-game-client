using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscPanel : BasePanel
{
    private Text text;
    private Button okBtn;

    public override void OnInit()
    {
        skinPath = "EscPanel";
        layer = PanelManager.Layer.TipPanel;
    }

    public override void OnShow(params object[] args)
    {
        // 寻找组件
        text = skin.transform.Find("Text").GetComponent<Text>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();

        // 监听
        okBtn.onClick.AddListener(OnClickOk);
    }

    public override void OnClose()
    {
        //NetManager.RemoveMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        //NetManager.RemoveMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
    }

    public void OnClickOk()
    {
        MsgLeaveRoom msg = new MsgLeaveRoom();
        NetManager.Send(msg);

        PanelManager.CreatePanel<HallPanel>();
        PanelManager.RemovePanel("AimPanel");
        PanelManager.RemovePanel("BattlePanel");

        Close();
    }
}

