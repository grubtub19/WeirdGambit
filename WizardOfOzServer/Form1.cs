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
using System.Diagnostics;
using System.IO;

namespace WizardOfOz
{
    public partial class Form1 : Form
    {
        delegate void SetTextCallback(string text);
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;
        Thread t = null;
        IPAddress ipAddress;


        public Form1()
        {
            InitializeComponent();
            ipAddress = Dns.Resolve("localhost").AddressList[0]; //ffmpeg uses ipAddress to select the destination
            listener = new TcpListener(ipAddress, 4545);
            listener.Start();
            client = listener.AcceptTcpClient();
            ns = client.GetStream();
            Console.Write("ip address: " + ipAddress);
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
            bool reading_input = true;
            try
            {
                int bytesRead = ns.Read(bytes, 0, bytes.Length);
                this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
            }
            catch (System.IO.IOException e)
            {
                    closeStream();
                    Application.Exit();
                    Environment.Exit(0);
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

        private Process videoProcess; //keep track of the process so we can kill it later
        private Process audioProcess;
        private bool videoProcessFirstStarted = false; //used to store whether or not the process has been started before. because if that's the case, it's null.
        private bool audioProcessFirstStarted = false;



        private void startStream()
        {
            if (!videoProcessFirstStarted || videoProcess.HasExited)
            {
                startFFplay("video");
            }
            if (!audioProcessFirstStarted || audioProcess.HasExited)
            {
                startFFplay("audio");
            }
        }

        /// <summary> 
        ///  Depending on the mediaType (either "video" or "audio), ffmpeg launches either media player. Some comments below to runs them without a window. Pretty slick.
        /// </summary> 

        private void startFFplay(String mediaType)
        {
            Process process = new Process();
            if (mediaType == "video")
            {
                videoProcess = new Process();
                process = videoProcess;
                process.StartInfo.Arguments = "-fflags nobuffer udp://" + ipAddress + ":1234"; //this is the video reciever. You can edit the address here
                videoProcessFirstStarted = true;
            }
            else if (mediaType == "audio")
            {
                audioProcess = new Process();
                process = audioProcess;
                process.StartInfo.Arguments = "-nodisp -fflags nobuffer udp://" + ipAddress + ":1235"; //this is the audio reciever. You can edit the address here. 
                                                                                       //Add -nodisp before "fflags" to remove the audio window
                audioProcessFirstStarted = true;
            }
            else
            {
                Console.WriteLine("Invalid media name");
                return;
            }
            process.StartInfo.FileName = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) +
                        @"\..\..\..\Libraries\ffplay.exe";
            process.StartInfo.UseShellExecute = false; //these lines let you run it without a window. disable for testing.
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }

        /// <summary> 
        ///  Tries to kill both ffmpeg processes. Checks if they are actually running first.
        /// </summary> 
        private void closeStream()
        {
            if (audioProcess != null && !audioProcess.HasExited)
            {
                audioProcess.Kill();
            }
            if (videoProcess != null && !videoProcess.HasExited)
            {
                videoProcess.Kill();
            }
        }

        /// <summary> 
        ///  "Start Stream Receiver" button
        /// </summary> 
        private void button1_Click_1(object sender, EventArgs e)
        {
            startStream();
        }

        /// <summary> 
        ///  "Stop Stream Receiver" button
        /// </summary> 
        private void button2_Click(object sender, EventArgs e)
        {
            closeStream();
        }

        /// <summary> 
        ///  We want to close the processes if they are still running before we close the form. It's not fun having invisible processes running without an easy way to close them.
        /// </summary> 
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Console.WriteLine("Closing Event Fired!");
            closeStream();
            Application.Exit();
            Environment.Exit(0);            
        }
    }
}
