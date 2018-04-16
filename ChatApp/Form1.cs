using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
using SharedClasses;

namespace SecuChat
{
    public partial class Form1 : Form
    {
        public static Socket ServerSocket = null;
        public static EndPoint RemoteEndPoint = null;
        Random random = new Random();
        List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();
        List<ChatGroup> chatGroups = new List<ChatGroup>();
        ClientSettings cSettings = new ClientSettings();
        List<SocketAsyncEventArgs> SocketList = new List<SocketAsyncEventArgs>();
        List<ChatBuffer> ChatBufferList = new List<ChatBuffer>();
        List<FlowLayoutPanel> ChatBufferPanels = new List<FlowLayoutPanel>();
        List<byte[]> filebuffer = new List<byte[]>();
        ChatBufferID ActiveWindow;
        bool shouldstop = false;
        byte[] Salt = new byte[Crypt.SaltLength];
        bool GlobalQuit = false;
        string SettingsPassword = "";
        bool SettingsLocked = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(StartUpConnection);
        }

        private void StartUpConnection(object state)
        {
            Invoke((MethodInvoker)delegate
            {
                UserInputTxtBx.Enabled = false;
            });

            while (!ConnectToServer())
            {
                if (shouldstop)
                    return;
                // Start Server;
                Console.WriteLine("Failed to Connect To Server!");
            }
            Invoke((MethodInvoker)delegate
            {
                UserInputTxtBx.Enabled = true;
            });
            Console.WriteLine("Connected To Server!");
            IniFile ini = new IniFile("SecuChat.ini");
            Invoke((MethodInvoker)delegate
            {
                SettingGroupTxtbx.Text = ini.Read("Groups", "BASE", "");
                if (SettingGroupTxtbx.Text == "")
                    SettingGroupTxtbx.Text = "Global";
                settingNameTxtbx.Text = ini.Read("Name", "BASE", "");
                SettingsPassword = ini.Read("SettingsPassword", "BASE", "");
                if (SettingsPassword == "")
                    SettingsLocked = false;
                else
                    SettingsLocked = true;
                LockSettingsChkBX.Enabled = false;
                LockSettingsChkBX.Checked = SettingsLocked;
                LockSettingsChkBX.Enabled = true;
            });
            ActiveWindow = new ChatBufferID();

            ThreadPool.QueueUserWorkItem(recvMessages);
            ThreadPool.QueueUserWorkItem(CheckIfLogoAvailable);
        }

        private object Lock = new object();
        #region ServerConnection
        private bool ConnectToServer()
        {
            ComConstants.Ip = Utils.GetIp();
            String[] Ip = ComConstants.Ip.Split('.');
            for (int i = 0; i < 0xFF && ServerSocket == null; i++)
            {
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                String MyIp = Ip[0] + "." + Ip[1] + "." + Ip[2] + "." + i;
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                e.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(MyIp), ComConstants.DefaultPort);
                e.UserToken = socket;
                e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
                socket.ConnectAsync(e);
                SocketList.Add(e);
                Thread.Sleep(1);
            }
            Thread.Sleep(1000);
            if (ServerSocket == null)
            {
                ClearSocketList();
                return false;
            }
            else
            {
                ClearSocketList();
                return true;
            }
        }

        private void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket tmpsock = ((Socket)e.UserToken);
            if (tmpsock.Connected && ServerSocket == null)
            {
                RemoteEndPoint = tmpsock.RemoteEndPoint;
                Console.WriteLine("Connected to server : " + tmpsock.RemoteEndPoint.ToString());
                nBuffer buffer = new nBuffer();
                buffer.AddID(MsgType.ClientConnect);
                ClientConnect CC = new ClientConnect();
                CC.Ip = ComConstants.Ip;
                CC.sockettype = ConnectionType.Client;
                CC.Salt = Crypt.GetSalt();
                Salt = CC.Salt;
                buffer.AddObject(CC);
                buffer.SendBuffer(tmpsock);
                cSettings.Ip = ComConstants.Ip;
                ServerSocket = tmpsock;
            }
        }

        private void ClearSocketList()
        {
            foreach (SocketAsyncEventArgs arg in SocketList)
            {
                Socket sock = ((Socket)arg.UserToken);
                if (!sock.Connected)
                {
                    sock.Close();
                    arg.Dispose();
                }
            }
            SocketList.Clear();
        }

        private void ToggleUserTxtBx(bool value)
        {
            Invoke((MethodInvoker)delegate
           {
               UserInputTxtBx.Enabled = value;
           });
        }

        private void reconnect(object state)
        {
            ToggleUserTxtBx(false);
            ServerStatus.ImageIndex = 1;
            foreach (ConnectedClient user in ConnectedClients)
            {
                user.Online = false;
            }
            UpdateListView();
            if (ServerSocket != null)
            {
                ServerSocket.Shutdown(SocketShutdown.Both);
                ServerSocket.Close(1);
                ServerSocket = null;
            }
            while (!ConnectToServer() && !GlobalQuit)
            {
                // Start Server;
                Console.WriteLine("Failed to Connect To Server!");
            }
            ToggleUserTxtBx(true);
            Console.WriteLine("Connected To Server!");
            Thread.Sleep(1000);
            // since the server restarted we need to send our credentials again.
            if (ServerSocket != null)
                SettingsSaveButton_Click(null, null);
        }
        #endregion

        #region recvMessages

        private void recvMessages(object sender)
        {
            while (!GlobalQuit)
            {
                try
                {
                    if (ServerSocket != null)
                    {
                        ServerStatus.ImageIndex = 0;
                        nBuffer buffer = new nBuffer();
                        if (buffer.ReceiveCryptoBuffer(ServerSocket, Salt) > 0)
                        {
                            #region Update Lists
                            if (buffer.GetID() == MsgType.UpdateLists)
                            {
                                ConnectedClients.Clear();
                                chatGroups.Clear();
                                int size = buffer.GetInt(4); // get number of chatgroups;
                                for (int i = 0; i < size; i++)
                                {
                                    ChatGroup CG = buffer.GetNextObject<ChatGroup>();
                                    CG.Members = new List<ClientSettings>();
                                    int membercount = buffer.GetInt(buffer.NextObjectIndex);
                                    for (int j = 0; j < membercount; j++)
                                    {
                                        ClientSettings CS = buffer.GetNextObject<ClientSettings>();
                                        if (CS.Name != "" && CS.Name != null)
                                            CG.Members.Add(CS);
                                    }
                                    chatGroups.Add(CG);
                                }
                                size = buffer.GetInt(buffer.NextObjectIndex);
                                for (int i = 0; i < size; i++)
                                {
                                    ConnectedClient CC = buffer.GetNextObject<ConnectedClient>();
                                    if (CC.Name != "" && CC.Name != null)
                                        ConnectedClients.Add(CC);
                                }

                                // Create Chatbuffer object for new groups/clients;
                                for (int i = 0; i < chatGroups.Count; i++)
                                {
                                    bool bufferExist = false;
                                    for (int j = 0; j < ChatBufferList.Count; j++)
                                    {
                                        if (ChatBufferList[j].name == chatGroups[i].GroupName)
                                        {
                                            bufferExist = true;
                                        }
                                    }
                                    if (!bufferExist)
                                    {
                                        Invoke((MethodInvoker)delegate
                                        {
                                            ChatBuffer buf = new ChatBuffer();
                                            buf.Ip = i.ToString();
                                            buf.name = chatGroups[i].GroupName;
                                            buf.BufferPanel.Size = ChatbufferTab.Size;
                                            ChatBufferList.Add(buf);
                                        });
                                    }
                                }
                                for (int i = 0; i < ConnectedClients.Count; i++)
                                {
                                    bool bufferExist = false;
                                    for (int j = 0; j < ChatBufferList.Count; j++)
                                    {
                                        if (ChatBufferList[j].Ip == ConnectedClients[i].CC.Ip)
                                        {
                                            bufferExist = true;
                                            // check if name needs to be updated;
                                            if (ChatBufferList[j].name != ConnectedClients[i].Name)
                                                ChatBufferList[j].name = ConnectedClients[i].Name;
                                        }
                                    }
                                    if (!bufferExist)
                                    {
                                        if (ConnectedClients[i].Name != "" && ConnectedClients[i].Name != null)
                                        {
                                            Invoke((MethodInvoker)delegate
                                           {
                                               ChatBuffer buf = new ChatBuffer();
                                               buf.Ip = ConnectedClients[i].CC.Ip;
                                               buf.name = ConnectedClients[i].Name;
                                               buf.BufferPanel.Size = ChatbufferTab.Size;
                                               ChatBufferList.Add(buf);
                                           });
                                        }
                                    }
                                }
                                UpdateListView();

                            }
                            #endregion

                            #region TextMessage

                            if (buffer.GetID() == MsgType.TextMessage)
                            {
                                TextMessage TM = buffer.GetObject<TextMessage>();
                                PutMessageInBuffer(TM);
                            }
                            #endregion

                            if (buffer.GetID() == MsgType.ServerTest)
                            {
                                // build Test Message;
                                TextMessage Tm = new TextMessage();
                                Tm.Message = "This Is A StressTestMessage";
                                Tm.From = cSettings.Name;
                                Tm.ID = ChatBufferMessageType.Text;

                                foreach (ChatGroup group in chatGroups)
                                {
                                    Tm.To = group.GroupName;
                                    buffer.innerbuffer.Clear();
                                    buffer.AddID(MsgType.TextMessage);
                                    buffer.AddObject(Tm);
                                    buffer.SendCryptoBuffer(ServerSocket, Salt);
                                }
                                foreach (ConnectedClient client in ConnectedClients)
                                {
                                    Tm.To = client.Name;
                                    buffer.innerbuffer.Clear();
                                    buffer.AddID(MsgType.TextMessage);
                                    buffer.AddObject(Tm);
                                    buffer.SendCryptoBuffer(ServerSocket, Salt);
                                }
                            }
                        }
                        else
                        {
                            reconnect(null);
                        }
                    }
                }
                catch (SocketException e)
                {
                    Console.WriteLine(e.Message);
                    if (e.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        reconnect(null);
                    }
                }
            }
            ServerSocket.Shutdown(SocketShutdown.Both);
            ServerSocket.Close(5);
        }

        #endregion

        #region GuiButtons

        private void UpdateListView()
        {
            // Update Listview;
            if (ClientListview.InvokeRequired)
            {
                ClientListview.BeginInvoke((MethodInvoker)delegate
                {
                    UpdateListViewDelegateFunction();
                });
            }
            else
            {
                UpdateListViewDelegateFunction();
            }
        }

        private void UpdateListViewDelegateFunction()
        {
            foreach (TreeNode BaseNode in ClientListview.Nodes)
            {
                if (BaseNode.Name == "Groups")
                {
                    for (int i = 0; i < chatGroups.Count(); i++)
                    {
                        TreeNode treeNode = new TreeNode(chatGroups[i].GroupName);
                        treeNode.Name = chatGroups[i].GroupName;
                        treeNode.Tag = chatGroups[i].GroupName + ".chatgroup";

                        #region Add or Update GroupNode
                        if (BaseNode.Nodes.ContainsKey(chatGroups[i].GroupName)) // base nodes contain group node;
                        {
                            foreach (TreeNode Node in BaseNode.Nodes) // groupNodes
                            {
                                if (Node.Text == chatGroups[i].GroupName)
                                {
                                    for (int j = 0; j < ChatBufferList.Count(); j++) // check chat buffer list for new messages.
                                    {
                                        if (ChatBufferList[j].name == Node.Text)

                                            if (ChatBufferList[j].NewMessage())
                                            {
                                                Node.StateImageIndex = 4;
                                            }
                                            else
                                            {
                                                Node.StateImageIndex = -1;
                                            }
                                    }
                                    bool ClientInGroup = false;
                                    foreach (string ClientGroup in cSettings.GetGroups) // now we check if we are in that group and set color based on group policy
                                    {
                                        if (ClientGroup == Node.Text) // client is in that group 
                                        {
                                            ClientInGroup = true;
                                            Node.ForeColor = Color.Black;
                                        }
                                    }
                                    if (!ClientInGroup)
                                    {
                                        if (chatGroups[i].Policy == GroupAuth.Closed) // equals Offline for users.
                                            Node.ForeColor = Color.Red;
                                        if (chatGroups[i].Policy == GroupAuth.Open)
                                            Node.ForeColor = Color.Black;

                                    }
                                }
                            }
                        }
                        else
                            BaseNode.Nodes.Add(treeNode); // add group to the treenode
                        #endregion
                        #region Add or Update GroupMembers
                        for (int j = 0; j < chatGroups[i].Members.Count(); j++) // loop chatgroupmembers
                        {
                            TreeNode treeNodeSub = new TreeNode(chatGroups[i].Members[j].Name);
                            treeNodeSub.Tag = chatGroups[i].Members[j].Ip;
                            treeNodeSub.Name = chatGroups[i].Members[j].Name;

                            for (int k = 0; k < ChatBufferList.Count(); k++)
                            {
                                if (ChatBufferList[k].Ip == chatGroups[i].Members[j].Ip)
                                {
                                    foreach (TreeNode groupnode in BaseNode.Nodes)
                                    {
                                        if (groupnode.Text == chatGroups[i].GroupName) // get groupnode 
                                        {
                                            bool InList = false;
                                            foreach (TreeNode clientnode in groupnode.Nodes)
                                            {
                                                if ((string)clientnode.Tag == ChatBufferList[k].Ip) // get ClientNode in groupnode based on IP;
                                                {
                                                    // Updte name
                                                    if (clientnode.Name != ChatBufferList[k].name)
                                                    {
                                                        foreach (TabPage page in ChatbufferTab.TabPages)
                                                            if ((string)page.Tag == ChatBufferList[k].Ip)
                                                            {
                                                                page.Name = ChatBufferList[k].name + ".TabPage";
                                                                page.Text = " X " + ChatBufferList[k].name;
                                                            }
                                                        clientnode.Name = ChatBufferList[k].name;
                                                        clientnode.Text = ChatBufferList[k].name;
                                                        if (ActiveWindow.Ip == ChatBufferList[k].Ip)
                                                            ActiveWindow.name = ChatBufferList[k].name;
                                                    }
                                                    InList = true;
                                                    if (ChatBufferList[k].NewMessage())
                                                    {
                                                        clientnode.StateImageIndex = 4;
                                                    }
                                                    else
                                                    {
                                                        clientnode.StateImageIndex = -1;
                                                    }
                                                }
                                            }
                                            if (!InList)
                                            {
                                                groupnode.Nodes.Add(treeNodeSub);
                                                BaseNode.Nodes[i].Expand();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                    #region Delete Member from group if not in it;
                    foreach (ConnectedClient Client in ConnectedClients) // loop al clients;
                    {
                        foreach (TreeNode group in BaseNode.Nodes) // loop al Groups
                        {
                            bool ClientInGroup = false;
                            bool ClientBoundToGroup = false;
                            int IndexInGroup = -1;
                            for (int i = 0; i < group.Nodes.Count; i++) // loop al Group.Clients
                            {
                                if (Client.CC.Ip == (string)group.Nodes[i].Tag) // client is currently in that group
                                {
                                    ClientInGroup = true;
                                    string[] splitstring = Client.Groups.Split(';'); // get al groups client is bound to;
                                    foreach (string ClientGroup in splitstring)
                                    {
                                        if (ClientGroup == group.Name)
                                        {
                                            ClientBoundToGroup = true;
                                        }
                                    }
                                    if (!ClientBoundToGroup)
                                        IndexInGroup = i;
                                }
                            }
                            if (ClientInGroup && !ClientBoundToGroup)
                                group.Nodes.RemoveAt(IndexInGroup);
                        }
                    }
                    #endregion
                }
                if (BaseNode.Name == "Clients")
                {
                    for (int i = 0; i < ConnectedClients.Count; i++)
                    {
                        TreeNode user = new TreeNode(ConnectedClients[i].Name);
                        user.Name = ConnectedClients[i].Name;
                        user.Tag = ConnectedClients[i].CC.Ip;
                        bool InList = false;
                        for (int j = 0; j < ChatBufferList.Count(); j++)
                        {
                            if (ChatBufferList[j].Ip == ConnectedClients[i].CC.Ip)
                                foreach (TreeNode Node in BaseNode.Nodes)
                                {
                                    if ((string)Node.Tag == ConnectedClients[i].CC.Ip)
                                    {
                                        InList = true;
                                        if (Node.Name != ConnectedClients[i].Name)
                                        {
                                            Node.Name = ConnectedClients[i].Name;
                                            Node.Text = ConnectedClients[i].Name;
                                            if (ActiveWindow.Ip == ConnectedClients[i].CC.Ip)
                                                ActiveWindow.name = ConnectedClients[i].Name;
                                        }
                                        if (ChatBufferList[j].NewMessage())
                                            Node.ImageIndex = 4;
                                        else
                                            Node.ImageIndex = -1;
                                        if (ConnectedClients[i].Online)
                                            Node.ForeColor = Color.Black;
                                        else
                                            Node.ForeColor = Color.Red;
                                    }
                                }
                        }
                        if (!InList)
                        {
                            if (ConnectedClients[i].CC.Ip != cSettings.Ip)
                            {
                                if (ConnectedClients[i].Online)
                                    user.ForeColor = Color.Black;
                                else
                                    user.ForeColor = Color.Red;
                                BaseNode.Nodes.Add(user);
                            }
                        }
                    }
                }
            }
            ClientListview.Sort();
        }

        private int SetBarValue(object _name, object _value)
        {
            string name = (string)_name;
            int value = (int)_value;
            Invoke((MethodInvoker)delegate
            {
                Control[] controls = Controls.Find(name + ".progbar", true);
                if (controls[0] != null)
                {
                    ProgressBar bar = (ProgressBar)controls[0];
                    if (value > 1 && value < 100)
                    {
                        bar.Visible = true;
                        bar.Value = value;
                    }
                    else
                    {
                        bar.Visible = false;
                        bar.Value = 0;
                    }
                }
            });
            return 0;
        }

        private void SettingsSaveButton_Click(object sender, EventArgs e)
        {
            if (!SettingsLocked)
            {
                try
                {
                    try
                    {
                        this.Text = "SecuChat : " + settingNameTxtbx.Text;
                    }
                    catch (Exception error) { }
                    cSettings.Name = settingNameTxtbx.Text;
                    cSettings.Groups = SettingGroupTxtbx.Text;
                    nBuffer buffer = new nBuffer();
                    buffer.AddID(MsgType.ClientSettings);
                    buffer.AddObject<ClientSettings>(cSettings);
                    buffer.SendCryptoBuffer(ServerSocket, Salt);
                    IniFile ini = new IniFile("SecuChat.ini");
                    ini.Write("Groups", cSettings.Groups, "BASE");
                    ini.Write("Name", cSettings.Name, "BASE");
                }
                catch (SocketException er)
                {
                    if (er.ErrorCode == (int)SocketError.SocketError)
                    {
                        var bla = 0;
                    }
                }
            }
        }

        private void UserInputTxtBx_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool AllowedToSendMessage = true;
            bool IsGroupMessage = false;
            if (e.KeyChar == (int)Keys.Enter)
            {
                foreach (ChatGroup CG in chatGroups)
                {
                    if (CG.GroupName == ActiveWindow.name) // send message to a group
                    {
                        bool InGroup = false;
                        IsGroupMessage = true;
                        foreach (ClientSettings member in CG.Members)
                        {
                            if (member.Name == cSettings.Name) // im in that group.
                            {
                                InGroup = true;
                                break;
                            }
                        }
                        if (!InGroup)
                        {
                            if (CG.Policy == GroupAuth.Closed)
                                AllowedToSendMessage = false;
                        }
                    }
                }
                if (IsGroupMessage && !AllowedToSendMessage)
                {
                    MessageBox.Show("Not allowed to send message to group you're not part of!");
                    e.Handled = true;
                    return;
                }
                TextMessage TM = new TextMessage();
                TM.From = cSettings.Name;
                TM.To = ActiveWindow.name;
                TM.Message = UserInputTxtBx.Text;
                TM.ID = ChatBufferMessageType.Text;
                nBuffer buffer = new nBuffer();
                buffer.AddID(MsgType.TextMessage);
                buffer.AddObject(TM);
                buffer.SendCryptoBuffer(ServerSocket, Salt);
                UserInputTxtBx.Text = string.Empty;
                Console.WriteLine(UserInputTxtBx.Text.Length);
                e.Handled = true;
            }
        }


        List<FileToSend> FilesList = new List<FileToSend>();
        Thread SendFileThread = null;
        private void SendFileButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < openFileDialog1.FileNames.Length; i++)
                {
                    FileToSend file = new FileToSend();
                    string[] splitname = openFileDialog1.SafeFileNames[i].Split('.');
                    for (int j = 0; j < splitname.Length - 1; j++)
                    {
                        file.Name += splitname[j];
                    }
                    file.Ext = "." + splitname[splitname.Length - 1];
                    if (File.Exists(openFileDialog1.FileNames[i]))
                        file.BinaryFile = File.ReadAllBytes(openFileDialog1.FileNames[i]);
                    FilesList.Add(file);
                    Console.WriteLine("File {0} Added to List", file.Name + file.Ext);

                }
                if (SendFileThread == null)
                {
                    SendFileThread = new Thread(new ParameterizedThreadStart(SendFile));
                    SendFileThread.Start();
                    Console.WriteLine("Thread assigned and Started!");
                }
                else if (SendFileThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    SendFileThread = new Thread(new ParameterizedThreadStart(SendFile));
                    SendFileThread.Start();
                    Console.WriteLine("SendFileThread stopped, Restarted the thread!");
                }
            }
        }
        #endregion


        private void CheckIfLogoAvailable(object sender)
        {
            while(!GlobalQuit)
            {
                if (ClientPictureBox.Image == null)
                {
                    Socket LogoSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    LogoSocket.Connect(RemoteEndPoint);
                    if (LogoSocket.Connected)
                    {
                        nBuffer buffer = new nBuffer();
                        buffer.AddID(MsgType.ClientConnect);
                        ClientConnect CC = new ClientConnect();
                        CC.Ip = ComConstants.Ip;
                        CC.sockettype = ConnectionType.Logo;
                        CC.Salt = Salt;
                        buffer.AddObject(CC);
                        buffer.SendBuffer(LogoSocket);
                        buffer.innerbuffer.Clear();
                        Thread.Sleep(1000);
                        buffer.AddID(MsgType.RequestLogo);
                        buffer.SendCryptoBuffer(LogoSocket, Salt);
                        buffer.innerbuffer.Clear();

                        buffer.ReceiveCryptoBuffer(LogoSocket, Salt);
                        if (buffer.GetID() == MsgType.NoLogo)
                        {
                            Console.WriteLine("No Logo on server!");
                        }
                        if (buffer.GetID() == MsgType.Logo)
                        {
                            ClientPictureBox.Image = buffer.GetObject<Bitmap>();
                            LogoSocket.Shutdown(SocketShutdown.Both);
                            LogoSocket.Close(5);
                            break;
                        }
                        LogoSocket.Shutdown(SocketShutdown.Both);
                        LogoSocket.Close(5);
                    }
                }
                Thread.Sleep(5000);
            }
        }

        private void SendFile(object sender)
        {
            // Create new connection to server;
            Socket FileSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FileSocket.Connect(RemoteEndPoint);
            if (FileSocket.Connected)
            {
                nBuffer buffer = new nBuffer();
                buffer.AddID(MsgType.ClientConnect);
                ClientConnect CC = new ClientConnect();
                CC.Ip = ComConstants.Ip;
                CC.sockettype = ConnectionType.File;
                CC.Salt = Salt;
                buffer.AddObject(CC);
                buffer.SendBuffer(FileSocket);
                buffer.innerbuffer.Clear();

                // we connected to socket. now we loop the FilestoSend list.
                for (int i = 0; i < FilesList.Count; i++)
                {
                    // Create object
                    PrepareFileSend PFS = new PrepareFileSend();
                    PFS.filesize = FilesList[i].BinaryFile.Length;
                    PFS.from = cSettings.Name;
                    PFS.to = ActiveWindow.name;
                    PFS.FileName = FilesList[i].Name;
                    PFS.Ext = FilesList[i].Ext;

                    // prepare buffer
                    buffer.AddID(MsgType.SendFile);
                    buffer.AddObject(PFS);
                    buffer.SendCryptoBuffer(FileSocket, Salt);
                    buffer.innerbuffer.Clear();

                    Console.WriteLine("{0} Info Sent to Server", PFS.FileName);

                    ServerFile SF = new ServerFile();
                    SF.Name = PFS.FileName;
                    SF.BinaryFile = FilesList[i].BinaryFile;
                    buffer.AddObject(SF);
                    buffer.SendCryptoFileBuffer(FileSocket, PFS.FileName, Salt, FileSendProgression);
                    buffer.innerbuffer.Clear();
                    Console.WriteLine("File {0} Send to Server", PFS.FileName);

                    buffer.innerbuffer.Clear();
                    buffer.ReceiveCryptoBuffer(FileSocket, Salt);
                    if (buffer.GetID() == MsgType.FileSendComplete)
                    {
                        buffer.innerbuffer.Clear();
                    }
                }
                FileSocket.Shutdown(SocketShutdown.Both);
                FileSocket.Close(5);
                Console.WriteLine("{0} Files Cleared from List", FilesList.Count);
                FilesList.Clear();
            }
        }

        private int FileSendProgression(object _name, object _percent)
        {
            int percent = (int)_percent;
            if (percent < 99)
            {
                Invoke((MethodInvoker)delegate
                {
                    FileSendProgBar.Enabled = true;
                    FileSendProgBar.Visible = true;
                    FileSendProgBar.Value = percent;
                });
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    FileSendProgBar.Visible = false;
                    FileSendProgBar.Value = 0;
                });
            }
            return 0;
        }

        private void ChatBufferPanel_fillPanel(ref FlowLayoutPanel ChatBufferPanel, ChatBufferMessage CBM)
        {
            {
                if (CBM.ID == ChatBufferMessageType.Text)
                {
                    Label label = new Label();
                    label.MaximumSize = new Size(ChatBufferPanel.Width, 0);
                    label.AutoSize = true;
                    label.Text = CBM.Message;
                    ChatBufferPanel.Controls.Add(label);
                    if (ChatBufferPanel.VerticalScroll.Visible)
                        ChatBufferPanel.ScrollControlIntoView(label);
                }
                if (CBM.ID == ChatBufferMessageType.Link)
                {
                    LinkLabel linkLabel = new LinkLabel();
                    linkLabel.Size = new Size(100, 100);
                    linkLabel.AutoSize = true;

                    string[] Message = CBM.Message.Split(':');
                    Message[1] = Message[1].Remove(0, 1);
                    string PreName = Message[0] + " has send a file: " + Message[1] + "\n\r";
                    int PreNameSize = PreName.Length;

                    string PostName = "Download File";
                    int PostNameSize = PostName.Length;

                    linkLabel.Text = PreName + "Download File";
                    linkLabel.Name = Message[1];
                    linkLabel.BackColor = Color.LightGray;
                    linkLabel.BorderStyle = BorderStyle.Fixed3D;

                    linkLabel.Links.Add(PreNameSize, PostNameSize, Message[1]); // put whole file name (Message[1]) as link data;
                    linkLabel.Links[0].Name = Message[1] + ".Link";
                    linkLabel.LinkClicked += LinkLabel_LinkClicked;
                    ChatBufferPanel.Controls.Add(linkLabel);

                    // progress br
                    ProgressBar bar = new ProgressBar();
                    bar.Style = ProgressBarStyle.Continuous;
                    bar.Name = Message[1] + ".progbar";
                    bar.Maximum = 100;
                    bar.Minimum = 0;
                    bar.Value = 0;
                    bar.Visible = false;
                    ChatBufferPanel.Controls.Add(bar);
                    if (ChatBufferPanel.VerticalScroll.Visible)
                        ChatBufferPanel.ScrollControlIntoView(bar);
                }
                Console.WriteLine("SCrolbar value : " + ChatBufferPanel.VerticalScroll.Value);
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Link.Name == "OpenFile")
            {
                Process p = new Process();
                p.StartInfo.FileName = (string)e.Link.LinkData;
                p.Start();
            }
            else
            {
                Thread DownloadFileThread = new Thread(new ParameterizedThreadStart(DownloadThread));
                DownloadFileThread.Start(e);
            }
        }

        public void DownloadThread(object sender)
        {
            LinkLabelLinkClickedEventArgs e = (LinkLabelLinkClickedEventArgs)sender;
            // get new connection to server;
            Socket FileSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            FileSocket.Connect(RemoteEndPoint);
            Thread.Sleep(100);
            nBuffer buffer = new nBuffer();
            if (FileSocket.Connected)
            {
                buffer.AddID(MsgType.ClientConnect);
                ClientConnect CC = new ClientConnect();
                CC.Ip = ComConstants.Ip;
                CC.sockettype = ConnectionType.File;
                CC.Salt = Salt;
                buffer.AddObject(CC);
                buffer.SendBuffer(FileSocket);
                buffer.innerbuffer.Clear();
                Thread.Sleep(100);


                string file = e.Link.LinkData.ToString();
                Console.WriteLine("Requesting file : " + file);
                RequestFile RF = new RequestFile();
                RF.Name = file;
                buffer.AddID(MsgType.RequestFile);
                buffer.AddObject(RF);
                buffer.SendCryptoBuffer(FileSocket, Salt);
                buffer.innerbuffer.Clear();
                // file request sent;

                buffer.ReceiveCryptoBuffer(FileSocket, Salt);
                // server wants to send file.
                if (buffer.GetID() == MsgType.Nofile)
                {
                    Invoke((MethodInvoker)delegate
                    {
                        foreach (ChatBuffer chatbuf in ChatBufferList)
                        {
                            foreach (Control Con in chatbuf.BufferPanel.Controls)
                            {
                                if (Con.Name == file)
                                {
                                    Con.Name = Con.Name + ".Old";
                                    ((LinkLabel)Con).Text += "\n\r File no longer on server! ";
                                    ((LinkLabel)Con).Links.Clear();
                                }
                            }
                        }
                    });
                }
                if (buffer.GetID() == MsgType.SendFile)
                {
                    string Path = "";
                    LinkLabel linkLabel = new LinkLabel();
                    PrepareFileSend PFS = buffer.GetObject<PrepareFileSend>();
                    buffer.innerbuffer.Clear();
                    Invoke((MethodInvoker)delegate
                    {
                        saveFileDialog1.FileName = PFS.FileName;
                        saveFileDialog1.DefaultExt = PFS.Ext;
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            Path = saveFileDialog1.FileName;
                            Control[] controls = Controls.Find(PFS.FileName, true);
                            if (controls[0] != null)
                            {
                                linkLabel = (LinkLabel)controls[0];
                                int Size = linkLabel.Text.Length;
                                linkLabel.Text += "\n\rOpen File";
                                linkLabel.Links.Add(Size, "\n\rOpen File".Length, Path);
                                linkLabel.Links[1].Name = "OpenFile";
                                linkLabel.Links[1].Enabled = false;
                            }
                            buffer.AddID(MsgType.SendOK);
                            buffer.SendCryptoBuffer(FileSocket, Salt);
                        }
                        else
                        {
                            buffer.AddID(MsgType.SendCansel);
                            buffer.SendCryptoBuffer(ServerSocket, Salt);
                        }
                    });

                    buffer.innerbuffer.Clear();
                    buffer.ReceiveCryptoFileBuffer(FileSocket, Salt, PFS.FileName, SetBarValue); // receive actual file.
                    ServerFile SF = buffer.GetObject<ServerFile>(0);
                    buffer.innerbuffer.Clear();
                    if (Path != "")
                    {
                        FileStream fs = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write);
                        if (fs.CanWrite)
                        {
                            fs.Write(SF.BinaryFile, 0, (int)SF.BinaryFile.Length);
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                    if (linkLabel.Links.Count == 2)
                        Invoke((MethodInvoker)delegate
                       {
                           linkLabel.Links[1].Enabled = true;
                       });
                }
                FileSocket.Disconnect(false);
            }
            FileSocket.Dispose();
        }

        private void PutMessageInBuffer(TextMessage TM)
        {
            Invoke((MethodInvoker)delegate
           {
               bool IsGroupMessage = false;
               for (int i = 0; i < chatGroups.Count(); i++)
               {
                   if (chatGroups[i].GroupName == TM.To)
                       IsGroupMessage = true;
               }
               // if Message to group Tm.To == Group else if From  ==  Me is To : 
               string toBuffer = IsGroupMessage ? TM.To : TM.From == cSettings.Name ? TM.To : TM.From;
               bool InChatBufferList = false;
               for (int i = 0; i < ChatBufferList.Count(); i++)
               {
                   if (ChatBufferList[i].name == toBuffer)
                   {
                       InChatBufferList = true;
                       ChatBufferMessage CBM = new ChatBufferMessage();
                       CBM.ID = TM.ID;
                       string from = "";

                       if (IsGroupMessage)
                           if (TM.From == cSettings.Name) // ME
                               from = "Me";
                           else
                               from = TM.From;
                       else if (TM.From == cSettings.Name)
                           from = "Me";
                       else
                           from = TM.From;

                       CBM.Message = from + ": " + TM.Message;
                       ChatBufferList[i].Messages.Add(CBM);
                       if (ChatBufferList[i].name == ActiveWindow.name)
                           ChatBufferList[i].OldMessageSize = ChatBufferList[i].Messages.Count();

                       ChatBufferList[i].BufferPanel.Size = ChatbufferTab.Size;
                       ChatBufferPanel_fillPanel(ref ChatBufferList[i].BufferPanel, CBM);
                   }
               }
               if (!InChatBufferList)
               {
                   ChatBuffer CB = new ChatBuffer();
                   CB.name = toBuffer;
                   ChatBufferMessage CBM = new ChatBufferMessage();
                   CBM.ID = TM.ID;
                   string from = "";

                   if (IsGroupMessage)
                       if (TM.From == cSettings.Name) // ME
                           from = "Me";
                       else
                           from = TM.From;
                   else if (TM.From == cSettings.Name)
                       from = "Me";
                   else
                       from = TM.From;

                   CBM.Message = from + ": " + TM.Message;
                   CBM.ID = TM.ID;
                   CB.Messages.Add(CBM);
                   if (CB.name == ActiveWindow.name)
                       CB.OldMessageSize = CB.Messages.Count();
                   ChatBufferPanel_fillPanel(ref CB.BufferPanel, CBM);
                   ChatBufferList.Add(CB);
               }
               UpdateListView();
           });
        }

        private void CheckChatBufferTab(string Ip)
        {
            bool tabExist = false;
            for (int i = 0; i < ChatbufferTab.TabPages.Count; i++)
            {
                if ((string)ChatbufferTab.TabPages[i].Tag == Ip)
                {
                    tabExist = true;
                    ActivateTab(Ip);
                    break;
                }
            }
            if (!tabExist) // Name changed or does not exist;
            {
                TabPage page = new TabPage(ActiveWindow.name);
                page.Name = ActiveWindow.name + ".TabPage";
                page.Text = " X " + ActiveWindow.name;
                page.BorderStyle = BorderStyle.Fixed3D;
                page.Tag = Ip;
                for (int i = 0; i < ChatBufferList.Count; i++)
                {
                    if (ChatBufferList[i].Ip == Ip)
                    {
                        page.Controls.Add(ChatBufferList[i].BufferPanel);
                    }
                }
                ChatbufferTab.Controls.Add(page);
                ActivateTab(Ip);
            }
        }

        public void ActivateTab(string Ip)
        {
            foreach (TabPage page in ChatbufferTab.TabPages)
            {
                if ((string)page.Tag == Ip)
                {
                    ChatbufferTab.SelectedTab = page;
                }
            }
        }

        private void settingTxtbx_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SettingsSaveButton_Click(sender, null);
                e.Handled = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GlobalQuit = true;
            Application.Exit();
        }

        private void ChatbufferTab_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage != null)
            {
                string[] namesplit = e.TabPage.Name.Split('.');
                ActiveWindow.name = namesplit[0];
                ActiveWindow.Ip = (string)e.TabPage.Tag;
                foreach (ChatBuffer cb in ChatBufferList)
                {
                    if (cb.name == namesplit[0])
                    {
                        cb.OldMessageSize = cb.Messages.Count;
                    }
                }
            }
            else
                ActiveWindow = new ChatBufferID();
            UpdateListView();
        }

        private void testbutton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 50; i++)
            {
                TextMessage TM = new TextMessage();
                TM.From = cSettings.Name;
                TM.To = ActiveWindow.name;
                TM.Message = UserInputTxtBx.Text;
                TM.ID = ChatBufferMessageType.Text;
                nBuffer buffer = new nBuffer();
                buffer.AddID(MsgType.TextMessage);
                buffer.AddObject(TM);
                buffer.SendCryptoBuffer(ServerSocket, Salt);
            }
        }

        private void ChatbufferTab_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.DrawString(this.ChatbufferTab.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left, e.Bounds.Top + 4);
            e.DrawFocusRectangle();
        }

        private void ChatbufferTab_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < this.ChatbufferTab.TabPages.Count; i++)
            {
                Rectangle r = ChatbufferTab.GetTabRect(i);
                //Getting the position of the "x" mark.
                Rectangle closeButton = new Rectangle(r.Left + 4, r.Top + 5, 12, 12);
                if (closeButton.Contains(e.Location))
                {
                    if (MessageBox.Show("Would you like to Close this Tab?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        this.ChatbufferTab.TabPages.RemoveAt(i);
                        break;
                    }
                }
            }
            if (ChatbufferTab.TabPages.Count == 0)
                ActiveWindow = new ChatBufferID();
        }

        private void ClientListview_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Text != null && (string)e.Node.Tag != "Seperator")
            {
                ActiveWindow.name = e.Node.Text;
                ActiveWindow.Ip = (string)e.Node.Tag;
                for (int i = 0; i < ChatBufferList.Count(); i++)
                {
                    if (ChatBufferList[i].name == e.Node.Text)
                    {
                        ChatBufferList[i].OldMessageSize = ChatBufferList[i].Messages.Count();
                        CheckChatBufferTab(ChatBufferList[i].Ip);
                    }
                }
            }
            UpdateListView();
        }
        
        private void LockSettingsChkBX_Click(object sender, EventArgs e)
        {
            string Prompt = "";
            if (LockSettingsChkBX.Checked)
                Prompt = "Set Password:";
            else
                Prompt = "Give Password:";

            string _SettingsPassword = Microsoft.VisualBasic.Interaction.InputBox(Prompt, "Settings Password");
            if (_SettingsPassword == "")
            {
                if (!LockSettingsChkBX.Checked)
                    LockSettingsChkBX.Checked = true;
                else
                {
                    LockSettingsChkBX.Checked = false;
                    MessageBox.Show("No Password, No Lock!");
                }
                return;
            }

            if (LockSettingsChkBX.Checked) // changed to checked;
            {
                SettingsPassword = _SettingsPassword;
                SettingsLocked = true;
                IniFile ini = new IniFile("SecuChat.ini");
                ini.Write("SettingsPassword", SettingsPassword, "BASE");
            }
            else
            {
                if (SettingsPassword == _SettingsPassword)
                {
                    SettingsLocked = false;
                    SettingsPassword = "";
                    IniFile ini = new IniFile("SecuChat.ini");
                    ini.Write("SettingsPassword", SettingsPassword, "BASE");
                }
                else
                {
                    MessageBox.Show("WRONG PASSWORD!");
                    LockSettingsChkBX.Enabled = false;
                    LockSettingsChkBX.Checked = true;
                    LockSettingsChkBX.Enabled = true;
                }
            }
        }
    }
}