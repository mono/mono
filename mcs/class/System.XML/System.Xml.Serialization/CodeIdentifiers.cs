// 
// System.Xml.Serialization.CodeIdentifiers 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;

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
				uniqueIdentifier = String.Format ("{0}{1}", identifier, i.ToString ());
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
