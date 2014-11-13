namespace System.Web.DynamicData.Util {
    using System;            
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Resources;
    using System.Globalization;
    using IDataBoundControlInterface = System.Web.UI.WebControls.IDataBoundControl;

    internal static class DataControlHelper {
        internal static IDynamicDataSource FindDataSourceControl(Control current) {
            for (; ; current = current.NamingContainer) {
                // Don't look further than the Page, or if the control is not added to a page hierarchy
                if (current == null || current is Page)
                    return null;

                IDataBoundControlInterface dataBoundControl = GetDataBoundControl(current, false /*failIfNotFound*/);

                // Not a data control: continue searching
                if (dataBoundControl == null) {
                    continue;
                }
                // Return its DynamicDataSource
                return dataBoundControl.DataSourceObject as IDynamicDataSource;
            }
        }

        internal static IDataBoundControlInterface GetDataBoundControl(Control control, bool failIfNotFound) {
            if (control is IDataBoundControlInterface) {
                return (IDataBoundControlInterface)control;
            }
            IDataBoundControlInterface dataBoundControl = null;
            if (control is Repeater) {
                dataBoundControl = GetControlAdapter(control);
            }            

            if (dataBoundControl == null && failIfNotFound) {
                throw new Exception(String.Format(
                    CultureInfo.CurrentCulture,
                    DynamicDataResources.DynamicDataManager_UnsupportedControl,
                    control.GetType()));
            }

            return dataBoundControl;
        }

        internal static IDataBoundControlInterface GetControlAdapter(Control control) {
            Repeater repeater = control as Repeater;
            if (repeater != null) {
                return new RepeaterDataBoundAdapter(repeater);
            }
            return null;
        }
    }
}
