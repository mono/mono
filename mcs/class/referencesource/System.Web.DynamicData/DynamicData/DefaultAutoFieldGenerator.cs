namespace System.Web.DynamicData {
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Web.DynamicData.Util;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class DefaultAutoFieldGenerator : IAutoFieldGenerator {
        private IMetaTable _metaTable;

        public DefaultAutoFieldGenerator(MetaTable table)
            : this((IMetaTable)table) {
        }

        internal DefaultAutoFieldGenerator(IMetaTable table) {
            if (table == null) {
                throw new ArgumentNullException("table");
            }
            _metaTable = table;
        }

        public ICollection GenerateFields(Control control) {
            DataBoundControlMode mode = GetMode(control);
            ContainerType containerType = GetControlContainerType(control);

            // Auto-generate fields from metadata.
            List<DynamicField> fields = new List<DynamicField>();
            foreach (MetaColumn column in _metaTable.GetScaffoldColumns(mode, containerType)) {
                fields.Add(CreateField(column, containerType, mode));
            }

            return fields;
        }

        protected virtual DynamicField CreateField(MetaColumn column, ContainerType containerType, DataBoundControlMode mode) {            
            string headerText = (containerType == ContainerType.List ? column.ShortDisplayName : column.DisplayName);

            var field = new DynamicField() {
                DataField = column.Name,
                HeaderText = headerText
            };
            // Turn wrapping off by default so that error messages don't show up on the next line.
            field.ItemStyle.Wrap = false;

            return field;
        }

        internal static ContainerType GetControlContainerType(Control control) {
            if (control is IDataBoundListControl || control is Repeater) {
                return ContainerType.List;
            } else if (control is IDataBoundItemControl) {
                return ContainerType.Item;
            }
            return ContainerType.List;
        }

        internal static DataBoundControlMode GetMode(Control control) {
            // Only item controls have distinct modes
            IDataBoundItemControl itemControl = control as IDataBoundItemControl;
            if (itemControl != null && GetControlContainerType(control) != ContainerType.List) {
                return itemControl.Mode;
            }

            return DataBoundControlMode.ReadOnly;
        }
    }
}
