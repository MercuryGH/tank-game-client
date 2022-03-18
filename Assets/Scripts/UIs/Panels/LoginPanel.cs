using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button registerBtn;
    private Image bgImage;

    private float startTime = float.MaxValue; // 开始显示的时间
    private bool showConnFail = false; // 显示连接失败

    // 硬编码服务器 IP 地址和端口
    private const string ip = "1.15.226.90"; 
    private const int port = 6666;

    public override void OnInit()
    {
        skinPath = "LoginPanel";
        layer = PanelManager.Layer.CommonPanel;
    }

    public override void OnShow(params object[] args)
    {
        idInput = skin.transform.Find("LoginDiv/IdInput").GetComponent<InputField>();
        pwInput = skin.transform.Find("LoginDiv/PwInput").GetComponent<InputField>();
        loginBtn = skin.transform.Find("LoginDiv/LoginBtn").GetComponent<Button>();
        registerBtn = skin.transform.Find("LoginDiv/RegisterBtn").GetComponent<Button>();
        bgImage = skin.transform.Find("BgImage").GetComponent<Image>();

        loginBtn.onClick.AddListener(OnClickLogin);
        registerBtn.onClick.AddListener(OnClickRegister);

        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);

        NetManager.Connect(ip, port);

        startTime = Time.time;
    }

    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);

        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
    }

    void OnConnectSucc(string arg)
    {
        Debug.Log("OnConnectSucc");
    }

    void OnConnectFail(string arg)
    {
        showConnFail = true;
        //PanelManager.Open<TipPanel>(err);
    }

    public void OnClickRegister()
    {
        PanelManager.CreatePanel<RegisterPanel>();
    }

    public void OnClickLogin()
    {
        if (idInput.text == "" || pwInput.text == "")
        {
            PanelManager.CreatePanel<TipPanel>("用户名或密码不能为空");
            return;
        }

        MsgLogin msgLogin = new MsgLogin();
        msgLogin.id = idInput.text;
        msgLogin.pw = pwInput.text;
        NetManager.Send(msgLogin);
    }

    public void OnMsgLogin(BaseMsg msgBase)
    {
        MsgLogin msg = (MsgLogin)msgBase;
        if (msg.result == 0)
        {
            Debug.Log("登录成功！");
            GameMain.id = msg.id;
            PanelManager.CreatePanel<HallPanel>();
            Close();
        }
        else if (msg.result == 1)
        {
            PanelManager.CreatePanel<TipPanel>("用户名或密码错误！");
        }
    }

    public void Update()
    {
        // 背景图动画效果（需要改进）
        // float w = Mathf.Ceil(Time.time * 2) % 10 == 0 ? 500f : 0.1f;//频率
        // float a = 1 + 0.1f - 0.1f * Mathf.Sin(w * Time.time);   //振幅
        // bgImage.transform.localScale = new Vector3(a, a, 1);

        if (showConnFail)
        {
            showConnFail = false;
            PanelManager.CreatePanel<TipPanel>("网络连接失败，请重新打开游戏");
        }
    }
}
