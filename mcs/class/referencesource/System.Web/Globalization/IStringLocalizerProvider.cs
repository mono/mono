using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 namespace System.Web.Globalization {
    public interface IStringLocalizerProvider {
        /// <summary>
        /// Retrieve the localized string with the given name and formatted with the supplied arguments.
        /// </summary>
        /// <param name="culture">The CultureInfo.</param>
        /// <param name="name">The name of the string resource.</param>
        /// <param name="arguments">The values to format the string with.</param>
        /// <returns>The formatted localized string.</returns>
        string GetLocalizedString(CultureInfo culture, string name, params object[] arguments);
    }
}