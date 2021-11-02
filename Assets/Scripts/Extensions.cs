public static class Extensions {
    public static int Mod(this int a, int b) {
        var remainder = a % b;
        return remainder >= 0 ? remainder : remainder + b;
    }
}
