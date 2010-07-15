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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class ValidatedControlConverter
#if NET_2_0
		: ControlIDConverter
#else
		: StringConverter
#endif
	{
		#region Public Constructors
		public ValidatedControlConverter() {
		}
		#endregion	// Public Constructors

		#region Public Instance Methods
		// We need to return all controls that have a validation property
		public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) 
		{
			if ((context != null) && (context.Container != null) && (context.Container.Components != null)) {
				ArrayList		values;
				int			count;
				string			id;
				ComponentCollection	components;		

				values = new ArrayList();
				components = context.Container.Components;
				count = components.Count;

				for (int i = 0; i < count; i++) {
					if (FilterControl((Control)components[i])) {	// We have a ValidationProperty
						id = ((Control)components[i]).ID;
						if ((id != null) && (id.Length > 0)) {
							values.Add(id);
						}
					}
				}

				// How do I sort the InvariantCulture way?
				values.Sort();
				if (values.Count > 0) {
					return new StandardValuesCollection(values);
				}
				return null;
			}
			return base.GetStandardValues (context);
		}

#if NET_2_0
		protected override 
#endif
		bool FilterControl (Control control) 
		{
			return BaseValidator.GetValidationProperty (control) != null;
		}
		#endregion	// Public Instance Methods
	}
}
