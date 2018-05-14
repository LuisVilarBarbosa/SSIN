// Based on a library: https://github.com/mgodave/Jpcap
// Based on http://www.rohitab.com/discuss/topic/28724-jsniff-tcpip-packet-sniffer-in-java/

import jpcap.JpcapCaptor;
import jpcap.NetworkInterface;
import jpcap.packet.Packet;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Scanner;

public class Sniffer {
    public static final String Filter_IP_ONLY = "ip";
    public static final String Filter_IP_AND_TCP_ONLY = "ip and tcp";
    public static final String Filter_IP_AND_UDP_ONLY = "ip and udp";
    public static final String Filter_IP_AND_TCP_AND_PORT_X_ONLY = "ip and tcp and port place_port_here";
    public static final String Filter_IP_AND_UDP_AND_PORT_X_ONLY = "ip and udp and port place_port_here";
    public static final String Filter_IP_AND_PORT_X_ONLY = "ip and port place_port_here";
    public static final String Filter_PORT_X_ONLY = "port place_port_here";
    public static final String FILTER_ACCEPT_ALL = "";
    private static final int maxNumberOfBytesCapturedAtOnce = 65535;
    private static final int timeoutMillis = 20;
    private JpcapCaptor captor;

    public Sniffer(Boolean promiscuousMode, String filter, Boolean optimizeFilter) throws IOException {
        NetworkInterface[] networkInterfaces = JpcapCaptor.getDeviceList();
        System.out.println("Available interfaces:");
        for (int i = 0; i < networkInterfaces.length; i++)
            System.out.println(i + " -> " + networkInterfaces[i].description);
        System.out.println("-------------------------\n");
        System.out.println("Choose interface (0,1...):");
        Scanner scanner = new Scanner(System.in);
        Integer choice = null;
        do {
            String choiceStr = scanner.nextLine();
            try {
                choice = Integer.parseInt(choiceStr);
                if (choice < 0 || choice >= networkInterfaces.length) {
                    choice = null;
                    System.out.println("Invalid option.");
                }
            } catch (NumberFormatException e) {
                System.out.println("Invalid input.");
            }
        } while (choice == null);
        System.out.println("Listening on interface -> " + networkInterfaces[choice].description);
        System.out.println("-------------------------\n");

        captor = JpcapCaptor.openDevice(networkInterfaces[choice], maxNumberOfBytesCapturedAtOnce, promiscuousMode, timeoutMillis);
        captor.setFilter(filter, optimizeFilter);
    }

    public static String putPortInFilter(String filter, int port) {
        return filter.replaceAll("place_port_here", Integer.toString(port));
    }

    public ArrayList<Packet> sniff(int count) {
        ArrayList<Packet> packets = new ArrayList<>();
        while (packets.size() < count) {
            Packet packet = captor.getPacket();
            if (packet != null)
                packets.add(packet);
        }
        return packets;
    }
}
