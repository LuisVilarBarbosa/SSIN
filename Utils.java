class Utils {

    public static String byteArrayToHexString(byte[] data) {
        StringBuilder sb = new StringBuilder();
        for (byte b : data)
            sb.append(String.format("%02x", b));
        return sb.toString();
    }

    public static byte[] getPartOfByteArray(byte[] data, int startIndex, int endIndex) {
        if (endIndex < startIndex)
            throw new IllegalArgumentException("The start index is higher than the end index.");
        byte[] partialData = new byte[endIndex - startIndex + 1];
        for (int i = 0, j = startIndex; j <= endIndex; i++, j++)
            partialData[i] = data[j];
        return partialData;
    }

    public static byte[] hexStringToByteArray(String s) {
        int length = s.length();
        byte[] data = new byte[length / 2];
        int radix = 16;
        int shift = 4;
        for (int i = 0, j = 0, k = 1; k < length; i++, j += 2, k += 2)
            data[i] = (byte) ((Character.digit(s.charAt(j), radix) << shift) + Character.digit(s.charAt(k), radix));
        return data;
    }
}
