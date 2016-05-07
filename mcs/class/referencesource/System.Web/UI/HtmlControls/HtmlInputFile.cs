//------------------------------------------------------------------------------
// <copyright file="HtmlInputFile.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * HtmlInputFile.cs
 *
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Web.UI.HtmlControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Security.Permissions;


/// <devdoc>
///    <para>
///       The <see langword='HtmlInputFile'/> class defines the
///       methods, properties, and events for the <see langword='HtmlInputFile'/> control. This class allows
///       programmatic access to the HTML &lt;input type= file&gt; element on the server.
///       It provides access to the stream, as well as a useful SaveAs functionality
///       provided by the <see cref='System.Web.UI.HtmlControls.HtmlInputFile.PostedFile'/>
///       property.
///    </para>
///    <note type="caution">
///       This class only works if the
///       HtmlForm.Enctype property is set to "multipart/form-data".
///       Also, it does not maintain its
///       state across multiple round trips between browser and server. If the user sets
///       this value after a round trip, the value is lost.
///    </note>
/// </devdoc>
    [
    ValidationProperty("Value")
    ]
    public class HtmlInputFile : HtmlInputControl, IPostBackDataHandler {

        /*
         * Creates an intrinsic Html INPUT type=file control.
         */

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.HtmlControls.HtmlInputFile'/> class.</para>
        /// </devdoc>
        public HtmlInputFile() : base("file") {
        }

        /*
         * Accept type property.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or sets a comma-separated list of MIME encodings that
        ///       can be used to constrain the file types that the browser lets the user
        ///       select. For example, to restrict the
        ///       selection to images, the accept value image/* should be specified.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string Accept {
            get {
                string s = Attributes["accept"];
                return((s != null) ? s : String.Empty);
            }
            set {
                Attributes["accept"] = MapStringAttributeToString(value);
            }
        }

        /*
         * The property for the maximum characters allowed.
         */

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the
        ///       maximum length of the file path of the file to upload
        ///       from the client machine.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int MaxLength {
            get {
                string s = Attributes["maxlength"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["maxlength"] = MapIntegerAttributeToString(value);
            }
        }

        /*
         * PostedFile property.
         */

        /// <devdoc>
        ///    <para>Gets access to the uploaded file specified by a client.</para>
        /// </devdoc>
        [
        WebCategory("Default"),
        DefaultValue(""),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpPostedFile PostedFile {
            get { return Context.Request.Files[RenderedNameAttribute];}
        }

        /*
         * The property for the width in characters.
         */

        /// <devdoc>
        ///    <para>Gets or sets the width of the file-path text box that the
        ///       browser displays when the <see cref='System.Web.UI.HtmlControls.HtmlInputFile'/>
        ///       control is used on a page.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(-1),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int Size {
            get {
                string s = Attributes["size"];
                return((s != null) ? Int32.Parse(s, CultureInfo.InvariantCulture) : -1);
            }
            set {
                Attributes["size"] = MapIntegerAttributeToString(value);
            }
        }

        // ASURT 122262 : The value property isn't submitted back to us when the a page
        // containing this control postsback, so required field validators are broken
        // (value would contain the empty string).  To fix this, we return the filename.

        [
        Browsable(false)
        ]
        public override string Value {
            get {
                HttpPostedFile postedFile = PostedFile;
                if (postedFile != null) {
                    return postedFile.FileName;
                }

                return String.Empty;
            }
            set {
                // Throw here because setting the value on this tag has no effect on the
                // rendering behavior and since we're always returning the posted file's
                // filename, we don't want to get into a situation where the user
                // sets a value and does not get back that value.
                throw new NotSupportedException(SR.GetString(SR.Value_Set_Not_Supported, this.GetType().Name));
            }
        }

        /*
         * Method of IPostBackDataHandler interface to process posted data.
         */

        /// <internalonly/>
        bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return LoadPostData(postDataKey, postCollection);
        }


        protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection) {
            return false;
        }

        /*
         * Method of IPostBackDataHandler interface which is invoked whenever
         * posted data for a control has changed.  RadioButton fires an
         * OnServerChange event.
         */

        /// <internalonly/>
        void IPostBackDataHandler.RaisePostDataChangedEvent() {
            RaisePostDataChangedEvent();
        }


        protected virtual void RaisePostDataChangedEvent() {
        }


        /// <devdoc>
        /// <para>Raises the <see langword='PreRender'/> event. This method uses event arguments
        ///    to pass the event data to the control.</para>
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // ASURT 35328: use multipart encoding if no encoding is currently specified
            HtmlForm form = Page.Form;
            if (form != null && form.Enctype.Length == 0) {
                form.Enctype = "multipart/form-data";
            }
        }
    }
}
