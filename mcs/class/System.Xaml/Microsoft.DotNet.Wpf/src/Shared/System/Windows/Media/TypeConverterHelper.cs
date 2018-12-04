//------------------------------------------------------------------------------
//  Microsoft Avalon
//  Copyright (c) Microsoft Corporation, All rights reserved.
//
//  File:       TypeConverterHelper.cs
//
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup;
using System.Runtime.InteropServices;
using System.Windows.Navigation;

namespace System.Windows.Media
{
    #region UriHolder

    /// <summary>
    /// UriHolder
    /// Holds Original and Base Uris
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct UriHolder
    {

        /// <summary>
        /// BaseUri
        /// <remarks>Can be null</remarks>
        /// </summary>
        internal Uri BaseUri;

        /// <summary>
        /// OriginalUri
        /// </summary>
        internal Uri OriginalUri;
    };

    #endregion

    #region TypeConverterHelper

    /// <summary>
    ///     This helper method is used primarily by type converters to resolve their uri
    /// </summary>
    /// <remarks>
    ///     There are three scenarios that can happen:
    ///
    ///     1) inputString is an absolute uri -- we return it as the resolvedUri
    ///     2) inputString is not absolute:
    ///         i) the relativeBaseUri (obtained from IUriContext) has the following values:
    ///                 a) is an absolute uri, we use relativeBaseUri as base uri and resolve
    ///                 the inputString against it
    ///
    ///                 b) is a relative uri, we use Application's base uri (obtained from
    ///                 BindUriHelperCore.BaseUri) as the base and resolve the relativeBaseUri
    ///                 against it; furthermore, we resolve the inputString against with uri
    ///                 obtained from the application base resolution.
    ///
    ///                 c) is "", we resolve inputString against the Application's base uri
    /// </remarks>
    internal static class TypeConverterHelper
    {
        internal static UriHolder GetUriFromUriContext(ITypeDescriptorContext context, object inputString)
        {
            UriHolder uriHolder = new UriHolder();

            if (inputString is string)
            {
                uriHolder.OriginalUri = new Uri((string)inputString, UriKind.RelativeOrAbsolute);
            }
            else
            {
                Debug.Assert(inputString is Uri);
                uriHolder.OriginalUri = (Uri)inputString;
            }

            if (uriHolder.OriginalUri.IsAbsoluteUri == false)
            {
                //Debug.Assert (context != null, "Context should not be null");
                if (context != null)
                {
                    IUriContext iuc = (IUriContext)context.GetService(typeof(IUriContext));

                    //Debug.Assert (iuc != null, "IUriContext should not be null here");
                    if (iuc != null)
                    {
                        // the base uri is NOT ""
                        if (iuc.BaseUri != null)
                        {

                            uriHolder.BaseUri = iuc.BaseUri;

                            if (!uriHolder.BaseUri.IsAbsoluteUri)
                            {
                                uriHolder.BaseUri = new Uri(BaseUriHelper.BaseUri, uriHolder.BaseUri);
                            }
                        } // uriHolder.BaseUriString != ""
                        else
                        {
                            // if we reach here, the base uri we got from IUriContext is ""
                            // and the inputString is a relative uri.  Here we resolve it to
                            // application's base
                            uriHolder.BaseUri = BaseUriHelper.BaseUri;
                        }
                    } // iuc != null
                } // context!= null
            } // uriHolder.OriginalUri.IsAbsoluteUri == false

            return uriHolder;
        }
    }

#endregion
}
