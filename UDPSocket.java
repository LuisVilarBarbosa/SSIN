import java.io.IOException;
import java.net.*;

public class UDPSocket {
    private final DatagramSocket datagramSocket;
    private final int arrayLength = 1024 * 4;

    public UDPSocket(String hostname, int port) throws SocketException {
        this.datagramSocket = new DatagramSocket(new InetSocketAddress(hostname, port));
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

    public void close(){
        datagramSocket.close();
    }
}
