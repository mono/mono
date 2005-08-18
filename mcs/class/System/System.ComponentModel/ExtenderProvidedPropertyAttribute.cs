//
// System.ComponentModel.ExtenderProvidedPropertyAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
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

using System;

namespace System.ComponentModel
{

	[AttributeUsage(AttributeTargets.All)]
	public sealed class ExtenderProvidedPropertyAttribute : Attribute
	{
		private PropertyDescriptor extender;
		private IExtenderProvider extenderProvider;
		private Type receiver;

		public ExtenderProvidedPropertyAttribute()
		{
		}

		// Call this method to create a ExtenderProvidedPropertyAttribute and set it's values
		internal static ExtenderProvidedPropertyAttribute CreateAttribute (PropertyDescriptor extenderProperty, IExtenderProvider provider, Type receiverType)
		{
			ExtenderProvidedPropertyAttribute NewAttribute = new ExtenderProvidedPropertyAttribute();
			NewAttribute.extender = extenderProperty;
			NewAttribute.receiver = receiverType;
			NewAttribute.extenderProvider = provider;
			return NewAttribute; 
		}

		public PropertyDescriptor ExtenderProperty {
			get {
				return extender;
			}
		}

		public IExtenderProvider Provider {
			get {
				return extenderProvider;
			}
		}

		public Type ReceiverType {
			get {
				return receiver;
			}
		}

		public override bool IsDefaultAttribute()
		{
			// FIXME correct implementation??
			return (extender == null) &&
			(extenderProvider == null) &&
			(receiver == null);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ExtenderProvidedPropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return ((ExtenderProvidedPropertyAttribute) obj).ExtenderProperty.Equals (extender) &&
			((ExtenderProvidedPropertyAttribute) obj).Provider.Equals (extenderProvider) &&
			((ExtenderProvidedPropertyAttribute) obj).ReceiverType.Equals (receiver);
		}

		public override int GetHashCode()
		{
			return extender.GetHashCode() ^ extenderProvider.GetHashCode() ^ receiver.GetHashCode();
		}
	}
}
