//
// System.ArgumentException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class ArgumentException : SystemException {
		private string param_name;

		// Constructors
		public ArgumentException ()
			: base ("An invalid argument was specified.")
		{
		}

		public ArgumentException (string message)
			: base (message)
		{
		}

		public ArgumentException (string message, Exception inner)
			: base (message, inner)
		{
		}

		public ArgumentException (string message, string param_name)
			: base (message)
		{
			this.param_name = param_name;
		}

		public ArgumentException (string message, string param_name, Exception inner)
			: base (message, inner)
		{
			this.param_name = param_name;
		}

		// Properties		
		public virtual string ParamName {
			get {
				return param_name;
			}
		}
	}
}