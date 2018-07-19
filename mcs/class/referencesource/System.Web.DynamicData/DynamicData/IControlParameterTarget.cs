using System.Security.Permissions;

namespace System.Web.DynamicData {

    /// <summary>
    /// Interface implemented by controls that can provide data to a DynamicControlParameter's.
    /// i.e. the control implmenting this interface can be set as the ControlId of the DynamicControlParameter.
    /// </summary>
    public interface IControlParameterTarget {
        /// <summary>
        /// The type of data being served by this provider
        /// </summary>
        MetaTable Table { get; }

        /// <summary>
        /// The column on which the parameter is being applied, if available.  e.g. a FilterUserControl
        /// can provide it, but a GridView selection cannot.
        /// </summary>
        MetaColumn FilteredColumn { get; }

        /// <summary>
        /// Get the expression that can be used as a ControlParameter's 'PropertyName'
        /// </summary>
        string GetPropertyNameExpression(string columnName);
    }
}
