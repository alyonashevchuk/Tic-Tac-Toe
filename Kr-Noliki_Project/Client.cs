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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kr_Noliki_Project
{
    public partial class ConnectClient : Form
    {
        Form1 form = null;
        public ConnectClient( Form1 f)
        {
            InitializeComponent();
            form = f;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != string.Empty && textBox2.Text != string.Empty)
            {
                form.IpAddress_ = textBox2.Text;
                form.Nick = textBox1.Text;
                DialogResult = DialogResult.OK;
            }
            else
                MessageBox.Show("Enter all info, please!");
        }
        private void ConnectClient_Load(object sender, EventArgs e)
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    textBox2.Text = ip.ToString();
                    return;
                }
            }
       }
    }
}
