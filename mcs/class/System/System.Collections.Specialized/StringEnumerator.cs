/* System.Collections.Specialized.StringEnumerator.cs
 * Authors:
 *   John Barnette (jbarn@httcb.net)
 *
 *  Copyright (C) 2001 John Barnette
*/

namespace System.Collections.Specialized {
	public class StringEnumerator {
		private StringCollection coll;
		private IEnumerator enumerable;
		
		// assembly-scoped constructor
		internal StringEnumerator(StringCollection coll) {
			this.coll = coll;
			this.enumerable = ((IEnumerable)coll).GetEnumerator();
		}
		
		// Public Instance Properties
		
		public string Current {
			get { return (string) enumerable.Current; }
		}
		
		
		// Public Instance Methods
		
		public bool MoveNext() {
			return enumerable.MoveNext();
		}
		
		public void Reset() {
			enumerable.Reset();
		}
	}
}
