/**
 * Namespace: System.Web.UI.WebControls
 * Class:     CheckBox
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  60%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class CheckBox : WebControl, IPostBackDataHandler
	{
		private static readonly object CheckedChangedEvent = new object();

		public CheckBox()
		{
			//
		}
		
		public virtual bool AutoPostBack
		{
			get
			{
				object o = ViewState["AutoPostBack"];
				if(o!=null)
					return (bool)AutoPostBack;
				return false;
			}
			set
			{
				ViewState["AutoPostBack"] = value;
			}
		}
		
		public virtual bool Checked
		{
			get
			{
				object o = ViewState["Checked"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["Checked"] = value;
			}
		}

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}
		
		public virtual TextAlign TextAlign
		{
			get
			{
				object o = ViewState["TextAlign"];
				if(o!=null)
					return (TextAlign)o;
				return TextAlign.Right;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(TextAlign), value))
					throw new ArgumentException();
				ViewState["TextAlign"] = value;
			}
		}
		
		public event EventHandler CheckedChanged
		{
			add
			{
				Events.AddHandler(CheckedChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CheckedChangedEvent, value);
			}
		}
		
		protected virtual void OnCheckedChanged(EventArgs e)
		{
			if(Events!=null)
			{
				EventHandler eh = (EventHandler)(Events[CheckedChangedEvent]);
				if(eh!=null)
					eh(this, e);
			}
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			//TODO: THE LOST WORLD!
			// I know I have to do it.
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			//TODO: THE LOST WORLD
			// Now what the hell is this!
			return false;
		}
		
		public void RaisePostDataChangedEvent()
		{
			//TODO: THE LOST WORLD...
			// Raise the bucket out of the well :))
			OnCheckedChanged(EventArgs.Empty);
		}
	}
}
