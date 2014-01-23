class X {
        public static void Main ()
        {
                int a;

                call (out a);
        }

        static void call (out int a)
        {
                while (true){
                        try {
                                a = 1;
                                return ;
                        } catch {
                        }
                }
        }
}
