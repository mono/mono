public class TestClass1
{
	void Test ()
	{
		double[] zCoords = new double[long.MaxValue];
		zCoords = new double[ulong.MaxValue];
		zCoords = new double[uint.MaxValue];
	}

	public static int Main ()
	{
		double[] zCoords = new double[2 * 2] { 1, 2, 3, 4 };
		return 0;
	}
}
