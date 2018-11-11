using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GIPS.NET
{
    class UkagakaSSTPConnection
    {
        private string sender;
        private string address;
        private string charset = "Shift-JIS";
        private int port;

        private const bool DEBUG = false;

        public UkagakaSSTPConnection(string sender, string hostname = "127.0.0.1", int port = 9801)
        {
            this.sender = sender;
            this.port = port;

            address = hostname;
        }
        
        public string SendNotify1_1(string eventName, params string[] reference)
        {
            string sendDataStr = "NOTIFY SSTP/1.1\r\n" + "Sender: " + sender + "\r\n" + "Event: " + eventName + "\r\n";

            StringBuilder sb = new StringBuilder(sendDataStr);

            for(var i = 0; i < reference.Length; i++)
            {
                sb.Append("Reference");
                sb.Append(i);
                sb.Append(": ");
                sb.Append(reference[i]);
                sb.Append("\r\n");
            }

            sb.Append("Charset: " + charset + "\r\n\r\n");

            string response;
            //try
            //{
                response = Send(sb.ToString());
            //}
            //catch (IOException e)
            //{
            //    response = "IOException";
            //}

            return response;
        }

        private string Send(string sendDataStr)
        {
            Encoding encode = Encoding.GetEncoding(charset);
            
            var tcp = new TcpClient(address, port);
            var sendBytes = encode.GetBytes(sendDataStr);

            var ns = tcp.GetStream();
            ns.Write(sendBytes, 0, sendBytes.Length);
            ns.Flush();

            System.Diagnostics.Debug.WriteLine(string.Join("", encode.GetChars(sendBytes)));

            var sr = new StreamReader(ns);

            var response = sr.ReadLine();

            tcp.Close();

            return "";//response;
        }
    }
}
