// 
// System.Xml.Serialization.CodeIdentifiers 
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Xml.Serialization {
	public class CodeIdentifiers {

		#region Fields

		bool useCamelCasing;

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

		[MonoTODO]
		public void Add (string identifier, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddReserved (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void AddUnique (string identifier, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsInUse (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string MakeRightCase (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public string MakeUnique (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveReserved (string identifier)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object ToArray (Type type)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
