using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.DataSource
{
    public interface IDynamicDataContainer
    {
        Type ContainedType
        {
            get;
        }

        int Update (IDictionary keys, IDictionary values, IDictionary oldValues);
        int Insert (IDictionary values);
        int Delete (IDictionary keys, IDictionary oldValues);
        IEnumerable Select (DataSourceSelectArguments args, string where, ParameterCollection whereParams);
        List<DynamicDataTable> GetTables ();
    }
}
