namespace TT
{
	class GG
	{
	}
}

namespace TT
{
	namespace A
	{
		using GG = System.DateTime;
		
		namespace X.Y
		{
			class X
			{
				public static void Main ()
				{
					System.DateTime dt = new GG ();
				}
			}
		}
	}
}