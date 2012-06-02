//
// SettingsBindableAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

namespace System.ComponentModel
{
	[AttributeUsage (AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class SettingsBindableAttribute : Attribute
	{
		public static readonly SettingsBindableAttribute Yes = new SettingsBindableAttribute (true);
		public static readonly SettingsBindableAttribute No = new SettingsBindableAttribute (false);

		public SettingsBindableAttribute (bool bindable)
		{
			this.bindable = bindable;
		}

		bool bindable;

		public bool Bindable {
			get { return bindable; }
		}

		public override int GetHashCode ()
		{
			return bindable ? 1 : -1;
		}

		public override bool Equals (object obj)
		{
			SettingsBindableAttribute other =
				obj as SettingsBindableAttribute;
			return other != null && bindable == other.bindable;
		}
	}
}
