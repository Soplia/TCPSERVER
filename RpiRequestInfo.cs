using SuperSocket.SocketBase.Protocol;

/// <summary>
/// ///  Rpi → Server 通信格式
/// ---------------------------------------
/// | 0-3 |        4  -  x-1    |x - x+3  | 
/// ---------------------------------------
/// | #   |                     |  $      | 
/// ---------------------------------------
/// 0-3,    固定开头    
/// 4，     消息类型  0：图片  
/// 5-x-1， 消息主体
/// x-x+3， 固定结尾
/// </summary>
/// 

namespace TCPSERVER
{
    class RpiRequestInfo : IRequestInfo
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
        /// 表示Rpi请求消息是否错误
        /// </summary>
        public bool isRequestErr_Rpi
        { get; set; }

        /// <summary>
        /// 表示Rpi是否成功接收到上一次的命令
        /// </summary>
        public bool isReceiveLastOrder_Rpi
        { get; set; }

        /// <summary>
        /// 标识Rpi发的信息的类型
        /// </summary>
        public string reqType_Rpi
        { get; set; }

        /// <summary>
        /// RpiMac地址，唯一标识一台Rpi
        /// </summary>
        public string macAddres_Rpi
        { get; set; }

        /// <summary>
        /// Rpi上传的图片信息
        /// </summary>
        public string picData_Rpi
        { get; set; }
    }

}
