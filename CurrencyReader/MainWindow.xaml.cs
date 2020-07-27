using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace CurrencyReader
{
    public partial class MainWindow : Window
    {
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";


        public MainWindow()
        {
            InitializeComponent();
        }

        // Sends InputBox text as a string to the server
        // Assigns received answer as an OutputBox text
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TcpClient client;
            
            //Collect data from the input box
            string textToSend = InputBox.Text;

            //---create a TCPClient object at the IP and port no.---
            client = new TcpClient(SERVER_IP, PORT_NO);
            NetworkStream nwStream = client.GetStream();
            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(textToSend);

            //---send the text---
            nwStream.Write(bytesToSend, 0, bytesToSend.Length);

            //---read server answer---
            byte[] bytesToRead = new byte[client.ReceiveBufferSize];
            int bytesRead = nwStream.Read(bytesToRead, 0, client.ReceiveBufferSize);

            //Return data to the output box
            OutputBox.Text = "" + Encoding.ASCII.GetString(bytesToRead, 0, bytesRead);

            Console.ReadLine();
            client.Close();
        }

    }
}
