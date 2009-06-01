using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.DataSource
{
    public abstract class DynamicDataContainer <T> : IDynamicDataContainer
    {
        public T Data {
            get; 
            set;
        }

        public virtual Type ContainedType
        {
            get { return typeof (T); }
        }

        public DynamicDataContainer (T data)
        {
            this.Data = data;
        }

        #region IDynamicDataContainer Members
        public abstract int Update (IDictionary keys, IDictionary values, IDictionary oldValues);
        public abstract int Insert(IDictionary values);
        public abstract int Delete(IDictionary keys, IDictionary oldValues);
        public abstract IEnumerable Select(DataSourceSelectArguments args, string where, ParameterCollection whereParams);
        public abstract List<DynamicDataTable> GetTables ();
        #endregion
    }
}
