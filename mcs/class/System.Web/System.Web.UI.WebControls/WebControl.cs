/**
 * Namespace: System.Web.UI.WebControls
 * Class:     WebControl
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  10%??
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
	public class WebControl : Control, IAttributeAccessor
	{
		//TODO: A list of private members may be incomplete

		private static int i = 0;
		private string _accessKey = string.Empty;
		private string _clientID;
		private Color  _backColor    = Color.Empty;
		private Color  _borderColor  = Color.Empty;
		//private BorderStyle _bStyle;
		private Unit   _borderWidth  = Unit.Empty;
		private Style  _controlStyle = null;			//TODO: What's initial value?
		private string _cssClass     = string.Empty;
		private bool   _enabled      = true;
		private FontInfo _font = new FontInfo();

		// TODO: Should this have other methods called? or
		// the values should be left blank - to be used up by the end-user?
		private AttributeCollection _attributes = new AttributeCollection( new System.Web.UI.StateBag());

		public virtual string AccessKey
		{
			get
			{
				return _accessKey;
			}
			set
			{
				_accessKey = value;
			}
		}

		public virtual AttributeCollection Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public virtual Color BackColor
		{
			get
			{
					return _backColor;
			}
			set
			{
				_backColor = value;
			}
		}

		public virtual Color BorderColor
		{
			get
			{
					return _borderColor;
			}
			set
			{
				_borderColor = value;
			}
		}

		// TODO: Confused with the enum BorderStyle and variable BorderStyle
		//public virtual BorderStyle BorderStyle { get; set; }

		public virtual Unit BorderWidth
		{
			get
			{
				return _borderWidth;
			}
			set
			{
				_borderWidth = value;
			}
		}

		public override string ClientID
		{
			get
			{
				_clientID = "WebControl" + i++;
				return _clientID;
			}
		}

		public Style ControlStyle
		{
			get
			{
				return _controlStyle;
			}
		}
		
		// TODO: The exact purpose of the field is not known
		// public bool ControlStyleCreated { get; }

		public virtual string CssClass
		{
			get
			{
				return _cssClass;
			}
			set
			{
				_cssClass = value;
			}
		}

		public virtual bool Enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				_enabled = value;
			}
		}

		public virtual FontInfo Font
		{
			get
			{
				return _font;
			}
		}

		// TODO: The constructors definitions
		protected WebControl()
		{
		}

		public WebControl(HtmlTextWriterTag tag)
		{
		}

		protected WebControl(string tag)
		{
		}

		// Implemented procedures
		
		public string GetAttribute(string key)
		{
			return "";
		}
		
		public void SetAttribute(string key, string val)
		{
			
		}

/*
		// Properties
		public ControlCollection Controls { virtual get; }
		public Style ControlStyle { get; }
		public bool ControlStyleCreated { get; }
		public bool EnableViewState { virtual get; virtual set; }
		public FontInfo Font { virtual get; }
		public Color ForeColor { virtual get; virtual set; }
		public Unit Height { virtual get; virtual set; }
		public string ID { virtual get; virtual set; }
		public Control NamingContainer { virtual get; }
		public Page Page { virtual get; virtual set; }
		public Control Parent { virtual get; }
		public ISite Site { virtual get; virtual set; }
		public CssStyleCollection Style { get; }
		public short TabIndex { virtual get; virtual set; }
		public string TemplateSourceDirectory { virtual get; }
		public string ToolTip { virtual get; virtual set; }
		public string UniqueID { virtual get; }
		public bool Visible { virtual get; virtual set; }
		public Unit Width { virtual get; virtual set; }

		// Events
		public event EventHandler DataBinding;
		public event EventHandler Disposed;
		public event EventHandler Init;
		public event EventHandler Load;
		public event EventHandler PreRender;
		public event EventHandler Unload;

		// Methods
		public void ApplyStyle(System.Web.UI.WebControls.Style s);
		public void CopyBaseAttributes(System.Web.UI.WebControls.WebControl controlSrc);
		public virtual void DataBind();
		public virtual void Dispose();
		public virtual bool Equals(object obj);
		public virtual System.Web.UI.Control FindControl(string id);
		public virtual int GetHashCode();
		public Type GetType();
		public virtual bool HasControls();
		public void MergeStyle(System.Web.UI.WebControls.Style s);
		public virtual void RenderBeginTag(System.Web.UI.HtmlTextWriter writer);
		public void RenderControl(System.Web.UI.HtmlTextWriter writer);
		public virtual void RenderEndTag(System.Web.UI.HtmlTextWriter writer);
		public string ResolveUrl(string relativeUrl);
		public void SetRenderMethodDelegate(System.Web.UI.RenderMethod renderMethod);
		public virtual string ToString();
//*/
	}
}
