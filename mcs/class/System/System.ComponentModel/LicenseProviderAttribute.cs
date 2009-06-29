//
// System.ComponentModel.LicenseProviderAttribute.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class LicenseProviderAttribute : Attribute
	{
		private Type Provider;

		public static readonly LicenseProviderAttribute Default = new LicenseProviderAttribute ();

		public LicenseProviderAttribute()
		{
			this.Provider = null;
		}

		public LicenseProviderAttribute (string typeName)
		{
			this.Provider = Type.GetType (typeName, false);
		}

		public LicenseProviderAttribute (Type type)
		{
			this.Provider = type;
		}

		public Type LicenseProvider {
			get { return Provider; }
		}

		public override object TypeId {
			get {
				// Seems to be MS implementation
				return (base.ToString() + (Provider != null ? Provider.ToString() : null));
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is LicenseProviderAttribute))
				return false;
			if (obj == this)
				return true;
			return ((LicenseProviderAttribute) obj).LicenseProvider.Equals (Provider);
		}

		public override int GetHashCode()
		{
			return Provider.GetHashCode ();
		}
	}
}

