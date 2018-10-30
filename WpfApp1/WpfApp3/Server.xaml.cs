using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.IO;
using WMPLib;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Server.xaml
    /// </summary>
    public partial class Server : Window
    {
        delegate void SetTextCallback(string text);
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;
        Thread t = null;
        IPAddress ipAddress;

        public Server(string hostName)
        {
            InitializeComponent();
            InitMessageBox();
            ipAddress = Dns.Resolve(hostName).AddressList[0]; //ffmpeg uses ipAddress to select the destination
            Console.WriteLine(ipAddress.ToString());
            listener = new TcpListener(ipAddress, 4545);
            listener.Start();
            client = listener.AcceptTcpClient();
            ns = client.GetStream();
            Console.Write("ip address: " + ipAddress);
            t = new Thread(DoWork);
            t.Start();
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            String message = inputBox.Text;
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
            allMessagesBox.AppendText("You: " + message + "\r\n");
            inputBox.Text = string.Empty;
        }
        public void DoWork()
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    this.SetText(Encoding.ASCII.GetString(bytes, 0, bytesRead));
                }
                catch (System.IO.IOException e)
                {
                    closeStream();
                    this.Dispatcher.Invoke(() =>
                    {
                        System.Windows.Application.Current.Shutdown();
                    });
                    Environment.Exit(0);
                }
            }
        }
        private void SetText(string text)
        {
            if (!Dispatcher.CheckAccess())
            {
                SetTextCallback d = new SetTextCallback(SetText);
                Dispatcher.Invoke(d, new object[] { text });
            }
            else
            {
                this.allMessagesBox.AppendText("Client: " + text + "\r\n");
            }
        }
        private void InitMessageBox()
        {
            inputBox.Text = "Message";
            inputBox.Foreground = Brushes.Gray;
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
        private void startStreamButton_Click(object sender, RoutedEventArgs e)
        {
            startStream();
        }
        private void stopStreamButton_Click(object sender, RoutedEventArgs e)
        {
            closeStream();
        }

        private void inputBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (inputBox.Foreground == Brushes.Gray)
            {
                inputBox.Text = string.Empty;
                inputBox.Foreground = Brushes.Black;
            }
        }

        private void inputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (inputBox.Text == string.Empty)
            {
                inputBox.Text = "Message";
                inputBox.Foreground = Brushes.Gray;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closeStream();
            Application.Current.Shutdown();
            Environment.Exit(0);
        }


        private void angry_button_click(object sender, RoutedEventArgs e)
        {
            String message = "angry_button_clicked";
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        private void confused_button_click(object sender, RoutedEventArgs e)
        {
            String message = "confused_button_clicked";
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        private void happy_button_clicked(object sender, RoutedEventArgs e)
        {
            String message = "happy_button_clicked";
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        private void mocking_button_clicked(object sender, RoutedEventArgs e)
        {
            String message = "mocking_button_clicked";
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }

        private void sad_button_click(object sender, RoutedEventArgs e)
        {
            String message = "sad_button_clicked";
            byte[] byteTime = Encoding.ASCII.GetBytes(message);
            ns.Write(byteTime, 0, byteTime.Length);
        }
    }
}
