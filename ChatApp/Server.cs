using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Def;

namespace ChatAppServer
{
    class Server
    {
        public List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();
        public List<ChatGroup> chatGroup = new List<ChatGroup>();
        public List<ServerFile> BinaryFiles = new List<ServerFile>();
        TcpListener serverSocket = null;
        public bool ShouldStop = false;
        public bool isRunning = false;
        Thread InCommingConnections = null;
        public void StartServer()
        {
            InCommingConnections = new Thread(ListenToIncommingConnections);
            InCommingConnections.Start();

            while (!ShouldStop)
            {
                for (int i = 0; i < ConnectedClients.Count(); i++)
                {
                    if (!ConnectedClients[i].IsRunning)
                    {
                        Thread t = new Thread(new ParameterizedThreadStart(recvMessage));
                        ConnectedClients[i].IsRunning = true;
                        t.Start(i);
                    }
                }
                isRunning = true;
                Thread.Sleep(1);
            }
            if (ConnectedClients.Count() > 0)
                foreach (ConnectedClient CC in ConnectedClients)
                {
                    CC.tcpClient.Client.Disconnect(false);
                }
            serverSocket.Stop();
            InCommingConnections.Abort();
        }

        private void ListenToIncommingConnections(object sender)
        {
            IPAddress localAddr = IPAddress.Parse(ComConstants.Ip);
            // TcpListener server = new TcpListener(port);
            serverSocket = new TcpListener(localAddr, ComConstants.DefaultPort);

            // Start listening for client requests.
            serverSocket.Start();
            Console.WriteLine("Server Started!");
            while (!ShouldStop)
            {
                try
                {
                    TcpClient ClientConnection = serverSocket.AcceptTcpClient();
                    if (ClientConnection != null)
                    {
                        byte[] buffer = new byte[(int)Msg.Size];
                        int recvbytes = 0;
                        recvbytes = ClientConnection.Client.Receive(buffer, (int)Msg.Size, SocketFlags.None);
                        if (BitConverter.ToUInt32(buffer, (int)Msg.ID) == (int)MsgType.ClientConnect)
                        {
                            ClientConnect CC = new ClientConnect();
                            CC = (ClientConnect)Utils.ByteArrayToObject(buffer);
                            Console.WriteLine("Client Connected from : " + CC.Ip);
                            ConnectedClient client = new ConnectedClient();
                            client.tcpClient = ClientConnection;
                            client.CC = new ClientConnect();
                            client.CC.sockettype = CC.sockettype;
                            client.CC.Ip = CC.Ip;

                            ConnectedClients.Add(client);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                Thread.Sleep(1);

            }
        }

        private void recvMessage(object s)
        {
            ConnectedClient client = ConnectedClients[(int)s];

            try
            {

                byte[] buffer = new byte[(int)Msg.Size];
                int recvbytes = 0;
                recvbytes = client.tcpClient.Client.Receive(buffer, (int)Msg.Size, SocketFlags.None);
                if (recvbytes > 0)
                {
                    #region RecvClientSettings
                    if (BitConverter.ToUInt32(buffer, (int)Msg.ID) == (int)MsgType.ClientSettings)
                    {
                        ClientSettings CSettings;
                        CSettings = (ClientSettings)Utils.ByteArrayToObject(buffer);
                        for (int i = 0; i < ConnectedClients.Count(); i++)
                        {
                            if (ConnectedClients[i].tcpClient.Client == client.tcpClient.Client)
                            {
                                ConnectedClients[i].Name = CSettings.Name;
                            }
                        }
                        client.Name = CSettings.Name;
                        client.Group = CSettings.Groups;
                        string[] tmpGroups = CSettings.Groups.Split(';');
                        foreach (string group in tmpGroups)
                        {
                            if (group != "")
                            {
                                bool InGroupList = false;
                                for (int i = 0; i < chatGroup.Count(); i++)
                                {
                                    if (group == chatGroup[i].GroupName)
                                    {
                                        InGroupList = true; // group is already in chatrgouplist,
                                                            // now we check if out player is already in that group
                                        bool PlayerAlreadyInGroup = false;
                                        for (int j = 0; j < chatGroup[i].Members.Count(); j++)
                                        {
                                            if (chatGroup[i].Members[j] == CSettings.Name)
                                            {
                                                PlayerAlreadyInGroup = true;
                                            }

                                        }
                                        if (!PlayerAlreadyInGroup)
                                        {
                                            chatGroup[i].Members.Add(CSettings.Name);
                                            Console.WriteLine(CSettings.Name + " added to group : " + chatGroup[i].GroupName);
                                        }

                                    }
                                }
                                if (!InGroupList)
                                {
                                    ChatGroup cGroup = new ChatGroup();
                                    cGroup.GroupName = group;
                                    cGroup.Members.Add(CSettings.Name);
                                    chatGroup.Add(cGroup);
                                    Console.WriteLine("Group " + group + " Added to List with And Added Member : " + CSettings.Name);
                                }
                            }
                        }

                        UpdateClientList();

                    }
                    #endregion
                    #region RecvTextMessage
                    if (BitConverter.ToInt32(buffer, (int)Msg.ID) == (int)MsgType.TextMessage)
                    {
                        TextMessage TM = new TextMessage();
                        TM = (TextMessage)Utils.ByteArrayToObject(buffer);
                        PassMessage(TM, buffer);

                    }
                    #endregion
                    #region FileRecv/Send
                    if (BitConverter.ToInt32(buffer, (int)Msg.ID) == (int)MsgType.PrepareSendFile)
                    {
                        PrepareFileSend PFS = new PrepareFileSend();
                        PFS = (PrepareFileSend)Utils.ByteArrayToObject(buffer);
                        PFS.ACK = 1;
                        Utils.ObjectToByteArray(PFS).CopyTo(buffer, 0);
                        BitConverter.GetBytes((int)MsgType.RequestFile).CopyTo(buffer, (int)Msg.ID);
                        client.tcpClient.Client.Send(buffer, (int)Msg.Size, SocketFlags.None); // Send Ready to recvfile.

                        buffer = new byte[PFS.filesize];
                        int chunk = (int)Msg.FileDataChunk;
                        recvbytes = 0;
                        int percent = 0;
                        while (recvbytes != buffer.Length)
                        {
                            recvbytes += client.tcpClient.Client.Receive(buffer, recvbytes, chunk, SocketFlags.None);
                            if (chunk > buffer.Length - recvbytes)
                                chunk = buffer.Length - recvbytes;
                            percent = (int)((100.0 / (float)buffer.Length) * (float)recvbytes);

                            SendFileRecvUpdate(client, percent);

                        }
                        SendFileRecvUpdate(client, 1000);

                        ServerFile SF = new ServerFile();
                        SF.Name = PFS.FileName;
                        SF.BinaryFile = new byte[PFS.filesize];
                        SF.size = PFS.filesize;
                        buffer.CopyTo(SF.BinaryFile, 0);
                        BinaryFiles.Add(SF);
                        // File added to BinaryList;
                        // now we need to notify clients that there is a file for them.
                        TextMessage TM = new TextMessage();
                        TM.To = PFS.to;
                        TM.From = PFS.from;
                        TM.ID = ChatBufferMessageType.Link;
                        TM.Message = PFS.FileName;
                        buffer = new byte[(int)Msg.Size];
                        BitConverter.GetBytes((int)MsgType.TextMessage).CopyTo(buffer, (int)Msg.ID);
                        Utils.ObjectToByteArray(TM).CopyTo(buffer, 0);
                        PassMessage(TM, buffer);
                    }

                    #endregion
                    #region File Request Awsering
                    if (BitConverter.ToInt32(buffer, (int)Msg.ID) == (int)MsgType.RequestFile)
                    {
                        RequestFile RF = new RequestFile();
                        RF = (RequestFile)Utils.ByteArrayToObject(buffer);
                        for (int i = 0; i < BinaryFiles.Count(); i++)
                        {
                            if (BinaryFiles[i].Name.Contains(RF.Name))
                            {
                                PrepareFileSend PFS = new PrepareFileSend();
                                PFS.filesize = BinaryFiles[i].size;
                                PFS.FileName = BinaryFiles[i].Name;
                                PFS.ACK = 0;
                                BitConverter.GetBytes((int)MsgType.PrepareSendFile).CopyTo(buffer, (int)Msg.ID);
                                Utils.ObjectToByteArray(PFS).CopyTo(buffer, 0);
                                client.tcpClient.Client.Send(buffer, (int)Msg.Size, SocketFlags.None);

                                recvbytes = 0;
                                recvbytes = client.tcpClient.Client.Receive(buffer, (int)Msg.Size, SocketFlags.None);
                                PFS = (PrepareFileSend)Utils.ByteArrayToObject(buffer);
                                if (PFS.ACK == 1)
                                {
                                    client.tcpClient.Client.Send(BinaryFiles[i].BinaryFile, (int)BinaryFiles[i].size, SocketFlags.None);
                                }
                            }
                        }
                    }
                    #endregion
                }
                if (recvbytes == 0)
                {
                    for (int i = 0; i < ConnectedClients.Count(); i++)
                    {
                        if (ConnectedClients[i].tcpClient.Client == client.tcpClient.Client)
                        {
                            ConnectedClients.RemoveAt(i);
                            break;
                        }
                    }
                    Console.WriteLine("Client Disconnected");
                }

                for (int i = 0; i < ConnectedClients.Count(); i++)
                {
                    if (client.Name == ConnectedClients[i].Name)
                        ConnectedClients[i].IsRunning = false;
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }

        }

        private void SendFileRecvUpdate(ConnectedClient client,int percent)
        {
            byte[] sendbuffer = new byte[(int)Msg.Size];
            BitConverter.GetBytes((int)MsgType.FileSendProgression).CopyTo(sendbuffer, (int)Msg.ID);
            SendProgression SP = new SendProgression();
            SP.PercentSend = percent;
            Console.WriteLine("File: " + percent + "%");
            Utils.ObjectToByteArray(SP).CopyTo(sendbuffer, 0);
            for (int i = 0; i < ConnectedClients.Count(); i++)
            {
                if (client.CC.Ip == ConnectedClients[i].CC.Ip) // Ip is same as otehr connection from same pc.
                    if (ConnectedClients[i].CC.sockettype == ConnectionType.Other) // sockettype isnt equel in both.
                        ConnectedClients[i].tcpClient.Client.Send(sendbuffer, (int)Msg.Size, SocketFlags.None);

            }
        }

        private void PassMessage(TextMessage TM, byte [] buffer)
        {
            bool GroupMessage = false;
            for (int i = 0; i < chatGroup.Count(); i++)
            {
                if (TM.To == chatGroup[i].GroupName) //message is for group;
                {
                    GroupMessage = true;
                    for (int j = 0; j < chatGroup[i].Members.Count(); j++)
                    {
                        for (int k = 0; k < ConnectedClients.Count(); k++)
                        {
                            if (chatGroup[i].Members[j] == ConnectedClients[k].Name)
                                ConnectedClients[k].tcpClient.Client.Send(buffer, (int)Msg.Size, SocketFlags.None);
                        }
                    }
                }
            }
            if (!GroupMessage)
            {
                for (int i = 0; i < ConnectedClients.Count(); i++)
                {
                    if (ConnectedClients[i].Name == TM.To || ConnectedClients[i].Name == TM.From)
                        ConnectedClients[i].tcpClient.Client.Send(buffer, (int)Msg.Size, SocketFlags.None);

                }
            }
        }

        private void UpdateClientList()
        {
            byte[] buffer = new byte[(int)Msg.Size];
            int offset = 0;
            BitConverter.GetBytes(chatGroup.Count()).CopyTo(buffer, offset); // write size of ChatgroupList to buffer
            offset += sizeof(int);
            foreach (ChatGroup CG in chatGroup)
            {
                Encoding.ASCII.GetBytes(CG.GroupName).CopyTo(buffer, offset); // write groupname to buffer.
                offset += 50;
                BitConverter.GetBytes(CG.Members.Count()).CopyTo(buffer, offset); // write number of members to buffer.
                offset += sizeof(int);
                foreach (string member in CG.Members)
                {
                    Encoding.ASCII.GetBytes(member).CopyTo(buffer, offset);
                    offset += 50;
                }
            }
            BitConverter.GetBytes(ConnectedClients.Count()).CopyTo(buffer, offset); // write number off connected clients to buffer.
            offset += sizeof(int);
            foreach (ConnectedClient CC in ConnectedClients)
            {
                Utils.ObjectToByteArray(CC).CopyTo(buffer, offset);
                offset += 200;
            }

            BitConverter.GetBytes((int)MsgType.UpdateLists).CopyTo(buffer, (int)Msg.ID);
            foreach (ConnectedClient CC in ConnectedClients)
            {
                CC.tcpClient.Client.Send(buffer, (int)Msg.Size, SocketFlags.None);
            }
        }
    }
}
