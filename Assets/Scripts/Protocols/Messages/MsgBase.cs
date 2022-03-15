using System;
using UnityEngine;
using System.Linq;

public class MsgBase
{
    public string protoName = "null";

    /**
     * 协议格式为 
     * struct Message {
     *     short totalLength; // size(Message) - sizeof(short)
     *     short msgTypeLength;
     *     byte[] msgType;
     *     byte[] infoDetail;
     * };
     */

    /**
     * 将msgBase串行化成JSON
     * 没有考虑长度等其他信息，还需要编码成协议格式才能发送
     * TODO: 封装编码方法
     */
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonUtility.ToJson(msgBase);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }

    /**
     * 将解码后的结果反串行化成 msgBase
     * @params protoName 协议名
     * @params bytes 原字节流
     * @params offset 从 bytes[offset] 的位置开始解释JSON
     * @params count 到 bytes[offset + count]的位置停止解释JSON
     */
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        Debug.Log("Debug decode: " + s);

        MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, Type.GetType(protoName));
        return msgBase;
    }

    /**
     * 编码协议名
     * @params msgBase 待发送消息
     * @return bytes 串行化后的协议名长度 + 协议名 byte array
     */
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);
        Int16 len = (Int16)nameBytes.Length;

        byte[] bytes = new byte[2 + len];
        // 组装2字节的长度信息
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        // 组装名字bytes
        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }

    /**
     * 解码协议名
     * @params bytes  原字节流
     * @params offset 从 bytes[offset] 开始提取 msgTypeLength
     * @return 协议名 msgType
     * @return_by_out count = sizeof(short) + len(msgType)
     */
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        count = 0;
        // 边界情况考虑，避免数组越界
        if (offset + 2 > bytes.Length)
        {
            return "";
        }

        // 读取长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if (offset + 2 + len > bytes.Length)
        {
            return "";
        }

        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return name;
    }
}


