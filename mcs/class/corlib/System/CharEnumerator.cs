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
				if (index == -1)
					throw new InvalidOperationException
						("The position is not valid.");

				return str [index];
			}
		}
		
		object IEnumerator.Current
		{
			get {
				if (index == -1)
					throw new InvalidOperationException
						("The position is not valid");

				return str [index];
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
			if (length == 0)
				return false;			

			index ++;
			
			if (index == length) 				
				return false;
			else
				return true;
		}
		
		public void Reset ()
		{
			index = -1;
		}
	}
}
