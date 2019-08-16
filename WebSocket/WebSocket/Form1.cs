using Fleck;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WebSocekt
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 
        /// </summary>
        WebSocketServer server = null;

        /// <summary>
        /// 
        /// </summary>
        Dictionary<string, List<IWebSocketConnection>> clientList = new Dictionary<string, List<IWebSocketConnection>>();

        /// <summary>
        /// 
        /// </summary>
        Dictionary<IWebSocketConnection, bool> allClients = new Dictionary<IWebSocketConnection, bool>();

        /// <summary>
        /// 
        /// </summary>
        bool isNeedRefreshList = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                switch (comboBox1.Text)
                {
                    case "所有客户端":
                        foreach (KeyValuePair<IWebSocketConnection, bool> item in allClients)
                        {
                            item.Key.Send(textBox1.Text);
                            richTextBox2.AppendText(item.Key.ConnectionInfo.ClientIpAddress + ":" + item.Key.ConnectionInfo.ClientPort + " - 发送成功!\n");
                        }
                        break;
                    case "未订阅连接类型":
                        foreach (KeyValuePair<IWebSocketConnection, bool> item in allClients)
                        {
                            if (!item.Value)
                            {
                                item.Key.Send(textBox1.Text);
                                richTextBox2.AppendText(item.Key.ConnectionInfo.ClientIpAddress + ":" + item.Key.ConnectionInfo.ClientPort + " - 发送成功!\n");
                            }
                        }
                        break;
                    default:
                        if (clientList.ContainsKey(comboBox1.Text))
                        {
                            foreach (IWebSocketConnection client in clientList[comboBox1.Text])
                            {
                                try
                                {
                                    client.Send(textBox1.Text);
                                    richTextBox2.AppendText(client.ConnectionInfo.ClientIpAddress + ":" + client.ConnectionInfo.ClientPort + " - 发送成功!\n");
                                }
                                catch
                                { }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                //WebSocekt 监听
                server = new WebSocketServer("ws://" + textBox2.Text + ":" + textBox3.Text);
                server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        if (!allClients.ContainsKey(socket))
                        {
                            allClients.Add(socket, false);
                        }
                        isNeedRefreshList = true;
                    };
                    socket.OnClose = () =>
                    {
                        foreach (KeyValuePair<string, List<IWebSocketConnection>> item in clientList)
                        {
                            if (item.Value.Contains(socket))
                            {
                                item.Value.Remove(socket);
                            }
                        }
                        if (allClients.ContainsKey(socket))
                        {
                            allClients.Remove(socket);
                        }
                        isNeedRefreshList = true;
                    };
                    socket.OnMessage = message =>
                    {
                        foreach (KeyValuePair<string, List<IWebSocketConnection>> item in clientList)
                        {
                            try
                            {
                                if (item.Value.Count > 0)
                                {
                                    if (item.Value.Contains(socket))
                                    {
                                        item.Value.Remove(socket);
                                    }
                                }
                            }
                            catch
                            { }
                        }
                        if (!string.IsNullOrEmpty(message))
                        {
                            string[] types = message.Split(',');
                            foreach (string type in types)
                            {
                                if (!clientList.ContainsKey(type))
                                {
                                    clientList.Add(type, new List<IWebSocketConnection>());
                                }
                                clientList[type].Add(socket);
                            }
                            if (allClients.ContainsKey(socket))
                            {
                                allClients[socket] = true;
                            }
                        }
                        else
                        {
                            allClients[socket] = false;
                        }
                        isNeedRefreshList = true;
                    };
                });
                button1.Enabled = true;
                button2.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (isNeedRefreshList)
            {
                isNeedRefreshList = false;
                StringBuilder sb = new StringBuilder();
                comboBox1.Items.Clear();
                comboBox1.Items.Add("所有客户端");
                foreach (KeyValuePair<IWebSocketConnection, bool> item in allClients)
                {
                    if (!item.Value)
                    {
                        if (!comboBox1.Items.Contains("未订阅连接类型"))
                        {
                            comboBox1.Items.Add("未订阅连接类型");
                        }
                        if (string.IsNullOrEmpty(sb.ToString()))
                        {
                            sb.AppendLine("[未订阅连接类型]");
                        }
                        sb.AppendLine(item.Key.ConnectionInfo.ClientIpAddress + ":" + item.Key.ConnectionInfo.ClientPort);
                    }
                }
                foreach (KeyValuePair<string, List<IWebSocketConnection>> item in clientList)
                {
                    try
                    {
                        if (item.Value.Count > 0)
                        {
                            comboBox1.Items.Add(item.Key);
                            sb.AppendLine("[" + item.Key + "]");
                            foreach (IWebSocketConnection item2 in item.Value)
                            {
                                sb.AppendLine(item2.ConnectionInfo.ClientIpAddress + ":" + item2.ConnectionInfo.ClientPort);
                            }
                        }
                    }
                    catch
                    { }
                }
                richTextBox1.Text = sb.ToString();
                comboBox1.SelectedIndex = 0;
            }
        }
    }
}
