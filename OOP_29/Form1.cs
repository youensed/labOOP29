using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace OOP_29
{
    public partial class Form1 : Form
    {
        const int LOCALPORT = 8001; // порт для приймання повідомлень
        const int REMOTEPORT = 8001; // порт для передавання повідомлень
        const int TTL = 20;
        const string HOST = "235.5.5.1"; // хост 

        IPAddress groupAddress; //адреса 
        string nickName; // ім’я користувача в чаті
        bool alive = false;
        UdpClient client;


        public Form1()
        {
            InitializeComponent();

            button1.Enabled = true; // вхід
            button2.Enabled = false; // вихід
            button3.Enabled = false; // відправка
            richTextBox1.ReadOnly = true; // поле для повідомлень
            groupAddress = IPAddress.Parse(HOST);

        }

        private void ReceiveMessages()
        {
            alive = true;
            try
            {
                while (alive)
                {
                    IPEndPoint remoteIp = null;
                    byte[] data = client.Receive(ref remoteIp);
                    string message = Encoding.Unicode.GetString(data);

                    // додаємо отримане повідомлення в текстове поле
                    this.Invoke(new MethodInvoker(() =>
                    {
                        string time = DateTime.Now.ToShortTimeString();
                        richTextBox1.Text = time + " " + message + "\r\n"
                        + richTextBox1.Text;
                    }));
                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive) return;
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            nickName = textBox1.Text;
            textBox1.ReadOnly = true;
            try
            {
                client = new UdpClient(LOCALPORT);
                //під'єднання до групового розсилання
                client.JoinMulticastGroup(groupAddress, TTL);

                // задача на приймання повідомлень
                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start();

                // перше повідомлення про вхід нового користувача
                string message = nickName + " увійшов до чату";
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                button1.Enabled = false;
                button2.Enabled = true;
                button3.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string message = String.Format("{0}: {1}", nickName, textBox2.Text);
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                textBox2.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            string message = nickName + " покидає чат";
            byte[] data = Encoding.Unicode.GetBytes(message);
            client.Send(data, data.Length, HOST, REMOTEPORT);
            client.DropMulticastGroup(groupAddress);
            alive = false;
            client.Close();
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            textBox1.ReadOnly = false;
            textBox1.Clear();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (alive)
            {
                string message = nickName + " покидає чат";
                byte[] data = Encoding.Unicode.GetBytes(message);
                client.Send(data, data.Length, HOST, REMOTEPORT);
                client.DropMulticastGroup(groupAddress);
                alive = false;
                client.Close();
                button1.Enabled = true;
                button2.Enabled = false;
                button3.Enabled = false;
                richTextBox1.ReadOnly = false;
                richTextBox1.Clear();
            }
        }

        //колір шрифту
        private void button4_Click(object sender, EventArgs e)
        {
            ColorDialog dialog = new ColorDialog();
            dialog.ShowDialog();
            richTextBox1.ForeColor = dialog.Color;
        }

        //тип шрифту
        private void button5_Click(object sender, EventArgs e)
        {         
                FontDialog fontDialog = new FontDialog();
                if (fontDialog.ShowDialog() == DialogResult.OK)
                {
                    richTextBox1.Font = fontDialog.Font;
                }
        }

        //збереження
        private void button6_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                DefaultExt = "txt",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, richTextBox1.Text);
                    MessageBox.Show("Chat log saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving chat log: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
