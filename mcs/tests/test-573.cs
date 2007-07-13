public class C
{
	private static float current_factor_width = 1f;

	public static void Main ()
	{
		int width = 5;
		width += -(int)(((current_factor_width) - 1f) * -4.0f);
	}
}