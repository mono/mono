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
		 private int idx;
			 
           // Constructor
		 internal CharEnumerator (string s)
		 {
			    str = s;
			    idx = -1;
		 }
			 
		 // Property
		 public char Current
		 {
			    get {
				  if (idx == -1)
						throw new InvalidOperationException ("The position is not valid.");
										  
				  return str[idx];
			    }
		 }

		 object IEnumerator.Current {
			    get {
					  if (idx == -1)
							throw new InvalidOperationException ("The position is not valid");
					  return str [idx];
			    }
		 }
			 
		 // Methods
		 public object Clone ()
		 {
			    CharEnumerator x = new CharEnumerator (str);
			    x.idx = idx;
			    return x;
		 }
			 
		 public bool MoveNext ()
		 {
			    if (idx > str.Length) {
					  idx = -1;
					  return false;
			    } else
					  return true;
		 }
			 
		 public void Reset ()
		 {
			    idx = 0;
		 }
   }
}
