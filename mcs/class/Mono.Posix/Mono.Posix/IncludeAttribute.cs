//
// IncludeAttribute.cs
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) Novell, Inc.  
//
using System;

namespace Mono.Posix {

	[AttributeUsage (AttributeTargets.Assembly)]
	public class IncludeAttribute : Attribute {
		string [] includes;
		string [] defines;
		
		public IncludeAttribute (string [] includes)
		{
			this.includes = includes;
		}

		public IncludeAttribute (string [] includes, string [] defines)
		{
			this.includes = includes;
			this.defines = defines;
		}

		public string [] Includes {
			get {
				if (includes == null)
					return new string [0];
				return includes;
			}
		}

		public string [] Defines {
			get {
				if (defines == null)
					return new string [0];
				return defines;
			}
		}
		
	}
}
