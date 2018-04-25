namespace System.Web.DynamicData {
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public interface IDynamicDataSource : IDataSource {

        bool AutoGenerateWhereClause { get; set; }

        Type ContextType { get; set; }

        bool EnableDelete { get; set; }

        bool EnableInsert { get; set; }

        bool EnableUpdate { get; set; }

        string EntitySetName { get; set; }

        string Where { get; set; }

        ParameterCollection WhereParameters { get; }

        event EventHandler<DynamicValidatorEventArgs> Exception;

    }

}
