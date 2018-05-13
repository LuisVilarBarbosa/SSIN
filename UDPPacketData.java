import jpcap.packet.Packet;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class UDPPacketData {
    private String senderIP;
    private String receiverIP;
    private int senderPort;
    private int receiverPort;

    public UDPPacketData(Packet packet) {
        String info = packet.toString();
        Pattern pattern = Pattern.compile(".*\\/([0-9.]+)->\\/([0-9.]+).+ ([0-9]+) > ([0-9]+).*");
        Matcher matcher = pattern.matcher(info);
        if (matcher.matches()) {
            senderIP = matcher.group(1);
            receiverIP = matcher.group(2);
            senderPort = Integer.parseInt(matcher.group(3));
            receiverPort = Integer.parseInt(matcher.group(4));
        } else
            throw new IllegalArgumentException();
    }

    public String getSenderIP() {
        return senderIP;
    }

    public String getReceiverIP() {
        return receiverIP;
    }

    public int getSenderPort() {
        return senderPort;
    }

    public int getReceiverPort() {
        return receiverPort;
    }
}
