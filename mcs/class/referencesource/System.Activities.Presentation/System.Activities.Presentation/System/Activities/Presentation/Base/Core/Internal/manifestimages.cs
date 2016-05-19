//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal 
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using System.Windows.Media.Imaging;
    using System.Diagnostics;
    using System.Runtime;
    using System.Activities.Presentation;

    // <summary>
    // Helper class that knows how to load up icons that live in assemblies and follow
    // our extensibility icon naming convention.
    // </summary>
    internal static class ManifestImages 
    {

        private static readonly string[] SupportedExtensions = new string[] {
                ".png", ".xaml", ".bmp", ".gif", ".jpg", ".jpeg"
            };

        // <summary>
        // ----s open the assembly to which the specified Type belongs and tries
        // to find and return an image that follows the naming conventions of:
        //
        //     My.Namespace.MyControl.Icon.png
        //
        // and matches the desired size most closely, if multiple such images are found.
        // </summary>
        // <param name="type">Type to look up</param>
        // <param name="desiredSize">Desired size (may not be met)</param>
        // <returns>Null (if no image was found), Image instance (for non-Xaml images),
        // or object instance (for Xaml-instantiated structures)</returns>
        // <exception cref="ArgumentNullException">if type is null</exception>
        public static object GetImage(Type type, Size desiredSize) 
        {
            if (type == null)
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }

            Assembly assembly = type.Assembly;
            string[] resourceNames = assembly.GetManifestResourceNames();

            if (resourceNames == null || resourceNames.Length == 0)
            {
                return null;
            }

            string fullTypeName = type.FullName;
            string typeName = type.Name;

            // Do a full namespace match first
            ImageInfo bestMatch = FindBestMatch(type, assembly, resourceNames, desiredSize, delegate(string extensionlessResourceName) 
            {
                return fullTypeName.Equals(extensionlessResourceName);
            });

            // Do a partial name match second, if full name didn't give us anything
            bestMatch = bestMatch ?? FindBestMatch(type, assembly, resourceNames, desiredSize, delegate(string extensionlessResourceName) 
            {
                return extensionlessResourceName != null && extensionlessResourceName.EndsWith(typeName, StringComparison.Ordinal);
            });

            if (bestMatch != null)
            {
                return bestMatch.Image;
            }

            return null;
        }

        private static ImageInfo FindBestMatch(
            Type type,
            Assembly assembly,
            string[] resourceNames,
            Size desiredSize,
            MatchNameDelegate matchName) 
        {

            Fx.Assert(type != null, "FindBestMatch - type parameter should not be null");
            Fx.Assert(resourceNames != null && resourceNames.Length > 0, "resourceNames parameter should not be null");
            Fx.Assert(matchName != null, "matchName parameter should not be null");

            ImageInfo bestMatch = null;

            for (int i = 0; i < resourceNames.Length; i++) 
            {

                string extension = Path.GetExtension(resourceNames[i]);

                if (!IsExtensionSupported(extension))
                {
                    continue;
                }

                if (!matchName(StripIconExtension(resourceNames[i])))
                {
                    continue;
                }

                ImageInfo info = ProcessResource(assembly, resourceNames[i]);
                if (info == null)
                {
                    continue;
                }

                // Try to match the found resource to the requested size
                float sizeMatch = info.Match(desiredSize);

                // Check for exact size match
                if (sizeMatch < float.Epsilon)
                {
                    return info;
                }

                // Keep the best image found so far
                if (bestMatch == null ||
                    bestMatch.LastMatch > sizeMatch)
                {
                    bestMatch = info;
                }
            }

            return bestMatch;
        }

        // Tries to load up an image
        private static ImageInfo ProcessResource(Assembly assembly, string resourceName) 
        {
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return null;
            }

            if (IsXamlContent(resourceName))
            {
                return new XamlImageInfo(stream);
            }
            else
            {
                return new BitmapImageInfo(stream);
            }
        }

        // Checks to see whether the given extension is supported
        private static bool IsExtensionSupported(string extension) 
        {
            for (int i = 0; i < SupportedExtensions.Length; i++)
            {
                if (SupportedExtensions[i].Equals(extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // Returns true if the passed in resource name ends in ".xaml"
        private static bool IsXamlContent(string resourceName) 
        {
            return ".xaml".Equals(Path.GetExtension(resourceName), StringComparison.OrdinalIgnoreCase);
        }

        // Strips ".Icon.ext" from a resource name
        private static string StripIconExtension(string resourceName) 
        {
            if (resourceName == null)
            {
                return null;
            }

            resourceName = Path.GetFileNameWithoutExtension(resourceName);
            int dotIconIndex = resourceName.LastIndexOf(".Icon", StringComparison.OrdinalIgnoreCase);
            if (dotIconIndex > 0)
            {
                return resourceName.Substring(0, dotIconIndex);
            }

            return null;
        }

        private delegate bool MatchNameDelegate(string extensionlessResourceName);

        // Helper class that has information about an image
        private abstract class ImageInfo 
        {

            private float _lastMatch = 1;

            protected ImageInfo() 
            {
            }

            public float LastMatch 
            { get { return _lastMatch; } }
            public abstract object Image 
            { get; }
            protected abstract Size Size 
            { get; }
            protected abstract bool HasFixedSize 
            { get; }

            // gets value range from 0 to 1: 0 == perfect match, 1 == complete opposite
            public float Match(Size desiredSize) 
            {

                if (!this.HasFixedSize) 
                {
                    _lastMatch = 0;
                }
                else 
                {
                    Size actualSize = this.Size;

                    float desiredAspectRatio = Math.Max(float.Epsilon, GetAspectRatio(desiredSize));
                    float actualAspectRatio = Math.Max(float.Epsilon, GetAspectRatio(actualSize));

                    float desiredArea = Math.Max(float.Epsilon, GetArea(desiredSize));
                    float actualArea = Math.Max(float.Epsilon, GetArea(actualSize));

                    // these values range from 0 to 1, 1 being perfect match, 0 being not so perfect match
                    float ratioDiff = desiredAspectRatio < actualAspectRatio ? desiredAspectRatio / actualAspectRatio : actualAspectRatio / desiredAspectRatio;
                    float areaDiff = desiredArea < actualArea ? desiredArea / actualArea : actualArea / desiredArea;

                    float diff = ratioDiff * areaDiff;

                    _lastMatch = Math.Min(1f, Math.Max(0f, 1f - diff));
                }

                return _lastMatch;
            }

            private static float GetAspectRatio(Size size) 
            {
                if (size.Height < float.Epsilon)
                {
                    return 0;
                }

                return (float)(size.Width / size.Height);
            }

            private static float GetArea(Size size) 
            {
                return (float)(size.Width * size.Height);
            }
        }

        // Helper class that knows how to deal with Xaml
        private class XamlImageInfo : ImageInfo 
        {

            private object _image;

            public XamlImageInfo(Stream stream) 
            {
                _image = XamlReader.Load(stream);
            }

            public override object Image 
            {
                get { return _image; }
            }

            protected override Size Size 
            {
                get { return Size.Empty; }
            }

            protected override bool HasFixedSize 
            {
                get { return false; }
            }
        }

        // Helper class that knows how to deal with bitmaps
        private class BitmapImageInfo : ImageInfo 
        {

            private Image _image;
            private Size _size;

            public BitmapImageInfo(Stream stream) 
            {
                BitmapImage bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.StreamSource = stream;
                bmp.EndInit();

                _image = new Image();
                _image.Source = bmp;

                _size = new Size(bmp.Width, bmp.Height);
            }

            public override object Image 
            {
                get { return _image; }
            }

            protected override Size Size 
            {
                get { return _size; }
            }

            protected override bool HasFixedSize 
            {
                get { return true; }
            }
        }
    }
}
