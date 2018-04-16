using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SharedClasses
{
    public class ComConstants
    {
        public static int DefaultPort = 20011;
        public static String Ip = "";
    }

    #region Networkable Classes
    [Serializable]
    public class ClientConnect 
    {
        public ConnectionType sockettype;
        public string Ip;
        public byte[] Salt = new byte[0xFF];
    }

    [Serializable]
    public class ConnectedClient
    {
        public ClientConnect CC;
        [NonSerialized]
        public Socket socket;
        public string Name;
        public string Groups;
        public bool Online = false;
        public bool IsRunning = false;
        [NonSerialized]
        public Thread thread = null;
        public object function;

        public virtual void StartThread(Action<object> func)
        {
            thread = new Thread(new ParameterizedThreadStart(func));
            thread.Start(this);
        }
    }

    [Serializable]
    public class ChatGroup
    {
        public string GroupName;
        public GroupAuth Policy;
        [NonSerialized]
        public List<ClientSettings> Members = new List<ClientSettings>();

        public ChatGroup()
        {
            Policy = GroupAuth.Closed;
        }
    }

    [Serializable]
    public class ClientSettings
    {
        public string Name;
        public string Ip;
        public string Groups;

        public List<string> GetGroups
        {
            get
            {
                return Groups.Split(';').ToList();
            }
        }
    }

    [Serializable]
    public class TextMessage
    {
        public string From;
        public string To;
        public string Message;
        public ChatBufferMessageType ID;
    }

    [Serializable]
    public class SendProgression
    {
        public int PercentSend;
    }

    [Serializable]
    public class PrepareFileSend
    {
        public string FileName;
        public string Ext;
        public string from;
        public string to;
        public long filesize;
        public byte ACK;
    }

    [Serializable]
    public class ServerFile
    {
        public byte[] BinaryFile;
        public int length;
        public string Name;
        public string Ext;
    }

    public class FileToSend :  ServerFile
    {
        bool Uploaded = false;
    }

    [Serializable]
    public class RequestFile
    {
        public string Name;
    }

    [Serializable]
    public class FileSendComplete
    {
        public bool complete;
    }

    #endregion

    #region Enums
    public enum ConnectionType
    {
        File = 5001,
        Client = 5002,
    }

    public enum GroupAuth
    {
        Closed = 0,
        Open = 1,
    }

    public enum MsgType : UInt32
    {
        ClientConnect = 4000,
        ClientSettings = 4001,
        UpdateLists = 4002,
        TextMessage = 4003,
        SendFile = 4004,
        RequestFile = 4005,
        FileSendProgression = 4006,
        Nofile = 4007,
        FileSendComplete = 4008,
        ConnectionEstablished = 4009,
        SendOK = 4010,
        SendCansel = 4011,
        ServerTest = 4012,
        None = 0,
    }

    public enum Msg
    {
        Size = 1024,
        SizeEncrypt = Size + 16,
        BeginMessage = 0,
        ID = Size - 4,
        FileDataChunk = 1048576, // 1mb chunk
    }

    public enum ChatBufferMessageType
    {
        Text = 0x1,
        Link = 0x2, // File
    }

    #endregion

    public class ChatGroupsContainer
    {
        private static List<ChatGroup> chatGroups = new List<ChatGroup>();
        private static volatile ChatGroupsContainer instance;
        private static object syncRoot = new Object();

        private ChatGroupsContainer() { }

        public static ChatGroupsContainer Instance
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                if (instance == null)
                {
                    if (instance == null)
                        instance = new ChatGroupsContainer();
                }
                return instance;
            }
        }

        public void Add(ChatGroup group)
        {
            chatGroups.Add(group);
        }

        public int Count
        {
            get
            {
                return chatGroups.Count;
            }
        }

        public ChatGroup at(int i)
        {
            return chatGroups[i];
        }

        public List<ChatGroup> WholeList
        {
            get
            {
                return chatGroups;
            }
        }
    }
}