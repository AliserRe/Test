using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Server(10086);
        }

        static void Server(int port)
        {
            try
            {
                //监听端口
                TcpListener server = new TcpListener(IPAddress.Any, port);
                server.Start();
                Console.WriteLine("{0:HH:mm:ss}->监听端口{1}....", DateTime.Now, port);

                //等待请求
                while(true)
                {
                    try
                    {
                        //接受请求
                        TcpClient client = server.AcceptTcpClient();//挂起，等待连接请求
                        Console.WriteLine("{0:HH:mm:ss}->接收到请求，准备解析数据....", DateTime.Now);
                        IPEndPoint ipEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        NetworkStream stream = client.GetStream();
                        //解析数据，长度<1024字节
                        string data = string.Empty;
                        byte[] bytes = new byte[1024];
                        int length = stream.Read(bytes, 0, bytes.Length);
                        if (length > 0)
                        {
                            data = Encoding.Default.GetString(bytes, 0, length);
                            Console.WriteLine("{0:HH:mm:ss}->接受数据(from {1}:{2}):{3}",
                        DateTime.Now, ipEndPoint.Address, ipEndPoint.Port, data);
                        }
                        //返回状态
                        byte[] messages = Encoding.Default.GetBytes("对方成功收到信息");
                        stream.Write(messages, 0, messages.Length);

                        //关闭流和客户端
                        stream.Close();
                        client.Close();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("{0:HH:mm:ss}->Error:{1}....", DateTime.Now, e.Message);
                    }
                }
            }
            catch(SocketException socketEx)
            {
                //10013 The requested address is a broadcast address, but flag is not set.
                if(socketEx.ErrorCode == 10013)
                    Console.WriteLine("{0:HH:mm:ss}->启动失败,请检查{1}端口有无其他程序占用.", DateTime.Now, port);
                else
                    Console.WriteLine("{0:HH:mm:ss}->Error:{1}\n{2}....", DateTime.Now, socketEx.ErrorCode, socketEx.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0:HH:mm:ss}->Error: {1}", DateTime.Now, e.Message);
            }
            Console.ReadKey();
        }
    }
}
