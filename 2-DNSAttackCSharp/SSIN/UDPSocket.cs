using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    public class UDPSocket {
        private IPEndPoint localIpEndPoint;
        private UdpClient udpClient;

        public UDPSocket(int port) {
            localIpEndPoint = new IPEndPoint(IPAddress.Any, port);
            udpClient = new UdpClient(localIpEndPoint);
        }

        public void SendUDPPacket(string host, int port, byte[] data) {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            if (addresses.Length == 0)
                throw new Exception("No IP found for the indicated host.");
            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(addresses[0].ToString()), port));
        }

        public UDPPacketData ReceiveUDPPacket() {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.Receive(ref remoteIpEndPoint);
            return new UDPPacketData(localIpEndPoint, remoteIpEndPoint, data);
        }

        public void Close() {
            udpClient.Close();
        }
    }
}
