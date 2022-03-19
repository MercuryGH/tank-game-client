using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TTLShower : MonoBehaviour
{
    public Text ttlText;
    private const float REFRESH_INTERVAL = 3;
    private static float lastRefreshTime = 0;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float curTime = Time.time;
        if (curTime - lastRefreshTime <= REFRESH_INTERVAL)
        {
            return;
        }

        float ttlFloat = 0;
        float pingPongInterval = NetManager.lastPongTime - NetManager.lastPingTime;

        if (pingPongInterval < 0) // 未回复
        {
            ttlFloat = Math.Max(curTime - NetManager.lastPingTime, NetManager.lastPingPongInterval);
        }
        else // 已回复
        {
            ttlFloat = pingPongInterval;
        }

        int ttl = (int)(ttlFloat * 1000);
        lastRefreshTime = curTime;

        // 超高延迟
        if (ttl > 1000)
        {
            ttlText.color = Color.red;
            ttlText.text = "Ping: >1000 ms";
            return;
        }

        if (ttl > 100)
        {
            ttlText.color = Color.yellow;
        }
        else
        {
            ttlText.color = Color.green;
        }
        ttlText.text = "Ping: " + ttl.ToString() + "ms";
    }
}
