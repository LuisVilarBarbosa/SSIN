using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DNS_Attack {
    class Utils {
        private static int hexadecimalRadix = 16;

        public static string ByteArrayToHexString(byte[] data) {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append(string.Format("{0:x2}", b));
            return sb.ToString();
        }

        public static byte[] GetPartOfByteArray(byte[] data, int startIndex, int endIndex) {
            if (endIndex < startIndex)
                throw new ArgumentException("The start index is higher than the end index.");
            byte[] partialData = new byte[endIndex - startIndex + 1];
            for (int i = 0, j = startIndex; j <= endIndex; i++, j++)
                partialData[i] = data[j];
            return partialData;
        }

        public static byte[] HexStringToByteArray(string str) {
            byte[] array = new byte[str.Length / 2];
            for (int i = 0, j = 0; i < str.Length; i += 2, j++) {
                string hexByte = str.Substring(i, 2);
                array[j] = Convert.ToByte(hexByte, hexadecimalRadix);
            }
            return array;
        }
    }
}
