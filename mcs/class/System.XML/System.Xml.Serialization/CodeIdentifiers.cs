// 
// System.Xml.Serialization.CodeIdentifiers 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Globalization;

namespace System.Xml.Serialization {
	public class CodeIdentifiers {

		#region Fields

		bool useCamelCasing;
		Hashtable table = new Hashtable ();
		Hashtable reserved = new Hashtable ();

		#endregion

		#region Constructors

		public CodeIdentifiers ()
		{
		}

		#endregion // Constructors

		#region Properties

		public bool UseCamelCasing {
			get { return useCamelCasing; }
			set { useCamelCasing = value; }
		}

		#endregion // Properties

		#region Methods

		public void Add (string identifier, object value)
		{
			table.Add (identifier, value);
		}

		public void AddReserved (string identifier)
		{
			reserved.Add (identifier, identifier);
		}

		public string AddUnique (string identifier, object value)
		{
			string unique = MakeUnique (identifier);
			Add (unique, value);
			return unique;
		}

		public void Clear ()
		{
			table.Clear ();
		}

		public bool IsInUse (string identifier)
		{
			return (table.ContainsKey (identifier) || reserved.ContainsKey (identifier));
		}

		public string MakeRightCase (string identifier)
		{
			if (UseCamelCasing)
				return CodeIdentifier.MakeCamel (identifier);
			else
				return CodeIdentifier.MakePascal (identifier);
		}

		public string MakeUnique (string identifier)
		{
			string uniqueIdentifier = identifier;
			int i = 1;

			while (IsInUse (uniqueIdentifier)) {
				uniqueIdentifier = String.Format (CultureInfo.InvariantCulture, "{0}{1}", identifier, i);
				i += 1;
			}

			return uniqueIdentifier;
		}

		public void Remove (string identifier)
		{
			table.Remove (identifier);
		}

		public void RemoveReserved (string identifier)
		{
			reserved.Remove (identifier);
		}

		public object ToArray (Type type)
		{
			Array list = Array.CreateInstance (type, table.Count);
			table.CopyTo (list, 0);
			return list;
		}

		#endregion // Methods
	}
}
