using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : BasePanel
{
    private Image winImage;
    private Image loseImage;
    private Button okBtn;

    //初始化
    public override void OnInit()
    {
        skinPath = "ResultPanel";
        layer = PanelManager.Layer.TipPanel;
    }
    //显示
    public override void OnShow(params object[] args)
    {
        //寻找组件
        winImage = skin.transform.Find("WinImage").GetComponent<Image>();
        loseImage = skin.transform.Find("LostImage").GetComponent<Image>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();
        //监听
        okBtn.onClick.AddListener(OnOkClick);
        //显示哪个图片
        if (args.Length == 1)
        {
            bool isWIn = (bool)args[0];
            if (isWIn)
            {
                winImage.gameObject.SetActive(true);
                loseImage.gameObject.SetActive(false);
            }
            else
            {
                winImage.gameObject.SetActive(false);
                loseImage.gameObject.SetActive(true);
            }
        }
    }

    //关闭
    public override void OnClose()
    {

    }

    //当按下确定按钮
    public void OnOkClick()
    {
        PanelManager.CreatePanel<RoomPanel>();
        Close();
    }
}
