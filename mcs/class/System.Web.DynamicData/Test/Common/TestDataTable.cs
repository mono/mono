using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using MonoTests.ModelProviders;
using MonoTests.DataSource;

namespace MonoTests.Common
{
	public class TestDataTable <DataType> : DynamicDataTable
	{
		public TestDataTable ()
		{
			this.DataType = typeof (DataType);
			this.Name = typeof (DataType).Name + "Table";
		}

		public override List<DynamicDataColumn> GetColumns ()
		{
			var ret = new List<DynamicDataColumn> ();

			Type type = typeof (DataType);
			MemberInfo[] members = type.GetMembers (BindingFlags.Public | BindingFlags.Instance);
			foreach (MemberInfo mi in members) {
				if (mi.MemberType != MemberTypes.Field && mi.MemberType != MemberTypes.Property)
					continue;

				ret.Add (new TestDataColumn <DataType> (mi));
			}
			return ret;
		}
	}
}
