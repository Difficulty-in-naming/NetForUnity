using System;
using System.Collections.Generic;
using System.Text;
[Serializable]
public class BinaryWriter
{
    public static readonly BinaryWriter Null = new BinaryWriter();

    public List<byte> Buffer = new List<byte>();
    private byte[] mBuffer = new byte[16];
    public int Position;

    public int Length
    {
        get { return Buffer.Count; }
    }
    private Encoding mEncoding;
    private Encoder mEncoder;

    private byte[] _largeByteBuffer;
    private int _maxChars;
    private const int LargeByteBufferSize = 256;

    public BinaryWriter()
    {
        mEncoding = new UTF8Encoding(false, true);
        mEncoder = mEncoding.GetEncoder();
    }

    private void WriteByte(byte value)
    {
        Buffer.Add(value);
        Position++;
    }

    private void WriteByte(byte[] value)
    {
        for (int i = 0; i < value.Length; i++)
            WriteByte(value[i]);
    }

    private void WriteByte(byte[] value,int index,int count)
    {
        for (int i = index; i < count; i++)
            WriteByte(value[i]);
    }

    public virtual void Write(bool value)
    {
        WriteByte((byte)(value ? 1 : 0));
    }

    public virtual void Write(byte value)
    {
        WriteByte(value);
    }

    public virtual void Write(sbyte value)
    {
        WriteByte((byte)value);
    }

    public virtual void Write(byte[] buffer)
    {
        WriteByte(buffer);
    }

    public virtual void Write(byte[] buffer, int index, int count)
    {
        for (int i = index; i < count; i++)
            WriteByte(buffer[i]);
    }

    public unsafe virtual void Write(char ch)
    {
/*        int numBytes = 0;
        fixed (byte* pBytes = Buffer)
        {
            numBytes = mEncoder.GetBytes(&ch, 1, pBytes, Buffer.Length, true);
        }
        OutStream.Write(Buffer, 0, numBytes);*/
    }

    public virtual void Write(char[] chars)
    {
        byte[] bytes = mEncoding.GetBytes(chars, 0, chars.Length);
        
        WriteByte(bytes);
    }

    public virtual void Write(char[] chars, int index, int count)
    {
        byte[] bytes = mEncoding.GetBytes(chars, index, count);
        WriteByte(bytes);
    }

    public unsafe virtual void Write(double value)
    {
        ulong TmpValue = *(ulong*)&value;
        mBuffer[0] = (byte)TmpValue;
        mBuffer[1] = (byte)(TmpValue >> 8);
        mBuffer[2] = (byte)(TmpValue >> 16);
        mBuffer[3] = (byte)(TmpValue >> 24);
        mBuffer[4] = (byte)(TmpValue >> 32);
        mBuffer[5] = (byte)(TmpValue >> 40);
        mBuffer[6] = (byte)(TmpValue >> 48);
        mBuffer[7] = (byte)(TmpValue >> 56);
        WriteByte(mBuffer, 0, 8);
    }

    public virtual void Write(decimal value)
    {
        var buffer = BitConverter.GetBytes(Convert.ToDouble(value));
        WriteByte(buffer,0,16);
    }

    public virtual void Write(short value)
    {
        mBuffer[0] = (byte)value;
        mBuffer[1] = (byte)(value >> 8);
        WriteByte(mBuffer,0,2);
    }

    public virtual void Write(ushort value)
    {
        mBuffer[0] = (byte)value;
        mBuffer[1] = (byte)(value >> 8);
        WriteByte(mBuffer, 0, 2);
    }

    public virtual void Write(int value)
    {
        mBuffer[0] = (byte)value;
        mBuffer[1] = (byte)(value >> 8);
        mBuffer[2] = (byte)(value >> 16);
        mBuffer[3] = (byte)(value >> 24);
        WriteByte(mBuffer, 0, 4);
    }

    public virtual void Write(uint value)
    {
        mBuffer[0] = (byte)value;
        mBuffer[1] = (byte)(value >> 8);
        mBuffer[2] = (byte)(value >> 16);
        mBuffer[3] = (byte)(value >> 24);
        WriteByte(mBuffer, 0, 4);
    }

    public virtual void Write(long value)
    {
        mBuffer[0] = (byte)value;
        mBuffer[1] = (byte)(value >> 8);
        mBuffer[2] = (byte)(value >> 16);
        mBuffer[3] = (byte)(value >> 24);
        mBuffer[4] = (byte)(value >> 32);
        mBuffer[5] = (byte)(value >> 40);
        mBuffer[6] = (byte)(value >> 48);
        mBuffer[7] = (byte)(value >> 56);
        WriteByte(mBuffer, 0, 8);
    }

    public virtual void Write(ulong value)
    {
        mBuffer[0] = (byte)value;
        mBuffer[1] = (byte)(value >> 8);
        mBuffer[2] = (byte)(value >> 16);
        mBuffer[3] = (byte)(value >> 24);
        mBuffer[4] = (byte)(value >> 32);
        mBuffer[5] = (byte)(value >> 40);
        mBuffer[6] = (byte)(value >> 48);
        mBuffer[7] = (byte)(value >> 56);
        WriteByte(mBuffer, 0, 8);
    }

    public unsafe virtual void Write(float value)
    {
        uint TmpValue = *(uint*)&value;
        mBuffer[0] = (byte)TmpValue;
        mBuffer[1] = (byte)(TmpValue >> 8);
        mBuffer[2] = (byte)(TmpValue >> 16);
        mBuffer[3] = (byte)(TmpValue >> 24);
        WriteByte(mBuffer, 0, 4);
    }

    public unsafe virtual void Write(String value)
    {
        int len = mEncoding.GetByteCount(value);
        Write7BitEncodedInt(len);

        if (_largeByteBuffer == null)
        {
            _largeByteBuffer = new byte[LargeByteBufferSize];
            _maxChars = _largeByteBuffer.Length / mEncoding.GetMaxByteCount(1);
        }

        if (len <= _largeByteBuffer.Length)
        {
            mEncoding.GetBytes(value, 0, value.Length, _largeByteBuffer, 0);
            WriteByte(_largeByteBuffer, 0, len);
        }
        else
        {
            int charStart = 0;
            int numLeft = value.Length;
            while (numLeft > 0)
            {
                int charCount = (numLeft > _maxChars) ? _maxChars : numLeft;
                int byteLen;
                checked
                {
                    if (charStart < 0 || charCount < 0 || charStart + charCount > value.Length)
                    {
                        throw new ArgumentOutOfRangeException("charCount");
                    }

                    fixed (char* pChars = value)
                    {
                        fixed (byte* pBytes = _largeByteBuffer)
                        {
                            byteLen = mEncoder.GetBytes(pChars + charStart, charCount, pBytes, _largeByteBuffer.Length, charCount == numLeft);
                        }
                    }
                }
                WriteByte(_largeByteBuffer, 0, byteLen);
                charStart += charCount;
                numLeft -= charCount;
            }
        }
    }

    protected void Write7BitEncodedInt(int value)
    {
        uint v = (uint)value;
        while (v >= 0x80)
        {
            Write((byte)(v | 0x80));
            v >>= 7;
        }
        Write((byte)v);
    }
}
