using SuperSocket.SocketBase.Protocol;

/// <summary>
///   App → AppServer
/// ____________________________________________________________________________________
/// |用户名| 密码  | 模式     | 前      | 后    | 左    | 右    | 抬    | 低 | 实时视频 | 
/// -------------------------------------------------------------------------------------
/// |0     | FF/00 | 00/01/02/|  00/01  | 00/01 | 00/01 | 00/01 | 00/01 | 00/01 | 00/01 |  
/// -------------------------------------------------------------------------------------
/// |0-3   | 4-7   | 8        |   9     | 10    |   11  |  12   |  13   |  14   |  15   |
/// ------------------------------------------------------------------------------------- 
/// 
/// </summary>
namespace TCPSERVER
{
    class MyAppRequestInfo : IRequestInfo
    {
        public string Key
        {
            get; set;
        }

        //app请求
        public string totalCmd_App
        { get; set; }

        //app请求类型
        public string requestType_App
        { get; set; }

        //app请求是否出错
        public bool requestErr_App
        { get; set; }

        //用户名
        public string username
        { get; set; }

        //用户密码
        public string password
        { get; set; }

        //前进
        public string front
        { get; set; }

        //后退
        public string back
        { get; set; }

        //向左
        public string left
        { get; set; }

        //向右
        public string right
        { get; set; }

        //抬头
        public string up
        { get; set; }

        //低头
        public string down
        { get; set; }

        //视频
        public string video
        { get; set; }
    }
}

