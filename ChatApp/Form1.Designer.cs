using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SecuChat
{
    partial class Form1
    {

        private System.ComponentModel.IContainer components = null;
        System.Timers.Timer timer;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.ChatTab = new System.Windows.Forms.TabControl();
            this.ChatPage = new System.Windows.Forms.TabPage();
            this.testbutton = new System.Windows.Forms.Button();
            this.ClientListview = new System.Windows.Forms.TreeView();
            this.OnOfflineImages = new System.Windows.Forms.ImageList(this.components);
            this.UserInputTxtBx = new System.Windows.Forms.TextBox();
            this.FileSendProgBar = new System.Windows.Forms.ProgressBar();
            this.ServerStatus = new System.Windows.Forms.Label();
            this.ChatbufferTab = new System.Windows.Forms.TabControl();
            this.SendFileButton = new System.Windows.Forms.Button();
            this.SettingsPage = new System.Windows.Forms.TabPage();
            this.LockSettingsChkBX = new System.Windows.Forms.CheckBox();
            this.SettingsSaveButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SettingGroupTxtbx = new System.Windows.Forms.TextBox();
            this.settingNameTxtbx = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.ClientPictureBox = new System.Windows.Forms.PictureBox();
            this.ChatTab.SuspendLayout();
            this.ChatPage.SuspendLayout();
            this.SettingsPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClientPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ChatTab
            // 
            resources.ApplyResources(this.ChatTab, "ChatTab");
            this.ChatTab.Controls.Add(this.ChatPage);
            this.ChatTab.Controls.Add(this.SettingsPage);
            this.ChatTab.Name = "ChatTab";
            this.ChatTab.SelectedIndex = 0;
            // 
            // ChatPage
            // 
            this.ChatPage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ChatPage.Controls.Add(this.ClientPictureBox);
            this.ChatPage.Controls.Add(this.testbutton);
            this.ChatPage.Controls.Add(this.ClientListview);
            this.ChatPage.Controls.Add(this.UserInputTxtBx);
            this.ChatPage.Controls.Add(this.FileSendProgBar);
            this.ChatPage.Controls.Add(this.ServerStatus);
            this.ChatPage.Controls.Add(this.ChatbufferTab);
            this.ChatPage.Controls.Add(this.SendFileButton);
            resources.ApplyResources(this.ChatPage, "ChatPage");
            this.ChatPage.Name = "ChatPage";
            this.ChatPage.UseVisualStyleBackColor = true;
            // 
            // testbutton
            // 
            resources.ApplyResources(this.testbutton, "testbutton");
            this.testbutton.Name = "testbutton";
            this.testbutton.UseVisualStyleBackColor = true;
            this.testbutton.Click += new System.EventHandler(this.testbutton_Click);
            // 
            // ClientListview
            // 
            this.ClientListview.HideSelection = false;
            this.ClientListview.HotTracking = true;
            resources.ApplyResources(this.ClientListview, "ClientListview");
            this.ClientListview.Name = "ClientListview";
            this.ClientListview.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            ((System.Windows.Forms.TreeNode)(resources.GetObject("ClientListview.Nodes"))),
            ((System.Windows.Forms.TreeNode)(resources.GetObject("ClientListview.Nodes1")))});
            this.ClientListview.ShowLines = false;
            this.ClientListview.ShowRootLines = false;
            this.ClientListview.StateImageList = this.OnOfflineImages;
            this.ClientListview.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ClientListview_NodeMouseClick);
            // 
            // OnOfflineImages
            // 
            this.OnOfflineImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("OnOfflineImages.ImageStream")));
            this.OnOfflineImages.TransparentColor = System.Drawing.Color.Transparent;
            this.OnOfflineImages.Images.SetKeyName(0, "Online Icon.png");
            this.OnOfflineImages.Images.SetKeyName(1, "Offline Icon.png");
            this.OnOfflineImages.Images.SetKeyName(2, "ServerOffline.png");
            this.OnOfflineImages.Images.SetKeyName(3, "ServerOnline.png");
            this.OnOfflineImages.Images.SetKeyName(4, "NewMessage.png");
            // 
            // UserInputTxtBx
            // 
            resources.ApplyResources(this.UserInputTxtBx, "UserInputTxtBx");
            this.UserInputTxtBx.Name = "UserInputTxtBx";
            this.UserInputTxtBx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.UserInputTxtBx_KeyPress);
            // 
            // FileSendProgBar
            // 
            resources.ApplyResources(this.FileSendProgBar, "FileSendProgBar");
            this.FileSendProgBar.Name = "FileSendProgBar";
            this.FileSendProgBar.Step = 1;
            this.FileSendProgBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // ServerStatus
            // 
            resources.ApplyResources(this.ServerStatus, "ServerStatus");
            this.ServerStatus.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ServerStatus.ImageList = this.OnOfflineImages;
            this.ServerStatus.Name = "ServerStatus";
            this.ServerStatus.UseCompatibleTextRendering = true;
            // 
            // ChatbufferTab
            // 
            this.ChatbufferTab.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            resources.ApplyResources(this.ChatbufferTab, "ChatbufferTab");
            this.ChatbufferTab.Name = "ChatbufferTab";
            this.ChatbufferTab.SelectedIndex = 0;
            this.ChatbufferTab.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.ChatbufferTab.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.ChatbufferTab_DrawItem);
            this.ChatbufferTab.Selected += new System.Windows.Forms.TabControlEventHandler(this.ChatbufferTab_Selected);
            this.ChatbufferTab.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ChatbufferTab_MouseDown);
            // 
            // SendFileButton
            // 
            resources.ApplyResources(this.SendFileButton, "SendFileButton");
            this.SendFileButton.Name = "SendFileButton";
            this.SendFileButton.UseVisualStyleBackColor = true;
            this.SendFileButton.Click += new System.EventHandler(this.SendFileButton_Click);
            // 
            // SettingsPage
            // 
            this.SettingsPage.Controls.Add(this.LockSettingsChkBX);
            this.SettingsPage.Controls.Add(this.SettingsSaveButton);
            this.SettingsPage.Controls.Add(this.label2);
            this.SettingsPage.Controls.Add(this.label1);
            this.SettingsPage.Controls.Add(this.SettingGroupTxtbx);
            this.SettingsPage.Controls.Add(this.settingNameTxtbx);
            resources.ApplyResources(this.SettingsPage, "SettingsPage");
            this.SettingsPage.Name = "SettingsPage";
            this.SettingsPage.UseVisualStyleBackColor = true;
            // 
            // LockSettingsChkBX
            // 
            resources.ApplyResources(this.LockSettingsChkBX, "LockSettingsChkBX");
            this.LockSettingsChkBX.Name = "LockSettingsChkBX";
            this.LockSettingsChkBX.UseVisualStyleBackColor = true;
            this.LockSettingsChkBX.Click += new System.EventHandler(this.LockSettingsChkBX_Click);
            // 
            // SettingsSaveButton
            // 
            resources.ApplyResources(this.SettingsSaveButton, "SettingsSaveButton");
            this.SettingsSaveButton.Name = "SettingsSaveButton";
            this.SettingsSaveButton.UseVisualStyleBackColor = true;
            this.SettingsSaveButton.Click += new System.EventHandler(this.SettingsSaveButton_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            this.label1.UseCompatibleTextRendering = true;
            // 
            // SettingGroupTxtbx
            // 
            resources.ApplyResources(this.SettingGroupTxtbx, "SettingGroupTxtbx");
            this.SettingGroupTxtbx.Name = "SettingGroupTxtbx";
            this.SettingGroupTxtbx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.settingTxtbx_KeyPress);
            // 
            // settingNameTxtbx
            // 
            resources.ApplyResources(this.settingNameTxtbx, "settingNameTxtbx");
            this.settingNameTxtbx.Name = "settingNameTxtbx";
            this.settingNameTxtbx.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.settingTxtbx_KeyPress);
            // 
            // ClientPictureBox
            // 
            resources.ApplyResources(this.ClientPictureBox, "ClientPictureBox");
            this.ClientPictureBox.Name = "ClientPictureBox";
            this.ClientPictureBox.TabStop = false;
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CausesValidation = false;
            this.Controls.Add(this.ChatTab);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.ChatTab.ResumeLayout(false);
            this.ChatPage.ResumeLayout(false);
            this.ChatPage.PerformLayout();
            this.SettingsPage.ResumeLayout(false);
            this.SettingsPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ClientPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        private TabControl ChatTab;
        private TabPage ChatPage;
        private TabPage SettingsPage;
        private Label label2;
        private Label label1;
        private TextBox SettingGroupTxtbx;
        private TextBox settingNameTxtbx;
        private Button SettingsSaveButton;
        private TextBox UserInputTxtBx;
        private Button SendFileButton;
        private OpenFileDialog openFileDialog1;
        private SaveFileDialog saveFileDialog1;
        private ProgressBar FileSendProgBar;
        private ImageList OnOfflineImages;
        private Label ServerStatus;
        private TabControl ChatbufferTab;
        private Button testbutton;
        private TreeView ClientListview;
        private CheckBox LockSettingsChkBX;
        private PictureBox ClientPictureBox;
    }
}

