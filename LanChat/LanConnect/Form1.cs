using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security;
using LanConnect.Class;
using LanConnect.Class.L2;


namespace LanConnect
{

    public partial class Form1 : Form
    {
        bool recstatus = false;     //Status Flag
        //Thread thrRec;
        UdpClient udprec;
        IPEndPoint lhost = new IPEndPoint(IPAddress.Loopback, 60000);           //ip endpoint for test
        IPEndPoint lep;
        IPEndPoint rep;
        IPEndPoint aep;
        IPEndPoint anyep;

        public Form1()
        {
            InitializeComponent();
            textBox4.AppendText("localhost = " + lhost + Environment.NewLine);                      //Show current ip address
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        //public static bool MsgReceived = false;

        //public static byte[] Msg;

        public void udprecCallback(IAsyncResult ar)
        {
            UdpClient udprec = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint rep = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
            TextBox textbox = (TextBox)((UdpState)(ar.AsyncState)).textbox;
            CheckBox checkbox1 = (CheckBox)((UdpState)(ar.AsyncState)).checkbox;
            TextBox textbox6 = (TextBox)((UdpState)(ar.AsyncState)).textbox6;
            if (checkBox1.Checked & !String.IsNullOrWhiteSpace(textbox6.Text))
            {
                //EncryptionStream Encrypt = new EncryptionStream();
                //byte[] Msg = Encrypt.aes256_en_de(udprec.EndReceive(ar, ref rep), SHA256.Create().ComputeHash(Encoding.UTF32.GetBytes(textBox6.Text)), false, MD5.Create().ComputeHash(Encoding.UTF32.GetBytes(textBox6.Text)));    //Decryption Required
                EncryptDataProtocol edp = new EncryptDataProtocol();
                edp.encrypt = false;
                byte[] Msg = edp.GetData(udprec.EndReceive(ar, ref rep), SHA256.Create().ComputeHash(Encoding.UTF32.GetBytes(textBox6.Text)));
                textbox.AppendText("Received: " + Encoding.UTF32.GetString(Msg) + Environment.NewLine);
            }
            else
            {
                textbox.AppendText("Received: " + Encoding.UTF32.GetString(udprec.EndReceive(ar, ref rep)) + Environment.NewLine);  //Decryption not required
            }
            UdpState st = new UdpState();
            st.e = rep;
            st.u = udprec;
            st.textbox = textbox;
            st.textbox6 = textbox6;
            st.checkbox = checkbox1;
            udprec.BeginReceive(new AsyncCallback(udprecCallback), st);
        }

        public class UdpState
        {
            public IPEndPoint e;
            public UdpClient u;
            public TextBox textbox;
            public CheckBox checkbox;
            public TextBox textbox6;
        }

        private void button1_Click(object sender, EventArgs e)                                      //Set UDP client properties and start listening
        {
            
            if (!recstatus)
            {
                button1.Enabled = false;//TBD
                lep = new IPEndPoint(IPAddress.Loopback, Convert.ToInt32(textBox1.Text));            //IPEndPoint
                rep = new IPEndPoint(IPAddress.Parse(textBox2.Text), Convert.ToInt32(textBox3.Text));
                aep = new IPEndPoint(IPAddress.Parse("0.0.0.0"), Convert.ToInt32(textBox1.Text));
                anyep = new IPEndPoint(IPAddress.Any, 0);
                textBox4.AppendText("lep: " + lep + Environment.NewLine);
                textBox4.AppendText("rep: " + rep + Environment.NewLine);
                textBox4.AppendText("aep: " + aep + Environment.NewLine);
                udprec = new UdpClient(aep);
                textBox4.AppendText("UDP Client Listening at " + aep + Environment.NewLine);
                UdpState st = new UdpState();
                st.e = rep;
                st.u = udprec;
                st.textbox = textBox4;
                st.textbox6 = textBox6;
                st.checkbox = checkBox1;
                udprec.BeginReceive(new AsyncCallback(udprecCallback), st);
                /*
                thrRec = new Thread(RecMsg);
                thrRec.Start();
                */
                recstatus = true;
                button1.Text = "Listening";
                button2.Enabled = true;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
            }
            else
            {
                //thrRec.Abort();
                udprec.Close();
                //(udprec as IDisposable).Dispose();
                recstatus = false;
                textBox4.AppendText("UDP Listener Stopped !" + Environment.NewLine);
                button1.Text = "Stopped";
                button2.Enabled = false;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                textBox3.Enabled = true;
            }

        }

        /*
        private void RecMsg()                                                                       //Keep checking memory if there comes an UDP package
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any,0);
            while (true)
            {

                if (udprec.Available < 1 || udprec == null)
                {
                    Thread.Sleep(50);
                    continue;
                }
                txtboxhandle tbh = new txtboxhandle();
                TextBoxdelegate addtxt = new TextBoxdelegate(tbh.addstr);
                TextBoxdelegate settxt = new TextBoxdelegate(tbh.setstr);
                addtxt(textBox4, "Received: ");
                addtxt(textBox4, Encoding.UTF32.GetString(udprec.Receive(ref rep)));             //Add new message into textbox4
                addtxt(textBox4, Environment.NewLine);
            }
        }
        
        public delegate void TextBoxdelegate(TextBox txtbox, string txt);                           //TextBox Delegate to Update UI
        
        public class txtboxhandle                                                                   //TextBox Handling Class
        {
            public void addstr(TextBox txtbox, string txt)                                          //Adding Text Method
            {
                txtboxhandle tbh = new txtboxhandle();
                TextBoxdelegate addtxt = new TextBoxdelegate(tbh.addstr);
                if (txtbox.InvokeRequired) txtbox.Invoke(addtxt, new object[] { txtbox, txt });
                else txtbox.AppendText(txt);
            }

            public void setstr(TextBox txtbox, string txt)                                          //Set or Clear Text Method
            {
                txtboxhandle tbh = new txtboxhandle();
                TextBoxdelegate settxt = new TextBoxdelegate(tbh.setstr);
                if (txtbox.InvokeRequired) txtbox.Invoke(settxt, new object[] { txtbox, txt });
                else txtbox.Text = txt;
            }
        }
        */

        private void button2_Click(object sender, EventArgs e)                                      //Send Message in textbox5
        {
            button2.Enabled = false;
            if (String.IsNullOrEmpty(textBox5.Text))
            {
                MessageBox.Show("Please Enter Something First.");
            }
            else
            {

                if (checkBox1.Checked & String.IsNullOrWhiteSpace(textBox6.Text))
                {
                    MessageBox.Show("You are using encryption mode! Please enter password!");
                    goto jieshu;
                }
                else if (checkBox1.Checked & !String.IsNullOrWhiteSpace(textBox6.Text))
                {
                    //EncryptionStream Encrypt = new EncryptionStream();
                    //byte[] Msg = Encrypt.aes256_en_de(Encoding.UTF32.GetBytes(textBox5.Text), SHA256.Create().ComputeHash(Encoding.UTF32.GetBytes(textBox6.Text)), true, MD5.Create().ComputeHash(Encoding.UTF32.GetBytes(textBox6.Text)));
                    EncryptDataProtocol edp = new EncryptDataProtocol();
                    edp.encrypt = true;
                    byte[] Msg = edp.GetData(Encoding.UTF32.GetBytes(textBox5.Text), SHA256.Create().ComputeHash(Encoding.UTF32.GetBytes(textBox6.Text)));
                    udprec.Send(Msg, Msg.Length, rep);
                }
                else
                {
                    udprec.Send(Encoding.UTF32.GetBytes(textBox5.Text), Encoding.UTF32.GetByteCount(textBox5.Text), rep);
                }
                textBox4.AppendText("Me: " + textBox5.Text + Environment.NewLine);
                textBox5.Text = "";
                
            jieshu: ;
            }
            button2.Enabled = true;
        }
        private void button3_Click(object sender, EventArgs e)                                      //End All
        {
            System.Environment.Exit(0);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox4.Text = "";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textBox6.Enabled = true;
                comboBox1.Enabled = true;
            }
            else
            {
                textBox6.Enabled = false;
                comboBox1.Enabled = false;
            }
        }
    }
}