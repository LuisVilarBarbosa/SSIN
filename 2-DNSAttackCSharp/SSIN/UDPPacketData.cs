using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    public class UDPPacketData {
        public string SenderIP { get; }
        public string ReceiverIP { get; }
        public int SenderPort { get; }
        public int ReceiverPort { get; }
        public byte[] Data { get; }

        public UDPPacketData(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, byte[] data) {
            SenderIP = remoteEndPoint.Address.ToString();
            ReceiverIP = localEndPoint.Address.ToString();
            SenderPort = remoteEndPoint.Port;
            ReceiverPort = localEndPoint.Port;
            Data = data;
        }
    }
}
