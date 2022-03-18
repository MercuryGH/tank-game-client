using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HallPanel : BasePanel
{
    private Text idText;
    private Text scoreText;

    private Button createButton;
    private Button reflashButton;

    private Transform content; // 房间列表
    private GameObject roomObj; // 房间模板，动态生成房间时copy一个roomObj，默认不会显示

    // 预览坦克视图
    private GameObject tankCamera;
    private GameObject tankObj;

    public override void OnInit()
    {
        skinPath = "RoomListPanel";
        layer = PanelManager.Layer.CommonPanel;
    }

    public override void OnShow(params object[] args)
    {
        idText = skin.transform.Find("InfoPanel/PlayerStatus/IdText").GetComponent<Text>();
        scoreText = skin.transform.Find("InfoPanel/PlayerStatus/ScoreText").GetComponent<Text>();
        createButton = skin.transform.Find("CtrlPanel/CreateButton").GetComponent<Button>();
        reflashButton = skin.transform.Find("CtrlPanel/ReflashButton").GetComponent<Button>();
        content = skin.transform.Find("ListPanel/Scroll View/Viewport/Content");
        roomObj = skin.transform.Find("Room").gameObject;
        tankCamera = skin.transform.Find("InfoPanel/TankCamera").gameObject;

        // 不激活房间
        roomObj.SetActive(false);

        idText.text = GameMain.id;

        createButton.onClick.AddListener(OnCreateClick);
        reflashButton.onClick.AddListener(OnReflashClick);

        NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);

        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);

        GameObject tankSkin = ResManager.LoadPrefab("tankPrefab");
        tankObj = (GameObject)Instantiate(tankSkin, tankCamera.transform);
        tankObj.transform.localPosition = new Vector3(0, -2, 25);
        tankObj.transform.Rotate(0, 90, -30);
    }

    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
    }

    private void OnMsgGetAchieve(BaseMsg msgBase)
    {
        MsgGetAchieve msg = (MsgGetAchieve)msgBase;
        scoreText.text = msg.win + "胜 " + msg.lose + "负";
    }

    private void OnMsgGetRoomList(BaseMsg msgBase)
    {
        MsgGetRoomList msg = (MsgGetRoomList)msgBase;

        // 清除房间列表
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            GameObject o = content.GetChild(i).gameObject;
            Destroy(o);
        }
        // 重新生成列表
        if (msg.rooms == null)
        {
            return;
        }
        for (int i = 0; i < msg.rooms.Length; i++)
        {
            GenerateRoom(msg.rooms[i]);
        }
    }

    public void GenerateRoom(RoomInfo roomInfo)
    {
        // 创建物体
        GameObject o = Instantiate(roomObj); // copy construct
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;

        Transform trans = o.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Button btn = trans.Find("JoinButton").GetComponent<Button>();

        idText.text = roomInfo.id.ToString(); // 房间id
        countText.text = roomInfo.count.ToString();
        if (roomInfo.status == 0)
        {
            statusText.text = "准备中";
        }
        else
        {
            statusText.text = "战斗中";
        }

        //btn.name = idText.text; 
        //btn.onClick.AddListener(delegate ()
        //{
        //    OnJoinClick(btn.name);
        //});
        btn.onClick.AddListener(() =>
        {
            MsgEnterRoom msg = new MsgEnterRoom
            {
                id = int.Parse(idText.text)
            };
            NetManager.Send(msg);
        });
    }

    private void OnReflashClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }

    //public void OnJoinClick(string idString)
    //{
    //    MsgEnterRoom msg = new MsgEnterRoom();
    //    msg.id = int.Parse(idString);
    //    NetManager.Send(msg);
    //}

    private void OnMsgEnterRoom(BaseMsg msgBase)
    {
        MsgEnterRoom msg = (MsgEnterRoom)msgBase;
        if (msg.result == 0)
        {
            PanelManager.CreatePanel<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.CreatePanel<TipPanel>("进入房间失败");
        }
    }

    private void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }

    private void OnMsgCreateRoom(BaseMsg baseMsg)
    {
        MsgCreateRoom msg = (MsgCreateRoom)baseMsg;
        // 创建成功后，自动进入房间
        if (msg.result == 0)
        {
            PanelManager.CreatePanel<TipPanel>("房间创建成功");
            PanelManager.CreatePanel<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.CreatePanel<TipPanel>("房间创建失败");
        }
    }

    public void Update()
    {
        //旋转更新坦克视图
        tankObj.transform.Rotate(0, Time.deltaTime * 2f, 0);
    }
}
