//
// SoapAttributeOverrides.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for SoapAttributeOverrides.
	/// </summary>
	public class SoapAttributeOverrides
	{
		public SoapAttributeOverrides ()
		{
		}

		[MonoTODO]
		public SoapAttributes this [Type type] 
		{
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public SoapAttributes this [Type type, string member] {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public void Add (Type type, SoapAttributes attributes) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Add (Type type, string member, SoapAttributes attributes) 
		{
			throw new NotImplementedException ();
		}
	}
}
