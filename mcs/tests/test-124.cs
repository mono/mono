using System.Drawing;
namespace N1
{	
	public class D
	{
		public static int Main ()
		{
			Rectangle rect = new Rectangle ();
			N (rect);
			
			return 0;
		}

		public static bool N (RectangleF rect)
		{
			if (rect.X > rect.Y)
				return true;
			
			return false;
		}
	}
}

