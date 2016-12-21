using System;
using System.Diagnostics;
using System.Text;

public class BinaryReader
{
    private const int MaxCharBytesSize = 128;
    public int Position;
    private byte[] mBuffer;
    private Decoder m_decoder;
    private byte[] m_charBytes;
    private char[] m_singleChar;
    private char[] m_charBuffer;
    private int m_maxCharsSize;  
    public byte[] Buffer;
    private bool m_2BytesPerChar = false;
    private bool m_isMemoryStream; 
    private bool m_leaveOpen;

    public BinaryReader(byte[] buffer)
    {
        var encoding = new UTF8Encoding();
        m_decoder = encoding.GetDecoder();
        m_maxCharsSize = encoding.GetMaxCharCount(MaxCharBytesSize);
        int minBufferSize = encoding.GetMaxByteCount(1);
        if (minBufferSize < 16)
            minBufferSize = 16;
        mBuffer = new byte[minBufferSize];
        Buffer = buffer;
    }

    private void ReadByte(int count)
    {
        int length = Position + count;
        for (int i = 0; i < count ;i++)
        {
            mBuffer[i] = Buffer[Position + i];
        }
        Position = length;
    }
    private byte[] ReadBytes(int count)
    {
        var buffer = new byte[count];
        int length = Position + count;
        for (int i = 0; i < count; i++)
        {
            buffer[i] = Buffer[Position + i];
        }
        Position = length;
        return buffer;
    }
    private byte[] ReadBytes(int count,byte[] bytes)
    {
        int length = Position + count;
        for (int i = 0; i < count; i++)
        {
            bytes[i] = Buffer[Position + i];
        }
        Position = length;
        return bytes;
    }
    public virtual int Read()
    {
        return InternalReadOneChar();
    }

    public virtual bool ReadBoolean()
    {
        ReadByte(1);
        return mBuffer[0] != 0;
    }

    public virtual byte ReadByte()
    {
        ReadByte(1);
        return mBuffer[0];
    }

    public virtual sbyte ReadSByte()
    {
        ReadByte(1);
        return (sbyte)mBuffer[0];
    }

    public virtual char ReadChar()
    {
        int value = Read();
        return (char)value;
    }

    public virtual short ReadInt16()
    {
        ReadByte(2);
        return (short)(mBuffer[0] | mBuffer[1] << 8);
    }

    public virtual ushort ReadUInt16()
    {
        ReadByte(2);
        return (ushort)(mBuffer[0] | mBuffer[1] << 8);
    }

    public virtual int ReadInt32()
    {
        ReadByte(4);
        return mBuffer[0] | mBuffer[1] << 8 | mBuffer[2] << 16 | mBuffer[3] << 24;
    }

    public virtual uint ReadUInt32()
    {
        ReadByte(4);
        return (uint)(mBuffer[0] | mBuffer[1] << 8 | mBuffer[2] << 16 | mBuffer[3] << 24);
    }

    public virtual long ReadInt64()
    {
        ReadByte(8);
        uint lo = (uint)(mBuffer[0] | mBuffer[1] << 8 |
                         mBuffer[2] << 16 | mBuffer[3] << 24);
        uint hi = (uint)(mBuffer[4] | mBuffer[5] << 8 |
                         mBuffer[6] << 16 | mBuffer[7] << 24);
        return (long)hi << 32 | lo;
    }

    public virtual ulong ReadUInt64()
    {
        ReadByte(8);
        uint lo = (uint)(mBuffer[0] | mBuffer[1] << 8 |
                         mBuffer[2] << 16 | mBuffer[3] << 24);
        uint hi = (uint)(mBuffer[4] | mBuffer[5] << 8 |
                         mBuffer[6] << 16 | mBuffer[7] << 24);
        return (ulong)hi << 32 | lo;
    }

    public virtual unsafe float ReadSingle()
    {
        ReadByte(4);
        uint tmpBuffer = (uint)(mBuffer[0] | mBuffer[1] << 8 | mBuffer[2] << 16 | mBuffer[3] << 24);
        return *(float*)&tmpBuffer;
    }

    public virtual unsafe double ReadDouble()
    {
        ReadByte(8);
        uint lo = (uint)(mBuffer[0] | mBuffer[1] << 8 |
            mBuffer[2] << 16 | mBuffer[3] << 24);
        uint hi = (uint)(mBuffer[4] | mBuffer[5] << 8 |
            mBuffer[6] << 16 | mBuffer[7] << 24);
        ulong tmpBuffer = (ulong)hi << 32 | lo;
        return *((double*)&tmpBuffer);
    }

    public virtual decimal ReadDecimal()
    {
        ReadByte(16);
        return (decimal)BitConverter.ToDouble(mBuffer,0);
    }

    public virtual String ReadString()
    {
        int currPos = 0;
        int n;
        int stringLength;
        int readLength;
        int charsRead;
        stringLength = Read7BitEncodedInt();
        if (stringLength == 0)
        {
            return String.Empty;
        }

        if (m_charBytes == null)
        {
            m_charBytes = new byte[MaxCharBytesSize];
        }

        if (m_charBuffer == null)
        {
            m_charBuffer = new char[m_maxCharsSize];
        }

        StringBuilder sb = null;
        do
        {
            readLength = ((stringLength - currPos) > MaxCharBytesSize) ? MaxCharBytesSize : (stringLength - currPos);
            n = readLength;
            m_charBytes = ReadBytes(readLength, m_charBytes);
            charsRead = m_decoder.GetChars(m_charBytes, 0, n, m_charBuffer, 0);

            if (currPos == 0 && n == stringLength)
                return new String(m_charBuffer, 0, charsRead);

            if (sb == null)
                sb = new StringBuilder(stringLength);
            sb.Append(m_charBuffer, 0, charsRead);
            currPos += n;

        } while (currPos < stringLength);

        return sb.ToString();
    }

    private int InternalReadOneChar()
    {
        int charsRead = 0;
        int numBytes = 0;
        long posSav = 0;
        if (m_charBytes == null)
        {
            m_charBytes = new byte[MaxCharBytesSize];
        }
        if (m_singleChar == null)
        {
            m_singleChar = new char[1];
        }

        while (charsRead == 0)
        {
            numBytes = m_2BytesPerChar ? 2 : 1;
            int r = ReadByte();
            m_charBytes[0] = (byte)r;
            if (r == -1)
                numBytes = 0;
            if (numBytes == 2)
            {
                r = ReadByte();
                m_charBytes[1] = (byte)r;
                if (r == -1)
                    numBytes = 1;
            }

            if (numBytes == 0)
            {
                return -1;
            }
            charsRead = m_decoder.GetChars(m_charBytes, 0, numBytes, m_singleChar, 0);
        }
        if (charsRead == 0)
            return -1;
        return m_singleChar[0];
    }

    internal protected int Read7BitEncodedInt()
    {
        int count = 0;
        int shift = 0;
        byte b;
        do
        {
            b = ReadByte();
            count |= (b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return count;
    }
}
