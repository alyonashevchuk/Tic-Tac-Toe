using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kr_Noliki_Project
{
    
    public partial class Form1 : Form
    {
        public string Nick  { get; set; }
        public string IpAddress_ { get; set; }
        string OtherNick = null;
        Thread t = null;
        Socket client_reciever = null;
        Socket client = null;
        Socket server = null;
        List<Button> buttons = new List<Button>();
        public Form1()
        {
            InitializeComponent();
            buttons.AddRange(new Button[] { button1, button2, button3, button4, button5, button6, button7, button8, button9 });
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ServerOrClient s = new ServerOrClient(this);
            if (s.ShowDialog() == DialogResult.OK)
            {
                if (s.isServer)
                {
                    label2.Text = "Waiting for client...";
                    server = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    Text = "Server";
                    IPAddress ip = IPAddress.Parse(GetLocalIPAddress());
                    server.Bind(new IPEndPoint(ip, 22000));
                    t = new Thread(Listener);
                    t.IsBackground = true;
                    t.Start(server);
                    buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = false);
                }
                else
                {
                    try
                    {
                        Text = "Client";
                        client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                        client.Connect(IpAddress_, 22000);
                        byte[] ConnectClient = ObjectToByteArray(new MyMessage { _MessageType = MessageType.Connect, Name = Nick });
                        client.Send(ConnectClient);
                        t = new Thread(ListenClient);
                        t.IsBackground = true;
                        t.Start(client);
                        buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = false);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("No connection with server!");
                        Application.Restart();
                    }
                }
            }
            else
                Close();           
        }
        void ListenClient(object obj)
        {
            Socket s = obj as Socket;
            if (s != null)
            {
                while (true)
                {
                    byte[] message = new byte[8000];
                    try
                    {
                        s.Receive(message);
                    }
                    catch (System.Net.Sockets.SocketException)
                    {

                    }
                    MyMessage m = (MyMessage)ByteArrayToObject(message);
                    switch (m._MessageType)
                    {
                        case MessageType.Click:
                            this.BeginInvoke(new Action(() =>
                            {
                                buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = true);
                                if (OtherNick == null)
                                {
                                    OtherNick = m.Name;
                                }
                                Button b = buttons.Where(x => Convert.ToInt32(x.Tag) == m.Number).First();
                                b.Text = "X";
                                b.Enabled = false;
                                if (IsWin("X"))
                                {
                                    MessageBox.Show("X is winner!");
                                    buttons.ForEach(x => x.Enabled = false);
                                }
                                else if (buttons.Where(x => x.Text != string.Empty).Count() == 9)
                                {
                                    byte[] data = ObjectToByteArray(new MyMessage { _MessageType = MessageType.Draw });
                                    client.Send(data);
                                    MessageBox.Show("The game is finished");
                                }
                                label2.Text = "Your choice";
                            }));
                            break;
                        case MessageType.Connect:
                            OtherNick = m.Name;
                            this.BeginInvoke(new Action(() =>
                            {
                                label2.Text = "Waiting for " + OtherNick + "...";
                            }));
                            break;
                        case MessageType.Disconnect:
                            MessageBox.Show("Player " + OtherNick + " is disconnected");
                            Thread.Sleep(5000);
                            this.BeginInvoke(new Action(() =>
                            {
                                client.Close();
                                client = null;
                                Close();
                            }
                            ));
                            break;
                        case MessageType.Draw:
                            MessageBox.Show("The game is finished");
                            break;
                    }
                }
            }
        }
        private void Listener(object obj)
        {
            Socket s = obj as Socket;
            if (s == null)
                return;
            s.Listen(1);
            while (true)
            {
                client_reciever = s.Accept();
                ThreadPool.QueueUserWorkItem(Listener2, client_reciever);
            }
        }
        private void Listener2(object obj)
        {
            Socket s = obj as Socket;
            if (s != null)
            {
                while (true)
                {
                    byte[] message = new byte[8000];
                    s.Receive(message);
                    MyMessage m = (MyMessage) ByteArrayToObject(message);
                    switch (m._MessageType)
                    {
                        case MessageType.Click:
                            this.BeginInvoke(new Action(() =>
                            {
                                if (OtherNick == null)
                                {
                                    OtherNick = m.Name;
                                }
                                Button b = buttons.Where(x => Convert.ToInt32(x.Tag) == m.Number).First();
                                b.Text = "O";
                                b.Enabled = false;
                                if (IsWin("O"))
                                {
                                    MessageBox.Show("O is winner!");
                                    buttons.ForEach(x => x.Enabled = false);
                                }
                                else if (buttons.Where(x => x.Text != string.Empty).Count() == 9)
                                {
                                    byte[] data = ObjectToByteArray(new MyMessage { _MessageType = MessageType.Draw });
                                    client_reciever.Send(data);
                                    MessageBox.Show("The game is finished");
                                }
                            }));
                            this.BeginInvoke(new Action(() =>
                            {
                                label2.Text = "Your choice";
                                buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = true);
                            }));
                            break;
                        case MessageType.Connect:
                            this.BeginInvoke(new Action(() =>
                            {
                                OtherNick = m.Name;
                                label2.Text = "Connected " + OtherNick;
                                buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = true);
                                byte[] ConnectMessageFromServer = ObjectToByteArray(new MyMessage { Name = Nick, _MessageType = MessageType.Connect });
                                client_reciever.Send(ConnectMessageFromServer);
                            }));
                            break;
                        case MessageType.Disconnect:
                            MessageBox.Show("Player " + OtherNick + " disconnected");
                            this.BeginInvoke(new Action(() =>
                            {
                                Close();
                            }));
                            break;
                        case MessageType.Draw:
                            MessageBox.Show("The game is finished");
                            break;
                    }
                }
            }
        }
        byte[] ObjectToByteArray(MyMessage obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        MyMessage ByteArrayToObject(byte[] obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(obj))
            {
                MyMessage m = (MyMessage)bf.Deserialize(ms);
                return m;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (client != null)
            {
                Button b = (Button)sender;
                b.Text = "O";
                b.Enabled = false;
                byte[] data = null;
                data = ObjectToByteArray(new MyMessage { Name = Nick, Number = Convert.ToInt32(b.Tag), _MessageType = MessageType.Click });
                client.Send(data);
                if (IsWin("O"))
                {
                    MessageBox.Show("You win!");
                    buttons.ForEach(x => x.Enabled = false);
                }
                label2.Text = "Waiting for " + OtherNick + "...";
                buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = false);
            }
            if (server != null)
            {
                try
                {
                    Button b = (Button)sender;
                    b.Text = "X";
                    b.Enabled = false;
                    byte[] data = null;
                    data = ObjectToByteArray(new MyMessage { Name = Nick, Number = Convert.ToInt32(b.Tag), _MessageType = MessageType.Click });
                    client_reciever.Send(data);
                    if (IsWin("X"))
                    {
                        MessageBox.Show("You win!");
                        buttons.ForEach(x => x.Enabled = false);
                    }
                    label2.Text = "Waiting for " + OtherNick + "...";
                    buttons.Where(x => x.Text == "").ToList().ForEach(x => x.Enabled = false);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (client != null)
                {
                    byte[] data = ObjectToByteArray(new MyMessage { _MessageType = MessageType.Disconnect });
                    client.Send(data);
                    client.Close();
                }
                else if (client_reciever != null)
                {
                    byte[] data = ObjectToByteArray(new MyMessage { _MessageType = MessageType.Disconnect });
                    client_reciever.Send(data);
                }
            }
            catch (Exception) { }
        }
        private bool IsWin(string s)
        {
            if (buttons[0].Text == s && buttons[1].Text == s && buttons[2].Text == s)
                return true;
            else if (buttons[0].Text == s && buttons[3].Text == s && buttons[6].Text == s)
                return true;
            else if (buttons[0].Text == s && buttons[4].Text == s && buttons[8].Text == s)
                return true;
            else if (buttons[2].Text == s && buttons[4].Text == s && buttons[6].Text == s)
                return true;
            else if (buttons[2].Text == s && buttons[5].Text == s && buttons[8].Text == s)
                return true;
            else if (buttons[1].Text == s && buttons[4].Text == s && buttons[7].Text == s)
                return true;
            else if (buttons[3].Text == s && buttons[4].Text == s && buttons[5].Text == s)
                return true;
            else if (buttons[6].Text == s && buttons[7].Text == s && buttons[8].Text == s)
                return true;
            else
                return false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }
    public enum MessageType
    {
        Connect,
        Click,
        Disconnect,
        Draw,
    }
    [Serializable]
    public class MyMessage
    {
        public MessageType _MessageType { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
    }
}
