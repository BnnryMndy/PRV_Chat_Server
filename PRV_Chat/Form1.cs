using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace PRV_Chat
{
    public partial class Form1 : Form
    {
        
        List<string> messages = new List<string>();
        int clientCount = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(this.ClientThread);
            thread.Start();
        }

        public void ClientThread()
        {
            NamedPipeServerStream ss = new NamedPipeServerStream("server", PipeDirection.InOut, 5, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            ss.WaitForConnection();
            ChangeClientCount(1);

            Thread thread = new Thread(ClientThread);
            thread.Start();

            StreamReader sr = new StreamReader(ss);
            StreamWriter sw = new StreamWriter(ss);

            int currentThreadMessages = 0; //если поставить 0, то сервер отправит всю историю чата. Если messages.Count, то сервер покажет только сообщения с момента входа
            Task<string> task = ReadLineAsync(sr);

            while(true)
            {
                if (task.IsCompleted)
                {
                    if (string.IsNullOrEmpty(task.Result))
                        break;

                    task = ReadLineAsync(sr);
                }

                while (currentThreadMessages < messages.Count)
                {
                    sw.WriteLine(messages[currentThreadMessages]);
                    sw.Flush();
                    currentThreadMessages++;
                }

            }
            ChangeClientCount(-1);
            ss.Dispose();
        }

        private async Task<string> ReadLineAsync(StreamReader sr)
        {
            string st = await sr.ReadLineAsync();
            if (!string.IsNullOrEmpty(st))
            {
                messages.Add(st);
                AddMsgToList(st);
            }
                
            return st;
        }

        private void ChangeClientCount(int cnt)
        {
            if(this.InvokeRequired)
            {
                Action<int> tmp = this.ChangeClientCount;
                this.Invoke(tmp, cnt);
                return;
            }

            clientCount += cnt;
            label1.Text = "Clients connected: " + clientCount.ToString();
        }

        private void AddMsgToList(string msg)
        {
            if (this.InvokeRequired)
            {
                Action<string> tmp = this.AddMsgToList;
                this.Invoke(tmp, msg);
                return;
            }
            listBox1.Items.Add(msg);

        }
    }
}
