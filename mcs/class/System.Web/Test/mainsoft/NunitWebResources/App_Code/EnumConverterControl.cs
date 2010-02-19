using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;
using System.Web.UI.WebControls;

namespace MonoTests.Controls
{
	[Flags]
	public enum EnumConverterTestValues
	{
		FlagOne = 0x01,
		FlagTwo = 0x02
	}

	public class EnumConverterTestValuesConverter : EnumConverter
	{
		public EnumConverterTestValuesConverter (Type type)
			: base (type)
		{ }
	}

	public class EnumConverterTextBox : TextBox
	{
		EnumConverterTestValues values;

		[TypeConverter (typeof (EnumConverterTestValuesConverter))]
		public EnumConverterTestValues Values {
			get { return values; }
			set { values = value; }
		}

		public EnumConverterTextBox ()
		{
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			Text = values.ToString ();
		}
	}
}
