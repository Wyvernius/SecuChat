using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using SharedClasses;
using System.Drawing;

namespace Server
{
    public partial class ServerIcon : Form
    {
        List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();
       // List<ChatGroup> chatGroups = new List<ChatGroup>();
        TcpListener serverSocket = null;
        public static bool ShouldStop = false;
        public static bool isRunning = false;
        Thread InCommingConnections = null;
        Thread MainLoop = null;
        // Key is only for this instance.
        private static byte[] BinaryFilesKey = new byte[0xFF] { 207, 99, 25, 232, 190, 182, 86, 114, 121, 140, 95, 45, 152, 49, 251, 92, 16, 234, 80, 112, 36, 87, 214, 202, 240, 49, 2, 227, 140, 232, 69, 57, 62, 188, 195, 45, 174, 35, 198, 182, 125, 147, 116, 75, 32, 43, 173, 171, 141, 108, 29, 243, 5, 29, 202, 91, 116, 250, 82, 114, 227, 25, 81, 46, 191, 56, 235, 178, 171, 68, 145, 174, 248, 188, 89, 170, 152, 76, 40, 135, 78, 58, 246, 135, 13, 17, 184, 29, 104, 222, 87, 250, 104, 232, 65, 2, 179, 105, 168, 230, 219, 241, 170, 64, 77, 247, 25, 229, 79, 224, 90, 133, 234, 194, 159, 26, 156, 175, 162, 198, 86, 192, 16, 140, 249, 25, 187, 183, 14, 189, 20, 43, 95, 240, 117, 84, 137, 135, 9, 226, 201, 131, 22, 72, 4, 42, 189, 113, 7, 231, 89, 76, 125, 54, 7, 240, 252, 5, 117, 7, 29, 220, 153, 115, 113, 223, 203, 179, 239, 141, 200, 126, 189, 107, 41, 237, 37, 133, 221, 134, 139, 100, 121, 138, 225, 213, 67, 81, 201, 78, 130, 61, 171, 65, 79, 17, 91, 65, 145, 37, 28, 144, 107, 119, 199, 64, 169, 247, 63, 173, 85, 228, 153, 18, 182, 209, 82, 244, 120, 208, 72, 0, 183, 62, 220, 207, 211, 181, 40, 248, 87, 124, 158, 164, 30, 48, 100, 238, 155, 170, 89, 248, 253, 147, 255, 229, 5, 54, 179, 233, 102, 189, 23, 55, 233 };
        private static string BinaryFilesPassword = "Default";

        public ServerIcon()
        {
            InitializeComponent();
        }

        private void ServerIcon_Load(object sender, EventArgs e)
        {
            ShouldStop = false;
            InCommingConnections = new Thread(ListenToIncommingConnections);
            InCommingConnections.Start();
        }

        private void mainloop(object sender)
        {
            while (!ShouldStop)
            {
                Thread.Sleep(1);
                for (int i = 0; i < ConnectedClients.Count(); i++)
                {
                    try
                    {
                        if (!ConnectedClients[i].IsRunning && ConnectedClients[i].Online && ConnectedClients[i].socket != null)
                        {
                            Thread t = new Thread(new ParameterizedThreadStart(recvMessage));
                            ConnectedClient cc = ConnectedClients[i];
                            ConnectedClients[i].IsRunning = true;
                            t.Start(cc);
                        }
                    }
                    catch (Exception e)
                    {
                       // Console.WriteLine("Failed to start Thread for Client {0} , {1} , /r/n {2}", ConnectedClients[i].CC.Ip, ConnectedClients[i].CC.sockettype.ToString(), e.Message.ToString());
                    }
                }
                isRunning = true;
            }
            try
            {
                for (int i = 0; i < ConnectedClients.Count(); i++)
                {
                    if (ConnectedClients[i].socket.Connected)
                    {
                        ConnectedClients[i].socket.Shutdown(SocketShutdown.Both);
                        ConnectedClients[i].socket.Close(5);
                    }
                    Console.WriteLine("Server: Disconnected " + i);
                }
                ConnectedClients.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + "\r\n " + ex.StackTrace);
            }
            InCommingConnections.Abort();
            serverSocket.Stop();
            isRunning = false;
        }

        private void ListenToIncommingConnections(object sender)
        {
            ComConstants.Ip = Utils.GetIp();
            IPAddress localAddr = IPAddress.Parse(ComConstants.Ip);

            serverSocket = new TcpListener(IPAddress.Any, ComConstants.DefaultPort);
            // Start listening for client requests.
            serverSocket.Start(50);
            Console.WriteLine("Server Started!");
 
            serverSocket.BeginAcceptSocket(new AsyncCallback(OnConnectRequest), serverSocket);
        }

        public void OnConnectRequest(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            try
            {
                Socket ClientConnection = listener.EndAcceptSocket(ar);
                listener.BeginAcceptSocket(new AsyncCallback(OnConnectRequest), listener);
                try
                {
                    nBuffer buffer = new nBuffer();
                    buffer.ReceiveBuffer(ClientConnection);
                    if (buffer.GetID() == MsgType.ClientConnect)
                    {
                        ClientConnect CC = buffer.GetObject<ClientConnect>();
                        ConnectedClient client = new ConnectedClient();
                        client.socket = ClientConnection;
                        client.CC = new ClientConnect();
                        client.CC.sockettype = CC.sockettype;
                        client.CC.Ip = CC.Ip;
                        client.CC.Salt = CC.Salt;

                        // Only add to clientlist if NOT FileSocket or LogoSocket
                        client.Online = false;
                        for (int i = 0; i < ConnectedClients.Count; i++)
                        {
                            if (ConnectedClients[i].CC.Ip == CC.Ip) // Second connection from same Ip should be discarded
                            {
                                if (CC.sockettype == ConnectionType.Client && ConnectedClients[i].CC.sockettype == ConnectionType.Client)
                                {
                                    client.Online = true;
                                    ConnectedClients[i] = client;
                                    Console.WriteLine("Client " + client.socket.RemoteEndPoint.ToString() + " of Type " + CC.sockettype.ToString() + ", Reconnected");
                                }
                            }
                        }
                        if (!client.Online)
                        {
                            ConnectedClients.Add(client);
                            client.Online = true;
                            Console.WriteLine("Client " + client.socket.RemoteEndPoint.ToString() + " of Type " + CC.sockettype.ToString() + " joined");
                        }
                        client.StartThread(recvMessage);
                    }

                    Console.WriteLine("Ready to receive next Incomming connection");
                }
                catch (Exception e)
                {
                    ClientConnection.Shutdown(SocketShutdown.Both);
                    ClientConnection.Close(1);
                    listener.BeginAcceptSocket(new AsyncCallback(OnConnectRequest), listener);
                    
                    Console.WriteLine("Client " + ClientConnection.RemoteEndPoint + ", timed-out!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + "\r\n " + e.Message);
            }
        }

        //every client runs on its own thread.
        //File send and receives also runs on its own thread. this is to keep GUI responsive.
        private void recvMessage(object s)
        {
            ConnectedClient client = (ConnectedClient)s;
            bool bRecvMessage = true; 
            while (bRecvMessage)
            {
                try
                {
                    nBuffer buffer = new nBuffer();
                    int ReadBytes;
                    if ((ReadBytes = buffer.ReceiveCryptoBuffer(client.socket, client.CC.Salt)) > 0)
                    {
                        #region RecvClientSettings
                        if (buffer.GetID() == MsgType.ClientSettings)
                        {
                            Console.WriteLine(client.socket.RemoteEndPoint.ToString() + ": Received Client settings");
                            ClientSettings CSettings;

                            CSettings = buffer.GetObject<ClientSettings>();
                            CSettings.Ip = client.CC.Ip;

                            bool InGroupList = false;

                            for (int i = 0; i < ConnectedClients.Count(); i++)
                            {
                                if (ConnectedClients[i].CC.Ip == client.CC.Ip)
                                {
                                    Console.WriteLine(ConnectedClients[i].Name + " Renamed to: " + CSettings.Name);
                                    ConnectedClients[i].Name = CSettings.Name;
                                }
                            }
                            client.Groups = CSettings.Groups;
                            string[] tmpGroups = CSettings.Groups.Split(';');
                            foreach (string group in tmpGroups)
                            {
                                if (group != "")
                                {
                                    InGroupList = false;
                                    for (int i = 0; i < ChatGroupsContainer.Instance.Count; i++)
                                    {
                                        if (group == ChatGroupsContainer.Instance.at(i).GroupName)
                                        {
                                            InGroupList = true; // group is already in chatrgrouplist,
                                        }
                                    }
                                    if (!InGroupList)
                                    {
                                        ChatGroup cGroup = new ChatGroup();
                                        cGroup.GroupName = group;
                                        ChatGroupsContainer.Instance.Add(cGroup);
                                        AddGroupToDataGridview(cGroup);
                                        Console.WriteLine("Group " + group + " Added to List");
                                    }
                                    // check if user is in group, else add it, and check if name updated;
                                    for (int i = 0; i < ChatGroupsContainer.Instance.Count; i++)
                                    {
                                        bool UserInList = false;
                                        if (ChatGroupsContainer.Instance.at(i).GroupName == group)
                                        {
                                            for (int j = 0; j < ChatGroupsContainer.Instance.at(i).Members.Count; j++)
                                            {
                                                if (ChatGroupsContainer.Instance.at(i).Members[j].Ip == CSettings.Ip) // Member is in list
                                                {
                                                    UserInList = true;
                                                    ChatGroupsContainer.Instance.at(i).Members[j].Name = CSettings.Name; // update name.
                                                }
                                            }
                                            if (!UserInList)
                                            {
                                                ChatGroupsContainer.Instance.at(i).Members.Add(CSettings);
                                                Console.WriteLine(CSettings.Name + " Added to: " + ChatGroupsContainer.Instance.at(i).GroupName);
                                            }
                                        }
                                    }
                                }
                            }

                            // check all groups if it has the user, if it does, check if the user has specified the group, else delete it from that group
                            for (int j = 0; j < ChatGroupsContainer.Instance.Count;j++)
                            {
                                bool UserInGroup = false;
                                bool UserSpecifiedGroup = false;
                                int IndexOfUser = -1;
                                for (int i = 0; i < ChatGroupsContainer.Instance.WholeList[j].Members.Count; i++)
                                {
                                    if (ChatGroupsContainer.Instance.at(j).Members[i].Ip == CSettings.Ip) // client is in the group
                                    {
                                        UserInGroup = true;
                                        foreach(string Cgroups in tmpGroups) // check if groupname is also specified by teh client;
                                        {
                                            if (Cgroups == ChatGroupsContainer.Instance.at(j).GroupName)
                                            {
                                                UserSpecifiedGroup = true;
                                                
                                            }
                                        }
                                        if (!UserSpecifiedGroup)
                                            IndexOfUser = i;
                                    }
                                }
                                if (UserInGroup && !UserSpecifiedGroup) // user is in chatgroup but not specified in tmpgroups.
                                {
                                    ChatGroupsContainer.Instance.at(j).Members.RemoveAt(IndexOfUser);
                                }
                            }
                            UpdateClientList();
                        }
                        #endregion

                        #region RecvTextMessage

                        if (buffer.GetID() == MsgType.TextMessage)
                        {
                            Console.WriteLine(client.socket.RemoteEndPoint.ToString() + ": Received TextMessage");
                            TextMessage TM = buffer.GetObject<TextMessage>();
                            PassMessage(TM);
                            if (LogChatMessages.Checked)
                                MessageLogger.LOG(TM,ChatGroupsContainer.Instance.WholeList);
                        }
                        #endregion

                        #region FileRecv/Send
                        if (buffer.GetID() == MsgType.SendFile) // Server Receives file
                        {
                            PrepareFileSend PFS = buffer.GetObject<PrepareFileSend>();
                            Console.WriteLine(client.socket.RemoteEndPoint.ToString() + ": Send File -> " + PFS.FileName + PFS.Ext);
                            buffer.innerbuffer.Clear();
                            buffer.ReceiveCryptoFileBuffer(client.socket, client.CC.Salt);
                            ServerFile SF = buffer.GetObject<ServerFile>(0);
                            // Encrypt current password into file;
                            byte[] Passwordbytes = Crypt.Base64Encode(BinaryFilesPassword);
                            byte[] binaryfile = new byte[Passwordbytes.Length + SF.BinaryFile.Length];
                            Buffer.BlockCopy(Passwordbytes, 0, binaryfile, 0, Passwordbytes.Length);
                            Buffer.BlockCopy(SF.BinaryFile, 0, binaryfile, Passwordbytes.Length, SF.BinaryFile.Length); // copy binary file;

                            binaryfile = Crypt.Encrypt(binaryfile, BinaryFilesKey, BinaryFilesPassword); // encrypt file and store it.

                            string Path = Utils.AssemblyDirectory + "\\" + PFS.FileName + PFS.Ext + ".scbd";
                            FileStream fs = null;
                            if (File.Exists(Path))
                                fs = new FileStream(Path, FileMode.Truncate, FileAccess.ReadWrite); // overwrite file
                            else
                                fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.ReadWrite); // create new;
                            if (fs.CanWrite)
                                fs.Write(binaryfile, 0, binaryfile.Length);
                            fs.Close();
                            fs.Dispose();
                            SF.length = binaryfile.Length;
                            SF.Ext = PFS.Ext;
                            binaryfile = null;

                            // now we need to notify clients that there is a file for them.
                            TextMessage TM = new TextMessage();
                            TM.To = PFS.to;
                            TM.From = PFS.from;
                            TM.ID = ChatBufferMessageType.Link;
                            TM.Message = PFS.FileName + PFS.Ext;
                            PassMessage(TM);

                            buffer.innerbuffer.Clear();
                            buffer.AddID(MsgType.FileSendComplete);
                            FileSendComplete FSC = new FileSendComplete();
                            FSC.complete = true;
                            buffer.AddObject(FSC);
                            buffer.SendCryptoBuffer(client.socket, client.CC.Salt);
                        }

                        #endregion

                        #region File Request Awsering
                        if (buffer.GetID() == MsgType.RequestFile) // Client requests file from server
                        {

                            bool NoFile = true;
                            RequestFile RF = buffer.GetObject<RequestFile>();
                            Console.WriteLine(client.socket.RemoteEndPoint.ToString() + ": Request File -> " + RF.Name);
                            buffer.innerbuffer.Clear();

                            byte[] binaryfile = new byte[] { };
                            string Path = Utils.AssemblyDirectory + "\\" + RF.Name + ".scbd";
                            if (File.Exists(Path))
                            {
                                binaryfile = Crypt.Decrypt(File.ReadAllBytes(Path), BinaryFilesKey, BinaryFilesPassword);
                                byte[] passwordbytes = Crypt.Base64Encode(BinaryFilesPassword); // get bytes of password
                                byte[] binarypasswordfromfile = new byte[passwordbytes.Length]; // allocate bytes
                                try
                                {
                                    // copy length of of current password from binrayfile on disk;
                                    Buffer.BlockCopy(binaryfile, 0, binarypasswordfromfile, 0, binarypasswordfromfile.Length); 
                                    string DecodedBinaryPassword = Crypt.Base64Decode(binarypasswordfromfile); // decode password
                                    if (DecodedBinaryPassword == BinaryFilesPassword) // password from file is the same as current password;
                                    {
                                        Buffer.BlockCopy(binaryfile, passwordbytes.Length, binaryfile, 0, binaryfile.Length - passwordbytes.Length);
                                    }
                                    else // password was longer/smaller or same length but different chars.
                                        Console.WriteLine("Encrypted password does not match current password, file could not be send to user : " + client.CC.Ip);
                                }
                                catch (Exception ex)
                                {
                                    NoFile = true;
                                    Console.WriteLine("Failed to Decrypt file from disk");
                                    Console.WriteLine(ex.Message + "/n/r " + ex.StackTrace);

                                }
                                NoFile = false;

                                PrepareFileSend PFS = new PrepareFileSend();
                                PFS.filesize = binaryfile.Length;
                                PFS.FileName = RF.Name;
                                PFS.ACK = 0;
                                buffer.AddID(MsgType.SendFile);
                                buffer.AddObject(PFS);
                                buffer.SendCryptoBuffer(client.socket, client.CC.Salt);
                                buffer.innerbuffer.Clear();
                                buffer.CryptoSize = 0;

                                buffer.ReceiveCryptoBuffer(client.socket, client.CC.Salt);
                                if (buffer.GetID() == MsgType.SendOK)
                                {
                                    buffer.innerbuffer.Clear();
                                    ServerFile serverfile = new ServerFile();
                                    string[] splitname = RF.Name.Split('.');
                                    serverfile.Name = splitname[0]; // name
                                    serverfile.Ext = splitname[1]; // extension
                                    serverfile.BinaryFile = binaryfile; // decrypt file to send it.
                                    binaryfile = null;
                                    buffer.AddObject(serverfile);
                                    Console.WriteLine("Server : " + buffer.SendCryptoBuffer(client.socket, client.CC.Salt) + " bytess  of " + RF.Name + " send!");
                                    buffer.innerbuffer.Clear();
                                }
                            }
                            if (NoFile)
                            {
                                PrepareFileSend PFS = new PrepareFileSend();
                                PFS.filesize = -1;
                                PFS.FileName = RF.Name;
                                PFS.ACK = 0;
                                buffer.AddID(MsgType.Nofile);
                                buffer.AddObject(PFS);
                                buffer.SendCryptoBuffer(client.socket, client.CC.Salt);
                                buffer.innerbuffer.Clear();
                                Thread.Sleep(10);
                            }
                        }
                        #endregion
                        #region Reguest Logo
                        if (buffer.GetID() == MsgType.RequestLogo)
                        {
                            if (Logo == null)
                            {
                                buffer.innerbuffer.Clear();
                                buffer.AddID(MsgType.NoLogo);
                                buffer.SendCryptoBuffer(client.socket, client.CC.Salt);
                            }
                            else
                            {
                                buffer.innerbuffer.Clear();
                                buffer.AddID(MsgType.Logo);

                                buffer.AddObject(Logo);
                                buffer.SendCryptoBuffer(client.socket, client.CC.Salt);
                            }
                        }
                        #endregion
                    }
                    else 
                    {
                        for (int i = 0; i < ConnectedClients.Count(); i++)
                        {
                            if (ConnectedClients[i].socket == client.socket)
                            {
                                if (client.CC.Ip == ConnectedClients[i].CC.Ip && client.CC.sockettype != ConnectionType.Client) // or delete if FileSocket.
                                {
                                    ConnectedClients[i].socket.Shutdown(SocketShutdown.Both);
                                    ConnectedClients[i].socket.Close(1);
                                    ConnectedClients.RemoveAt(i);
                                    Console.WriteLine("Socket of Type {0} Disconnected!",client.CC.sockettype);
                                    bRecvMessage = false;
                                    return;
                                }

                                client.socket.Shutdown(SocketShutdown.Both);
                                client.socket.Close(1);
                                client.socket = null;
                                client.Online = false;
                                bRecvMessage = false;

                                Console.WriteLine("Client {0} of Type {1} Disconnected!", client.CC.Ip,client.CC.sockettype);
                                UpdateClientList();
                                return;
                            }
                        }
                      //  if (ConnectedClients[index].CC.sockettype != ConnectionType.File) // only update clientlist if socket is not a filesocket.
                     //       UpdateClientList();
                    }
                }
                catch (Exception e)
                {
                    for (int i = 0; i < ConnectedClients.Count(); i++)
                    {
                        if (client.socket == ConnectedClients[i].socket)
                        {
                            Console.WriteLine(ConnectedClients[i].CC.Ip + "   " + e.Message + "\r\n Disconnected");
                            if (i < ConnectedClients.Count())
                            {
                                /*
                                ConnectedClients[i].Online = false;
                                ConnectedClients[i].socket = null;
                                ConnectedClients[i].IsRunning = false;
                                */
                                client.socket.Shutdown(SocketShutdown.Both);
                                client.socket.Close(1);
                                client.socket = null;
                                client.Online = false;
                                bRecvMessage = false;
                            }
                        }
                    }
                }
            }

        }

        private void PassMessage(TextMessage TM)
        {
            nBuffer buffer = new nBuffer();
            buffer.AddID(MsgType.TextMessage);
            buffer.AddObject(TM);
            bool GroupMessage = false;
            for (int i = 0; i < ChatGroupsContainer.Instance.Count; i++)
            {
                if (TM.To == ChatGroupsContainer.Instance.at(i).GroupName) //message is for group;
                {
                    GroupMessage = true;
                    bool Ingroup = false;
                    for (int k = 0; k < ConnectedClients.Count(); k++)
                    {
                        for (int j = 0; j < ChatGroupsContainer.Instance.at(i).Members.Count(); j++) // loop member of grou against ConnectedClient
                        {
                            if (ChatGroupsContainer.Instance.at(i).Members[j].Ip == ConnectedClients[k].CC.Ip && ConnectedClients[k].Online) // if Client is in group;
                            {
                                Ingroup = true;
                                buffer.SendCryptoBuffer(ConnectedClients[k].socket, ConnectedClients[k].CC.Salt);
                            }
                        }
                        if (ChatGroupsContainer.Instance.at(i).Policy == GroupAuth.Open && !Ingroup && ConnectedClients[k].Online) // if client not in group but group is open.
                            buffer.SendCryptoBuffer(ConnectedClients[k].socket, ConnectedClients[k].CC.Salt);
                    }
                }
            }
            if (!GroupMessage)
            {
                for (int i = 0; i < ConnectedClients.Count(); i++)
                {
                    if (ConnectedClients[i].Name == TM.To || ConnectedClients[i].Name == TM.From)
                    {
                        if (ConnectedClients[i].socket != null)
                            buffer.SendCryptoBuffer(ConnectedClients[i].socket, ConnectedClients[i].CC.Salt);
                    }

                }
            }
        }

        private void UpdateClientList()
        {
            nBuffer buffer = new nBuffer();
            buffer.AddID(MsgType.UpdateLists);
            buffer.AddInt(ChatGroupsContainer.Instance.Count);

            foreach (ChatGroup CG in ChatGroupsContainer.Instance.WholeList)
            {
                buffer.AddObject(CG);
                buffer.AddInt(CG.Members.Count); // Add MemberCount;
                foreach (ClientSettings CS in CG.Members)
                {
                    buffer.AddObject(CS); // Add every Memeber;
                }
            }
            buffer.AddInt(ConnectedClients.Count);
            foreach (ConnectedClient CC in ConnectedClients)
            {
                if (CC.CC.sockettype == ConnectionType.Client)
                    buffer.AddObject(CC);
            }
            foreach (ConnectedClient CC in ConnectedClients)
            {
                if (CC.socket != null && CC.CC.sockettype == ConnectionType.Client)
                {
                    buffer.SendCryptoBuffer(CC.socket, CC.CC.Salt);
                }
            }
        }

        private void ServerNotifyMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            Application.ApplicationExit += Application_ApplicationExit;
            Application.Exit();
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            ShouldStop = true;
        }

        private void passwordsetbutton_Click(object sender, EventArgs e)
        {
            BinaryFilesPassword = Fileencryptionpasswordtextbox.Text;
        }

        private void PublicGroupChkBx_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ChatGroup group in ChatGroupsContainer.Instance.WholeList)
            {
                if (PublicGroupChkBx.Checked)
                    group.Policy = GroupAuth.Open;
                else
                    group.Policy = GroupAuth.Closed;
            }
            UpdateClientList();
        }

        private void AddGroupToDataGridview(ChatGroup CG)
        {
            Invoke((MethodInvoker)delegate
            {
                bool InGridView = false;
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if ((string)row.Cells[0].Value == CG.GroupName)
                    {
                        InGridView = true;
                        break;
                    }
                }
                if (!InGridView)
                {
                    dataGridView1.Rows.Add(new object[] { CG.GroupName,
                    (CG.Policy == GroupAuth.Open) ? 1 : 0 });
                }
            });
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                DataGridViewCellCollection cells = dataGridView1.Rows[e.RowIndex].Cells;
                DataGridViewCheckBoxCell chk = (DataGridViewCheckBoxCell)cells[1];
                foreach (ChatGroup group in ChatGroupsContainer.Instance.WholeList)
                {
                    if (group.GroupName == (string)cells[0].Value)
                    {
                        if (chk.Value == chk.TrueValue)
                        {
                            group.Policy = GroupAuth.Open;
                        }
                        if (chk.Value == chk.FalseValue)
                        {
                            group.Policy = GroupAuth.Closed;
                        }
                    }
                }
                UpdateClientList();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        bool Testtoggle = false;
        Thread TestThread = null;
        private void ServerTestButton_Click(object sender, EventArgs e)
        {
            Testtoggle = !Testtoggle;
            if (Testtoggle)
            {
                ServerTestButton.Text = "Running!";
                TestThread = new Thread(new ThreadStart(TestFunc));
                TestThread.Start();
            }
            else
                ServerTestButton.Text = "StressTest!";
        }

        private void TestFunc()
        {
            while (Testtoggle)
            {
                nBuffer buffer = new nBuffer();
                buffer.AddID(MsgType.ServerTest);
                foreach (ConnectedClient client in ConnectedClients)
                {
                    buffer.SendCryptoBuffer(client.socket, client.CC.Salt);
                    Thread.Sleep(250);
                }
                Console.WriteLine("Test Send to clients");
            }
        }

        public static Bitmap Logo = null;
        private void ServerPictureBox_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select Picture";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                ServerPictureBox.ImageLocation = fileDialog.FileName;
                Logo = new Bitmap(fileDialog.FileName);
            }
        }
    }
}
