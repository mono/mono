using System.Collections;

	class PtrHashtable : Hashtable {
		class PtrComparer : IComparer {
			public int Compare (object x, object y)
			{
				if (x == y)
					return 0;
				else
					return 1;
			}
		}
		
		public PtrHashtable ()
		{
			comparer = new PtrComparer ();
		}
	}

