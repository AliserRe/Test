using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            //设置监听的端口号
            Console.WriteLine("请输入服务器需要监听的端口: ");
            string input = Console.ReadLine();
            int port = int.Parse(input);
            //调用方法启动服务器
            Server(port);
        }

        static void Server(int port)
        {
            //初始化服务器ip
            //IPAddress localAddress = IPAddress.Parse("127.0.0.1");
            //设置监听
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            //提示信息
            Console.WriteLine("{0:HH:mm:ss}->监听端口{1}....", DateTime.Now, port);

            //循环等待客户端的连接请求
            while(true)
            {
                ChatServerHandle usr = new ChatServerHandle(listener.AcceptTcpClient());
                Console.WriteLine(usr.ip + "加入聊天室");
            }
        }

    }
}
