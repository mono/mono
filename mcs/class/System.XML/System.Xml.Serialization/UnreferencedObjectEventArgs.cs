//
// UnreferencedObjectEventArgs.cs: 
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
	/// Summary description for UnreferencedObjectEventArgs.
	/// </summary>
	public class UnreferencedObjectEventArgs : EventArgs
	{
		private object unreferencedObject;
		private string unreferencedId;
		
		public UnreferencedObjectEventArgs(object o, string id)
		{
			unreferencedObject = o;
			unreferencedId = id;
		}
		
		public string UnreferencedId {
			get{ return unreferencedId; }
		}
		public object UnreferencedObject {
			get{ return unreferencedObject; }
		}
	}
}
