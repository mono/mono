class Program
{
	static int x;
	static int y;

    public static int Main ()
    {
    	bool b = false;
        ref int targetBucket = ref b ? ref x : ref y;

        return 0;
    }
}