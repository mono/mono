namespace A
{
	using Base = B.Color;

	class Color
	{
		protected Base Base
		{
			get { return Base.Blue; }
		}

		protected Base NewBase {
			get {
				return Base.From(1);
			}
		}

		public static void Main ()
		{
		}
	}
}

namespace B
{
	public struct Color
	{
		public static Color Blue = new Color ();
		
		public static Color From (int i)
		{
			return new Color ();
		}
	}
}