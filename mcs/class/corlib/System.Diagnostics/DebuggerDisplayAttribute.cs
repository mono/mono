//
// System.Diagnostics.DebuggerDisplayAttribute.cs
//
// Author:
//   Chris Toshok (toshok@novell.com)
//

//
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

using System;

#if NET_2_0
using System.Runtime.InteropServices;
#endif

namespace System.Diagnostics {

	[AttributeUsageAttribute(AttributeTargets.Class |
				 AttributeTargets.Struct |
				 AttributeTargets.Enum |
				 AttributeTargets.Field |
				 AttributeTargets.Delegate |
				 AttributeTargets.Property |
				 AttributeTargets.Assembly, AllowMultiple=true)]	
#if NET_2_0
	[ComVisible (true)]
	public sealed class DebuggerDisplayAttribute : Attribute
#else
	internal sealed class DebuggerDisplayAttribute : Attribute
#endif
	{
		string value, type, name;
		string target_type_name;
		Type target_type;

		public DebuggerDisplayAttribute (string value) {
			if (value == null)
				value = string.Empty;

			this.value = value;
			this.type = string.Empty;
			this.name = string.Empty;
		}

		public string Value {
			get {
				return value;
			}
		}

		public Type Target {
			get {
				return target_type;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				target_type = value;
				target_type_name = target_type.AssemblyQualifiedName;
			}
		}

		public string TargetTypeName {
			get {
				return target_type_name;
			}
			set {
				target_type_name = value;
			}
		}

		public string Type {
			get {
				return type;
			}

			set {
				type = value;
			}
		}

		public string Name {
			get {
				return name;
			}

			set {
				name = value;
			}
		}
	}

}
