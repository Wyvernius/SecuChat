using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Threading;

namespace SharedClasses
{
    /*
    Main buffer class.
    contains all logic for sending and receiving data.
    */
    public class nBuffer
    {
        public List<byte> innerbuffer = new List<byte>();
        public UInt32 CryptoSize;
        public int NextObjectIndex = 4;
        public int AddedObjectSize = 0;

        // adds message id to buffer
        public void AddID(MsgType ID)
        {
            innerbuffer.AddRange(BitConverter.GetBytes((int)ID));
        }

        public void AddInt(Int32 var)
        {
            innerbuffer.AddRange(BitConverter.GetBytes(var));
        }

        public int GetInt(int index)
        {
            NextObjectIndex += sizeof(int);
            return BitConverter.ToInt32(innerbuffer.ToArray(), index);
        }

        public MsgType GetID()
        {
            int ID = 0;
            if (innerbuffer.Count > 0)
                ID = BitConverter.ToInt32(innerbuffer.ToArray(), 0);
            return (MsgType)ID;
        }

        public void AddObject<T>(T Object)
        {
            innerbuffer.AddRange(Utils.ObjectToByteArray(Object));
        }

        public byte[] Getbuffer()
        {
            byte[] arr = new byte[innerbuffer.Count];
            innerbuffer.CopyTo(arr, 0);
            return arr;
        }

        // get the first object after the index.
        // adds size to NextObjectIndex.
        public T GetObject<T>(int index = 4)
        {
            int size = 0;
            T VAR = (T)Utils.ByteArrayToObject(innerbuffer.ToArray(), out size, index, Getsize() - index);
            NextObjectIndex += size;
            return VAR;
        }

        public T GetNextObject<T>()
        {
            T VAR;
            try
            {
                int size = 0;
                VAR = (T)Utils.ByteArrayToObject(innerbuffer.ToArray(), out size, NextObjectIndex, Getsize() - NextObjectIndex);
                NextObjectIndex += GetObjectSize(VAR);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "  " + ex.StackTrace);
                VAR = (T)new Object();
            }
            return VAR;
        }

        public int Getsize()
        {
            return innerbuffer.Count;
        }

        private int GetObjectSize(object TestObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] Array;
            bf.Serialize(ms, TestObject);
            Array = ms.ToArray();
            return Array.Length;
        }

        public byte[] GetCryptoBuffer(byte[] Salt)
        {
            CryptoSize = (UInt32)Crypt.Encrypt(innerbuffer.ToArray(), Salt).Length;
            byte[] tmparr = Crypt.Encrypt(innerbuffer.ToArray(), Salt);
            return tmparr;
        }

        public UInt32 GetCryptoSize()
        {
            return CryptoSize;
        }

        public int SendBuffer(Socket s)
        {
            return s.Send(Getbuffer());
        }

        public int SendCryptoBuffer(Socket s, byte[] Salt)
        {
            int sendbytes = 0;
            try
            {
                byte[] tmpbuf = GetCryptoBuffer(Salt);
                s.Send(BitConverter.GetBytes(CryptoSize));
                int sendchunk = (int)Msg.Size;
                while (sendbytes < CryptoSize)
                {
                    if ((CryptoSize - sendbytes) < sendchunk)
                        sendchunk = (int)CryptoSize - sendbytes;
                    sendbytes += s.Send(tmpbuf, sendbytes, sendchunk, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Send CryptoBuffer: {0}", ex.Message);
                sendbytes = -1;
            }
            return sendbytes;
        }

        public int ReceiveBuffer(Socket s)
        {
            int receivedbytes = 0;
            byte[] b = new byte[1];
            receivedbytes = s.Receive(b);
            if (s.Available > 0)
            {
                byte[] tmpbuffer = new byte[s.Available + 1];
                b.CopyTo(tmpbuffer, 0);
                receivedbytes = s.Receive(tmpbuffer, 1, s.Available, SocketFlags.None);
                innerbuffer.AddRange(tmpbuffer);
            }
            //Console.WriteLine("Recv Bytes = " + receivedbytes + " From : " + s.RemoteEndPoint);
            return receivedbytes;

        }

        public int ReceiveCryptoBuffer(Socket s, byte[] Salt) // single read, small objects;
        {
            int receivedbytes = 0;
            int recvchunk = (int)Msg.Size;
            byte[] b = new byte[4];
            try
            {
                s.Receive(b);
                CryptoSize = BitConverter.ToUInt32(b, 0);
                byte[] tmpbuffer = new byte[CryptoSize];

                while (receivedbytes < CryptoSize)
                {
                    if ((CryptoSize - receivedbytes) < recvchunk)
                        recvchunk = (int)CryptoSize - receivedbytes;
                    receivedbytes += s.Receive(tmpbuffer, receivedbytes, recvchunk, SocketFlags.None);
                }
                tmpbuffer = Crypt.Decrypt(tmpbuffer, Salt);
                innerbuffer.AddRange(tmpbuffer);
            }
            catch (Exception ex)
            {
                receivedbytes = -1;
            }
            return receivedbytes;
        }

        public int ReceiveCryptoFileBuffer(Socket s, byte[] Salt, string name = null, Func<object, object, int> CallbackFunction = null)
        {
            try
            {
                int recvbytes = 0;
                float percent = 0;
                int recvchunk = (int)Msg.FileDataChunk;
                byte[] b = new byte[4];
                s.Receive(b);
                CryptoSize = BitConverter.ToUInt32(b, 0);
                Console.WriteLine("Crypt File Size = " + CryptoSize);
                byte[] buffer = new byte[CryptoSize];
                while (recvbytes < CryptoSize)
                {
                    if (recvchunk > (CryptoSize - recvbytes))
                        recvchunk = (int)CryptoSize - recvbytes;
                    recvbytes += s.Receive(buffer, recvbytes, recvchunk, SocketFlags.None);
                    percent = 100.0f / (float)CryptoSize * (float)recvbytes;
                    if (CallbackFunction != null)
                     CallbackFunction?.Invoke(name,(int)percent);
                    //Thread.Sleep(1);
                }

                innerbuffer.AddRange(Crypt.Decrypt(buffer, Salt));
                if (CallbackFunction != null)
                    CallbackFunction?.Invoke(name, 100); // Set BAr Value
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            return 0;
        }

        public int SendCryptoFileBuffer(Socket s, string name, byte[] Salt, Func<object, object, int> CallbackFunction = null)
        {
            byte[] tmpbuf = GetCryptoBuffer(Salt);
            s.Send(BitConverter.GetBytes(CryptoSize));

            float percent = 0;
            int sendbytes = 0;
            int sendchunk = (int)Msg.FileDataChunk;
            while (sendbytes < CryptoSize)
            {
                if ((CryptoSize - sendbytes) < sendchunk)
                    sendchunk = (int)CryptoSize - sendbytes;
                sendbytes += s.Send(tmpbuf, sendbytes, sendchunk, SocketFlags.None);
                percent = 100.0f / (float)CryptoSize * (float)sendbytes;
                if (CallbackFunction != null)
                    CallbackFunction?.Invoke(name, (int)percent);
            }
            return sendbytes;
        }
    }
}
