//
// System.Collections.Specialized.CollectionsUtil.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System.Collections;

namespace System.Collections.Specialized {
	
	public class CollectionsUtil {
		
		public CollectionsUtil () {}
		
		public static Hashtable CreateCaseInsensitiveHashtable ()
		{
			return new Hashtable (CaseInsensitiveHashCodeProvider.Default, 
					      CaseInsensitiveComparer.Default);			
		}
		

		public static Hashtable CreateCaseInsensitiveHashtable (IDictionary d) {
			return new Hashtable (d, CaseInsensitiveHashCodeProvider.Default, 
						 CaseInsensitiveComparer.Default);
		}

		public static Hashtable CreateCaseInsensitiveHashtable (int capacity) {
			return new Hashtable (capacity, CaseInsensitiveHashCodeProvider.Default, 
							CaseInsensitiveComparer.Default);
		}


		public static SortedList CreateCaseInsensitiveSortedList () {
			return new SortedList (CaseInsensitiveComparer.Default);
		}
	}
}