using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TTLShower : MonoBehaviour
{
    public int ttl;
    public Text ttlText;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (ttl > 500)
        {
            ttlText.color = Color.red;
        }
        else
        {
            ttlText.color = Color.green;
        }
        ttlText.text = "Ping: " + ttl.ToString() + "ms";
    }
}
