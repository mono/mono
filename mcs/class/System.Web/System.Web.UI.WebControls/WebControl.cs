/**
 * Namespace: System.Web.UI.WebControls
 * 
 * Author:  Gaurav Vaish
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class WebControl : Control, IAttributeAccessor
	{
		// TODO: A list of private members may be incomplete
		private string _accessKey = String.Empty;
		private AttributeCollection _attributeCollection = null;
					// TODO: A list of all possible attributes,
					// which depend upon the control object
		private Color  _backColor    = null;		//TODO: What's initial value?
		private Color  _borderColor  = null;		//TODO: What's initial value?
		private Unit   _borderWidth  = Unit.Empty;
		private Style  _controlStyle = null;		//TODO: What's initial value?
		private string _cssClass     = String.Empty;
		private bool   _enabled      = true;
		
		public enum BorderStyle = {
			NotSet
			//TODO: Put all the possible values of BorderStyle
		};

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

		// TODO: Attributes		
		public AttributeCollection Attributes { get; }

		public virtual Color BackColor
		{
			get
			{
				if(_backColor != null)
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
				if( _borderColor != null)
					return _borderColor;
			}
			set
			{
				_borderColor = value;
			}
		}

		// TODO: Confused with the enum BorderStyle and variable BorderStyle
		public virtual BorderStyle BorderStyle { get; set; }

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

		// TODO: The constructors definitions
		protected WebControl()
		{
		}

		public WebControl(HTMLTextWriterTag tag)
		{
		}

		protected WebControl(string tag)
		{
		}
	}
}
