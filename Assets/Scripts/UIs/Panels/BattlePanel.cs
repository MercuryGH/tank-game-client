using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePanel : BasePanel {
	//hp
	private Image hpFill;
	private Text hpText;
	//info
	private Text camp1Text;
	private Text camp2Text;

	//初始化
	public override void OnInit() {
		skinPath = "BattlePanel";
		layer = PanelManager.Layer.CommonPanel;
	}
	//显示
	public override void OnShow(params object[] args) {
		//寻找组件
		hpFill = skin.transform.Find("HpBar/Fill").GetComponent<Image>();
		hpText = skin.transform.Find("HpBar/HpText").GetComponent<Text>();
		camp1Text = skin.transform.Find("CampInfo/Camp1Text").GetComponent<Text>();
		camp2Text = skin.transform.Find("CampInfo/Camp2Text").GetComponent<Text>();
		ReflashCampInfo();

		NetManager.AddMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
		NetManager.AddMsgListener("MsgHit", OnMsgHit);

		BaseTank tank = BattleManager.GetCtrlTank();
		if(tank != null){
			ReflashHp(Mathf.CeilToInt(tank.hp));
		}

	}


	//更新信息
	private void ReflashCampInfo(){
		int count1 = 0;
		int count2 = 0;
		foreach(BaseTank tank in BattleManager.tanks.Values){
			if(tank.IsDie()){
				continue;
			}

			if(tank.team == 1){count1++;};
			if(tank.team == 2){count2++;};
		}
		camp1Text.text = "红:" + count1.ToString();
		camp2Text.text = count2.ToString()+":蓝"; 
	}

	//更新hp
	private void ReflashHp(int hp){
		if(hp < 0){hp=0;}
		hpFill.fillAmount = hp/100f;
		hpText.text = "hp:" + hp;
	}

	//关闭
	public override void OnClose() {
		NetManager.RemoveMsgListener("MsgLeaveBattle", OnMsgLeaveBattle);
		NetManager.RemoveMsgListener("MsgHit", OnMsgHit);
	}

	//收到玩家退出协议
	public void OnMsgLeaveBattle(MsgBase msgBase){
		ReflashCampInfo();
	}

	//收到击中协议
	public void OnMsgHit(MsgBase msgBase){
		MsgHit msg = (MsgHit)msgBase;
		if(msg.targetId == GameMain.id){
			BaseTank tank = BattleManager.GetCtrlTank();
			if(tank != null){
				ReflashHp(Mathf.CeilToInt(tank.hp));
			}
		}
		ReflashCampInfo();

	}
}
