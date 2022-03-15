using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{
    private Text text;
    private Button okBtn;

    public override void OnInit()
    {
        skinPath = "TipPanel";
        layer = PanelManager.Layer.TipPanel;
    }

    public override void OnShow(params object[] args)
    {
        // 寻找组件
        text = skin.transform.Find("Text").GetComponent<Text>();
        okBtn = skin.transform.Find("OkBtn").GetComponent<Button>();

        // 监听
        okBtn.onClick.AddListener(OnClickOk);

        // 提示语
        if (args.Length == 1)
        {
            text.text = (string)args[0];
        }
    }

    public override void OnClose()
    {

    }

    public void OnClickOk()
    {
        Close();
    }
}
