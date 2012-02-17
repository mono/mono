//
// System.Configuration.ElementInformation.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0
using System.Collections;

namespace System.Configuration
{
	public sealed class ElementInformation
	{
		readonly PropertyInformation propertyInfo;
		readonly ConfigurationElement owner;
		readonly PropertyInformationCollection properties;

		internal ElementInformation (ConfigurationElement owner, PropertyInformation propertyInfo)
		{
			this.propertyInfo = propertyInfo;
			this.owner = owner;

			properties = new PropertyInformationCollection ();
			foreach (ConfigurationProperty prop in owner.Properties)
				properties.Add (new PropertyInformation (owner, prop));
		}

		[MonoTODO]
		public ICollection Errors {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsCollection {
			get { return owner is ConfigurationElementCollection; }
		}
		
		public bool IsLocked {
			get { return propertyInfo != null ? propertyInfo.IsLocked : false; }
		}
		
		[MonoTODO("Support multiple levels of inheritance")]
		public bool IsPresent {
			get { return owner.IsElementPresent; }
		}
		
		public int LineNumber {
			get { return propertyInfo != null ? propertyInfo.LineNumber : 0; }
		}
		
		public string Source {
			get { return propertyInfo != null ? propertyInfo.Source : null; }
		}
		
		public Type Type {
			get { return propertyInfo != null ? propertyInfo.Type : owner.GetType (); }
		}
		
		public ConfigurationValidatorBase Validator {
			get { return propertyInfo != null ? propertyInfo.Validator : new DefaultValidator(); }
		}
		
		public PropertyInformationCollection Properties {
			get { return properties; }
		}
		
		internal void Reset (ElementInformation parentInfo)
		{
			foreach (PropertyInformation prop in Properties) {
				PropertyInformation parentProp = parentInfo.Properties [prop.Name];
				prop.Reset (parentProp);
			}
		}
	}
}
#endif
