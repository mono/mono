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
