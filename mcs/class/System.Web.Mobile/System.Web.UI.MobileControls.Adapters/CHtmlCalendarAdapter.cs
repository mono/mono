/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls.Adapters
 * Class     : CHtmlCalendarAdapter
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.IO;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.MobileControls;

namespace System.Web.UI.MobileControls.Adapters
{
	public class CHtmlCalendarAdapter : HtmlControlAdapter
	{
		private const int optionPrompt = 0x00;
		private const int typeDate     = 0x01;
		private const int dateOption   = 0x02;
		private const int weekOption   = 0x03;
		private const int mthOption    = 0x04;
		private const int chooseMonth  = 0x05;
		private const int chooseWeek   = 0x06;
		private const int chooseDay    = 0x07;
		private const int defDateDone  = 0x08;
		private const int typeDateDone = 0x09;
		private const int done         = 0x0A;
		private const int firstPrompt  = 0xFFFFFFF;

		private static int[] options;
		private static char[] fmtChars;

		private const string daySeparator = " - ";
		private const string space        = " ";

		private int chooseOption;
		private int eraCount;
		private int monthsDisplayCount;
		private bool   reqFormTag;
		private string textBoxErrMsg;

		private Command command;
		private List dayList;
		private List monthList;
		private List optionList;
		private List weekList;
		private SelectionList selectList;
		private TextBox textBox;
		private System.Globalization.Calendar threadCal;

		public CHtmlCalendarAdapter()
		{
		}

		protected new Calendar Control
		{
			get
			{
				return base.Control as Calendar;
			}
		}

		public override bool RequiresFormTag
		{
			get
			{
				return this.reqFormTag;
			}
		}

		[MonoTODO]
		public override bool HandlePostBackEvent(string eventArg)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public override void LoadAdapterState(object state)
		{
			throw new NotImplementedException();
		}

		public override object SaveAdapterState()
		{
			throw new NotImplementedException();
		}

		public override void OnInit(EventArgs e)
		{
			throw new NotImplementedException();
		}

		public override void OnLoad(EventArgs e)
		{
			throw new NotImplementedException();
		}

		public override void OnPreRender(EventArgs e)
		{
			throw new NotImplementedException();
		}

		public override void Render(HtmlMobileTextWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
