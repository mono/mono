
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
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : HtmlMobileTextWriter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.IO;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls.Adapters
{
	public class HtmlMobileTextWriter : MobileTextWriter
	{
		private bool beforeFirstControlWritten = true;
		private bool maintainState = true;
		private bool renderBodyColor = true;
		private bool renderBold = true;
		private bool renderDivAlign = true;
		private bool renderDivNoWrap = false;
		private bool renderFontColor = true;
		private bool renderFontName = true;
		private bool renderFontSize = true;
		private bool renderItalic = true;
		private bool requiresNoBreak = false;
		private bool shouldEnsureStyle = true;

		private WriterState currentState;

		[MonoTODO]
		public HtmlMobileTextWriter(TextWriter writer,
		                 MobileCapabilities capabilities)
		                 : base(writer, capabilities)
		{
			RenderBold = capabilities.SupportsBold;
			RenderItalic = capabilities.SupportsItalic;
			RenderFontSize = capabilities.SupportsFontSize;
			RenderFontName = capabilities.SupportsFontName;
			RenderFontColor = capabilities.SupportsFontColor;
			RenderBodyColor = capabilities.SupportsBodyColor;
			RenderDivAlign = capabilities.SupportsDivAlign;
			RenderDivNoWrap = capabilities.SupportsDivNoWrap;
			RequiresNoBreakInFormatting = capabilities.RequiresNoBreakInFormatting;

			currentState = new WriterState(this);
		}

		public bool BeforeFirstControlWritten
		{
			get
			{
				return beforeFirstControlWritten;
			}
			set
			{
				beforeFirstControlWritten = value;
			}
		}

		public bool MaintainState
		{
			get
			{
				return maintainState;
			}
			set
			{
				maintainState = value;
			}
		}

		public bool RenderBodyColor
		{
			get
			{
				return renderBodyColor;
			}
			set
			{
				renderBodyColor = value;
			}
		}

		public bool RenderBold
		{
			get
			{
				return renderBold;
			}
			set
			{
				renderBold = value;
			}
		}

		public bool RenderDivAlign
		{
			get
			{
				return renderDivAlign;
			}
			set
			{
				renderDivAlign = value;
			}
		}

		public bool RenderDivNoWrap
		{
			get
			{
				return renderDivNoWrap;
			}
			set
			{
				renderDivNoWrap = value;
			}
		}

		public bool RenderFontColor
		{
			get
			{
				return renderFontColor;
			}
			set
			{
				renderFontColor = value;
			}
		}

		public bool RenderFontName
		{
			get
			{
				return renderFontName;
			}
			set
			{
				renderFontName = value;
			}
		}

		public bool RenderFontSize
		{
			get
			{
				return renderFontSize;
			}
			set
			{
				renderFontSize = value;
			}
		}

		public bool RenderItalic
		{
			get
			{
				return renderItalic;
			}
			set
			{
				renderItalic = value;
			}
		}

		public bool RequiresNoBreakInFormatting
		{
			get
			{
				return requiresNoBreak;
			}
			set
			{
				requiresNoBreak = value;
			}
		}

		public bool ShouldEnsureStyle
		{
			get
			{
				return shouldEnsureStyle;
			}
			set
			{
				shouldEnsureStyle = value;
			}
		}

		[MonoTODO]
		public void EnterLayout(Style style)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ExitLayout(Style style, bool breakAfter)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void EnterStyle(System.Web.UI.MobileControls.Style style)
		{
			throw new NotImplementedException();
		}

		private void EnterStyle(WriterStyle style)
		{
			currentState.Push(style);
		}

		public void ExitStyle(Style style)
		{
			ExitStyle(style, false);
		}

		[MonoTODO]
		public void ExitStyle(Style style, bool breakAfter)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void WriteText(string text, bool encode)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Write(char c)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void Write(string text)
		{
			throw new NotImplementedException();
		}

		public void BeginStyleContext()
		{
			if(currentState.IsBreakPending)
			{
				WriteBreak();
				currentState.IsBreakPending = false;
			}
			currentState.PushState();
			EnterStyle(new WriterStyle());
		}
		
		public void EndStyleContext()
		{
			if(currentState.IsBreakPending)
			{
				WriteBreak();
				currentState.IsBreakPending = false;
			}
			currentState.PopState();
			currentState.Pop();
			currentState.Transition(new WriterStyle());
		}
		
		public void EnterFormat(Style style)
		{
			WriterStyle wstyle = new WriterStyle(style);
			wstyle.Layout = false;
			EnterStyle(wstyle);
		}
		
		public virtual void ExitFormat(Style style)
		{
			ExitStyle(style);
		}
		
		public void WriteBreak()
		{
			WriteLine("<br>");
		}
		
		public override void WriteLine(string text)
		{
			EnsureStyle();
			base.WriteLine(text);
		}
		
		internal void EnsureStyle()
		{
			if(shouldEnsureStyle)
			{
				if(currentState.Count > 0)
				{
					currentState.Transition(currentState.Peek());
				}
				shouldEnsureStyle = false;
			}
			if(BeforeFirstControlWritten)
			{
				BeforeFirstControlWritten = false;
			}
		}
	}
}
