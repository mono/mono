//
// System.Xml.NameTable.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Xml
{
	public class NameTable : XmlNameTable
	{
		// Fields
		Hashtable table;
		
		// Constructor
		public NameTable ()
			: base ()
		{
			table = new Hashtable ();
		}
	  
		// Method
		public override string Add (string key)
		{
			if (table.Contains (key))
				return (string) table [key];
			else {
				// We don't have to check IsInterned since the implementation
				// of String.Intern is mono_string_is_interned_lookup.
				String.Intern (key);
				table.Add (key, key);
				return key;
			}
		}

		public override string Add (char[] key, int start, int len)
		{
			if (((0 > start) && (start >= key.Length))
			    || ((0 > len) && (len >= key.Length - len)))
				throw new IndexOutOfRangeException ("The Index is out of range.");
					
			if (len == 0)
				return String.Empty;

			string item = new string (key, start, len);

			return Add (item);
		}

		public override string Get (string key)
		{
			if (! (table.Contains (key)))
				return null;
		        else
				return (string) table [key];

		}
	  
		public override string Get (char[] array, int offset, int length)
		{
			if (((0 > offset) && (offset >= array.Length))
			    || ((0 > length) && (length >= array.Length - offset)))
				throw new IndexOutOfRangeException ("The Index is out of range.");

			if (length == 0)
				return String.Empty;

			string key = new string (array, offset, length);

			return Get (key);
		}
	}
}
