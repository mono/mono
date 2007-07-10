struct Color
{
	public static Color From (int i)
	{
		return new Color ();
	}
	
	public int ToArgb ()
	{
		return 0;
	}
}

class C
{
		public Color Color {
			get {
				return new Color();
			}
		}
		
		void ResetCustomColors ()
		{
			int default_color = Color.From(0).ToArgb ();
		}
		
		public static void Main ()
		{
		}
}
