//------------------------------------------------------------------------------
// <copyright file="XmlResolver.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml
{
    using System;
    using System.IO;
    using System.Text;
    using System.Security;
    using System.Security.Permissions;
#if !SILVERLIGHT
    using System.Net;
    using System.Threading.Tasks;
#endif
    using System.Runtime.Versioning;
    
    /// <include file='doc\XmlResolver.uex' path='docs/doc[@for="XmlResolver"]/*' />
    /// <devdoc>
    ///    <para>Resolves external XML resources named by a Uniform
    ///       Resource Identifier (URI). This class is <see langword='abstract'/>
    ///       .</para>
    /// </devdoc>
    public abstract partial class XmlResolver {
        /// <include file='doc\XmlResolver.uex' path='docs/doc[@for="XmlResolver.GetEntity1"]/*' />
        /// <devdoc>
        ///    <para>Maps a
        ///       URI to an Object containing the actual resource.</para>
        /// </devdoc>

        public abstract Object GetEntity(Uri absoluteUri,
                                         string role,
                                         Type ofObjectToReturn);

        

        /// <include file='doc\XmlResolver.uex' path='docs/doc[@for="XmlResolver.ResolveUri"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
#if !SILVERLIGHT
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
#endif
        public virtual Uri ResolveUri(Uri baseUri, string relativeUri) {
            if ( baseUri == null || ( !baseUri.IsAbsoluteUri && baseUri.OriginalString.Length == 0 ) ) {
                Uri uri = new Uri( relativeUri, UriKind.RelativeOrAbsolute );
#if !SILVERLIGHT // Path.GetFullPath is SecurityCritical
                if ( !uri.IsAbsoluteUri && uri.OriginalString.Length > 0 ) {
                    uri = new Uri( Path.GetFullPath( relativeUri ) );
                }
#endif
                return uri;
            }
            else {
                if (relativeUri == null || relativeUri.Length == 0) {
                    return baseUri;
                }
                // relative base Uri
                if ( !baseUri.IsAbsoluteUri ) {
#if SILVERLIGHT
                    // create temporary base for the relative URIs
                    Uri tmpBaseUri = new Uri("tmp:///");

                    // create absolute base URI with the temporary base
                    Uri absBaseUri = new Uri(tmpBaseUri, baseUri.OriginalString);

                    // resolve the relative Uri into a new absolute URI
                    Uri resolvedAbsUri = new Uri(absBaseUri, relativeUri);

                    // make it relative by removing temporary base
                    Uri resolvedRelUri = tmpBaseUri.MakeRelativeUri(resolvedAbsUri);

                    return resolvedRelUri;
#else
                    throw new NotSupportedException(Res.GetString(Res.Xml_RelativeUriNotSupported));
#endif
                }
                return new Uri( baseUri, relativeUri );
            }
        }

#if !SILVERLIGHT
        //UE attension
        /// <include file='doc\XmlResolver.uex' path='docs/doc[@for="XmlResolver.Credentials"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual ICredentials Credentials {
            set { }
        }
#endif

        public virtual bool SupportsType(Uri absoluteUri, Type type) {
            if (absoluteUri == null) {
                throw new ArgumentNullException("absoluteUri");
            }
            if (type == null || type == typeof(Stream)) {
                return true;
            }
            return false;
        }
    }
}
