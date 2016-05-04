
using System.ComponentModel;

namespace System.Web.DynamicData {
    /// <summary>
    /// Class that holds the name of common Actions for convenience
    /// </summary>
    public static class PageAction {
        /// <summary>
        /// Returns "Details"
        /// </summary>
        public static string Details { get { return "Details"; } }

        /// <summary>
        /// Returns "List"
        /// </summary>
        public static string List { get { return "List"; } }

        /// <summary>
        /// Returns "Edit"
        /// </summary>
        public static string Edit { get { return "Edit"; } }

        /// <summary>
        /// Returns "Insert"
        /// </summary>
        public static string Insert { get { return "Insert"; } }
    }

    // 
    internal class ActionConverter : StringConverter {
        private static string[] _targetValues = {
                                                       PageAction.Details,
                                                       PageAction.Edit,
                                                       PageAction.Insert,
                                                       PageAction.List
                                                   };

        private StandardValuesCollection _values;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (_values == null) {
                _values = new StandardValuesCollection(_targetValues);
            }
            return _values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
    }
}
