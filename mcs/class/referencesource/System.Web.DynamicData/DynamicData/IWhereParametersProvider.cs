using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {

    /// <summary>
    /// Interface implemented by Parameter classes that want to contribute one or many Where parameters.
    /// </summary>
    public interface IWhereParametersProvider {
        /// <summary>
        /// Get the list of Where parameters that should be added to the Where param collection.
        /// Typically, this Where paramater is replaced by the parameters it contributes.
        /// </summary>
        /// <param name="dataSource">The data source that this parameter was found on</param>
        /// <returns>A list of Paramaters</returns>
        IEnumerable<Parameter> GetWhereParameters(IDynamicDataSource dataSource);
    }
}
