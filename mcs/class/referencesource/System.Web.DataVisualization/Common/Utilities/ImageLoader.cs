//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ImageLoader.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Utilities
//
//	Classes:	ImageLoader
//
//  Purpose:	ImageLoader utility class loads specified image and 
//              caches it in the memory for the future use.
//          
//              Images can be loaded from different places including 
//              Files, URIs, WebRequests and Control Resources.
//
//	Reviewed:	AG - August 7, 2002
//              AG - Microsoft 5, 2007
//
//===================================================================


#region Used Namespaces

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Net;
using System.IO;
using System.Security;
using System.Resources;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web;
    using System.Web.UI.DataVisualization.Charting;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Utilities
#else
	namespace System.Web.UI.DataVisualization.Charting.Utilities
#endif
{
	/// <summary>
    /// ImageLoader utility class loads and returns specified image 
    /// form the File, URI, Web Request or Chart Resources. 
    /// Loaded images are stored in the internal hashtable which 
    /// allows to improve performance if image need to be used 
    /// several times.
	/// </summary>
	internal class ImageLoader : IDisposable, IServiceProvider
	{
		#region Fields

		// Image storage
		private Hashtable			_imageData = null;

		// Reference to the service container
		private IServiceContainer	_serviceContainer = null;

		#endregion

		#region Constructors and Initialization

		/// <summary>
		/// Default constructor is not accessible.
		/// </summary>
		private ImageLoader()
		{
		}

		/// <summary>
		/// Default public constructor.
		/// </summary>
		/// <param name="container">Service container.</param>
		public ImageLoader(IServiceContainer container)
		{
			if(container == null)
			{
				throw(new ArgumentNullException(SR.ExceptionImageLoaderInvalidServiceContainer));
			}
            _serviceContainer = container;
		}

		/// <summary>
		/// Returns Image Loader service object
		/// </summary>
		/// <param name="serviceType">Requested service type.</param>
		/// <returns>Image Loader service object.</returns>
		[EditorBrowsableAttribute(EditorBrowsableState.Never)]
		object IServiceProvider.GetService(Type serviceType)
		{
			if(serviceType == typeof(ImageLoader))
			{
				return this;
			}
			throw (new ArgumentException( SR.ExceptionImageLoaderUnsupportedType( serviceType.ToString())));
		}

		/// <summary>
		/// Dispose images in the hashtable
		/// </summary>
		public void Dispose()
		{
            if (_imageData != null)
			{
                foreach (DictionaryEntry entry in _imageData)
				{
                    if (entry.Value is IDisposable)
                    {
                        ((IDisposable)entry.Value).Dispose();
                    }
				}
                _imageData = null;
				GC.SuppressFinalize(this);  
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Loads image from URL. Checks if image already loaded (cached).
		/// </summary>
        /// <param name="imageURL">Image name (FileName, URL, Resource).</param>
		/// <returns>Image object.</returns>
		public System.Drawing.Image LoadImage(string imageURL)
		{
			return LoadImage(imageURL, true);
		}
			
		/// <summary>
		/// Loads image from URL. Checks if image already loaded (cached).
		/// </summary>
		/// <param name="imageURL">Image name (FileName, URL, Resource).</param>
		/// <param name="saveImage">True if loaded image should be saved in cache.</param>
		/// <returns>Image object</returns>
        public System.Drawing.Image LoadImage(string imageURL, bool saveImage)
		{
            System.Drawing.Image image = null;

			// Check if image is defined in the chart image collection
            if (_serviceContainer != null)
			{
                Chart chart = (Chart)_serviceContainer.GetService(typeof(Chart));
				if(chart != null)
				{
					foreach(NamedImage namedImage in chart.Images)
					{
						if(namedImage.Name == imageURL)
						{
							return namedImage.Image;
						}
					}
				}
			}

			// Create new hashtable
            if (_imageData == null)
			{
                _imageData = new Hashtable(StringComparer.OrdinalIgnoreCase);
			}

			// First check if image with this name already loaded
            if (_imageData.Contains(imageURL))
			{
				image = (System.Drawing.Image)_imageData[imageURL];
			}

#if ! Microsoft_CONTROL

			// Try to load as relative URL using the Control object
			if(image == null)
			{
                Chart control = (Chart)_serviceContainer.GetService(typeof(Chart));
                if (control != null && control.Page != null)
                {
                    if (!control.IsDesignMode())
                    {
                        image = LoadFromFile(control.Page.MapPath(imageURL));
                    }
                    else if (control.IsDesignMode() && !String.IsNullOrEmpty(control.webFormDocumentURL))
                    {   
                        // Find current web page path and fileName
                        Uri pageUri = new Uri(control.webFormDocumentURL);
                        string path = pageUri.LocalPath;
                        string pageFile = pageUri.Segments[pageUri.Segments.Length-1];

                        // Find full image fileName
                        string imageFileRelative = control.ResolveClientUrl(imageURL);
                        string imageFile = path.Replace(pageFile, imageFileRelative);
                        
                        // Load image
                        image = LoadFromFile(imageFile);
                    }
                }

                else if ( HttpContext.Current != null )
                {
                    image = LoadFromFile(HttpContext.Current.Request.MapPath(imageURL));
                }
			}
#endif

			// Try to load image from resource
			if(image == null)
			{
                try
                {

                    // Check if resource class type was specified
                    int columnIndex = imageURL.IndexOf("::", StringComparison.Ordinal);
                    if (columnIndex > 0)
                    {
                        string resourceRootName = imageURL.Substring(0, columnIndex);
                        string resourceName = imageURL.Substring(columnIndex + 2);
                        System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(resourceRootName, Assembly.GetExecutingAssembly());
                        image = (System.Drawing.Image)(resourceManager.GetObject(resourceName));
                    }
#if Microsoft_CONTROL
                    else if (Assembly.GetEntryAssembly() != null)
                    {
                        // Check if resource class type was specified
                        columnIndex = imageURL.IndexOf(':');
                        if (columnIndex > 0)
                        {
                            string resourceRootName = imageURL.Substring(0, columnIndex);
                            string resourceName = imageURL.Substring(columnIndex + 1);
                            System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(resourceRootName, Assembly.GetEntryAssembly());
                            image = (Image)(resourceManager.GetObject(resourceName));
                        }
                        else
                        {
                            // Try to load resource from every type defined in entry assembly
                            Assembly entryAssembly = Assembly.GetEntryAssembly();
                            if (entryAssembly != null)
                            {
                                foreach (Type type in entryAssembly.GetTypes())
                                {
                                    System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager(type);
                                    try
                                    {
                                        image = (Image)(resourceManager.GetObject(imageURL));
                                    }
                                    catch (ArgumentNullException)
                                    {
                                    }
                                    catch (MissingManifestResourceException)
                                    {
                                    }

                                    // Check if image was loaded
                                    if (image != null)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
#endif
                }
                catch (MissingManifestResourceException)
                {
                }
			}
		

			// Try to load image using the Web Request
			if(image == null)
			{
				Uri	imageUri = null;
				try 
				{
					// Try to create URI directly from image URL (will work in case of absolute URL)
					imageUri = new Uri(imageURL);
				}
				catch(UriFormatException)
				{}


				// Load image from file or web resource
				if(imageUri != null)
				{
                    try
                    {
                        WebRequest request = WebRequest.Create(imageUri);
                        image = System.Drawing.Image.FromStream(request.GetResponse().GetResponseStream());
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (NotSupportedException)
                    {
                    }
                    catch (SecurityException)
                    {
                    }
				}
            }
#if Microsoft_CONTROL
            // absolute uri(without Server.MapPath)in web is not allowed. Loading from replative uri Server[Page].MapPath is done above.
            // Try to load as file
			if(image == null)
			{

                image = LoadFromFile(imageURL);
            }
#endif

            // Error loading image
			if(image == null)
			{
#if ! Microsoft_CONTROL
				throw(new ArgumentException( SR.ExceptionImageLoaderIncorrectImageUrl( imageURL ) ) );
#else
				throw(new ArgumentException( SR.ExceptionImageLoaderIncorrectImageLocation( imageURL ) ) );
#endif
            }

			// Save new image in cache
			if(saveImage)
			{
                _imageData[imageURL] = image;
			}

			return image;
		}

		/// <summary>
		/// Helper function which loads image from file.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Loaded image or null.</returns>
        private System.Drawing.Image LoadFromFile(string fileName)
		{
			// Try to load image from file
			try
			{
				return System.Drawing.Image.FromFile(fileName);
			}
			catch(FileNotFoundException)
			{
				return null;
			}
		}

        /// <summary>
        /// Returns the image size taking the image DPI into consideration.
        /// </summary>
        /// <param name="name">Image name (FileName, URL, Resource).</param>
        /// <param name="graphics">Graphics used to calculate the image size.</param>
        /// <param name="size">Calculated size.</param>
        /// <returns>false if it fails to calculate the size, otherwise true.</returns>
        internal bool GetAdjustedImageSize(string name, Graphics graphics, ref SizeF size)
        {
            Image image = LoadImage(name);

            if (image == null)
                return false;

            GetAdjustedImageSize(image, graphics, ref size);

            return true;
        }

        /// <summary>
        /// Returns the image size taking the image DPI into consideration.
        /// </summary>
        /// <param name="image">Image for whcih to calculate the size.</param>
        /// <param name="graphics">Graphics used to calculate the image size.</param>
        /// <param name="size">Calculated size.</param>
        internal static void GetAdjustedImageSize(Image image, Graphics graphics, ref SizeF size)
        {
            if (graphics != null)
            {
                //this will work in case the image DPI is specified, otherwise the image DPI will be assumed to be same as the screen DPI
                size.Width = image.Width * graphics.DpiX / image.HorizontalResolution;
                size.Height = image.Height * graphics.DpiY / image.VerticalResolution;
            }
            else
            {
                size.Width = image.Width;
                size.Height = image.Height;
            }
        }

        /// <summary>
        /// Checks if the image has the same DPI as the graphics object.
        /// </summary>
        /// <param name="image">Image to be checked.</param>
        /// <param name="graphics">Graphics object to be used.</param>
        /// <returns>true if they match, otherwise false.</returns>
        internal static bool DoDpisMatch(Image image, Graphics graphics)
        {
            return graphics.DpiX == image.HorizontalResolution && graphics.DpiY == image.VerticalResolution;
        }

        internal static Image GetScaledImage(Image image, Graphics graphics)
        {
            Bitmap scaledImage = new Bitmap(image, new Size((int)(image.Width * graphics.DpiX / image.HorizontalResolution),
                (int)(image.Height * graphics.DpiY / image.VerticalResolution)));

            return scaledImage;
        }


		#endregion
	}
}
