namespace System.Web.DynamicData {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.UI;

    /// <summary>
    /// Registers a DataControl for use with Dynamic Data
    /// </summary>
    public class DataControlReference {
        /// <summary>
        /// Dynamic data manager registering the data control
        /// </summary>
        [Browsable(false)]
        public DynamicDataManager Owner {
            get;
            internal set;
        }

        /// <summary>
        /// ControlID of the DataControl
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        IDReferenceProperty(),
        ResourceDescription("DataControlReference_ControlID"),
        TypeConverter("System.Web.DynamicData.Design.DataControlReferenceIDConverter, " + AssemblyRef.SystemWebDynamicDataDesign),
        SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")
        ]
        public string ControlID {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ToString() {
            if (String.IsNullOrEmpty(ControlID)) {
                return "DataControl";
            }
            else {
                return "DataControl: " + ControlID;
            }
        }
    }
}
