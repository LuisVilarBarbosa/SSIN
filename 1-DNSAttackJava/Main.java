public class Main {
    private static final int goodExitStatus = 0;

    public static void main(String[] args) {
        if (args.length < 3 || args.length > 4) {
            System.out.println("Usage: Main <mode_number> <timeout_millis> <erroneous_ip>");
            System.out.println("- Mode 1: send fake answer to the first sniffed query.");
            System.out.println("- Mode 2: send fake answer to a host that queries this machine.");
            System.out.println("This program was designed only for IPv4");
            return;
        }
        final int mode = Integer.parseInt(args[0]);
        final int timeoutMillis = Integer.parseInt(args[1]);
        final String erroneousIP = args[2];
        if (mode == 1)
            new DNSAttack().sendFakeAnswerToSniffedQuery(timeoutMillis, erroneousIP);
        else if (mode == 2)
            new DNSAttack().sendFakeAnswerToHost(timeoutMillis, erroneousIP);
        else
            System.out.println("Invalid mode.");
        System.exit(goodExitStatus);
    }
}
