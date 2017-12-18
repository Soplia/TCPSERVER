using System;
using SuperSocket.Facility.Protocol;
using System.Drawing;
using System.IO;

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

namespace TCPSERVER.Filter
{
    //固定开始结尾符的FILTER
    class RpiReceiveFilter : BeginEndMarkReceiveFilter<RpiRequestInfo>
    {
        string TAG = "RpiReceiveFilter:";

        private readonly static byte[] BeginMark = System.Text.Encoding.Default.GetBytes("#!#!#!#!");
        private readonly static byte[] EndMark = System.Text.Encoding.Default.GetBytes("$!$!$!$!");
        
        int nameOfPic =1;//图片名字

        public RpiReceiveFilter(): base(BeginMark, EndMark) //传入开始标记和结束标记
        {
        }
        
        protected override RpiRequestInfo ProcessMatchedRequest(byte[] readBuffer, int offset, int length)
        {
            RpiRequestInfo instanceRpi = new RpiRequestInfo();
            instanceRpi.reqType_Rpi = "0";
            instanceRpi.isRequestErr_Rpi = false;

            //DateTime startTime = DateTime.Now;
            //Console.WriteLine(startTime.Second+":"+startTime.Millisecond+TAG + "开始写入文件...");
            //Console.WriteLine(TAG + "写入文件开始...");
            string file = nameOfPic + ".jpg";
            FileStream fs = new FileStream("C:\\inetpub\\wwwroot\\image\\" + file, FileMode.Create);
            
            //FileStream fs = new FileStream("C:\\Program Files\\Apache Software Foundation\\Tomcat8.0\\webapps\\ROOT\\image" + file, FileMode.Create);
            fs.Write(readBuffer, 8, length - 8);
            fs.Close();
            //Console.WriteLine(TAG + "写入文件结束...");
            instanceRpi.picData_Rpi = "" + nameOfPic;
            nameOfPic++;
            //DateTime endTime = DateTime.Now;
            //Console.WriteLine(TAG + "写入文件需要的时间:" + (endTime.Millisecond - startTime.Millisecond));

            return instanceRpi;

            //以后使用
            //readBuffer[offset + i].ToString("X2");
            //System.Text.Encoding.Default.GetString(readBuffer, offset + i, 1);
            
        }
       
        //byte数组转化为Image
        public static Image BytesToImage(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Image image =Image.FromStream(ms);
            return image;
        }

        //将十六进制字符串转化为byte数组
        public static byte[] hexStringToByte(String hexStr)
        {
            String str = "0123456789ABCDEF";
            char[] hexs = hexStr.ToCharArray();
            byte[] bytes = new byte[hexStr.Length / 2];
            int n;
            for (int i = 0; i < bytes.Length; i++)
            {
                n = str.IndexOf(hexs[2 * i]) * 16;
                n += str.IndexOf(hexs[2 * i + 1]);
                bytes[i] = (byte)(n & 0xff);
            }
            return bytes;
        }
    }
    
}
