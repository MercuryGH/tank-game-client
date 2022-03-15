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
        //寻找组件
        idInput = skin.transform.Find("IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("PwInput").GetComponent<InputField>();
        repeatPwInput = skin.transform.Find("RepInput").GetComponent<InputField>();
        registerBtn = skin.transform.Find("RegisterBtn").GetComponent<Button>();
        closeBtn = skin.transform.Find("CloseBtn").GetComponent<Button>();
        //监听
        registerBtn.onClick.AddListener(OnClickRegister);
        closeBtn.onClick.AddListener(OnCloseClick);
        //网络协议监听
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
            PanelManager.CreatePanel<TipPanel>("用户名和密码不能为空");
            return;
        }
        if (repeatPwInput.text != pwInput.text)
        {
            PanelManager.CreatePanel<TipPanel>("两次输入的密码不同");
            return;
        }

        MsgRegister msgReg = new MsgRegister();
        msgReg.id = idInput.text;
        msgReg.pw = pwInput.text;
        NetManager.Send(msgReg);
    }

    public void OnMsgRegister(MsgBase msgBase)
    {
        MsgRegister msg = (MsgRegister)msgBase;
        if (msg.result == 0)
        {
            Debug.Log("注册成功");
            //提示
            PanelManager.CreatePanel<TipPanel>("注册成功");
            //关闭界面
            Close();
        }
        else
        {
            PanelManager.CreatePanel<TipPanel>("注册失败");
        }
    }

    public void OnCloseClick()
    {
        Close();
    }
}
