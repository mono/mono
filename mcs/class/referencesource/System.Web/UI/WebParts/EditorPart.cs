//------------------------------------------------------------------------------
// <copyright file="EditorPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    [
    Bindable(false),
    Designer("System.Web.UI.Design.WebControls.WebParts.EditorPartDesigner, " + AssemblyRef.SystemDesign),
    ]
    public abstract class EditorPart : Part {

        private WebPart _webPartToEdit;
        private WebPartManager _webPartManager;
        private EditorZoneBase _zone;

        /// <devdoc>
        /// Whether the editor part should be displayed to the user.
        /// An editor part may decide that it should not be shown based on the state
        /// or the type of web part it is associated with.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual bool Display {
            get {
                // Always want EditorPart to be visible at design time (VSWhidbey 458247)
                if (DesignMode) {
                    return true;
                }

                if (WebPartToEdit != null) {
                    // Do not display EditorParts for a ProxyWebPart, regardless of the value
                    // of AllowEdit, IsShared, and PersonalizationScope
                    if (WebPartToEdit is ProxyWebPart) {
                        return false;
                    }

                    if (!WebPartToEdit.AllowEdit &&
                        WebPartToEdit.IsShared &&
                        WebPartManager != null &&
                        WebPartManager.Personalization.Scope == PersonalizationScope.User) {
                        return false;
                    }

                    return true;
                }

                // If there is no WebPartToEdit, return false as a default case
                return false;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string DisplayTitle {
            get {
                string displayTitle = Title;
                if (String.IsNullOrEmpty(displayTitle)) {
                    displayTitle = SR.GetString(SR.Part_Untitled);
                }
                return displayTitle;
            }
        }

        protected WebPartManager WebPartManager {
            get {
                return _webPartManager;
            }
        }

        /// <devdoc>
        /// The web part that is being edited by this editor part.  Set by the EditorZoneBase after
        /// the EditorPart is added to the zone's control collection.
        /// </devdoc>
        protected WebPart WebPartToEdit {
            get {
                return _webPartToEdit;
            }
        }

        protected EditorZoneBase Zone {
            get {
                return _zone;
            }
        }

        /// <devdoc>
        /// Called by the Zone when the EditorPart should apply values to its associated control.  True indicates
        /// that the save was successful, false indicates that an error occurred.
        /// </devdoc>
        public abstract bool ApplyChanges();

        // If custom errors are enabled, we do not want to render the exception message to the browser. (VSWhidbey 381646)
        internal string CreateErrorMessage(string exceptionMessage) {
            if (Context != null && Context.IsCustomErrorEnabled) {
                return SR.GetString(SR.EditorPart_ErrorSettingProperty);
            }
            else {
                return SR.GetString(SR.EditorPart_ErrorSettingPropertyWithExceptionMessage, exceptionMessage);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override IDictionary GetDesignModeState() {
            IDictionary state = new HybridDictionary(1);
            state["Zone"] = Zone;
            return state;
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (Zone == null) {
                throw new InvalidOperationException(SR.GetString(SR.EditorPart_MustBeInZone, ID));
            }

            // Need to set Visible=false so postback is handled correctly for child controls
            // i.e. CheckBox child controls will always be set to false after postback unless
            // they are marked as not visible
            if (Display == false) {
                Visible = false;
            }
        }

        private void RenderDisplayName(HtmlTextWriter writer, string displayName, string associatedClientID) {
            if (Zone != null) {
                Zone.LabelStyle.AddAttributesToRender(writer, this);
            }
            writer.AddAttribute(HtmlTextWriterAttribute.For, associatedClientID);
            writer.RenderBeginTag(HtmlTextWriterTag.Label);
            writer.WriteEncodedText(displayName);
            writer.RenderEndTag();  // Label
        }

        internal void RenderPropertyEditors(HtmlTextWriter writer, string[] propertyDisplayNames, string[] propertyDescriptions,
                                            WebControl[] propertyEditors, string[] errorMessages) {
            Debug.Assert(propertyDisplayNames.Length == propertyEditors.Length);
            Debug.Assert(propertyDisplayNames.Length == errorMessages.Length);
            Debug.Assert(propertyDescriptions == null || (propertyDescriptions.Length == propertyDisplayNames.Length));

            if (propertyDisplayNames.Length == 0) {
                return;
            }

            writer.AddAttribute(HtmlTextWriterAttribute.Border, "0");
            writer.AddAttribute(HtmlTextWriterAttribute.Cellspacing, "4");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            for (int i = 0; i < propertyDisplayNames.Length; i++) {
                WebControl editUIControl = propertyEditors[i];
                if (Zone != null && !Zone.EditUIStyle.IsEmpty) {
                    editUIControl.ApplyStyle(Zone.EditUIStyle);
                }

                string propertyDescription = (propertyDescriptions != null) ? propertyDescriptions[i] : null;
                if (!String.IsNullOrEmpty(propertyDescription)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, propertyDescription);
                }

                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                if (editUIControl is CheckBox) {
                    editUIControl.RenderControl(writer);
                    writer.Write("&nbsp;");
                    RenderDisplayName(writer, propertyDisplayNames[i], editUIControl.ClientID);
                }
                else {
                    string associatedClientID;
                    CompositeControl compositeControl = editUIControl as CompositeControl;
                    if (compositeControl != null) {
                        // The <label for> tag should point to the first child control of the
                        // composite control. (VSWhidbey 372756)
                        associatedClientID = compositeControl.Controls[0].ClientID;
                    }
                    else {
                        // The <label for> tag should point to the editUIControl itself.
                        associatedClientID = editUIControl.ClientID;
                    }

                    RenderDisplayName(writer, propertyDisplayNames[i] + ":", associatedClientID);
                    writer.WriteBreak();
                    writer.WriteLine();
                    editUIControl.RenderControl(writer);
                }
                writer.WriteBreak();
                writer.WriteLine();

                string errorMessage = errorMessages[i];
                if (!String.IsNullOrEmpty(errorMessage)) {
                    if (Zone != null && !Zone.ErrorStyle.IsEmpty) {
                        Zone.ErrorStyle.AddAttributesToRender(writer, this);
                    }

                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.WriteEncodedText(errorMessage);
                    writer.RenderEndTag();  // Span
                    writer.WriteBreak();
                    writer.WriteLine();
                }

                writer.RenderEndTag();  // Td
                writer.RenderEndTag();  // Tr
            }

            writer.RenderEndTag();  // Table
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["Zone"];
                if (o != null) {
                    SetZone((EditorZoneBase)o);
                }
            }
        }

        internal void SetWebPartToEdit(WebPart webPartToEdit) {
            _webPartToEdit = webPartToEdit;
        }

        internal void SetWebPartManager(WebPartManager webPartManager) {
            _webPartManager = webPartManager;
        }

        internal void SetZone(EditorZoneBase zone) {
            _zone = zone;
        }

        /// <devdoc>
        /// Called by the Zone when the EditorPart should [....] its values because other EditorParts
        /// may have changed control properties.  This is only called after all the ApplyChanges have returned.
        /// </devdoc>
        public abstract void SyncChanges();
    }
}
