using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace MonoTests.Common
{
	[MetadataType (typeof (FooMetaDataType))]
	class FooWithMetadataType
	{
		[DefaultValue ("Value")]
		[Description ("Description")]
		public string Column1 { get; set; }
		public string Column2 { get; set; }

		[DisplayName ("Column three")]
		public string Column3 { get; set; }
		public string Column4 { get; set; }

		[Required]
		public bool Column5 { get; set; }

		// This will cause the column to be treated the way nullable database
		// columns are - that is, its associated MetaColumn.IsRequired will
		// return false (this is thanks to our test DynamicDataContainerColumnProvider
		public bool? Column6 { get; set; }
		public string Column7 { get; set; }
	}

	[DisplayColumn ("Column2")]
	class FooMetaDataType
	{
		[DisplayFormat (
			ApplyFormatInEditMode = true, 
			ConvertEmptyStringToNull=true,
			DataFormatString="Item: {0}"
		)]
		public string Column1;

		[DefaultValue ("Value")]
		[DataType (DataType.Time)]
		public string Column2;

		[DefaultValue (123)]
		[DataType (DataType.Currency)]
		[Description ("Description")]
		public int Column3;

		[DisplayName ("Column four")]
		public string Column4 { get; set; }

		[Required]
		public string Column7;
	}
}
