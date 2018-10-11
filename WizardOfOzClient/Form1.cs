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
using System.Speech.Synthesis;

namespace WizardOfOzClient
{
    public partial class Form1 : Form
    {
        public const int portNumber = 4545;
        delegate void SetTextCallback(string Text);

        TcpClient client;
        NetworkStream ns;
        Thread t = null;
        private const string hostName = "localhost";
        SpeechSynthesizer reader; // Text-to-speech class

        public Form1()
        {
            InitializeComponent();
            reader = new SpeechSynthesizer();
            client = new TcpClient(hostName, portNumber);
            ns = client.GetStream();
            String s = "Connected";
            byte[] byteTime = Encoding.ASCII.GetBytes(s);
            ns.Write(byteTime, 0, byteTime.Length);
            t = new Thread(DoWork);
            t.Start();
        }   

        private void sendButton_Click(object sender, EventArgs e)
        {
            String message = messageBox.Text;
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }
        
        public void DoWork()
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                int bytesRead = ns.Read(bytes, 0, bytes.Length);
                this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
            }
        }

        private void SetText(string text)

        {

            // InvokeRequired required compares the thread ID of the

            // calling thread to the thread ID of the creating thread.

            // If these threads are different, it returns true.

            if (this.allMessagesBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if(text.Contains("Speak:"))
                {
                    reader.Speak(text.Substring(7));
                }
                else
                {
                    this.allMessagesBox.AppendText(text + "\r\n");
                }
            }
        }

        private void allMessagesBox_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
