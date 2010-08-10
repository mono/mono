using System;

namespace Repro2
{
	struct Color
	{
		public static Color Black = new Color (0);
		public static Color White = new Color (255);
		public static Color Transparent = new Color (-1);

		public int ID;

		public Color (int id)
		{
			this.ID = id;
		}
	}

	static class ExtensionMethods
	{
		public static Color Transparent (this Color c)
		{
			return Color.White;
		}
	}

	class MainClass
	{
		public static int Main ()
		{
			var c = Color.Black.Transparent ();
			return 0;
		}
	}
}
