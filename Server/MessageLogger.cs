using System.Collections.Generic;
using SharedClasses;
using System.IO;

namespace Server
{
    public static class MessageLogger
    {
        static object LogWriteLock = new object();
        public static void LOG(TextMessage TM,List<ChatGroup> ChatGroups)
        {
            string Path = Utils.AssemblyDirectory + "\\" + TM.From + TM.To + ".chatlog";
            string AltPath = Utils.AssemblyDirectory + "\\" + TM.To + TM.From + ".chatlog";
            string DefPath = "";
            foreach (ChatGroup group in ChatGroups)
                if (TM.To == group.GroupName)
                    DefPath = Utils.AssemblyDirectory + "\\" + TM.To + ".chatlog";
            if (DefPath == "")
            {
                if (File.Exists(Path))
                    DefPath = Path;
                else if (File.Exists(AltPath))
                    DefPath = AltPath;
                if (DefPath == "")
                    DefPath = Path;
            }
            string MessageText = TM.From + ": " + TM.Message;
            StreamWriter sw = new StreamWriter(DefPath,true);
            sw.WriteLine(MessageText);
            sw.Close();
        }
    }
}
