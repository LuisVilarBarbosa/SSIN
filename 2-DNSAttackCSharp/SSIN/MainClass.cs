using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    public class MainClass {
        private static int goodExitStatus = 0;

        public static void Main(string[] args) {
            if (args.Length < 3 || args.Length > 4) {
                Console.WriteLine("Usage: Main <mode_number>...");
                Console.WriteLine("Usage: Main 1 <timeout_millis> <erroneous_ip>");
                Console.WriteLine("Usage: Main 2 <fake_source_ip> <timeout_millis> <erroneous_ip>");
                Console.WriteLine("- Mode 1: send fake answer to the first sniffed query (not implemented).");
                Console.WriteLine("- Mode 2: send fake answer to a host that queries this machine.");
                Console.WriteLine("This program was designed only for IPv4");
                return;
            }
            try {
                int mode = int.Parse(args[0]);
                if (mode == 1) {
                    int timeoutMillis = int.Parse(args[1]);
                    string erroneousIP = args[2];
                    new DNSAttack().SendFakeAnswerToSniffedQuery(timeoutMillis, erroneousIP);
                }
                else if (mode == 2) {
                    string fakeSourceIP = args[1];
                    int timeoutMillis = int.Parse(args[2]);
                    string erroneousIP = args[3];
                    new DNSAttack().SendFakeAnswerToHost(fakeSourceIP, timeoutMillis, erroneousIP);
                }
                else
                    Console.WriteLine("Invalid mode.");
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            Environment.Exit(goodExitStatus);
        }
    }
}
