//------------------------------------------------------------------------------
// <copyright file="ImageDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Converters;
    using System.Web.UI.Design.MobileControls.Util;

    using Image = System.Web.UI.MobileControls.Image;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class ImageDesigner : MobileControlDesigner
    {
        private Image                   _image;
        private TemporaryBitmapFile     _tempBmpFile;
        private Uri                     _cachedWbmpUri;
        private String                  _baseUrl = String.Empty;

        private String BaseUrl
        {
            get
            {
                if (_baseUrl != null && _baseUrl.Length == 0)
                {
                    IWebFormsDocumentService wfServices =
                        (IWebFormsDocumentService)GetService(typeof(IWebFormsDocumentService));

                                        Debug.Assert(wfServices != null);
                    _baseUrl = wfServices.DocumentUrl;
                }
                return _baseUrl;
            }
        }

        /// <summary>
        ///    <para>
        ///       Initializes the designer.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element being designed.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component being
        ///       designed.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.Image,
                         "ImageDesigner.Initialize - Invalid Image Control");
            _image = (System.Web.UI.MobileControls.Image) component;
            base.Initialize(component);
        }

        /// <summary>
        ///    <para>
        ///       Disposes of the resources (other than memory) used by the
        ///    <see cref='System.Web.UI.Design.MobileControls.ListDesigner'/>.
        ///    </para>
        /// </summary>
        /// <remarks>
        ///    <para>
        ///       Call <see langword='Dispose'/> when
        ///       you are finished using the <see cref='System.Web.UI.Design.MobileControls.ListDesigner'/>. The <see langword='Dispose'/> method leaves the <see cref='System.Web.UI.Design.WebControls.DataListDesigner'/> in an unusable state. After
        ///       calling <see langword='Dispose'/>, you must release all
        ///       references to the <see cref='System.Web.UI.Design.MobileControls.ListDesigner'/> so the memory it was occupying
        ///       can be reclaimed by garbage collection.
        ///    </para>
        ///    <note type="note">
        ///       Always call <see langword='Dispose'/> before you release your last reference to
        ///       the <see cref='System.Web.UI.Design.MobileControls.ListDesigner'/>. Otherwise, the resources
        ///       the <see cref='System.Web.UI.Design.MobileControls.ListDesigner'/> is using will not be freed
        ///       until garbage collection calls the <see cref='System.Web.UI.Design.MobileControls.ListDesigner'/> object's
        ///       destructor.
        ///    </note>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        protected override void Dispose(bool disposing) 
        {
            if (disposing && _tempBmpFile != null)
            {
                _tempBmpFile.Dispose();
                _cachedWbmpUri = null;
                _tempBmpFile = null;
            }

            base.Dispose(disposing);
        }

        private String GetConvertedImageURI(String imageUriString)
        {
            Uri baseUri = new Uri(BaseUrl);
            Uri imageUri = new Uri(baseUri, imageUriString);
            String extension = Path.GetExtension(imageUriString);

            if(extension.Equals(".wbmp"))
            {
                if(_tempBmpFile != null)
                {
                    if(_cachedWbmpUri != null
                        && _cachedWbmpUri.Equals(imageUri))
                    {
                        return _tempBmpFile.Url;
                    }
                    else
                    {
                        _tempBmpFile.Dispose();
                        _tempBmpFile = null;
                        _cachedWbmpUri = null;
                    }
                }

                Byte[] buffer = FileReader.Read(imageUri);
                if(buffer == null)
                {
                    // Could not read image from URI, return original URI to
                    // Trident and let it render as a broken image.
                    goto ConversionError;
                }
                Bitmap bitmap = WbmpConverter.Convert(buffer);
                if(bitmap == null)
                {
                    // .wbmp appears to be corrupt, return original URI to
                    // Trident and let it render as a broken image.
                    goto ConversionError;
                }
                _tempBmpFile = new TemporaryBitmapFile(bitmap);
                imageUriString = _tempBmpFile.Url;
                _cachedWbmpUri = imageUri;
            }
ConversionError:
            return imageUriString;
        }


        /// <summary>
        ///    <para>
        ///       Gets the HTML to be used for the design time representation of the control runtime.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       The design time HTML.
        ///    </para>
        /// </returns>
        protected override String GetDesignTimeNormalHtml()
        {
            String tempUrl = String.Empty;
            bool replaceUrl = (_image.ImageUrl.Length > 0);
            DesignerTextWriter writer = new DesignerTextWriter();

            tempUrl = _image.ImageUrl;
            _image.ImageUrl = GetConvertedImageURI(_image.ImageUrl);
            _image.Adapter.Render(writer);
            _image.ImageUrl = tempUrl;

            return writer.ToString();
        }

        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs e)
        {
            if ((e.Member != null) && e.Member.Name.Equals("NavigateUrl"))
            {
                _image.NavigateUrl = NavigateUrlConverter.GetUrl(
                    _image,
                    e.NewValue.ToString(),
                    e.OldValue.ToString()
                    );

                e = new ComponentChangedEventArgs(e.Component, e.Member, e.OldValue, _image.NavigateUrl);
            }
             
            base.OnComponentChanged(sender, e);
        }
    }
}
