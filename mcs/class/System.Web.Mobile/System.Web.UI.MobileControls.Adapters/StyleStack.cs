/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : StyleStack
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Collections;
using System.Web.Mobile;

namespace System.Web.UI.MobileControls.Adapters
{
	class StyleStack
	{
		private HtmlMobileTextWriter writer;
		private Stack stack;

		protected StyleStack(HtmlMobileTextWriter writer)
		{
			this.writer = writer;
			stack = new Stack();
		}
		
		public int Count
		{
			get
			{
				return stack.Count;
			}
		}
		
		public WriterStyle Peek()
		{
			WriterStyle retVal = null;
			if(stack.Count > 0)
				retVal = (WriterStyle)stack.Peek();
			return retVal;
		}
		
		public WriterStyle Pop()
		{
			WriterStyle retVal = null;
			if(stack.Count > 0)
				retVal = (WriterStyle)stack.Pop();
			return retVal;
		}
		
		public void Push(WriterStyle style)
		{
			stack.Push(style);
			writer.ShouldEnsureStyle = true;
		}
	}
}
