//
// System.TypeInitializationException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	public class TypeInitializationException : SystemException {
		string type_name;

		// Constructors
		public TypeInitializationException (string type_name, Exception inner)
			: base (Locale.GetText ("An exception was thrown by the type initializer for ") + type_name, inner)
		{
			this.type_name = type_name;
		}

		// Properties
		public string TypeName {
			get {
				return type_name;
			}
		}
	}

}
