using System;
using SuperSocket.Facility.Protocol;

///   App → AppServer
/// _________________________________________________________________________________________________________
/// |#|  用户名 |#| 密码 |#| 模式 |# | 前 |# | 后 |# | 左 |# | 右 |# | 抬 |# | 低 |# | 实时视频 |#  | 
/// ---------------------------------------------------------------------------------------------------------
/// |#|    1-4  |5| 6-8  |9| 10   |11| 12 |13| 14 |15|16  |17| 18 |19| 20 |21| 22 |23|  24      |25 |
/// ---------------------------------------------------------------------------------------------------------
/// |# |0       | FF/00 | 01/02/03/|  00/01  | 00/01 | 00/01 | 00/01 | 00/01 | 00/01 | 00/01 |  
/// --------------------------------------------------------------------------------------------------------- 
/// 
///使用固定分割符的过滤器（#）
///  MODE
/// 0000    APP DOWN
/// 0001    APP UP
/// 0010    APP CONTROL
/// 0011    APP LOGIN
/// 0100    APP REGISTE
/// 0101    APP IMAGE INSTRUCTION

namespace TCPSERVER.Filter
{
    class MyAppReceiveFilter : CountSpliterReceiveFilter<MyAppRequestInfo>
    {
        string TAG = "AppReceiveFilter:";

        public MyAppReceiveFilter() : base((byte)'#', 11)
        {
        }
        
        protected override MyAppRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            DateTime endTime = DateTime.Now;
            MyAppRequestInfo instanceAppInfo = new MyAppRequestInfo();
            
            //分割字符串
            string request = "";
            for(int i=0;i<length;i++)
                request+= System.Text.Encoding.Default.GetString(readBuffer, offset + i, 1);
            string [] requestArr=new string[11];
            requestArr = request.Split('#');
            
            //获取正常显示的命令
            for (int i = 1; i < 11; i++)
                instanceAppInfo.totalCmd_App += requestArr[i];
            
            instanceAppInfo.username = requestArr[1];
            instanceAppInfo.password = requestArr[2];
            //登录
            if (requestArr[3].CompareTo("0011")==0)
            {
                instanceAppInfo.requestType_App = "0011";
                instanceAppInfo.requestErr_App = false;
                return instanceAppInfo;
            }
            //注册
            else if (requestArr[3].CompareTo("0100") == 0)
            {
                instanceAppInfo.requestType_App = "0100";
                instanceAppInfo.requestErr_App = false;
                return instanceAppInfo;
            }
            //控制
            else if (requestArr[3].CompareTo("0010") == 0)
            {
                Console.WriteLine("控制kaish");
                instanceAppInfo.requestType_App = "0010";
                instanceAppInfo.requestErr_App = false;
                instanceAppInfo.front = requestArr[4];
                instanceAppInfo.back = requestArr[5];
                instanceAppInfo.left = requestArr[6];
                instanceAppInfo.right = requestArr[7];
                instanceAppInfo.up = requestArr[8];
                instanceAppInfo.down = requestArr[9];
                instanceAppInfo.video = "0";
                return instanceAppInfo;
            }
            //控制停止
            else if (requestArr[3].CompareTo("0111") == 0)
            {
                Console.WriteLine("控制停止");
                instanceAppInfo.requestType_App = "0111";
                instanceAppInfo.requestErr_App = false;
                return instanceAppInfo;
            }
            //APP DOWN
            else if(requestArr[3].CompareTo("0000") == 0)
            {
                instanceAppInfo.requestType_App = "0000";
                return instanceAppInfo;
            }
            //APP UP
            else if (requestArr[3].CompareTo("0001") == 0)
            {
                instanceAppInfo.requestType_App = "0001";
                return instanceAppInfo;
            }
            //直接将图片信息发送6969
            else if (requestArr[3].CompareTo("0101") == 0)
            {
                instanceAppInfo.requestType_App = "0101";
                instanceAppInfo.requestErr_App = false;
                instanceAppInfo.front = "0";
                instanceAppInfo.back = "0";
                instanceAppInfo.left = "0";
                instanceAppInfo.right = "0";
                instanceAppInfo.up = "0";
                instanceAppInfo.down = "0";
                instanceAppInfo.video = requestArr[10];
                return instanceAppInfo;
            }
            else
            {
                instanceAppInfo.requestErr_App = true;
                return instanceAppInfo;
            }
        }
    }

}
        
