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
		 private int len;
			 
           // Constructor
		 internal CharEnumerator (string s)
		 {
			    str = s;
			    idx = -1;
 			    len = s.Length;
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
  			    if (len < 0)
					  return false;

			    idx++;

			    if (idx > len) {
					  idx = -2;
					  return false;
			    }
			    
			    return true;
		 }
			 
		 public void Reset ()
		 {
			    idx = -1;
		 }
   }
}
