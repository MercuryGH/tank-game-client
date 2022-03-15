using System;

public class ByteBuffer
{
    private const int DEFAULT_SIZE = 1024;

    public byte[] bytes;

    // 读写指针
    public int readIdx = 0;
    public int writeIdx = 0;

    private readonly int initSize; // 初始化时的容量
    private int capacity; // 当前容量（可扩容）

    public int RemainSize { get { return capacity - writeIdx; } }

    public int CurSize { get { return writeIdx - readIdx; } }

    public ByteBuffer(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    public ByteBuffer(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    public void ReSize(int size)
    {
        if (size < CurSize || size < initSize)
        {
            return;
        }

        int acc = 1;
        while (acc < size)
        {
            acc *= 2;
        }
        capacity = acc;

        byte[] newBytes = new byte[capacity];
        Array.Copy(bytes, readIdx, newBytes, 0, writeIdx - readIdx);
        bytes = newBytes;
        writeIdx = CurSize;
        readIdx = 0;
    }

    public int Write(byte[] bs, int offset, int count)
    {
        if (RemainSize < count)
        {
            ReSize(CurSize + count);
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }

    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, CurSize);
        Array.Copy(bytes, 0, bs, offset, count);
        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }

    // 检查并移动数据
    public void CheckAndMoveBytes()
    {
        // 如果当前数据很少，移动操作基本上是 O(CurSize) = O(1) 的，就移动
        if (CurSize < 8)
        {
            MoveBytes();
        }
    }

    // 移动数据
    public void MoveBytes()
    {
        Array.Copy(bytes, readIdx, bytes, 0, CurSize); // C++ 可以用 std::move()
        writeIdx = CurSize;
        readIdx = 0;
    }

    // 读取 Int16
    public Int16 ReadInt16()
    {
        if (CurSize < 2)
        {
            return 0;
        }

        Int16 ret = BitConverter.ToInt16(bytes, readIdx);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }

    // 读取 Int32
    public Int32 ReadInt32()
    {
        if (CurSize < 4)
        {
            return 0;
        }

        Int32 ret = BitConverter.ToInt32(bytes, readIdx);
        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }

    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIdx, CurSize);
    }

    public string PrintDebug()
    {
        return string.Format("readIdx({0}) writeIdx({1}) bytes({2})",
            readIdx,
            writeIdx,
            BitConverter.ToString(bytes, 0, capacity)
        );
    }
}
