using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace ChatSystem
{
    class ChatServerHandle
    {
        public static Hashtable Clients = new Hashtable();//客户端连接记录表
        public TcpClient client;//客户端
        public string username;//客户端用户名

        public string ip;//客户端IP
        public int port;//客户端端口

        public byte[] data;//客户端收发数据

        bool firstConnet = true;

        public ChatServerHandle(TcpClient client)
        {
            this.client = client;
            
            //保存客户端IP和端口
            IPEndPoint ipEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            this.ip = ipEndPoint.Address.ToString();
            this.port = ipEndPoint.Port;

            //初始化收发数据
            this.data = new byte[client.ReceiveBufferSize];

            //开启异步从客户端获取消息，获取到的数据流存入data，回调ReceiveMessage方法
            client.GetStream().BeginRead(data, 0, data.Length, ReceiveMessage, null);
        }
        //向客户端发送消息
        public void SendMessage(string message)
        {
            try
            {
                NetworkStream stream;
                lock(client.GetStream())
                {
                    stream = client.GetStream();
                }
                byte[] bytes = Encoding.Default.GetBytes(message);
                stream.Write(bytes, 0, bytes.Length);
                //stream.Flush();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0:HH:mm:ss}->[SendMessage]Error: {1}", DateTime.Now, e.Message);
            }
        }
        //从客户端接受到的数据，再广播给所有客户端
        public void ReceiveMessage(IAsyncResult ar)
        {
           try
            {
                int bytesRead;
                lock(client.GetStream())
                {
                    bytesRead = client.GetStream().EndRead(ar);
                }
                if (bytesRead < 1)
                {//接收不到任何数据，则删除在客户端连接记录表
                    Clients.Remove(this.ip);
                    //向所有客户端广播该用户的下线信息
                    Broadcast("<color=#00ffffff>"+this.username + "</color> 于" + DateTime.Now + " 已下线....");
                    return;
                }
                else
                {
                    string recMessage = Encoding.Default.GetString(data, 0, bytesRead);
                    if (firstConnet)
                    {//第一次连接，将客户端信息记录入客户端连接记录表中
                        Clients.Add(this.ip,this);
                        //获取发送的用户名信息
                        this.username = recMessage;
                        //向所有客户端广播该用户的上线信息
                        Broadcast("<color=#00ffffff>" + this.username + "</color> 于" + DateTime.Now+ " 已上线....");
                        //不在是第一次连接
                        firstConnet = false;
                    }
                    else
                    {
                        //向所有客户端广播该用户发送的
                        Broadcast(DateTime.Now + " ->【"+ this.username+"】" + recMessage);
                    }
                    //Console.WriteLine(recMessage);
                }
                lock(client.GetStream())
                {
                    //再次开启异步从服务器获取消息，形成循环
                    client.GetStream().BeginRead(data, 0, client.ReceiveBufferSize, ReceiveMessage, null);
                }
            }
            catch(Exception e)
            {
                //删除连接记录
                Clients.Remove(this.ip);
                //向所有客户端广播该用户的下线信息
                Broadcast("<color=#00ffffff>" + this.username + "</color> 于" + DateTime.Now + " 已下线....");

                Console.WriteLine("{0:HH:mm:ss}->[ReceiveMessage]Error: {1}", DateTime.Now, e.Message);
            }
        }

        //向所有连接到的客户端广播
        public void Broadcast(string message)
        {
            Console.WriteLine(message);
            foreach (DictionaryEntry item in Clients)
            {
                ChatServerHandle c = item.Value as ChatServerHandle;
                c.SendMessage(message);
            }
        }
    }
}
