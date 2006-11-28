// 
// System.Web.Services.Description.ServiceDescriptionBaseCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Collections;
using System.Web.Services;

namespace System.Web.Services.Description {
	public abstract class ServiceDescriptionBaseCollection : CollectionBase {
		
		#region Fields

		Hashtable table = new Hashtable ();
		object parent;

		#endregion // Fields

		#region Constructors

		internal ServiceDescriptionBaseCollection (object parent)
		{
			this.parent = parent;
		}

		#endregion // Constructors

		#region Properties

		protected virtual IDictionary Table {
			get { return table; }
		}

		#endregion // Properties

		#region Methods

		protected virtual string GetKey (object value) 
		{
			return null; 
		}

		protected override void OnClear ()
		{
			Table.Clear ();
		}

		protected override void OnInsertComplete (int index, object value)
		{
			if (GetKey (value) != null)
				Table [GetKey (value)] = value;
			SetParent (value, parent);
		}

		protected override void OnRemove (int index, object value)
		{
			if (GetKey (value) != null)
				Table.Remove (GetKey (value));
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			if (GetKey (oldValue) != null) 
				Table.Remove (GetKey (oldValue));
			if (GetKey (newValue) != null)
				Table [GetKey (newValue)] = newValue;
			SetParent (newValue, parent);
		}

		protected virtual void SetParent (object value, object parent)
		{
		}
			
		#endregion // Methods
	}
}
