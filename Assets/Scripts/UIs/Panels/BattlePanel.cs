using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : BasePanel
{
    private Image hpFill;
    private Text hpText;

    private Text camp1Text;
    private Text camp2Text;

    private bool toggleEscPanel = false;

    public override void OnInit()
    {
        skinPath = "BattlePanel";
        layer = PanelManager.Layer.CommonPanel;
    }

    public override void OnShow(params object[] args)
    {
        hpFill = skin.transform.Find("HpBar/Fill").GetComponent<Image>();
        hpText = skin.transform.Find("HpBar/HpText").GetComponent<Text>();
        camp1Text = skin.transform.Find("CampInfo/Camp1Text").GetComponent<Text>();
        camp2Text = skin.transform.Find("CampInfo/Camp2Text").GetComponent<Text>();
        UpdatePlayerTeamInfo();

        NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
        NetManager.AddMsgListener("MsgHit", OnMsgHit);

        BaseTank tank = BattleManager.GetCtrlTank();
        if (tank != null)
        {
            UpdateHp(Mathf.CeilToInt(tank.hp));
        }
    }

    // 更新信息
    private void UpdatePlayerTeamInfo()
    {
        int count1 = 0;
        int count2 = 0;
        foreach (BaseTank tank in BattleManager.tanks.Values)
        {
            if (tank.IsDie())
            {
                continue;
            }

            if (tank.team == 1) { count1++; };
            if (tank.team == 2) { count2++; };
        }
        camp1Text.text = "红:" + count1.ToString();
        camp2Text.text = count2.ToString() + ":蓝";
    }

    // 更新 hp
    private void UpdateHp(int hp)
    {
        if (hp < 0) { hp = 0; }
        hpFill.fillAmount = hp / 100f;
        hpText.text = "hp:" + hp;
    }

    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
        NetManager.RemoveMsgListener("MsgHit", OnMsgHit);
    }

    public void OnMsgLeaveBattle(BaseMsg msgBase)
    {
        UpdatePlayerTeamInfo();
    }

    public void OnMsgHit(BaseMsg msgBase)
    {
        MsgHit msg = (MsgHit)msgBase;
        if (msg.targetId == GameMain.id)
        {
            BaseTank tank = BattleManager.GetCtrlTank();
            if (tank != null)
            {
                UpdateHp(Mathf.CeilToInt(tank.hp));
            }
        }
        UpdatePlayerTeamInfo();
    }

    private void Update()
    {
        // 按下ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (toggleEscPanel == false)
            {
                PanelManager.CreatePanel<EscPanel>();
                toggleEscPanel = true;
            }
            else
            {
                PanelManager.RemovePanel("EscPanel");
                toggleEscPanel = false;
            }
        }
    }
}
