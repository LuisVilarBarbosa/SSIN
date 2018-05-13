public class Main {
    private static final int goodExitStatus = 0;

    public static void main(String[] args) {
        if (args.length != 2) {
            System.out.println("Usage: Main <timeout_millis> <erroneous_ip>");
            return;
        }
        final int timeoutMillis = Integer.parseInt(args[0]);
        final String erroneousIP = args[1];
        new DNSAttack().sendFakeAnswerToSniffedRequest(timeoutMillis, erroneousIP);
        System.exit(goodExitStatus);
    }
}
