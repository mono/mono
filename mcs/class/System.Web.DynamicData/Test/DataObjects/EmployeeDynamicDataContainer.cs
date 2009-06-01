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
    public class EmployeeDynamicDataContainer : DynamicDataContainer <List <Employee>>
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

        public EmployeeDynamicDataContainer (List<Employee> data)
            : base (data)
        {
            if (data == null)
                PopulateWithData ();
        }

        void PopulateWithData ()
        {
            Data = new List<Employee> {
                new Employee { FirstName = "Marek", LastName = "Habersack" }
            };
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
            List<Employee> data = Data;
            int count = data == null ? 0 : data.Count;

            if (args.RetrieveTotalRowCount)
                args.TotalRowCount = count;

            int startIndex = args.StartRowIndex;
            if (count == 0 || count < startIndex)
                return new Employee[0];

            int max = args.MaximumRows;
            if (max == 0 || max > count)
                max = count;
            max -= startIndex;

            var ret = new List<Employee> ();
            ret.AddRange (data.GetRange (args.StartRowIndex, max));

            return ret;
        }

        public override List<DynamicDataTable> GetTables ()
        {
            var ret = new List<DynamicDataTable> ();
            ret.Add (new EmployeeTable ());

            return ret;
        }
    }
}
