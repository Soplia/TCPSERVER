using System;
using System.Net;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;

namespace TCPSERVER
{
    class RequestInfoTest : IReceiveFilterFactory<MyAppRequestInfo>
    {
        public IReceiveFilter<MyAppRequestInfo> CreateFilter(IAppServer appServer, IAppSession appSession, IPEndPoint remoteEndPoint)
        {
            throw new NotImplementedException();
        }
    }
}
