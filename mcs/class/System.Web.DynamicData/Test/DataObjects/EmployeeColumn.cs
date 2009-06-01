using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.DynamicData;

using MonoTests.DataSource;

namespace MonoTests.DataObjects
{
    public class EmployeeColumn : DynamicDataColumn
    {
        public EmployeeColumn (MemberInfo member)
        {
            if (member == null)
                throw new ArgumentNullException ("member");

            this.Name = member.Name;
            switch (member.MemberType) {
                case MemberTypes.Field:
                    var fi = member as FieldInfo;
                    this.DataType = fi.FieldType;
                    break;

                case MemberTypes.Property:
                    var pi = member as PropertyInfo;
                    this.DataType = pi.PropertyType;
                    break;

                default:
                    throw new ArgumentException ("Member information must refer to either a field or a property.", "member");
            }
        }
    }
}
