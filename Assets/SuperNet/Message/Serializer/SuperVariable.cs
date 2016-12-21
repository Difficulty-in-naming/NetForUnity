using System;
using System.Diagnostics;

namespace SWX
{
    public class WriteHelper
    {
        private BinaryWriter mWriter;
        public WriteHelper()
        {
            mWriter = new BinaryWriter();
        }
        public bool Write(object obj)
        {
            if (obj is int)
                mWriter.Write((int) obj);
            else if(obj is byte)
                mWriter.Write((byte)obj);
            else if(obj is byte[])
                mWriter.Write((byte[])obj);
            else if (obj is bool)
                mWriter.Write((bool) obj);
            else if (obj is long)
                mWriter.Write((long) obj);
            else if (obj is double)
                mWriter.Write((double) obj);
            else if (obj is float)
                mWriter.Write((float) obj);
            else if (obj is short)
                mWriter.Write((short) obj);
            else if (obj is string)
                mWriter.Write((string) obj);
            else if (obj is uint)
                mWriter.Write((uint) obj);
            else if (obj is ulong)
                mWriter.Write((ulong) obj);
            else if (obj is ushort)
                mWriter.Write((ushort) obj);
            else
            {
                return false;
            }
            return true;
        }

        public long Position
        {
            get { return mWriter.Position; }
        }

        public int Length
        {
            get { return mWriter.Length; }
        }

        public byte[] Bytes
        {
            get
            {
                return mWriter.Buffer.ToArray();
            }
        }
    }

    public class ReadHelper
    {
        private BinaryReader mReader;
        public ReadHelper(byte[] bytes)
        {
            mReader = new BinaryReader(bytes);
        }
        public object Read(Type fieldType)
        {
            if (fieldType == typeof(int))
                return mReader.ReadInt32();
            else if (fieldType == typeof (byte))
                return mReader.ReadByte();
            else if (fieldType == typeof (bool))
                return mReader.ReadBoolean();
            else if (fieldType == typeof (long))
                return mReader.ReadInt64();
            else if (fieldType == typeof (double))
                return mReader.ReadDouble();
            else if (fieldType == typeof (float))
                return mReader.ReadSingle();
            else if (fieldType == typeof (short))
                return mReader.ReadInt16();
            else if (fieldType == typeof (string))
                return mReader.ReadString();
            else if (fieldType == typeof (uint))
                return mReader.ReadUInt32();
            else if (fieldType == typeof (ulong))
                return mReader.ReadUInt64();
            else if (fieldType == typeof (ushort))
                return mReader.ReadUInt16();
            else if (fieldType.IsClass)
                return Activator.CreateInstance(fieldType);
            return null;
        }

        public float ReadFloat()
        {
            return mReader.ReadSingle();
        }

        public short ReadShort()
        {
            return mReader.ReadInt16();
        }

        public uint ReadUint()
        {
            return mReader.ReadUInt32();
        }

        public ulong ReadUlong()
        {
            return mReader.ReadUInt64();
        }

        public ushort ReadUShort()
        {
            return mReader.ReadUInt16();
        }

        public int ReadInt()
        {
            return mReader.ReadInt32();
        }

        public string ReadString()
        {
            return mReader.ReadString();
        }

        public long ReadLong()
        {
            return mReader.ReadInt64();
        }

        public double ReadDouble()
        {
            return mReader.ReadDouble();
        }

        public long Position
        {
            get { return mReader.Position; }
        }
        public byte ReadByte()
        {
            return mReader.ReadByte();
        }
    }
}
