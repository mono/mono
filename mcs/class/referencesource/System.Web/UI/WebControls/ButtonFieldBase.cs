//------------------------------------------------------------------------------
// <copyright file="ButtonFieldBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;    
    using System.ComponentModel;


    /// <devdoc>
    /// Defines the base class for DataControlFields whose main purpose is to contain buttons for commands.
    /// </devdoc>
    public abstract class ButtonFieldBase : DataControlField {
    

        /// <devdoc>
        /// <para>Indicates the button type for the field.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(ButtonType.Link),
        WebSysDescription(SR.ButtonFieldBase_ButtonType)
        ]
        public virtual ButtonType ButtonType {
            get {
                object o = ViewState["ButtonType"];
                if (o != null)
                    return(ButtonType)o;
                return ButtonType.Link;
            }
            set {
                if (value < ButtonType.Button || value > ButtonType.Link) {
                    throw new ArgumentOutOfRangeException("value");
                }
                object oldValue = ViewState["ButtonType"];
                if (oldValue == null || (ButtonType)oldValue != value) {
                    ViewState["ButtonType"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.ButtonFieldBase_CausesValidation)
        ]
        public virtual bool CausesValidation {
            get {
                object o = ViewState["CausesValidation"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                object oldValue = ViewState["CausesValidation"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["CausesValidation"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(false),
        WebSysDescription(SR.DataControlField_ShowHeader)
        ]
        public override bool ShowHeader {
            get {
                object o = ViewState["ShowHeader"];
                if (o != null) {
                    return (bool)o;
                }
                return false;
            }
            set {
                object oldValue = ViewState["ShowHeader"];
                if (oldValue == null || (bool)oldValue != value) {
                    ViewState["ShowHeader"] = value;
                    OnFieldChanged();
                }
            }
        }


        [
        WebCategory("Behavior"),
        DefaultValue(""),
        WebSysDescription(SR.ButtonFieldBase_ValidationGroup)
        ]
        public virtual string ValidationGroup {
            get {
                object o = ViewState["ValidationGroup"];
                if (o != null) {
                    return (string)o;
                }
                return String.Empty;
            }
            set {
                if (!String.Equals(value, ViewState["ValidationGroup"])) {
                    ViewState["ValidationGroup"] = value;
                    OnFieldChanged();
                }
            }
        }

        protected override void CopyProperties(DataControlField newField) {
            ((ButtonFieldBase)newField).ButtonType = ButtonType;
            ((ButtonFieldBase)newField).CausesValidation = CausesValidation;
            ((ButtonFieldBase)newField).ValidationGroup = ValidationGroup;
            base.CopyProperties(newField);
        }
    }
}


