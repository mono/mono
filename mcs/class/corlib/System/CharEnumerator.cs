//
// System.CharEnumerator.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System.Collections;

namespace System
{
	[Serializable]
	public sealed class CharEnumerator : IEnumerator, ICloneable
	{
		private string str;
		private int index;
		private int length;
		// Representation invariant:
		// length == str.Length
		// -1 <= index <= length
		
		// Constructor
		internal CharEnumerator (string s)
		{
			 str = s;
			 index = -1;
			 length = s.Length;
		}
		
		// Property
		public char Current
		{
			get {
				if (index == -1 || index >= length)
					throw new InvalidOperationException
						("The position is not valid.");

				return str [index];
			}
		}
		
		object IEnumerator.Current
		{
			get { 
				return Current;
			}
		}
		
		// Methods
		public object Clone ()
		{
			CharEnumerator x = new CharEnumerator (str);
			x.index = index;
			return x;
		}
		
		public bool MoveNext ()
		{
			// Representation invariant holds: -1 <= index <= length

			index ++;

			// Now: 0 <= index <= length+1;
			//   <=>
			// 0 <= index < length (OK) || 
			// length <= index <= length+1 (Out of bounds)
			
			if (index >= length) {
				index = length;
				// Invariant restored:
				// length == index
				//   =>
				// -1 <= index <= length
				return false;	
			}
			else
				return true;
		}
		
		public void Reset ()
		{
			index = -1;
		}
	}
}
