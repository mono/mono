// Compiler options: -unsafe

class T {
        static unsafe public int Main () {
                int *a = null;
                int **b = &a;
                if (*b == null)
                        return 0;
                return 1;
        }
}
