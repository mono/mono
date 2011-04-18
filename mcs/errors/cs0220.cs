// CS0220: The operation overflows at compile time in checked mode
// Line: 7

public class MainClass {
        static void Main () {
                const long a = long.MaxValue;
                long b = 2 * a;
        }
}


