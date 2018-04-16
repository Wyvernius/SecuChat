using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedClasses;
using System.Windows.Forms;

namespace SecuChat
{
    #region chatbuffer
    public class ChatBufferMessage
    {
        public string Message;
        public ChatBufferMessageType ID;
    }

    public class ChatBufferID
    {
        public string name = "";
        public string Ip = ""; // used as identifier for name changes;
    }

    public class ChatBuffer : ChatBufferID
    {

        public List<ChatBufferMessage> Messages = new List<ChatBufferMessage>();
        public FlowLayoutPanel BufferPanel;
        public GroupAuth Policy;
        public int panelScrolbarBottom = 0; //  0 =  not set; 1 = false; 2 = true;
        public int OldMessageSize = 0;
        public bool NewMessage()
        {
            return OldMessageSize < Messages.Count();
        }

        public ChatBuffer()
        {
            Policy = GroupAuth.Closed;
            BufferPanel = new FlowLayoutPanel();
            BufferPanel.BackColor = System.Drawing.Color.White;
            BufferPanel.AutoScroll = true;
            BufferPanel.FlowDirection = FlowDirection.TopDown;
            BufferPanel.WrapContents = false;
            BufferPanel.Visible = true;
        }
    }

    #endregion
}
