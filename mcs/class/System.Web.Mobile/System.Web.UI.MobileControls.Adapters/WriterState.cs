/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : WriterState
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.IO;
using System.Collections;
using System.Drawing;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls.Adapters
{
	class WriterState : StyleStack
	{
		// stack of tagswritten and writerstyles
		private Stack stack = new Stack();
		private Stack tagsWritten = new Stack();
		private WriterStyle current = new WriterStyle();
		private HtmlMobileTextWriter writer;

		private bool isInTransition = false;
		private bool isBreakPending = false;
		private int  fontLevel = -1;
		private int  divLevel  = -1;
		private int  mark      = 0;

		public WriterState(HtmlMobileTextWriter writer) : base(writer)
		{
			this.writer = writer;
		}

		public bool IsBreakPending
		{
			get
			{
				return this.isBreakPending;
			}
			set
			{
				this.isBreakPending = value;
			}
		}

		public int FontLevel
		{
			get
			{
				return this.fontLevel;
			}
			set
			{
				this.fontLevel = value;
			}
		}

		public int DivLevel
		{
			get
			{
				return this.divLevel;
			}
			set
			{
				this.divLevel = value;
			}
		}

		public WriterStyle Current
		{
			get
			{
				return this.current;
			}
		}

		// The tags that have been written.
		public Stack TagsWritten
		{
			get
			{
				return this.tagsWritten;
			}
		}

		public HtmlTextWriter Writer
		{
			get
			{
				return this.writer;
			}
		}

		public void MarkStyleContext()
		{
			this.mark = this.tagsWritten.Count;
		}

		public void UnmarkStyleContext()
		{
			while(tagsWritten.Count > mark)
				CloseTag();
		}

		public WriterStyle PopState()
		{
			writer.ShouldEnsureStyle = true;
			IsBreakPending = (bool) stack.Pop();

			while(tagsWritten.Count > 0)
				CloseTag();

			tagsWritten = (Stack)stack.Pop();
			current = (WriterStyle)stack.Pop();
			return current;
		}

		public void PushState()
		{
			writer.ShouldEnsureStyle = true;
			stack.Push(current);
			stack.Push(tagsWritten);
			stack.Push(IsBreakPending);
			current = new WriterStyle();
			tagsWritten = new Stack();
			IsBreakPending = false;
		}

		public void CloseTag()
		{
			StyleTag tag = tagsWritten.Pop() as StyleTag;
			if(tag != null)
				tag.CloseTag(this);
		}

		public void Transition(WriterStyle style)
		{
			Transition(style, true);
		}

		[MonoTODO]
		public void Transition(WriterStyle style, bool captureOutput)
		{
			HtmlMobileTextWriter tempWriter = this.writer;
			try
			{
				if(!captureOutput && !isInTransition)
				{
					this.writer = new HtmlMobileTextWriter(
					              new HtmlTextWriter(
					              new StringWriter()),
					              tempWriter.Device);
					isInTransition = true;
					if(Count > 0)
					{
						while(Count > 0)
						{
							CloseTag();
						}
						isInTransition = false;
					} else
					{
						if(current.Italic && writer.RenderItalic)
						{
							while(current.Italic)
							{
								CloseTag();
							}
						}
						if(current.Bold && writer.RenderBold)
						{
							while(current.Bold)
							{
								CloseTag();
							}
						}
						if(current.FontColor != Color.Empty && writer.RenderFontColor)
						{
							while(current.FontColor != Color.Empty)
							{
								CloseTag();
							}
						}
						if(current.FontName != String.Empty && writer.RenderFontName)
						{
							while(current.FontName != String.Empty)
							{
								CloseTag();
							}
						}
						if(FontChange(style))
						{
							while(FontLevel > Count)
							{
								CloseTag();
							}
						}
						if(current.Wrapping == Wrapping.NoWrap && writer.RenderDivNoWrap)
						{
							while(current.Wrapping == Wrapping.NoWrap)
							{
								CloseTag();
							}
						}
						if(current.Alignment != Alignment.NotSet && writer.RenderDivAlign)
						{
							while(DivLevel > Count)
							{
								CloseTag();
							}
						}
						bool dc = DivChange(style);
						if(IsBreakPending && !dc)
						{
							writer.WriteBreak();
							IsBreakPending = false;
						}
						if(dc)
						{
							while(FontLevel > Count)
							{
								if(current.Bold || current.Italic)
									CloseTag();
							}
						}
						bool fc = FontChange(style);
						dc = DivChange(style);
						if(dc)
						{
							throw new NotImplementedException();
							//DivStyleTag
							// Actually Render
						}
						if(fc)
						{
							throw new NotImplementedException();
							// Actually Render
						}
						// Push Bold, Italic etc in current Stack
						isInTransition = false;
						throw new NotImplementedException();
					}
				}
			} finally
			{
				this.writer = tempWriter;
			}
		}

		private bool DivChange(WriterStyle newStyle)
		{
			bool retVal = false;
			if(newStyle.Layout)
			{
				if((current.Wrapping != newStyle.Wrapping
				     && writer.RenderDivNoWrap) ||
				    (current.Alignment != newStyle.Alignment
				     && writer.RenderDivAlign))
				     retVal = true;
			}
			return retVal;
		}

		private bool FontChange(WriterStyle newStyle)
		{
			bool retVal = false;
			if( (current.FontColor != newStyle.FontColor
			     && writer.RenderFontColor) ||
			    (current.FontSize != newStyle.FontSize
			     && writer.RenderFontSize) ||
			    (current.FontName != newStyle.FontName
			     && writer.RenderFontName))
			     retVal = true;
			return retVal;
		}
	}
}
