using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

using MonoTests.ModelProviders;

namespace MonoTests.Common
{
	class Baz
	{
		// DO NOT change the order of properties - tests depend on it
		// DO NOT change the column types - tests depend on it
		public int Column1 { get; set; }
		public int PrimaryKeyColumn1 { get; set; }
		public string PrimaryKeyColumn2 { get; set; }
		public bool PrimaryKeyColumn3 { get; set; }
		public string CustomPropertyColumn1 { get; set; }

		[UIHint ("UI Hint")]
		public string CustomPropertyColumn2 { get; set; }
		
		public int GeneratedColumn1 { get; set; }

		[UIHint ("UI Hint")]
		public int GeneratedColumn2 { get; set; }

		[ReadOnly (true)]
		public int ReadOnlyColumn { get; private set;  }

		[ReadOnly (false)]
		public int ReadWriteColumn { get; private set; }

		[DisplayFormat (NullDisplayText="Text")]
		public DateTime NullDisplayTextColumn { get; set; }

		[Required (ErrorMessage = "Custom error message")]
		public int ErrorMessageColumn1 { get; set; }

		[Required (ErrorMessage = "s")]
		public int ErrorMessageColumn2 { get; set; }

		[UIHint ("")]
		public int EmptyHintColumn { get; set; }

		[DynamicDataSortable (true)]
		public int SortableColumn1 { get; set; }

		[UIHint ("MyCustomUIHintTemplate")]
		public string CustomUIHintColumn { get; set; }

		public Baz ()
		{
			Column1 = 123;
			PrimaryKeyColumn1 = 456;
			PrimaryKeyColumn2 = "primary key value";
			PrimaryKeyColumn3 = true;
		}
	}
}
