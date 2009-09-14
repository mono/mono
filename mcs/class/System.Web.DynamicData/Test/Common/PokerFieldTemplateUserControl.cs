using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData;

namespace MonoTests.Common
{
	public class PokerFieldTemplateUserControl : FieldTemplateUserControl
	{
		public string GetChildrenPath ()
		{
			return ChildrenPath;
		}

		public string GetForeignKeyPath ()
		{
			return ForeignKeyPath;
		}

		public string CallBuildChildrenPath (string path)
		{
			return BuildChildrenPath (path);
		}
	}
}
