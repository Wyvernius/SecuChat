using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SharedClasses
{
    public class Utils
    {
        /*
        get ip, the IPv4 address.
        return "local host" if not found
        */
        public static string GetIp()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr.ToString();
                }
            }
            return "127.0.0.1";
        }

        public static byte[] ObjectToByteArray(Object obj)
        {
            int size = 0;
            return ObjectToByteArray(obj, ref size);
        }
        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(Object obj, ref int size)
        {
            if (obj == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, obj);
            size = ms.ToArray().Length;
            return ms.ToArray();
        }

        // Convert a byte array to an Object
        public static Object ByteArrayToObject(byte[] arrBytes, out int size, int index = 0, int length = 0)
        {
            Object obj = new object();
            if (length == 0)
                length = arrBytes.Length;
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, index, length);
            memStream.Seek(0, SeekOrigin.Begin);
            size = (int)memStream.Length;
            try
            {
                obj = (Object)binForm.Deserialize(memStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "  " + ex.StackTrace);
            }
            return obj;
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
