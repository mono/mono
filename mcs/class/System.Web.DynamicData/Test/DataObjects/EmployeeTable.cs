using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.DynamicData;

using MonoTests.DataSource;

namespace MonoTests.DataObjects
{
    public class EmployeeTable : DynamicDataTable
    {
        public EmployeeTable ()
        {
            this.DataType = typeof (Employee);
            this.Name = "Employee";
        }

        public override List<DynamicDataColumn> GetColumns ()
        {
            var ret = new List<DynamicDataColumn> ();

            Type type = typeof (Employee);
            MemberInfo[] members = type.GetMembers (BindingFlags.Public | BindingFlags.Instance);
            foreach (MemberInfo mi in members) {
                if (mi.MemberType != MemberTypes.Field && mi.MemberType != MemberTypes.Property)
                    continue;

                ret.Add (new EmployeeColumn (mi));
            }
            return ret;
        }
    }
}
