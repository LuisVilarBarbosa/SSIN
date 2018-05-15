using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    public class UDPPacketData {
        private string senderIP;
        private string receiverIP;
        private int senderPort;
        private int receiverPort;
        private byte[] data;

        public UDPPacketData(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, byte[] data) {
            this.senderIP = remoteEndPoint.Address.ToString();
            this.receiverIP = localEndPoint.Address.ToString();
            this.senderPort = remoteEndPoint.Port;
            this.receiverPort = localEndPoint.Port;
            this.data = data;
            /*string info = packet.toString();
            Pattern pattern = Pattern.compile(".*\\/([0-9.]+)->\\/([0-9.]+).+ ([0-9]+) > ([0-9]+).*");
            Matcher matcher = pattern.matcher(info);
            if (matcher.matches()) {
                senderIP = matcher.group(1);
                receiverIP = matcher.group(2);
                senderPort = Integer.parseInt(matcher.group(3));
                receiverPort = Integer.parseInt(matcher.group(4));
            } else
                throw new IllegalArgumentException();*/
        }

        public string GetSenderIP() {
            return senderIP;
        }

        public string GetReceiverIP() {
            return receiverIP;
        }

        public int GetSenderPort() {
            return senderPort;
        }

        public int GetReceiverPort() {
            return receiverPort;
        }

        public byte[] GetData() {
            return data;
        }
    }
}
