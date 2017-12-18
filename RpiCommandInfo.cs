using SuperSocket.SocketBase.Protocol;

namespace TCPSERVER
{
    //如果只是通过该服务器想rpi传送信息的话
    //并没有用
    class RpiCommandInfo:IRequestInfo
    {
        public string Key
        {
            get; set;
        }

        /// <summary>
        /// 存储服务器解析得到的Rpi发的整个信息
        /// </summary>
        public string totalCmd_Rpi
        { get; set; }

        /// <summary>
        /// 表示Rpi是否成功接收到上一次的命令
        /// </summary>
        public bool isReceiveLastOrder_Rpi
        { get; set; }

        /// <summary>
        /// Rpi上传的测距信息
        /// </summary>
        public string disData_Rpi
        { get; set; }

        /// <summary>
        /// Rpi上传的测距信息-前
        /// </summary>
        public string disFront_Rpi
        { get; set; }

        /// <summary>
        /// Rpi上传的测距信息-后
        /// </summary>
        public string disBack_Rpi
        { get; set; }

        /// <summary>
        /// Rpi上传的测距信息-左
        /// </summary>
        public string disLeft_Rpi
        { get; set; }

        /// <summary>
        /// Rpi上传的测距信息-右
        /// </summary>
        public string disRight_Rpi
        { get; set; }

    }
}
