/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlInputFile : HtmlInputControl, IPostBackDataHandler{
		
		public HtmlInputFile():base("file"){}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
						       NameValueCollection postCollection)
		{
			string postValue = postCollection [postDataKey];
			if (postValue != null)
				Value = postValue;
			return false;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
		}
		
		public string Accept{
			get{
				string attr = Attributes["accept"];
				if (attr != null)
					return attr;
				return String.Empty;
			}
			set{
				Attributes["accept"] = AttributeToString(value);
			}
		}
		
		public int MaxLength{
			get{
				string attr = Attributes["maxlength"];
				if (attr != null)
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["accept"] = AttributeToString(value);
			}
		}
		
		public int Size{
			get{
				string attr = Attributes["size"];
				if (attr != null)
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["size"] = AttributeToString(value);
			}
		}
		
		public HttpPostedFile PostedFile{
			get{
				return Context.Request.Files[RenderedName];
			}
		}
		
	} // class HtmlInputFile
} // namespace System.Web.UI.HtmlControls

