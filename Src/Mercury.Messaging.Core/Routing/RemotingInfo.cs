using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Globalization;
using Mercury.Net;
using Mercury.Messaging.ServiceModel;

namespace Mercury.Messaging.Routing
{
    internal class RemotingInfo
    {
        internal RemotingInfo(string resourceOrNodeId, IPEndPoint endPoint)
            : this(resourceOrNodeId, endPoint, null)
        {
        }

        internal RemotingInfo(string resourceOrNodeId, IPEndPoint endPoint, Socket socket)
        {
            this.NodeId = RemotingInfo.GetNodeId(resourceOrNodeId);
            this.EndPoint = endPoint;
            this.Socket = socket;
            this.Key = endPoint.ToString();
        }

        internal string NodeId { get; private set; }
        internal IPEndPoint EndPoint { get; private set; }
        internal Socket Socket { get; set; }
        internal string Key { get; private set; }
        private int _lockReceive;

        internal bool AssignReceive()
        {
            if (Interlocked.CompareExchange(ref this._lockReceive, 1, 0) == 0)
                return true;
            return false;
        }

        internal bool HasReceive
        {
            get { return this._lockReceive == 1; }
        }

        internal static void TryEnsureSocket(RemotingInfo info)
        {
            if (info.Socket == null)
                info.Socket = TcpSockets.Connect(info.EndPoint);
            if (info.Socket != null && !info.Socket.Connected)
            {
                info.Socket.Shutdown(SocketShutdown.Both);
                info.Socket.Close();
                info.Socket = TcpSockets.Connect(info.EndPoint);
            }
        }

        internal static string GetNodeId(string resId)
        {
            Uri nodeUri = null;
            if (Uri.TryCreate(resId, UriKind.Absolute, out nodeUri))
                return string.Format(CultureInfo.InvariantCulture, "{0}/", nodeUri.GetLeftPart(UriPartial.Authority));
            return null;
        }
    }
}
