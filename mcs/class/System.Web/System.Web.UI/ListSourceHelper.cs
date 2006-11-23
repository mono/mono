//
// System.Web.UI.ListSourceHelper
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.Web.UI {
	public static class ListSourceHelper {
		
		public static bool ContainsListCollection (IDataSource dataSource)
		{
			return dataSource.GetViewNames ().Count > 0;
		}
		
		public static IList GetList (IDataSource dataSource)
		{
			if (dataSource.GetViewNames ().Count == 0)
				return null;

			ListSourceList list = new ListSourceList ();
			list.Add (dataSource);
			return list;
		}

		sealed class ListSourceList : List<IDataSource>, ITypedList
		{
			#region ITypedList Members

			PropertyDescriptorCollection ITypedList.GetItemProperties (PropertyDescriptor [] listAccessors) {
				ICollection viewNames = this [0].GetViewNames ();
				PropertyDescriptor [] a = new PropertyDescriptor [viewNames.Count];
				int i = 0;
				foreach (string viewName in viewNames) {
					a[i++] = new ListSourcePropertyDescriptor (viewName, null);
				}
				return new PropertyDescriptorCollection (a);
			}

			string ITypedList.GetListName (PropertyDescriptor [] listAccessors) {
				return String.Empty;
			}

			#endregion
		}

		sealed class ListSourcePropertyDescriptor : PropertyDescriptor
		{
			public ListSourcePropertyDescriptor (MemberDescriptor descr)
				: base (descr) {
			}

			public ListSourcePropertyDescriptor (string name, Attribute [] attrs)
				: base (name, attrs) {
			}

			public ListSourcePropertyDescriptor (MemberDescriptor descr, Attribute [] attrs)
				: base (descr, attrs) {
			}

			public override bool CanResetValue (object component) {
				throw new NotImplementedException ();
			}

			public override Type ComponentType {
				get { throw new NotImplementedException (); }
			}

			public override object GetValue (object component) {
				IDataSource dataSource = component as IDataSource;
				if (dataSource == null)
					return null;

				DataSourceView view = dataSource.GetView (Name);
				return view.ExecuteSelect (DataSourceSelectArguments.Empty);
			}

			public override bool IsReadOnly {
				get { return true; }
			}

			public override Type PropertyType {
				get { return typeof(IEnumerable); }
			}

			public override void ResetValue (object component) {
				throw new NotImplementedException ();
			}

			public override void SetValue (object component, object value) {
				throw new NotImplementedException ();
			}

			public override bool ShouldSerializeValue (object component) {
				throw new NotImplementedException ();
			}
		}
	}
	

}
#endif


