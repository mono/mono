using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	class BazColumnAttributes
	{
		public string ColumnNoAttributes { get; set; }

		[DisplayFormat (
			ApplyFormatInEditMode=true, 
			ConvertEmptyStringToNull=true, 
			DataFormatString="Item: {0}"
		)]
		public string ColumnFormatInEditMode { get; set; }

		[DataType (DataType.EmailAddress)]
		public string ColumnWithDataType { get; set; }

		[DefaultValue (12345L)]
		public long ColumnWithDefaultLongValue { get; set; }

		[DefaultValue ("Value")]
		public string ColumnWithDefaultStringValue { get; set; }

		[Description ("Description")]
		public string ColumnWithDescription { get; set; }

		[DisplayName ("Display Name")]
		public string ColumnWithDisplayName { get; set; }

		[ScaffoldColumn (false)]
		public string NoScaffoldColumn { get; set; }

		[ScaffoldColumn (true)]
		public string ScaffoldAttributeColumn { get; set; }

		[UIHint ("UI Hint")]
		public string UIHintColumn { get; set; }
	}
}
