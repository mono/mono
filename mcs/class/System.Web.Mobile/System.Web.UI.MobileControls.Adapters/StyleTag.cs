/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : StyleTag
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
	abstract class StyleTag
	{
		private int level;

		public StyleTag(int level)
		{
			this.level = level;
		}

		public int Level
		{
			get
			{
				return this.level;
			}
			set
			{
				this.level = value;
			}
		}

		public abstract void CloseTag(WriterState state);
	}
}
