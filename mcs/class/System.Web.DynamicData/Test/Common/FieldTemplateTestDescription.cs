using System;


namespace MonoTests.Common
{
	sealed class FieldTemplateTestDescription
	{
		public string ColumnName { get; private set; }
		public string ControlVirtualPath { get; private set; }
		public bool IsNull { get; private set; }

		public FieldTemplateTestDescription (string columnName)
			: this (columnName, String.Empty, true) { }

		public FieldTemplateTestDescription (string columnName, string virtualPath)
			: this (columnName, virtualPath, false) { }

		public FieldTemplateTestDescription (string columnName, string virtualPath, bool isNull)
		{
			ColumnName = columnName;
			ControlVirtualPath = virtualPath;
			IsNull = isNull;
		}
	}
}
