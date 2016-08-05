using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocketChatClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client("192.168.10.5", 10086, "加油啊！显康！你的未来不是梦！");//加油啊！显康！你的未来不是梦！
        }

        static void Client(string ip,int port,string message)
        {
            try
            {
                //发送数据
                TcpClient client = new TcpClient(ip, port);
                IPEndPoint ipEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                NetworkStream stream = client.GetStream();
                byte[] messages = Encoding.Default.GetBytes(message);
                stream.Write(messages, 0, messages.Length);
                Console.WriteLine("{0:HH:mm:ss}->发送数据(to {1}) : {2}", DateTime.Now, ip, message);

                //接受数据，长度<1024
                byte[] bytes = new byte[1024];
                string data = string.Empty;
                int length = stream.Read(bytes, 0, bytes.Length);
                if(length>0)
                {
                    data = Encoding.Default.GetString(bytes, 0, length);
                    Console.WriteLine("{0:HH:mm:ss}->接受数据(from {1}:{2}):{3}", 
                        DateTime.Now, ipEndPoint.Address, ipEndPoint.Port, data);
                }

                //关闭流和客户端
                stream.Close();
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("{0:HH:mm:ss}->Error: {1}",DateTime.Now,e.Message);
            }
            Console.ReadKey();
        }
    }
}
