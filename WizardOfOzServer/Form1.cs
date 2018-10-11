using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;

namespace WizardOfOz
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;
        Thread t = null;

        public Form1()
        {
            InitializeComponent();
            IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
            listener = new TcpListener(ipAddress, 4545);
            listener.Start();
            client = listener.AcceptTcpClient();
            ns = client.GetStream();
            t = new Thread(DoWork);
            t.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String message = messageBox.Text;
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        public void DoWork()
        {
            byte[] bytes = new byte[1024];
            while(true)
            {
                int bytesRead = ns.Read(bytes, 0, bytes.Length);
                this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
            }
        }

        private void SetText(string text)
        {
            if(this.allMessagesBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.allMessagesBox.AppendText(text + "\r\n");
            }
        }
    }
}
