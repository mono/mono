using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MonoTests.Common
{
	public class BazWithDataTypeAttribute
	{
		[DataType (DataType.Custom)]
		public string CustomColumn1 { get; set; }

		[DataType (DataType.Custom)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string CustomColumn2 { get; set; }

		[DataType ("MyCustomType")]
		public string CustomColumn3 { get; set; }

		[DataType ("MyCustomType")]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string CustomColumn4 { get; set; }

		[DataType (DataType.DateTime)]
		public string DateTimeColumn1 { get; set; }

		[DataType (DataType.DateTime)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string DateTimeColumn2 { get; set; }

		[DataType (DataType.Date)]
		public string DateColumn1 { get; set; }

		[DataType (DataType.Date)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string DateColumn2 { get; set; }

		[DataType (DataType.Date)]
		public long DateColumn3 { get; set; }

		[DataType (DataType.Date)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public long DateColumn4 { get; set; }

		[DataType (DataType.Time)]
		public string TimeColumn1 { get; set; }

		[DataType (DataType.Time)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string TimeColumn2 { get; set; }

		[DataType (DataType.Duration)]
		public string DurationColumn1 { get; set; }

		[DataType (DataType.Duration)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string DurationColumn2 { get; set; }

		[DataType (DataType.PhoneNumber)]
		public string PhoneNumberColumn1 { get; set; }

		[DataType (DataType.PhoneNumber)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string PhoneNumberColumn2 { get; set; }

		[DataType (DataType.Currency)]
		public string CurrencyColumn1 { get; set; }

		[DataType (DataType.Currency)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string CurrencyColumn2 { get; set; }

		[DataType (DataType.Text)]
		public long TextColumn1 { get; set; }

		[DataType (DataType.Text)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public long TextColumn2 { get; set; }

		[DataType (DataType.Html)]
		public string HtmlColumn1 { get; set; }

		[DataType (DataType.Html)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string HtmlColumn2 { get; set; }

		[DataType (DataType.MultilineText)]
		public string MultilineTextColumn1 { get; set; }

		[DataType (DataType.MultilineText)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string MultilineTextColumn2 { get; set; }

		[DataType (DataType.EmailAddress)]
		public string EmailAddressColumn1 { get; set; }

		[DataType (DataType.EmailAddress)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string EmailAddressColumn2 { get; set; }

		[DataType (DataType.Password)]
		public string PasswordColumn1 { get; set; }

		[DataType (DataType.Password)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string PasswordColumn2 { get; set; }

		[DataType (DataType.Url)]
		public string UrlColumn1 { get; set; }

		[DataType (DataType.Url)]
		[UIHint ("MyCustomUIHintTemplate_Text")]
		public string UrlColumn2 { get; set; }
	}
}
