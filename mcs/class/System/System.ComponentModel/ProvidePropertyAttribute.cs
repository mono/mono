//
// System.ComponentModel.ProvidePropertyAttribute.cs
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
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
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class ProvidePropertyAttribute : Attribute
	{

		private string Property;
		private string Receiver;

		public ProvidePropertyAttribute (string propertyName, string receiverTypeName)
		{
			Property = propertyName;
			Receiver = receiverTypeName;
		}

		public ProvidePropertyAttribute (string propertyName, Type receiverType)
		{
			Property = propertyName;
			Receiver = receiverType.AssemblyQualifiedName;
		}

		public string PropertyName {
			get { return Property; }
		}

		public string ReceiverTypeName {
			get { return Receiver; }
		}

		public override object TypeId {
			get { 
				// seems to be MS implementation
				return base.TypeId + Property; 
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ProvidePropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return (((ProvidePropertyAttribute) obj).PropertyName == Property) &&
				(((ProvidePropertyAttribute) obj).ReceiverTypeName == Receiver);
		}

		public override int GetHashCode()
		{
			return (Property + Receiver).GetHashCode ();
		}
	}
}
