using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MonoTests.DataSource
{
    public abstract class DynamicDataTable
    {
        public string Name
        {
            get;
            set;
        }

        public Type DataType
        {
            get;
            set;
        }
        public abstract List<DynamicDataColumn> GetColumns ();
    }
}
