import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;

public class UDPSocket {
    private final DatagramSocket datagramSocket;
    private final int arrayLength = 1024 * 4;

    public UDPSocket(int port) throws SocketException {
        System.loadLibrary("MySocket");
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

    public native void SendUDPPacketNative(String sourceIP, int sourcePort, String destinationIP, int destinationPort, byte[] data);

    public native void ReceiveNative(int port, byte[] buffer);
}
