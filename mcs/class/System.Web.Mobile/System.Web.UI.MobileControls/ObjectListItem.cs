
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
/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : ObjectListCommandEventHandler
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls
{
	public class ObjectListItem : MobileListItem
	{
		private ObjectList owner;
		private string[]   fields;
		private bool       dirty = false;

		internal ObjectListItem(ObjectList owner, object dataItem)
		                      : base(dataItem, null, null)
		{
			this.owner = owner;
			this.fields = new string[owner.AllFields.Count];
		}

		internal ObjectListItem(ObjectList owner)
		                    : this(owner, null)
		{
		}

		public string this[int key]
		{
			get
			{
				if(fields != null && fields.Length >= key - 1
				   && fields[key] != null)
					return fields[key];
				return String.Empty;
			}
			set
			{
				if(fields != null && fields.Length >= key - 1)
					fields[key] = value;
				if(IsTrackingViewState)
					dirty = true;
			}
		}

		internal bool Dirty
		{
			get
			{
				return dirty;
			}
			set
			{
				dirty = value;
			}
		}

		public string this[string fieldName]
		{
			get
			{
				return this[IndexOf(fieldName)];
			}
			set
			{
				this[IndexOf(fieldName)] = value;
			}
		}

		[MonoTODO("Exception_Details_Not_Exact")]
		private int IndexOf(string fieldName)
		{
			int index = owner.AllFields.IndexOf(fieldName);
			if(index < 0)
			{
				throw new ArgumentException("ObjectList_FieldNotFound");
			}
			return index;
		}

		public override bool Equals(object obj)
		{
			bool retVal = false;
			if(obj is ObjectListItem)
			{
				ObjectListItem oli = (ObjectListItem) obj;
				if(oli.fields != null && this.fields != null)
				{
					if(this.fields.Length == oli.fields.Length)
					{
						int i;
						for(i = 0; i < fields.Length; i++)
						{
							if(fields[i] != oli.fields[i])
								break;
						}
						if(i == fields.Length)
							retVal = true;
					}
				}
				retVal &= (Value == oli.Value);
				retVal &= (Text == oli.Text);
			}
			return retVal;
		}

		public override int GetHashCode()
		{
			return (fields == null ? Value.GetHashCode() :
				                     fields[0].GetHashCode());
		}
	}
}
