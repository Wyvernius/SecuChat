namespace Server
{
    partial class ServerIcon
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerIcon));
            this.ServerNotifyMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ItemMenuClose = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerNotify = new System.Windows.Forms.NotifyIcon(this.components);
            this.Fileencryptionpasswordtextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.passwordsetbutton = new System.Windows.Forms.Button();
            this.LogChatMessages = new System.Windows.Forms.CheckBox();
            this.PublicGroupChkBx = new System.Windows.Forms.CheckBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Group = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Public = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ServerTestButton = new System.Windows.Forms.Button();
            this.ServerPictureBox = new System.Windows.Forms.PictureBox();
            this.ServerNotifyMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ServerPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // ServerNotifyMenuStrip
            // 
            this.ServerNotifyMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ItemMenuClose});
            this.ServerNotifyMenuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.VerticalStackWithOverflow;
            this.ServerNotifyMenuStrip.Name = "ServerNotifyMenuStrip";
            this.ServerNotifyMenuStrip.Size = new System.Drawing.Size(139, 26);
            this.ServerNotifyMenuStrip.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ServerNotifyMenuStrip_ItemClicked);
            // 
            // ItemMenuClose
            // 
            this.ItemMenuClose.CheckOnClick = true;
            this.ItemMenuClose.Name = "ItemMenuClose";
            this.ItemMenuClose.Size = new System.Drawing.Size(138, 22);
            this.ItemMenuClose.Text = "Close Server";
            this.ItemMenuClose.ToolTipText = "Hello";
            // 
            // ServerNotify
            // 
            this.ServerNotify.ContextMenuStrip = this.ServerNotifyMenuStrip;
            this.ServerNotify.Icon = ((System.Drawing.Icon)(resources.GetObject("ServerNotify.Icon")));
            this.ServerNotify.Visible = true;
            // 
            // Fileencryptionpasswordtextbox
            // 
            this.Fileencryptionpasswordtextbox.Location = new System.Drawing.Point(0, 34);
            this.Fileencryptionpasswordtextbox.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Fileencryptionpasswordtextbox.Name = "Fileencryptionpasswordtextbox";
            this.Fileencryptionpasswordtextbox.Size = new System.Drawing.Size(242, 26);
            this.Fileencryptionpasswordtextbox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(183, 20);
            this.label1.TabIndex = 2;
            this.label1.Text = "File encryption password";
            // 
            // passwordsetbutton
            // 
            this.passwordsetbutton.Location = new System.Drawing.Point(250, 30);
            this.passwordsetbutton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.passwordsetbutton.Name = "passwordsetbutton";
            this.passwordsetbutton.Size = new System.Drawing.Size(112, 35);
            this.passwordsetbutton.TabIndex = 3;
            this.passwordsetbutton.Text = "set";
            this.passwordsetbutton.UseVisualStyleBackColor = true;
            this.passwordsetbutton.Click += new System.EventHandler(this.passwordsetbutton_Click);
            // 
            // LogChatMessages
            // 
            this.LogChatMessages.AutoSize = true;
            this.LogChatMessages.Location = new System.Drawing.Point(0, 68);
            this.LogChatMessages.Name = "LogChatMessages";
            this.LogChatMessages.Size = new System.Drawing.Size(93, 24);
            this.LogChatMessages.TabIndex = 4;
            this.LogChatMessages.Text = "Log Chat";
            this.LogChatMessages.UseVisualStyleBackColor = true;
            // 
            // PublicGroupChkBx
            // 
            this.PublicGroupChkBx.AutoSize = true;
            this.PublicGroupChkBx.Location = new System.Drawing.Point(0, 98);
            this.PublicGroupChkBx.Name = "PublicGroupChkBx";
            this.PublicGroupChkBx.Size = new System.Drawing.Size(127, 24);
            this.PublicGroupChkBx.TabIndex = 6;
            this.PublicGroupChkBx.Text = "Public Groups";
            this.PublicGroupChkBx.UseVisualStyleBackColor = true;
            this.PublicGroupChkBx.CheckedChanged += new System.EventHandler(this.PublicGroupChkBx_CheckedChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Group,
            this.Public});
            this.dataGridView1.Location = new System.Drawing.Point(0, 128);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(343, 150);
            this.dataGridView1.TabIndex = 7;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellValueChanged);
            // 
            // Group
            // 
            this.Group.HeaderText = "GroupName";
            this.Group.Name = "Group";
            this.Group.Width = 200;
            // 
            // Public
            // 
            this.Public.DataPropertyName = "Public";
            this.Public.FalseValue = "0";
            this.Public.HeaderText = "Public";
            this.Public.IndeterminateValue = "-1";
            this.Public.Name = "Public";
            this.Public.TrueValue = "1";
            // 
            // ServerTestButton
            // 
            this.ServerTestButton.Location = new System.Drawing.Point(12, 305);
            this.ServerTestButton.Name = "ServerTestButton";
            this.ServerTestButton.Size = new System.Drawing.Size(75, 32);
            this.ServerTestButton.TabIndex = 8;
            this.ServerTestButton.Text = "Test";
            this.ServerTestButton.UseVisualStyleBackColor = true;
            this.ServerTestButton.Click += new System.EventHandler(this.ServerTestButton_Click);
            // 
            // ServerPictureBox
            // 
            this.ServerPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ServerPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ServerPictureBox.Location = new System.Drawing.Point(525, 9);
            this.ServerPictureBox.Name = "ServerPictureBox";
            this.ServerPictureBox.Size = new System.Drawing.Size(189, 122);
            this.ServerPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ServerPictureBox.TabIndex = 9;
            this.ServerPictureBox.TabStop = false;
            this.ServerPictureBox.Click += new System.EventHandler(this.ServerPictureBox_Click);
            // 
            // ServerIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(716, 463);
            this.Controls.Add(this.ServerPictureBox);
            this.Controls.Add(this.ServerTestButton);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.PublicGroupChkBx);
            this.Controls.Add(this.LogChatMessages);
            this.Controls.Add(this.passwordsetbutton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Fileencryptionpasswordtextbox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ServerIcon";
            this.Text = "ServerIcon";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Load += new System.EventHandler(this.ServerIcon_Load);
            this.ServerNotifyMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ServerPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip ServerNotifyMenuStrip;
        private System.Windows.Forms.NotifyIcon ServerNotify;
        private System.Windows.Forms.ToolStripMenuItem ItemMenuClose;
        private System.Windows.Forms.TextBox Fileencryptionpasswordtextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button passwordsetbutton;
        private System.Windows.Forms.CheckBox LogChatMessages;
        private System.Windows.Forms.CheckBox PublicGroupChkBx;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Group;
        private System.Windows.Forms.DataGridViewCheckBoxColumn Public;
        private System.Windows.Forms.Button ServerTestButton;
        private System.Windows.Forms.PictureBox ServerPictureBox;
    }
}