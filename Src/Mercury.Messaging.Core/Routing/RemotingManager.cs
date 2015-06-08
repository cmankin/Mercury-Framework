using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Mercury;
using Mercury.Net;
using Mercury.Messaging.Channels;
using Mercury.Messaging.Serialization;
using Mercury.Messaging.ServiceModel;

namespace Mercury.Messaging.Routing
{
    internal static class RemotingManager
    {
        private static object cacheLock = new object();
        private static ConcurrentDictionary<string, RemotingInfo> _registeredNodes = new ConcurrentDictionary<string, RemotingInfo>();
        private static RingCache<RemotingInfo> _cache = new RingCache<RemotingInfo>(30, _DisposeCacheItem);
        private static ConcurrentDictionary<int, PostedOperation> _operations = new ConcurrentDictionary<int, PostedOperation>();
        private static int _currentId;

        #region Connection Cache
        internal static int CacheSize
        {
            get { return _cache.BufferSize; }
            set
            {
                if (_cache.BufferSize != value)
                {
                    lock (cacheLock)
                    {
                        if (_cache.BufferSize != value)
                        {
                            _cache.Clear(true);
                            _cache = new RingCache<RemotingInfo>(value, _DisposeCacheItem);
                        }
                    }
                }
            }
        }

        private static void _DisposeCacheItem(RemotingInfo info)
        {
            if (info != null && info.Socket != null)
            {
                info.Socket.Shutdown(SocketShutdown.Both);
                info.Socket.Close();
                info.Socket = null;
            }
        }

        internal static bool ContainsNode(string nodeId)
        {
            if (nodeId == null)
                return false;
            return _registeredNodes.ContainsKey(nodeId);
        }

        internal static RemotingInfo TryResolve(string nodeId)
        {
            if (nodeId != null && _registeredNodes.ContainsKey(nodeId))
                return _registeredNodes[nodeId];
            return null;
        }

        internal static RemotingInfo TryResolve(IPEndPoint endPoint)
        {
            if (endPoint != null)
                return _cache.GetValue(endPoint.ToString());
            return null;
        }

        internal static void Cache(IPEndPoint endPoint)
        {
            if (endPoint == null)
                return;
            RemotingManager.Cache(string.Empty, endPoint);
        }

        internal static void Cache(string resourceOrNodeId, IPEndPoint endPoint)
        {
            if (resourceOrNodeId == null || endPoint == null)
                return;
            RemotingManager.Cache(new RemotingInfo(resourceOrNodeId, endPoint));
        }

        internal static void Cache(RemotingInfo info)
        {
            RemotingManager._CacheRemotingInfo(info);
        }

        internal static void Register(string nodeId, IPEndPoint endPoint)
        {
            if (nodeId == null || endPoint == null)
                return;
            if (!_registeredNodes.ContainsKey(nodeId))
                RemotingManager.Register(new RemotingInfo(nodeId, endPoint, TcpSockets.Connect(endPoint)));
        }

        internal static void Register(RemotingInfo info)
        {
            if (info != null && info.NodeId != null && info.EndPoint != null)
            {
                _registeredNodes.TryAdd(info.NodeId, info);
                RemotingManager._CacheRemotingInfo(info);
            }
        }

        internal static void Unregister(string nodeId)
        {
            RemotingInfo info = null;
            if (_registeredNodes.ContainsKey(nodeId))
                _registeredNodes.TryRemove(nodeId, out info);
        }

        private static void _CacheRemotingInfo(RemotingInfo info)
        {
            _cache.Add(info.Key, info);
        }
        #endregion

        internal static int GetOperationId()
        {
            if (Interlocked.CompareExchange(ref _currentId, 0, int.MaxValue) == int.MaxValue)
                return 0;
            return Interlocked.Increment(ref _currentId);
        }

        internal static void RegisterOperation(int operationid, RemotingChannel channel,TimeSpan timeout)
        {
            if (timeout.TotalMilliseconds == -1.0)
                timeout = TimeSpan.FromHours(1.0);
            var store = new PostedOperation(operationid, channel, timeout);
            _operations.TryAdd(store.OperationId, store);
        }

        internal static void PostError(int operationId, Exception ex)
        {
            PostedOperation store = null;
            if (_operations.TryGetValue(operationId, out store))
            {
                if (store.Channel != null && store.Channel.ErrorHandler != null)
                    store.Channel.ErrorHandler.Invoke(operationId, ex);
            }
            RemotingManager.ExpireOperations();
        }

        internal static void ExpireOperations()
        {
            // Remove expired operations.
            IList<PostedOperation> expired = new List<PostedOperation>();
            foreach (KeyValuePair<int, PostedOperation> kv in _operations)
            {
                if (kv.Value != null && kv.Value.IsExpired)
                    expired.Add(kv.Value);
            }
            foreach (PostedOperation po in expired)
            {
                PostedOperation outVal = null;
                _operations.TryRemove(po.OperationId, out outVal);
            }
        }

        internal static bool Write(Socket socket, byte[] packet, int messageId)
        {
            if (MessagingService._hostedListener != null)
            {
                // Ensure read
                MessagingService._hostedListener.AssignTcpSocketHandler(socket);
                // Send
                SocketError s_err;
                socket.Send(packet, 0, packet.Length, SocketFlags.None, out s_err);
                if (s_err != SocketError.Success)
                    throw new SocketException((int)s_err);
                return true;
            }
            return false;
        }
    }
}
