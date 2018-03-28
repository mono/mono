namespace System.Web.UI.WebControls {
    using System;    
    using System.Security.Permissions;

    public interface IDataBoundItemControl : IDataBoundControl {
        DataKey DataKey { 
            get; 
        }

        DataBoundControlMode Mode {
            get;
        }
    }
}
