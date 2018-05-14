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

namespace MySocket {
    public class MySocket {
        // IP packet example: 45001D010008011007F0017F001C1F7C1F7097DEA0450042011008011007F0017F001C1F903502E1111684B120010000016676F6F676C6527074001010029100000000
        private static string ipHeaderExample = "45001D010008011007F0017F001C1F7C1F7097DE"; // From a real IP packet (that does not contain the 'options' section)
        private static int hexadecimalRadix = 16;
        private static IPAddress localhost = IPAddress.Parse("127.0.0.1");

        private static byte[] HexStringToByteArray(string str) {
            byte[] array = new byte[str.Length / 2];
            for (int i = 0, j = 0; i < str.Length; i += 2, j++) {
                string hexByte = str.Substring(i, 2);
                array[j] = Convert.ToByte(hexByte, hexadecimalRadix);
            }
            return array;
        }

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

        private static byte[] GenerateIPHeader(string sourceIP, string destinationIP, int dataLength) {
            string[] sourceIPParts = sourceIP.Split('.');
            string[] destinationIPParts = sourceIP.Split('.');
            if (sourceIPParts.Length != 4 || destinationIPParts.Length != 4)
                throw new Exception("Invalid format for source IP or destination IP");
            int ipHeaderLength = 20;
            int ipPacketLength = ipHeaderLength + dataLength;
            byte[] ipHeader = HexStringToByteArray(ipHeaderExample);
            if (ipHeader.Length != ipHeaderLength)
                throw new Exception("Bug in the code");
            ipHeader[2] = Convert.ToByte(ipPacketLength >> 8);
            ipHeader[3] = Convert.ToByte(ipPacketLength & 0xff);
            ipHeader[12] = Convert.ToByte(int.Parse(sourceIPParts[0]));
            ipHeader[13] = Convert.ToByte(int.Parse(sourceIPParts[1]));
            ipHeader[14] = Convert.ToByte(int.Parse(sourceIPParts[2]));
            ipHeader[15] = Convert.ToByte(int.Parse(sourceIPParts[3]));
            ipHeader[16] = Convert.ToByte(int.Parse(destinationIPParts[0]));
            ipHeader[17] = Convert.ToByte(int.Parse(destinationIPParts[1]));
            ipHeader[18] = Convert.ToByte(int.Parse(destinationIPParts[2]));
            ipHeader[19] = Convert.ToByte(int.Parse(destinationIPParts[3]));
            return ipHeader;
        }

        private static byte[] ConcatArrays(byte[] udpHeader, byte[] ipHeader, byte[] data) {
            byte[] array = new byte[udpHeader.Length + ipHeader.Length + data.Length];
            Buffer.BlockCopy(udpHeader, 0, array, 0, udpHeader.Length);
            Buffer.BlockCopy(ipHeader, 0, array, udpHeader.Length, ipHeader.Length);
            Buffer.BlockCopy(data, 0, array, udpHeader.Length + ipHeader.Length, data.Length);
            return array;
        }

        private static void ShowAsHexadecimalString(byte[] array) {
            for (int i = 0; i < array.Length; ++i)
                Console.Write("{0:x}", array[i]);
            Console.WriteLine();
        }

        public static void SendUDPPacket(string sourceIP, int sourcePort, string destinationIP, int destinationPort, byte[] data) {
            Socket socket = GenerateSocket(localhost, sourcePort);
            byte[] ipHeader = GenerateIPHeader(sourceIP, destinationIP, data.Length);
            byte[] udpHeader = GenerateUDPHeader(sourcePort, destinationPort, ipHeader.Length + data.Length);
            byte[] toSend = ConcatArrays(udpHeader, ipHeader, data);
            socket.SendTo(toSend, new IPEndPoint(IPAddress.Parse(destinationIP), destinationPort));
        }

        public static void Receive(int port, byte[] buffer) {
            Socket socket = GenerateSocket(localhost, port);
            int read = socket.Receive(buffer);
        }

        private static Socket GenerateSocket(IPAddress localAddress, int localPort) {
            Socket socket = new Socket(AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(localAddress, localPort));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4] { 1, 0, 0, 0 };
            socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            return socket;
        }
    }
}
