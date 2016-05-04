using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web.UI.WebControls {
    /// <summary>
    /// Indicates which <see cref='System.Globalization.CultureInfo'/> to use when converting string values to types.
    /// </summary>
    public enum ParsingCulture {

        /// <summary>
        /// <see cref='System.Globalization.CultureInfo.InvariantCulture'/> is used. This is the default.
        /// </summary>
        Invariant, 

        /// <summary>
        /// <see cref='System.Globalization.CultureInfo.CurrentCulture'/> is used.
        /// </summary>
        Current
    }
}
