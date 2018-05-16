import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;

public class UDPSocket {
    private final DatagramSocket datagramSocket;
    private static final int arrayLength = 1024 * 4;

    public UDPSocket(int port) throws SocketException {
        this.datagramSocket = new DatagramSocket(port);
    }

    public void sendUDPPacket(String host, int port, byte[] data) throws IOException {
        InetAddress address = InetAddress.getByName(host);
        DatagramPacket packet = new DatagramPacket(data, data.length, address, port);
        datagramSocket.send(packet);
    }

    public DatagramPacket receiveUDPPacket() throws IOException {
        byte[] data = new byte[arrayLength];
        DatagramPacket packet = new DatagramPacket(data, data.length);
        datagramSocket.receive(packet);
        return packet;
    }

    public void close() {
        datagramSocket.close();
    }
}
