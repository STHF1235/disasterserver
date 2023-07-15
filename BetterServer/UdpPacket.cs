﻿using BetterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer
{
    public class UdpPacket
    {
        public int Length { get; private set; }
        
        private byte[] _buffer = new byte[128]; //i never exceed 64 anyways
        private int _position = 0;

        public UdpPacket(PacketType type, params dynamic[] args)
        {
            Write((byte)0);
            Write((byte)type);

            foreach(var dyn in args)
                Write(dyn);
        }

        public UdpPacket(PacketType type)
        {
            Write((byte)0);
            Write((byte)type);
        }

        public void Write(byte value)
        {
            lock (_buffer)
            {
                _buffer[_position++] = value;
                Length = _position;
            }
        }

        public void Write(char value)
        {
            Write((byte)value);
        }

        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        public void Write(bool value)
        {
            Write((byte)(value ? 1 : 0));
        }

        public void Write(ushort value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                }
                else
                {
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(short value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                }
                else
                {
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(uint value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[3]);
                }
                else
                {
                    Write((byte)ptr[3]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(int value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[3]);
                }
                else
                {
                    Write((byte)ptr[3]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(float value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[3]);
                }
                else
                {
                    Write((byte)ptr[3]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(ulong value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[3]);
                    Write((byte)ptr[4]);
                    Write((byte)ptr[5]);
                    Write((byte)ptr[6]);
                    Write((byte)ptr[7]);
                }
                else
                {
                    Write((byte)ptr[7]);
                    Write((byte)ptr[6]);
                    Write((byte)ptr[5]);
                    Write((byte)ptr[4]);
                    Write((byte)ptr[3]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(double value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[3]);
                    Write((byte)ptr[4]);
                    Write((byte)ptr[5]);
                    Write((byte)ptr[6]);
                    Write((byte)ptr[7]);
                }
                else
                {
                    Write((byte)ptr[7]);
                    Write((byte)ptr[6]);
                    Write((byte)ptr[5]);
                    Write((byte)ptr[4]);
                    Write((byte)ptr[3]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }
        public void Write(long value)
        {
            unsafe
            {
                byte* ptr = (byte*)&value;

                if (BitConverter.IsLittleEndian)
                {
                    Write((byte)ptr[0]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[3]);
                    Write((byte)ptr[4]);
                    Write((byte)ptr[5]);
                    Write((byte)ptr[6]);
                    Write((byte)ptr[7]);
                }
                else
                {
                    Write((byte)ptr[7]);
                    Write((byte)ptr[6]);
                    Write((byte)ptr[5]);
                    Write((byte)ptr[4]);
                    Write((byte)ptr[3]);
                    Write((byte)ptr[2]);
                    Write((byte)ptr[1]);
                    Write((byte)ptr[0]);
                }
            }
        }

        public void Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);

            foreach (var c in bytes)
                Write(c);

            Write((char)'\0');
        }

        public byte[] ToArray()
        {
            return _buffer;
        }
    }
}
