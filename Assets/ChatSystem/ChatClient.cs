using UnityEngine;
using System.Net.Sockets;
using System;

using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class ChatClient : MonoBehaviour
{
    public GameObject content;
    public InputField input;  //用于发送输入框的信息
    public InputField usernameInput;//客户端用户名
    public GameObject messageItem;//预制件的宽高
    public Button connectButton; //连接按钮

    TcpClient client;//客户端
    NetworkStream nstream;//网络数据流
    string ip = "192.168.10.5";//客户端IP
    int port = 10086;//客户端端口
    byte[] data;//客户端收发数据
    bool isConnect = false;

    List<GameObject> messageList;//存储接受的消息列表
    public int messagesMaxSize = 20;//存储接受的消息列表最大长度
    bool isNewMessage = false;//消息更新的标志位
    string newMessage;//接受到的最新信息
    //预制件的宽高
    float height = 0;
    float width = 0;
    void Start()
    {
        messageList = new List<GameObject>();
        messageList.Capacity = messagesMaxSize;

        //获取预设件的宽高
        width = messageItem.GetComponent<RectTransform>().rect.width;
        height = messageItem.GetComponent<RectTransform>().rect.height;

        //初始化组件
        if (content == null)
            content = transform.FindChild("Chat View/Viewport/Content").gameObject;
        if (input == null)
            input = transform.FindChild("InputField").GetComponent<InputField>();
        if (usernameInput == null)
            usernameInput = transform.FindChild("UsernameInput").GetComponent<InputField>();

        if (connectButton == null)
            connectButton = transform.FindChild("ConnectButton").GetComponent<Button>();
        //给连接按钮添加OnConnect事件,控制链接开启和关闭
        connectButton.onClick.AddListener(delegate { OnConnect(); });        
    }
    void Update()
    {
        if (isNewMessage)
        {//若存在新的消息，则添加消息更新界面
            AddMessage(newMessage);
            isNewMessage = false;
        }
    }
    public void OnDestroy()
    {
        Disconnect();
    }


    //连接按钮的点击事件调用Onconnect连接函数
    public void OnConnect()
    {
        isConnect = !isConnect;
        if (isConnect)
        {
            connectButton.transform.FindChild("Text").GetComponent<Text>().text = "断开连接";
            Connect();
        }
        else
        {
            connectButton.transform.FindChild("Text").GetComponent<Text>().text = "连接服务器";
            Disconnect();
        }
    }




    //中断网络连接
    void Disconnect()
    {
        //关闭流和客户端
        if(nstream.CanRead|| nstream.CanWrite)
            nstream.Close();
        if(client.Connected)
            client.Close();
    }

    //网络连接
    void Connect()
    {
        Info.debugStr = "";
        if ("" == usernameInput.text)
        {
            Info.debugStr = DateTime.Now + "->用户名不能为空";
            return;
        }
        try
        {
            //创建TCP连接
            client = new TcpClient(ip, port);
            nstream = client.GetStream();
            //先发送用户名
            SendMessage(usernameInput.text);
            //初始化收发数据
            data = new byte[client.ReceiveBufferSize];

            //开启异步从服务器获取消息，获取到的数据流存入data，回调ReceiveMessage方法
            nstream.BeginRead(data, 0, data.Length, AsyncReceive, null);
        }
        catch (SocketException socketEx)
        {//输出错误码
            Info.debugStr = DateTime.Now + "->" + socketEx.ErrorCode + ": " + socketEx.Message;
            //关闭流和客户端
            client.Close();
        }
        catch (Exception e)
        {   //显示错误信息
            Info.debugStr = DateTime.Now + "->" + e.Message;
            //断开连接
            Disconnect();
        }
    }

  

    //发送消息到服务器
    public void Send()
    {
        Info.debugStr = "";

        if ("" == input.text)
        {
            Info.debugStr = DateTime.Now + "->消息不能为空";
            return;
        }
        //发送消息到服务器
        SendMessage(input.text);
        //清空输入框
        input.text = "";
    }
    new public void SendMessage(string message)
    {
        try
        {   //把输入框的消息写入数据流，发送服务器
            //NetworkStream stream = client.GetStream();
            byte[] bytes = System.Text.Encoding.Default.GetBytes(message);
            nstream.Write(bytes, 0, bytes.Length);
        }
        catch (Exception e)
        {
            //显示错误信息
            Info.debugStr = DateTime.Now + "->" + e.Message;
            //断开连接
            Disconnect();
        }
    }


   
    //异步处理从服务器获得的消息
    void AsyncReceive(IAsyncResult ar)
    {
        Info.debugStr = "";
        try
        {
            int bytesRead = client.GetStream().EndRead(ar);
            if (bytesRead < 1)
            {   //接收不到任何消息
                return;
            }
            else
            {
                //把接收到的消息编码为系统默认的编码
                newMessage = System.Text.Encoding.Default.GetString(data, 0, bytesRead);
                //设置标志位，在UI上添加一条消息
                isNewMessage = true;
            }
            //再次开启异步从服务器获取消息，形成循环等待服务器消息
            nstream.BeginRead(data, 0, client.ReceiveBufferSize, AsyncReceive, null);
        }
        catch (Exception e)
        {
            Info.debugStr = DateTime.Now + "->" + e.Message;
            //断开连接
            Disconnect();
        }
    }
    
    
    
    //在UI上添加一条消息
    private void AddMessage(string uiMessage)
    {
        //把接收到的消息存入消息列表中
        GameObject item = Instantiate<GameObject>(messageItem);
        item.GetComponent<Text>().text = uiMessage;
        //超出列表上限时，再删除第一条
        if (messageList.Count > messageList.Capacity)
            messageList.RemoveAt(0);
        messageList.Add(item);

        //刷新界面
        ShowMessages();
    }
   
    private void ShowMessages()
    {
        int i = 0;
        foreach (GameObject m in messageList)
        {
            //设置为显示内容面板的子对象，便于位置的设置位置
            m.transform.SetParent(content.transform, false);
            //按序设置位置
            RectTransform rf = m.GetComponent<RectTransform>();
            rf.sizeDelta = new Vector2(width, height);
            rf.localPosition = new Vector2(0, -i * height);
            ++i;
        }
        content.GetComponent<RectTransform>().sizeDelta = new Vector2(width, i * height);
    }
}