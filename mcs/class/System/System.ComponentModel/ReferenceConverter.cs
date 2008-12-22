//
// System.ComponentModel.ReferenceConverter.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Ivan N. Zlatev (contact@i-nz.net)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr

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

using System.Globalization;
using System.Collections;
using System.ComponentModel.Design;

namespace System.ComponentModel
{
	public class ReferenceConverter : TypeConverter
	{

		private Type reference_type;

		public ReferenceConverter (Type type)
		{
			reference_type = type;
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context,
						     Type sourceType)
		{
			if (context != null && sourceType == typeof (string))
				return true;
			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			if (!(value is string))
				return base.ConvertFrom(context, culture, value);
			
			
			if (context != null) {
				object reference = null;
				// try 1 - IReferenceService
				IReferenceService referenceService = context.GetService (typeof (IReferenceService)) as IReferenceService;
				if (referenceService != null)
					reference = referenceService.GetReference ((string)value);

				// try 2 - Component by name
				if (reference == null) {
					if (context.Container != null && context.Container.Components != null)
						reference = context.Container.Components[(string)value];
				}

				return reference;
			}

			return null;
		}

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if (destinationType != typeof (string))
				return base.ConvertTo(context, culture, value, destinationType);
			
			if (value == null)
				return "(none)";
			
			string referenceName = String.Empty;

			if (context != null) {
				// try 1 - IReferenceService
				IReferenceService referenceService = context.GetService (typeof (IReferenceService)) as IReferenceService;
				if (referenceService != null)
					referenceName = referenceService.GetName (value);
	
				// try 2 - Component by name
				if ((referenceName == null || referenceName.Length == 0) && value is IComponent) {
					IComponent component = (IComponent) value;
					if (component.Site != null && component.Site.Name != null)
						referenceName = component.Site.Name;
				}
			}

			return referenceName;
		}

		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList ();

			if (context != null) {
				IReferenceService referenceService = context.GetService (typeof (IReferenceService)) as IReferenceService;
				if (referenceService != null) {
					foreach (object reference in referenceService.GetReferences (reference_type)) {
						if (IsValueAllowed (context, reference)) {
							values.Add (reference);
						}
					}
				} else {
					if (context.Container != null && context.Container.Components != null) {
						foreach (object component in context.Container.Components) {
							// avoid duplicate null values here
							if (component != null && 
							    IsValueAllowed (context, component) &&
							    reference_type.IsInstanceOfType (component))
								values.Add (component);
						}
					}
				}

				values.Add (null);
			}

			return new StandardValuesCollection (values);
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		protected virtual bool IsValueAllowed (ITypeDescriptorContext context, object value)
		{
			return true;
		}
	}
}
