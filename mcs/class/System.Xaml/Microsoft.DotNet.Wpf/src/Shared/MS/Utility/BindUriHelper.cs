using System;
using System.IO;
using System.Net; // HttpWebRequest
using System.Net.Cache; // HttpRequestCachePolicy
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text; 
using System.Windows.Navigation; // BaseUriHelper

#if !PBTCOMPILER
using MS.Win32;
#endif

using System.Security;
using System.Security.Permissions;
// The functionality in this class is shared across framework and core. The functionality in core
// is a subset of the functionality in framework, so rather than create a dependency from core to
// framework we have choses to duplicate this chunk of  code.
#if PRESENTATION_CORE
namespace MS.Internal.PresentationCore
#elif PRESENTATIONFRAMEWORK
using MS.Internal.PresentationFramework; // SecurityHelper

namespace MS.Internal.Utility
#elif PBTCOMPILER
using MS.Internal.PresentationBuildTasks // SecurityHelper

namespace MS.Internal.Utility
#elif REACHFRAMEWORK
using MS.Internal.ReachFramework; // SecurityHelper

namespace MS.Internal.Utility
#else
#error Class is being used from an unknown assembly.
#endif
{
   // 
   // Methods in this partial class are shared by PresentationFramework and PresentationBuildTasks.
   // See also WpfWebRequestHelper.
   //
   internal static  partial class BindUriHelper
   {
        private const int MAX_PATH_LENGTH = 2048 ;
        private const int MAX_SCHEME_LENGTH = 32;
        public const int MAX_URL_LENGTH = MAX_PATH_LENGTH + MAX_SCHEME_LENGTH + 3; /*=sizeof("://")*/
 
        //
        // Uri-toString does 3 things over the standard .toString()
        //
        //  1) We don't unescape special control characters. The default Uri.ToString() 
        //     will unescape a character like ctrl-g, or ctrl-h so the actual char is emitted. 
        //     However it's considered safer to emit the escaped version. 
        //
        //  2) We truncate urls so that they are always <= MAX_URL_LENGTH
        // 
        // This method should be called whenever you are taking a Uri
        // and performing a p-invoke on it. 
        //
        internal static string UriToString(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }            
         
            return new StringBuilder(
                uri.GetComponents(
                    uri.IsAbsoluteUri ? UriComponents.AbsoluteUri : UriComponents.SerializationInfoString, 
                    UriFormat.SafeUnescaped), 
                MAX_URL_LENGTH).ToString();
        }        
        
#if PRESENTATION_CORE || PRESENTATIONFRAMEWORK
        // Base Uri.
        /// <SecurityNote>
        /// Critical: as it sets the baseUri
        /// </SecurityNote>
        static internal Uri BaseUri
        {
            get
            {
                return BaseUriHelper.BaseUri;
            }
            [SecurityCritical]
            set
            {
                 BaseUriHelper.BaseUri = BaseUriHelper.FixFileUri(value);
            }
        }

        static internal bool DoSchemeAndHostMatch(Uri first, Uri second)
        {
            // Check that both the scheme and the host match. 
           return (SecurityHelper.AreStringTypesEqual(first.Scheme, second.Scheme) && first.Host.Equals(second.Host) == true);
        }

        static internal Uri GetResolvedUri(Uri baseUri, Uri orgUri)
        {
            Uri newUri;
            
            if (orgUri == null)
            {
                newUri = null;
            }
            else if (orgUri.IsAbsoluteUri == false)
            {
                // if the orgUri is an absolute Uri, don't need to resolve it again.
                
                Uri baseuri = (baseUri == null) ? BindUriHelper.BaseUri : baseUri;

#if CF_Envelope_Activation_Enabled
                bool isContainer = false ;

                //
                // if the BaseUri starts with pack://application we know that we're not in a container. 
                //
                // By deferring the registration of the container scheme - we avoid registering the ssres protocol. 
                // and enable less code that requires elevation. 
                // 
                // Note that when container moves to pack: ( PS 25616) - we won't need this check anyway. 
                // 

                if (  // Check that the baseuri starts with pack://application:,,,/
                       ! DoSchemeAndHostMatch(baseuri, BaseUriHelper.PackAppBaseUri))
                {
                    isContainer = String.Compare(baseuri.Scheme, CompoundFileUri.UriSchemeContainer, StringComparison.OrdinalIgnoreCase) == 0;
                }           

                Debug.Assert(baseuri.OriginalString == BaseUriHelper.FixFileUri(baseuri).OriginalString, "Base Uri is legacy file Uri and may not resolve relative uris correctly. This method should be updated");

                // ToDo (younggk): PS# 25616 Once we move to PackUri, we don't need a special way
                //  of resolving Uri. We can use the regurlar one.
                if (isContainer)
                {
                    newUri = ResolveContainerUri(baseuri, orgUri);
                }
                else
                {
#endif
                    newUri = new Uri(baseuri, orgUri);
#if CF_Envelope_Activation_Enabled
                }
#endif
            }
            else
            {
                newUri = BaseUriHelper.FixFileUri(orgUri);
            }

            return newUri;
        }        

        /// <summary>
        /// Gets the referer to set as a header on the HTTP request.
        /// We do not set the referer if we are navigating to a 
        /// differnet security zone or to a different Uri scheme.
        /// </summary>
        internal static string GetReferer(Uri destinationUri)
        {
            string referer = null;

            Uri sourceUri = MS.Internal.AppModel.SiteOfOriginContainer.BrowserSource;
            if (sourceUri != null)
            {
                SecurityZone sourceZone = MS.Internal.AppModel.CustomCredentialPolicy.MapUrlToZone(sourceUri);
                SecurityZone targetZone = MS.Internal.AppModel.CustomCredentialPolicy.MapUrlToZone(destinationUri);

                // We don't send any referer when crossing zone
                if (sourceZone == targetZone)
                {
                    // We don't send any referer when going cross-scheme
                    if (SecurityHelper.AreStringTypesEqual(sourceUri.Scheme, destinationUri.Scheme))
                    {
                        // HTTPHeader requires the referer uri to be escaped. 
                        referer = sourceUri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
                    }
                }
            }

            return referer;
        }       


#endif // PRESENTATION_CORE || PRESENTATIONFRAMEWORK
    }
}
