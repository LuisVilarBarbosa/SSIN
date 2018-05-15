/* Based on:
 * https://www.codeproject.com/Questions/194927/c-how-to-send-raw-packets
 * https://www.tutorialspoint.com/ipv4/ipv4_packet_structure.htm
 * https://en.wikipedia.org/wiki/User_Datagram_Protocol
 * https://en.wikipedia.org/wiki/IPv4 */

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    public class MySocket {
        private static IPAddress localhost = IPAddress.Loopback;

        private static byte[] GenerateUDPHeader(int sourcePort, int destinationPort, int ipPacketLength) {
            int udpHeaderLength = 8;
            int udpPacketLength = udpHeaderLength + ipPacketLength;
            byte checksum = 0;   // not implemented by us - optional
            byte[] udpHeader = new byte[udpHeaderLength];
            udpHeader[0] = Convert.ToByte(sourcePort >> 8);
            udpHeader[1] = Convert.ToByte(sourcePort & 0xff);
            udpHeader[2] = Convert.ToByte(destinationPort >> 8);
            udpHeader[3] = Convert.ToByte(destinationPort & 0xff);
            udpHeader[4] = Convert.ToByte(udpPacketLength >> 8);
            udpHeader[5] = Convert.ToByte(udpPacketLength & 0xff);
            udpHeader[6] = checksum;
            udpHeader[7] = checksum;
            return udpHeader;
        }

        /* Based on:
         * https://cyb3rspy.wordpress.com/2008/03/27/ip-header-checksum-function-in-c/
         * https://www.thegeekstuff.com/2012/05/ip-header-checksum/ */
        private static ushort ComputeIpHeaderChecksum(byte[] header) {
            int sum = 0;
            for (int i = 0; i < header.Length; i += 2) {
                int word16 = (((header[i] << 8)) + (header[i + 1]));
                sum += (word16 & 0xffff) + (word16 >> 16);  // add carry bits
            }
            sum = ~sum;
            return (ushort)sum;
        }

        private static byte[] GenerateIPHeader(string sourceIP, string destinationIP, int dataLength) {
            string[] sourceIPParts = sourceIP.Split('.');
            string[] destinationIPParts = sourceIP.Split('.');
            if (sourceIPParts.Length != 4 || destinationIPParts.Length != 4)
                throw new Exception("Invalid format for source IP or destination IP");
            int ipHeaderLength = 20;
            int ipPacketLength = ipHeaderLength + dataLength;
            byte[] ipHeader = new byte[ipHeaderLength];
            if (ipHeader.Length != ipHeaderLength)
                throw new Exception("Bug in the code");
            byte DSCPandECN = 0;    // decided by us
            byte identification = 1;    // decided by us
            byte flags = 0;
            ushort fragmentOffset = 0;
            byte timeToLive = 64;
            byte protocol = 17; // UDP
            byte initialChecksum = 0;
            ipHeader[0] = Convert.ToByte((4 << 4) + ipHeader.Length / 4);  // IPv4 and Number of 32 bit words
            ipHeader[1] = DSCPandECN;
            ipHeader[2] = Convert.ToByte(ipPacketLength >> 8);
            ipHeader[3] = Convert.ToByte(ipPacketLength & 0xff);
            ipHeader[4] = Convert.ToByte(identification >> 8);
            ipHeader[5] = Convert.ToByte(identification & 0xff);
            ipHeader[6] = Convert.ToByte((flags << 5) + ((fragmentOffset & 0x1f00) >> 8));
            ipHeader[7] = Convert.ToByte(fragmentOffset & 0xff);
            ipHeader[8] = timeToLive;
            ipHeader[9] = protocol;
            ipHeader[10] = initialChecksum;
            ipHeader[11] = initialChecksum;
            ipHeader[12] = Convert.ToByte(int.Parse(sourceIPParts[0]));
            ipHeader[13] = Convert.ToByte(int.Parse(sourceIPParts[1]));
            ipHeader[14] = Convert.ToByte(int.Parse(sourceIPParts[2]));
            ipHeader[15] = Convert.ToByte(int.Parse(sourceIPParts[3]));
            ipHeader[16] = Convert.ToByte(int.Parse(destinationIPParts[0]));
            ipHeader[17] = Convert.ToByte(int.Parse(destinationIPParts[1]));
            ipHeader[18] = Convert.ToByte(int.Parse(destinationIPParts[2]));
            ipHeader[19] = Convert.ToByte(int.Parse(destinationIPParts[3]));
            ushort ipHeaderChecksum = ComputeIpHeaderChecksum(ipHeader);
            ipHeader[10] = Convert.ToByte(ipHeaderChecksum >> 8);
            ipHeader[11] = Convert.ToByte(ipHeaderChecksum & 0xff);
            return ipHeader;
        }

        private static byte[] ConcatArrays(byte[] array1, byte[] array2, byte[] array3) {
            byte[] array = new byte[array1.Length + array2.Length + array3.Length];
            Buffer.BlockCopy(array1, 0, array, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, array, array1.Length, array2.Length);
            Buffer.BlockCopy(array3, 0, array, array1.Length + array2.Length, array3.Length);
            return array;
        }

        private static void ShowAsHexadecimalString(byte[] array) {
            for (int i = 0; i < array.Length; ++i)
                Console.Write("{0:x2}", array[i]);
            Console.WriteLine();
        }

        public static void SendUDPPacket(string sourceIP, int sourcePort, string destinationIP, int destinationPort, byte[] data) {
            Socket socket = GenerateSocket(localhost, sourcePort);
            byte[] udpHeader = GenerateUDPHeader(sourcePort, destinationPort, data.Length);
            byte[] ipHeader = GenerateIPHeader(sourceIP, destinationIP, udpHeader.Length + data.Length);
            byte[] toSend = ConcatArrays(ipHeader, udpHeader, data);
            socket.SendTo(toSend, new IPEndPoint(IPAddress.Parse(destinationIP), destinationPort));
        }

        public static void Receive(int port, byte[] buffer) {
            Socket socket = GenerateSocket(localhost, port);
            int read = socket.Receive(buffer);
        }

        private static Socket GenerateSocket(IPAddress localAddress, int localPort) {
            Socket socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Raw, ProtocolType.Udp);
            socket.Bind(new IPEndPoint(localAddress, localPort));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4] { 1, 0, 0, 0 };
            socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            return socket;
        }
    }
}
