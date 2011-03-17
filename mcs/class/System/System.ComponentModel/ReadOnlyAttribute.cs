//
// System.ComponentModel.ReadOnlyAttribute.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
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
namespace System.ComponentModel {
	[AttributeUsage (AttributeTargets.All)]
	sealed public class ReadOnlyAttribute : Attribute {
		bool read_only;
		
		public static readonly ReadOnlyAttribute No;
		public static readonly ReadOnlyAttribute Yes;
		public static readonly ReadOnlyAttribute Default;

		static ReadOnlyAttribute ()
		{
			No = new ReadOnlyAttribute (false);
			Yes = new ReadOnlyAttribute (true);
			Default = new ReadOnlyAttribute (false);
		}
		
		public ReadOnlyAttribute (bool isReadOnly)
		{
			this.read_only = isReadOnly;
		}

		public bool IsReadOnly {
			get {
				return read_only;
			}
		}

		public override int GetHashCode ()
		{
			return read_only.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ReadOnlyAttribute))
				return false;

			return (((ReadOnlyAttribute) obj).IsReadOnly.Equals (read_only));
		}

		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}
	}
}
