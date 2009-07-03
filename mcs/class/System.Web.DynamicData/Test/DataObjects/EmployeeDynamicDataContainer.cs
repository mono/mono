using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using MonoTests.DataSource;

namespace MonoTests.DataObjects
{
    public class EmployeeDynamicDataContainer : DynamicDataContainer <Employee>
    {
        public override Type ContainedType
        {
            get
            {
                return typeof (Employee);
            }
        }

        public EmployeeDynamicDataContainer ()
            : this (null)
        {
        }

        public EmployeeDynamicDataContainer (string tableName)
            : base (tableName)
        {
        }
        
        public override int Update (IDictionary keys, IDictionary values, IDictionary oldValues)
        {
            throw new NotImplementedException ();
        }

        public override int Insert (IDictionary values)
        {
            throw new NotImplementedException ();
        }

        public override int Delete (IDictionary keys, IDictionary oldValues)
        {
            throw new NotImplementedException ();
        }

        public override IEnumerable Select (DataSourceSelectArguments args, string where, ParameterCollection whereParams)
        {
		throw new NotImplementedException ();
        }

        public override List<DynamicDataTable> GetTables ()
        {
            var ret = new List<DynamicDataTable> ();
            ret.Add (new EmployeeTable ());

            return ret;
        }
    }
}
