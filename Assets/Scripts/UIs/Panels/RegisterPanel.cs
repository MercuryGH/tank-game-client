using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private InputField repeatPwInput;
    private Button registerBtn;
    private Button closeBtn;

    public override void OnInit()
    {
        skinPath = "RegisterPanel";
        layer = PanelManager.Layer.CommonPanel;
    }

    public override void OnShow(params object[] args)
    {
        idInput = skin.transform.Find("IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("PwInput").GetComponent<InputField>();
        repeatPwInput = skin.transform.Find("RepInput").GetComponent<InputField>();
        registerBtn = skin.transform.Find("RegisterBtn").GetComponent<Button>();
        closeBtn = skin.transform.Find("CloseBtn").GetComponent<Button>();

        registerBtn.onClick.AddListener(OnClickRegister);
        closeBtn.onClick.AddListener(OnCloseClick);

        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
    }

    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgRegister", OnMsgRegister);
    }

    public void OnClickRegister()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelManager.CreatePanel<TipPanel>("用户名或密码不能为空！");
            return;
        }
        if (repeatPwInput.text != pwInput.text)
        {
            PanelManager.CreatePanel<TipPanel>("两次输入的密码不一致！");
            return;
        }

        MsgRegister msgReg = new MsgRegister();
        msgReg.id = idInput.text;
        msgReg.pw = pwInput.text;
        NetManager.Send(msgReg);
    }

    public void OnMsgRegister(BaseMsg msgBase)
    {
        MsgRegister msg = (MsgRegister)msgBase;
        if (msg.result == 0)
        {
            Debug.Log("注册成功");
            PanelManager.CreatePanel<TipPanel>("注册成功");
            Close();
        }
        else if (msg.result == 1)
        {
            PanelManager.CreatePanel<TipPanel>("该用户名已存在！");
        }
        else if (msg.result == 2)
        {
            PanelManager.CreatePanel<TipPanel>("用户名或密码含有非法字符！");
        }
        else if (msg.result == 3)
        {
            PanelManager.CreatePanel<TipPanel>("注册时发生未知错误！");
        }
    }

    public void OnCloseClick()
    {
        Close();
    }
}
