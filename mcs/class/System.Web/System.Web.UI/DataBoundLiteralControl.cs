//
// System.Web.UI.DataBoundLiteralCOntrol.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.ComponentModel;
using System.Text;

namespace System.Web.UI {

	[ToolboxItem(false)]
	public sealed class DataBoundLiteralControl : Control
	{
		private string [] staticLiterals;
		private string [] dataBoundLiterals;
		
		public DataBoundLiteralControl (int staticLiteralsCount,
						int dataBoundLiteralCount)
		{
			staticLiterals = new string [staticLiteralsCount];
			dataBoundLiterals = new string [dataBoundLiteralCount];
			PreventAutoID ();
		}

		public string Text {
			get {
				StringBuilder text = new StringBuilder ();
				int stLength = staticLiterals.Length;
				int dbLength = dataBoundLiterals.Length;
				int max = (stLength > dbLength) ? stLength : dbLength;
				for (int i = 0; i < max; i++){
					if (i < stLength)
						text.Append (staticLiterals [i]);
					if (i < dbLength)
						text.Append (dataBoundLiterals [i]);
				}

				return text.ToString ();
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null)
				dataBoundLiterals = (string []) savedState;
		}

		protected override void Render (HtmlTextWriter output)
		{
			output.Write (Text);
		}

		protected override object SaveViewState ()
		{
			if (dataBoundLiterals.Length == 0)
				return null;
			return dataBoundLiterals;
		}

		public void SetDataBoundString (int index, string s)
		{
			dataBoundLiterals [index] = s;
		}

		public void SetStaticString (int index, string s)
		{
			staticLiterals [index] = s;
		}
	}
}

