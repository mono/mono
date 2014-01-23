class Test {
        public static int Main ()
        {
                int i = 5;
                switch (i) {
                case 5:
                        if (i == 5)
                                break;
                        return 1;
                default:
                        return 2;
                }
                System.Console.WriteLine (i);
		return 0;
        }
}

