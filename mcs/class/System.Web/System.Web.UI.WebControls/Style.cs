/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Style
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  10%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class Style : Component, IStateManager
	{
		internal static int MARKED      = (0x01 << 0);
		internal static int BACKCOLOR   = (0x01 << 1);
		internal static int BORDERCOLOR = (0x01 << 2);
		internal static int BORDERSTYLE = (0x01 << 3);
		internal static int BORDERWIDTH = (0x01 << 4);
		internal static int CSSCLASS    = (0x01 << 5);
		internal static int FORECOLOR   = (0x01 << 6);
		internal static int HEIGHT      = (0x01 << 7);
		internal static int WIDTH       = (0x01 << 8);
		internal static int FONT_BOLD   = (0x01 << 9);
		internal static int FONT_ITALIC = (0x01 << 10);
		internal static int FONT_NAMES  = (0x01 << 11);
		internal static int FONT_SIZE   = (0x01 << 12);
		internal static int FONT_STRIKE = (0x01 << 13);
		internal static int FONT_OLINE  = (0x01 << 14);
		internal static int FONT_ULINE  = (0x01 << 15);
		
		internal static string selectionBitString = "_!SBS";

		private StateBag viewState;
		private bool     marked;
		private int      selectionBits;
		private bool     selfStateBag;				

		private FontInfo font;

		public Style()
		{
			Initialize(null);
			selfStateBag = true;
			
		}
		
		public Style(StateBag bag): base()
		{
			Initialize(bag);
			selfStateBag = false;
		}
		
		private void Initialize(StateBag bag)
		{
			viewState     = bag;
			marked        = false;
			selectionBits = 0x00;			
		}
		
		StateBag ViewState
		{
			get
			{
				if(stateBag == null)
				{
					stateBag = new stateBag(false);
					if(IsTrackingViewState)
						stateBag.TrackViewState();
				}
				return stateBag;
			}
		}

		internal bool IsSet(int bit)
		{
			return ( (selectionBits & bitIndex) != 0x00);
		}
		
		virtual void Set(int bit)
		{
			selectionBits |= bit;
			if(IsTrackingViewState)
				selectionBits |= MARKED;
		}
		
		public Color BackColor
		{
			get
			{
				if(IsSet(BACKCOLOR))
					return (Color)ViewState["BackColor"];
				return Color.Empty;
			}
			set
			{
				ViewState["BackColor"] = value;
				Set(BACKCOLOR);
			}
		}
		
		public Color BorderColor
		{
			get
			{
				if(IsSet(BORDERCOLOR))
					return (Color)ViewState["BorderColor"];
				return Color.Empty;
			}
			set
			{
				ViewState["BorderColor"] = value;
				Set(BORERCOLOR);
			}
		}
		
		public BorderStyle BorderStyle
		{
			get
			{
				if(IsSet(BORDERSTYLE))
					return (Color)ViewState["BorderStyle"];
				return BorderStyle.NotSet;
			}
			set
			{
				ViewState["BorderStyle"] = value;
				Set(BORDERSTYLE);
			}
		}
		
		public Unit BorderWidth
		{
			get
			{
				if(IsSet(BORDERWIDTH))
					return (Unit)ViewState["BorderWidth"];
				return Unit.Empty;
			}
			set
			{
				ViewState["BorderWidth"] = value;
				Set(BORDERWIDTH);
			}
		}
		
		public string CssClass
		{
			get
			{
				if(Set(CSSCLASS))
					return (string)ViewState["CssClass"];
				return string.Empty;
			}
			set
			{
				ViewState["CssClass"] = value;
				Set(CSSCLASS);
			}
		}
		
		public Color ForeColor
		{
			get
			{
				if(IsSet(FORECOLOR))
					return (Color)ViewState["ForeColor"];
				return Color.Empty;
			}
			set
			{
				ViewState["ForeColor"] = value;
				Set(FORECOLOR);
			}
		}
		
		public Unit Height
		{
			get
			{
				if(IsSet(HEIGHT))
					return (Unit)ViewState["Height"];
				return Unit.Empty;
			}
			set
			{
				ViewState["Height"] = value;
				Set(HEIGHT);
			}
		}
		
		public Unit Width
		{
			get
			{
				if(IsSet(WIDTH))
					return (Unit)ViewState["Width"];
				return Unit.Empty;
			}
			set
			{
				ViewState["Width"] = value;
				Set(HEIGHT);
			}
		}
		
		public FontInfo Font
		{
			get
			{
				if(font==null)
					font = new FontInfo(this);
				return font;
			}
		}
		
		public void AddAttributesToRender(HtmlTextWriter writer)
		{
			AddAttributesToRender(writer, null);
		}
		
		public void AddAttributesToRender(HtmlTextWriter writer, WebControl owner)
		{
			if(Set(CSSCLASS))
			{
				string cssClass = (string)ViewState["CssClass"];
				//if(cssClass.Length > 0)
				//	writer.Add(HtmlTextWriterAttribute.
			}
		}
	}
}

