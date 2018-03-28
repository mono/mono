//------------------------------------------------------------------------------
// <copyright file="FileUpload.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// 





namespace System.Web.UI.WebControls {

    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.IO;
    using System.Text;
    using System.Web.UI.HtmlControls;


    /// <devdoc>
    /// Displays a text box and browse button that allows the user to select a file for uploading.
    /// </devdoc>
    [ControlValueProperty("FileBytes")]
    [ValidationProperty("FileName")]
    [Designer("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + AssemblyRef.SystemDesign)]
    public class FileUpload : WebControl {

        private static readonly IList<HttpPostedFile> _emptyFileCollection = new HttpPostedFile[0];
        private IList<HttpPostedFile> _postedFiles;

        public FileUpload() : base(HtmlTextWriterTag.Input) {
        }

        [
        Browsable(true),
        DefaultValue(false),
        WebCategory("Behavior"),
        WebSysDescription(SR.FileUpload_AllowMultiple)
        ]
        public virtual bool AllowMultiple {
            get {
                object o = ViewState["AllowMultiple"];
                return (o != null) ? (bool)o : false;
            }
            set {
                ViewState["AllowMultiple"] = value;
            }
        }

        /// <devdoc>
        /// Gets the byte contents of the uploaded file.  Needed for ControlParameters and templatized
        /// ImageFields.
        /// </devdoc>
        [
        Bindable(true),
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public byte[] FileBytes {
            get {
                Stream fileStream = FileContent;
                if (fileStream != null && fileStream != Stream.Null) {
                    long fileStreamLength = fileStream.Length;
                    BinaryReader reader = new BinaryReader(fileStream);
                    Byte[] completeImage = null;
                    
                    if (fileStreamLength > Int32.MaxValue) {
                        throw new HttpException(SR.GetString(SR.FileUpload_StreamTooLong));
                    }

                    if (!fileStream.CanSeek) {
                        throw new HttpException(SR.GetString(SR.FileUpload_StreamNotSeekable));
                    }

                    int currentStreamPosition = (int)fileStream.Position;
                    int fileStreamIntLength = (int)fileStreamLength;
                    try {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        completeImage = reader.ReadBytes(fileStreamIntLength);
                    }
                    finally {
                        // Don't close or dispose of the BinaryReader because doing so would close the stream.
                        // We want to put the stream back to the original position in case this getter is called again
                        // and the stream supports seeking, the bytes will be returned again.
                        fileStream.Seek(currentStreamPosition, SeekOrigin.Begin);
                    }
                    if (completeImage.Length != fileStreamIntLength) {
                        throw new HttpException(SR.GetString(SR.FileUpload_StreamLengthNotReached));
                    }
                    return completeImage;
                }
                return new byte[0];
            }
        }


        /// <devdoc>
        /// Gets the contents of the uploaded file.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Stream FileContent {
            get {
                HttpPostedFile f = PostedFile;
                if (f != null) {
                    return PostedFile.InputStream;
                }

                return Stream.Null;
            }
        }


        /// <devdoc>
        /// The name of the file on the client's computer, not including the path.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public string FileName {
            get {
                HttpPostedFile postedFile = PostedFile;
                string fileName = string.Empty;

                if (postedFile != null) {
                    string fullFileName = postedFile.FileName;

                    try {
                        // Some browsers (IE 6, Netscape 4) return the fully-qualified filename,
                        // like "C:\temp\foo.txt".  The application writer is probably not interested
                        // in the client path, so we just return the filename part.
                        fileName = Path.GetFileName(fullFileName);
                    }
                    catch {
                        fileName = fullFileName;
                    }
                }

                return fileName;
            }
        }


        /// <devdoc>
        /// Whether or not a file was uploaded.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool HasFile {
            get {
                // Unfortunately returns false if a 0-byte file was uploaded, since we see a 0-byte
                // file if the user entered nothing, an invalid filename, or a valid filename
                // of a 0-byte file.  We feel this scenario is uncommon.
                HttpPostedFile f = PostedFile;
                return f != null && f.ContentLength > 0;
            }
        }

        /// <devdoc>
        /// Whether or not at least 1 non-0-length file was uploaded.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public bool HasFiles {
            get {
                // Unfortunately returns false if a 0-byte file was uploaded, since we see a 0-byte
                // file if the user entered nothing, an invalid filename, or a valid filename
                // of a 0-byte file.  We feel this scenario is uncommon.
                return PostedFiles.Any(f => f.ContentLength > 0);
            }
        }


        /// <devdoc>
        /// Provides access to the underlying HttpPostedFile.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public HttpPostedFile PostedFile {
            get {
                if (Page != null && Page.IsPostBack) {
                    return Context.Request.Files[UniqueID];
                }

                return null;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public IList<HttpPostedFile> PostedFiles {
            get {
                if (_postedFiles == null) {
                    IList<HttpPostedFile> result = _emptyFileCollection;
                    if (Page != null && Page.IsPostBack) {
                        result = Context.Request.Files.GetMultiple(UniqueID);
                        Debug.Assert(result != null);
                    }
                    _postedFiles = result;
                }
                return _postedFiles;
            }
        }

        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "file");

            if (AllowMultiple) {
                writer.AddAttribute(HtmlTextWriterAttribute.Multiple, "multiple");
            }

            string uniqueID = UniqueID;
            if (uniqueID != null) {
                writer.AddAttribute(HtmlTextWriterAttribute.Name, uniqueID);
            }

            base.AddAttributesToRender(writer);
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);
            HtmlForm form = Page.Form;
            if (form != null && form.Enctype.Length == 0) {
                form.Enctype = "multipart/form-data";
            }
        }


        protected internal override void Render(HtmlTextWriter writer) {
            // Make sure we are in a form tag with runat=server.
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            base.Render(writer);
        }


        /// <devdoc>
        /// Initiates a utility method to save an uploaded file to disk.
        /// </devdoc>
        public void SaveAs(string filename) {
            HttpPostedFile f = PostedFile;
            if (f != null) {
                f.SaveAs(filename);
            }
        }

    }
}
