using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel : BasePanel
{
    private Image winImage;
    private Image loseImage;
    private Button okBtn;

    public override void OnInit()
    {
        skinPath = "ResultPanel";
        layer = PanelManager.Layer.TipPanel;
    }

    public override void OnShow(params object[] args)
    {
        winImage = skin.transform.Find("WinImage").GetComponent<Image>();
        loseImage = skin.transform.Find("LostImage").GetComponent<Image>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();

        okBtn.onClick.AddListener(OnOkClick);

        // 根据胜负显示不同的图片
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

    public override void OnClose()
    {

    }

    public void OnOkClick()
    {
        PanelManager.CreatePanel<RoomPanel>();
        Close();
    }
}
