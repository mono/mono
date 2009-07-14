using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.DynamicData;

namespace MonoTests.DataObjects
{
	public class Employee
	{
		public string FirstName	{ get; set; }

		[UIHint ("CustomFieldTemplate")]
		[DisplayFormat (ConvertEmptyStringToNull=true, NullDisplayText="No value for this column")]
		public string LastName { get; set; }

		[DisplayFormat (ApplyFormatInEditMode=true, DataFormatString="Boolean value: {0}")]
		public bool Active { get; set; }

		public Color FavoriteColor { get; set; }
	}
}
