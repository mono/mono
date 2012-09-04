//
// SwitchAttribute.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C) 2007 Novell, Inc.
//

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
using System.Diagnostics;
using System.Reflection;

namespace System.Diagnostics
{
	[MonoLimitation ("This attribute is not considered in trace support.")]
	[AttributeUsageAttribute (
	 AttributeTargets.Assembly |
	 AttributeTargets.Class |
	 AttributeTargets.Constructor |
	 AttributeTargets.Method |
	 AttributeTargets.Property |
	 AttributeTargets.Event)]
	public sealed class SwitchAttribute : Attribute
	{
		public static SwitchAttribute [] GetAll (Assembly assembly)
		{
			object [] atts = assembly.GetCustomAttributes (typeof (SwitchAttribute), false);
			SwitchAttribute [] ret = new SwitchAttribute [atts.Length];
			for (int i = 0; i < atts.Length; i++)
				ret [i] = (SwitchAttribute) atts [i];
			return ret;
		}

		public SwitchAttribute (string switchName, Type switchType)
		{
			if (switchName == null)
				throw new ArgumentNullException ("switchName");
			if (switchType == null)
				throw new ArgumentNullException ("switchType");
			name = switchName;
			type = switchType;
		}

		string name, desc = String.Empty;
		Type type;

		public string SwitchName {
			get { return name; }
			set {
				if (name == null)
					throw new ArgumentNullException ("value");
				name = value;
			}
		}

		public string SwitchDescription {
			get { return desc; }
			set {
				if (desc == null)
					throw new ArgumentNullException ("value");
				desc = value;
			}
		}

		public Type SwitchType {
			get { return type; }
			set {
				if (type == null)
					throw new ArgumentNullException ("value");
				type = value;
			}
		}
	}
}
