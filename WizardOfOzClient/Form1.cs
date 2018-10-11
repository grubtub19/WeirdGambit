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
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics;
using System.IO;

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
            InitComboBoxes();
            reader = new SpeechSynthesizer();
            try
            {
                client = new TcpClient(hostName, portNumber);
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Application.Exit();
                Environment.Exit(0);
            }
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
                try {
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                catch (System.IO.IOException e)
                {
                    //The server has close, we need to close the application
                    closeStream();
                    Application.Exit();
                    Environment.Exit(0);
                }
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

        private void InitComboBoxes()
        {
            InitComboBox1();
            InitComboBox2();
        }

        /// <summary> 
        ///  Initializes the webcam dropdown menu. Populates it with the avaliable webcams on your system. If you pick one that doesn't work, it will fail to launch the video.
        /// </summary> 
        private void InitComboBox1()
        {
            bool DeviceExist = false;
            FilterInfoCollection videoDevices;
            //VideoCaptureDevice videoSource = null;

            try
            {
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                comboBox1.Items.Clear();
                if (videoDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                comboBox1.Items.Add("No capture device on your system");
            }
        }

        /// <summary> 
        ///  Initializes the audio dropdown menu. Populates it with the avaliable webcams on your system. 
        /// </summary> 
        private void InitComboBox2()
        {
            bool DeviceExist = false;
            FilterInfoCollection audioDevices;

            try
            {
                audioDevices = new FilterInfoCollection(FilterCategory.AudioInputDevice);
                comboBox2.Items.Clear();
                if (audioDevices.Count == 0)
                    throw new ApplicationException();

                DeviceExist = true;
                foreach (FilterInfo device in audioDevices)
                {
                    comboBox2.Items.Add(device.Name);
                }
                comboBox2.SelectedIndex = 0;
            }
            catch (ApplicationException)
            {
                DeviceExist = false;
                comboBox2.Items.Add("No capture device on your system");
            }
        }

        private Process videoProcess; //keep track of the process so we can kill it later
        private Process audioProcess;
        private bool videoProcessFirstStarted = false; //used to store whether or not the process has been started before. because if that's the case, it's null.
        private bool audioProcessFirstStarted = false;

        /// <summary> 
        ///  "Start Stream" button. Checks if the streams are already running before starting either of them.
        /// </summary> 
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null)
            {
                if (!videoProcessFirstStarted || videoProcess.HasExited)
                {
                    startFFmpeg("video", comboBox1.Text);
                }
                if (!audioProcessFirstStarted || audioProcess.HasExited)
                {
                    startFFmpeg("audio", comboBox2.Text);
                }
            }
            else
            {
                allMessagesBox.AppendText("Select input devices!!!\r\n");
            }
        }


        /// <summary> 
        ///  Depending on the mediaType (either "video" or "audio), ffmpeg launches either stream. Some comments below to runs them without a window. Pretty slick.
        /// </summary> 
        private void startFFmpeg(String mediaType, String hardwareName)
        {
            Process process = new Process();
            if (mediaType == "video")
            {
                videoProcess = new Process();
                process = videoProcess;
                process.StartInfo.Arguments = "-f dshow -video_size 640x480 -r 30 -i video=\"" + hardwareName + "\" -filter:v \"setpts=(39/40)*PTS\" -vcodec libx264 -preset ultrafast -f mpegts udp://127.0.0.1:1234";
                                                                            //the is the video encoder/streamer. You can edit the udp address. 
                videoProcessFirstStarted = true;
            }
            else if (mediaType == "audio")
            {
                audioProcess = new Process();
                process = audioProcess;
                process.StartInfo.Arguments = "-f dshow -i audio=\"" + hardwareName + "\" -af asetrate=44100*(20/19),aresample=44100 -acodec aac -f mpegts udp://127.0.0.1:1235 ";
                                                                            //this is the audio encoder/streamer. You can edit the address here. 
                audioProcessFirstStarted = true;

            }
            else
            {
                Console.WriteLine("Invalid media name");
                return;
            }
            process.StartInfo.FileName = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) +
                                    @"\..\..\..\Libraries\ffmpeg.exe";
            //process.StartInfo.UseShellExecute = false; //these lines let you run it without a window. disable for testing.
            //process.StartInfo.CreateNoWindow = true;
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
        ///  "Stop Stream" button
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
            //Console.WriteLine("Closing Event Triggered");
            closeStream();
            Application.Exit();
            Environment.Exit(0);
        }
    }
}
