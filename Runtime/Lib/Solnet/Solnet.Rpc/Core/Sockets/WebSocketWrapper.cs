using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Solnet.Rpc.Core.Sockets
{
    internal class WebSocketWrapper : IWebSocket
    {
        private readonly ClientWebSocket webSocket;

        public WebSocketState State => webSocket.State;

        internal WebSocketWrapper(ClientWebSocket webSocket)
        {
            this.webSocket = webSocket;
        }

        public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            => webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
            => webSocket.ConnectAsync(uri, cancellationToken);

        public Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            return webSocket.ReceiveAsync(new ArraySegment<byte>(buffer.Array), cancellationToken);
        }

        public Task SendAsync(List<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            => webSocket.SendAsync(new ArraySegment<byte>(buffer.ToArray()), messageType, endOfMessage, cancellationToken);

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    webSocket.Dispose();
                }

                disposedValue = true;
            }
        }

        #endregion IDisposable Support
    }
}