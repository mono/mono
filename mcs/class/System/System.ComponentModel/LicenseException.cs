//
// System.ComponentModel.LicenseException.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.ComponentModel {

	[Serializable]
	public class LicenseException : SystemException
	{

		private Type type;

		public LicenseException (Type type)
			: this (type, null)
		{
		}
		
		public LicenseException (Type type, object instance)
		{
			// LAMESPEC what should we do with instance?
			this.type = type;
		}
		
		public LicenseException (Type type, object instance, 
					 string message)
			: this (type, instance, message, null)
		{
		}
		
		public LicenseException (Type type, object instance,
					 string message,
					 Exception innerException)
			: base (message, innerException)
		{
			// LAMESPEC what should we do with instance?
			this.type = type;
		}

		protected LicenseException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			type = (Type) info.GetValue ("LicensedType", typeof (Type));
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("LicensedType", type);
			base.GetObjectData (info, context);
		}

		public Type LicensedType {
			get { return type; }
		}		
	}
}
