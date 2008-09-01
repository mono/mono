using System;

namespace N
{
	class Item
	{
		public Item ()
		{
		}
		
		public enum ItemSlot
		{
			ItemM1,
			ItemM2
		}
	}
}

namespace N
{
	public class Test
	{
		Item this [Test slot]
		{
			get { return null; }
		}
		
		void Foo (Item.ItemSlot i)
		{
			object oo = this [null];
			
			switch (i)
			{
				case Item.ItemSlot.ItemM1:
					break;
			}
		}
	
		public static int Main ()
		{
			return 0;
		}
	}
}
