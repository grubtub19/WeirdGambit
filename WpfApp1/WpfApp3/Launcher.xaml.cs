using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Windows.Shapes;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Net.Sockets;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Launcher : Window
    {
        public Launcher()
        {
            InitializeComponent();
            InitComboBoxes();
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(addressBox.Text == "Oz")
            {
                Server window = new Server("localhost");
                window.Show();
                this.Close();
            } else if (addressBox.Text == "Oz network")
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !ip.ToString().StartsWith("192"))
                    {
                        Console.WriteLine("Waiting for connection on " + ip.ToString());
                        Server window = new Server(ip.ToString());
                        window.Show();
                        this.Close();
                    }
                    else
                    {
                        Console.WriteLine("Not ipv4");
                    }
                }
            }
            else
            { 
                IPAddress address;
                if(IPAddress.TryParse(addressBox.Text, out address))
                {
                    Console.WriteLine("address = *" + addressBox.Text + "*");
                    Client window = new Client(addressBox.Text, comboBox1.Text, comboBox2.Text);
                    window.Show();
                    this.Close();
                }
            }
        }
    }
}
