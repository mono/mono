//
// System.ComponentModel.PasswordPropertyTextAttribute
//
// Authors:
//  Marek Habersack (grendello@gmail.com)
//
// (C) 2007 Marek Habersack
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
	[AttributeUsageAttribute(AttributeTargets.All)] 
	public sealed class PasswordPropertyTextAttribute : Attribute
	{
		public static readonly PasswordPropertyTextAttribute Default;
		public static readonly PasswordPropertyTextAttribute No;
		public static readonly PasswordPropertyTextAttribute Yes;

		bool _password;
		
		public bool Password {
			get { return _password; }
		}
		
		static PasswordPropertyTextAttribute ()
		{
			No = new PasswordPropertyTextAttribute (false);
			Yes = new PasswordPropertyTextAttribute (true);
			Default = No;
		}
		
		public PasswordPropertyTextAttribute () : this (false)
		{
		}

		public PasswordPropertyTextAttribute (bool password)
		{
			this._password = password;
		}

		public override bool Equals (object o)
		{
			if (!(o is PasswordPropertyTextAttribute))
				return false;
			return ((PasswordPropertyTextAttribute) o).Password == Password;
		}

		public override int GetHashCode ()
		{
			return Password.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return Default.Equals (this);
		}
	}
}
