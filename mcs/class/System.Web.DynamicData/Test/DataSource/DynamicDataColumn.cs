using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MonoTests.DataSource
{
    public abstract class DynamicDataColumn
    {
        public Type DataType
        {
            get; protected set;
        }

        public string Name
        {
            get;
            protected set;
        }
    }
}
