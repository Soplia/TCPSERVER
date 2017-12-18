namespace TCPSERVER
{
    interface IDespatchServer
    {
        //发送正常消息.发送给app图片名字，发送给rpi控制命令信息.
        void DispatchNormalMessage(string macKey, string message);

        //Server → App 发送错误消息.根据错误的类型发送不同的消息
        void ServerDispatchDisMessageToApp(string message);

        //发送App Down给Rpi和Rpicommand
        void DispatchAppDownMessageToRpi(string macKey, string message);
         //Server → Rpi 发送错误消息.根据错误的类型发送不同的消息
         //void ServerDispatchErrMessageToRpi(string mode);

        //Server → App 发送错误消息.根据错误的类型发送不同的消息
        //void ServerDispatchErrMessageToApp(string mode);

    }
}
