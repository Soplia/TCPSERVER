using System;
using SuperSocket.Facility.Protocol;

namespace TCPSERVER.Filter
{
    class UdpServerFilter : CountSpliterReceiveFilter<UdpRequestInfo>
    {
        string TAG = "UdpServerFilter:";
        public UdpServerFilter() : base((byte)'#', 2)
        {
        }
        protected override UdpRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            UdpRequestInfo instanceAppInfo = new UdpRequestInfo();


            //分割字符串
            string request = "";
            for (int i = 0; i < length; i++)
                request += System.Text.Encoding.Default.GetString(readBuffer, offset + i, 1);
            string[] requestArr = new string[11];
            requestArr = request.Split('#');

            //获取正常显示的命令
            for (int i = 1; i < 2; i++)
                instanceAppInfo.totalCmd_App += requestArr[i];
            //Console.WriteLine(TAG + "App请求:" + instanceAppInfo.totalCmd_App);
            Console.WriteLine(TAG + "App请求:" );

            return instanceAppInfo;
        }
    }
}
