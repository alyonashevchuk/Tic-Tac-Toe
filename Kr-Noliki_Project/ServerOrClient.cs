using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace Kr_Noliki_Project
{
    public partial class ServerOrClient : Form
    {
        public bool isServer = false;
        Form1 form;
        public ServerOrClient(Form1 f)
        {
            InitializeComponent();
            form = f;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ConnectClient c = new ConnectClient(form);
            if (c.ShowDialog() == DialogResult.OK)
            {
                isServer = false;
                DialogResult = DialogResult.OK;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            CreateServer s = new CreateServer(form);
            if (s.ShowDialog() == DialogResult.OK)
            {
                isServer = true;
                DialogResult = DialogResult.OK;
            }
        }
    }
}
