using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Quobject.SocketIoClientDotNet.Client;

namespace EmbedChatWinform
{
    public partial class Form1 : Form
    {
        public Socket socket;
        public string serverUrl = "http://bakechat.com:666/";
        public string username = "Hacker";

        public Form1()
        {
            InitializeComponent();

            socket = IO.Socket(serverUrl);
            socket.Close(); //close any orphaned connections
            socket = IO.Socket(serverUrl);
            socket.On(Socket.EVENT_DISCONNECT, () => SocketClosed());
            socket.On(Socket.EVENT_ERROR, e => SocketError(e));

            // register for 'chat message' events
            socket.On("chat message", (data) =>
            {
                UpdateChatText(data.ToString());
            });

            // make the socket.io connection
            socket.Connect();
            SocketOpened();

            username = Prompt.ShowDialog("Username", "Enter a username");
        }

        public void UpdateChatText(string message)
        {
            if (txtChat.InvokeRequired)
            {
                txtChat.Invoke((MethodInvoker)(() => txtChat.Text += "\r\n" + message));
                txtChat.Invoke((MethodInvoker)(() => txtChat.SelectionStart = txtChat.Text.Length));
                txtChat.Invoke((MethodInvoker)(() => txtChat.ScrollToCaret()));
             }
            else
            {
                txtChat.Text += "\r\n" + message;
                txtChat.SelectionStart = txtChat.Text.Length;
                txtChat.ScrollToCaret();
            }
            if (txtMessage.InvokeRequired)
                txtMessage.Invoke((MethodInvoker)(() => txtMessage.Focus()));
            else
                txtMessage.Focus();
        }

        public void SocketOpened()
        {
            this.Text += " - " + serverUrl;
            UpdateChatText("Connected to " + serverUrl);
            socket.Emit("chat message", "Winforms Client Connected from " + Environment.MachineName);
        }

        public static void SocketError(object e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            dynamic args = e;
            Console.WriteLine("Socket error: {0}", args.Message);
            Console.ResetColor();
        }

        public static void SocketClosed()
        {
            Console.WriteLine("Socket closed");
        }

        private void txtMessage_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13 && !e.Shift)
            {
                btnSend_Click(sender, e);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtMessage.Text.Trim() != string.Empty)
            {
                socket.Emit("chat message", username + ": " + txtMessage.Text.Trim());
                txtMessage.Text = "";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            socket.Emit("chat message", username + " left the chat");
            socket.Close();
        }

        private void changeUsernameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string currentUsername = username;
            username = Prompt.ShowDialog("Username", "Enter a username");
            socket.Emit("chat message", currentUsername + " changed their name to " + username);
        }
    }
}
