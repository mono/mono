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
		static Hashtable table = new Hashtable ();

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

		[MonoTODO ("What does this do?")]
		public void AddReserved (string identifier)
		{
			throw new NotImplementedException ();
		}

		public void AddUnique (string identifier, object value)
		{
			Add (MakeUnique (identifier), value);
		}

		public void Clear ()
		{
			table.Clear ();
		}

		public bool IsInUse (string identifier)
		{
			return (table.ContainsKey (identifier));
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

		[MonoTODO ("What does this do?")]
		public void RemoveReserved (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Need to determine how to do the conversion.")]
		public object ToArray (Type type)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
