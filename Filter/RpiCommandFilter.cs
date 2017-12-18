using SuperSocket.Facility.Protocol;
using System;

/// <summary>
/// ///  Rpi → CommandServer 通信格式
/// --------------------------------------------------------------
/// | 0 |  1   |  2    |  3    |   4  |  5  |  6  |   7   |  8   | 
/// --------------------------------------------------------------
/// | # | 100  | ,     | 100   | ,    | 100 | ,   | 100   |  $   |    
/// --------------------------------------------------------------
/// 1,    前    
/// 2，   右  
/// 5，   后
/// 7，   左
/// </summary>
///
//1：测距  2：是否成功接受Server命令
//测距：约定使用几位表示；接受命令：1位表示，1：成功接收，0：接收失败。

 namespace TCPSERVER.Filter
{
    /// <summary>
    ///固定长度
    /// </summary>
    class RpiCommandFilter : FixedSizeReceiveFilter<RpiCommandInfo>
    {
        string TAG = "CommandFilter:";

        public RpiCommandFilter() : base(19)
        {
        }

        protected override RpiCommandInfo ProcessMatchedRequest(byte[] buffer, int offset, int length, bool toBeCopied)
        {
            RpiCommandInfo instanceCommandInfo = new RpiCommandInfo();
            

            for (int i = 2; i < 17; i++)
                instanceCommandInfo.totalCmd_Rpi += System.Text.Encoding.Default.GetString(buffer, offset + i, 1);

            Console.WriteLine( TAG+"指令:" + instanceCommandInfo.totalCmd_Rpi);

            instanceCommandInfo.disFront_Rpi = System.Text.Encoding.Default.GetString(buffer, offset + 2, 1);
            instanceCommandInfo.disRight_Rpi = System.Text.Encoding.Default.GetString(buffer, offset + 4, 1);
            instanceCommandInfo.disBack_Rpi = System.Text.Encoding.Default.GetString(buffer, offset + 6, 1);
            instanceCommandInfo.disLeft_Rpi = System.Text.Encoding.Default.GetString(buffer, offset + 8, 1);


            return instanceCommandInfo;
        }

    }
}
 
 


/*分隔符
namespace TCPSERVER.Filter
{
    /// <summary>
    ///分隔符
    /// </summary>
    class RpiCommandFilter : CountSpliterReceiveFilter<RpiCommandInfo>
    {
        string TAG = "CommandFilter:";

        public RpiCommandFilter()  : base((byte)',', 5)
        {
        }

        protected override RpiCommandInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            RpiCommandInfo instanceCommandInfo = new RpiCommandInfo();
            for (int i = 1; i < length-1; i++)
                instanceCommandInfo.totalCmd_Rpi += System.Text.Encoding.Default.GetString(readBuffer, offset + i, 1);
            Console.WriteLine(TAG+"指令:" + instanceCommandInfo.totalCmd_Rpi);
           
            return instanceCommandInfo;
        }

    }
}

*/
