
//
// System.Reflection.Emit/LocalBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public sealed class LocalBuilder {
		private Type type;
		private string name;
		internal int position;

		internal LocalBuilder (Type t) {
			type = t;
		}
		public void SetLocalSymInfo( string lname) {
			name = lname;
		}
		public void SetLocalSymInfo( string lname, int startOffset, int endOffset) {
			name = lname;
		}

		public Type LocalType {
			get {return type;}
		}
	}
}
