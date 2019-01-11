using System;
using System.IO;
using System.Net;
#if XAMCORE_2_0
using CoreFoundation;
using Foundation;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#else
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
#endif

#if SYSTEM_NET_HTTP && !MONOMAC
namespace System.Net.Http {

	public partial class HttpClient {

		public HttpClient ()
			: this (GetDefaultHandler (), true)
		{
		}

		// note: the linker will re-write this to only reference the selected handler
		// but we want this to work "as expected" even if the application is not being linked
		static HttpMessageHandler GetDefaultHandler ()
		{
#if MONOTOUCH_WATCH
			// There's only one valid handler type for watchOS
			return new NSUrlSessionHandler ();
#else
			return RuntimeOptions.GetHttpMessageHandler ();
#endif
		}
	}
#else
// due to historical reasons (around CFNetwork) Xamarin.Mac includes this inside it's profile assembly
// and not a custom System.Net.Http assembly
namespace Foundation {
#endif

	partial class NSUrlSessionHandler {

		bool allowAutoRedirect;
		ICredentials credentials;
		bool sentRequest;

		public bool AllowAutoRedirect {
			get {
				return allowAutoRedirect;
			}
			set {
				EnsureModifiability ();
				allowAutoRedirect = value;
			}
		}

		public ICredentials Credentials {
			get {
				return credentials;
			}
			set {
				EnsureModifiability ();
				credentials = value;
			}
		}

		internal void EnsureModifiability ()
		{
			if (sentRequest)
				throw new InvalidOperationException (
					"This instance has already started one or more requests. " +
					"Properties can only be modified before sending the first request.");
		}

		// almost identical to ModernHttpClient version but it uses the constants from monotouch.dll | Xamarin.[iOS|WatchOS|TVOS].dll
		static Exception createExceptionForNSError(NSError error)
		{
			// var webExceptionStatus = WebExceptionStatus.UnknownError;

			var innerException = new NSErrorException(error);

			// errors that exists in both share the same error code, so we can use a single switch/case
			// this also ease watchOS integration as if does not expose CFNetwork but (I would not be 
			// surprised if it)could return some of it's error codes
#if MONOTOUCH_WATCH
			if (error.Domain == NSError.NSUrlErrorDomain) {
#else
			if ((error.Domain == NSError.NSUrlErrorDomain) || (error.Domain == NSError.CFNetworkErrorDomain)) {
#endif
				// Parse the enum into a web exception status or exception. Some
				// of these values don't necessarily translate completely to
				// what WebExceptionStatus supports, so made some best guesses
				// here.  For your reading pleasure, compare these:
				//
				// Apple docs: https://developer.apple.com/library/mac/documentation/Cocoa/Reference/Foundation/Miscellaneous/Foundation_Constants/index.html#//apple_ref/doc/constant_group/URL_Loading_System_Error_Codes
				// .NET docs: http://msdn.microsoft.com/en-us/library/system.net.webexceptionstatus(v=vs.110).aspx
				switch ((NSUrlError) (long) error.Code) {
				case NSUrlError.Cancelled:
				case NSUrlError.UserCancelledAuthentication:
#if !MONOTOUCH_WATCH
				case (NSUrlError) NSNetServicesStatus.CancelledError:
#endif
					// No more processing is required so just return.
					return new OperationCanceledException(error.LocalizedDescription, innerException);
// 				case NSUrlError.BadURL:
// 				case NSUrlError.UnsupportedURL:
// 				case NSUrlError.CannotConnectToHost:
// 				case NSUrlError.ResourceUnavailable:
// 				case NSUrlError.NotConnectedToInternet:
// 				case NSUrlError.UserAuthenticationRequired:
// 				case NSUrlError.InternationalRoamingOff:
// 				case NSUrlError.CallIsActive:
// 				case NSUrlError.DataNotAllowed:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.Socks5BadCredentials:
// 				case (NSUrlError) CFNetworkErrors.Socks5UnsupportedNegotiationMethod:
// 				case (NSUrlError) CFNetworkErrors.Socks5NoAcceptableMethod:
// 				case (NSUrlError) CFNetworkErrors.HttpAuthenticationTypeUnsupported:
// 				case (NSUrlError) CFNetworkErrors.HttpBadCredentials:
// 				case (NSUrlError) CFNetworkErrors.HttpBadURL:
// #endif
// 					webExceptionStatus = WebExceptionStatus.ConnectFailure;
// 					break;
// 				case NSUrlError.TimedOut:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.NetServiceTimeout:
// #endif
// 					webExceptionStatus = WebExceptionStatus.Timeout;
// 					break;
// 				case NSUrlError.CannotFindHost:
// 				case NSUrlError.DNSLookupFailed:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.HostNotFound:
// 				case (NSUrlError) CFNetworkErrors.NetServiceDnsServiceFailure:
// #endif
// 					webExceptionStatus = WebExceptionStatus.NameResolutionFailure;
// 					break;
// 				case NSUrlError.DataLengthExceedsMaximum:
// 					webExceptionStatus = WebExceptionStatus.MessageLengthLimitExceeded;
// 					break;
// 				case NSUrlError.NetworkConnectionLost:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.HttpConnectionLost:
// #endif
// 					webExceptionStatus = WebExceptionStatus.ConnectionClosed;
// 					break;
// 				case NSUrlError.HTTPTooManyRedirects:
// 				case NSUrlError.RedirectToNonExistentLocation:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.HttpRedirectionLoopDetected:
// #endif
// 					webExceptionStatus = WebExceptionStatus.ProtocolError;
// 					break;
// 				case NSUrlError.RequestBodyStreamExhausted:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.SocksUnknownClientVersion:
// 				case (NSUrlError) CFNetworkErrors.SocksUnsupportedServerVersion:
// 				case (NSUrlError) CFNetworkErrors.HttpParseFailure:
// #endif
// 					webExceptionStatus = WebExceptionStatus.SendFailure;
// 					break;
// 				case NSUrlError.BadServerResponse:
// 				case NSUrlError.ZeroByteResource:
// 				case NSUrlError.CannotDecodeRawData:
// 				case NSUrlError.CannotDecodeContentData:
// 				case NSUrlError.CannotParseResponse:
// 				case NSUrlError.FileDoesNotExist:
// 				case NSUrlError.FileIsDirectory:
// 				case NSUrlError.NoPermissionsToReadFile:
// 				case NSUrlError.CannotLoadFromNetwork:
// 				case NSUrlError.CannotCreateFile:
// 				case NSUrlError.CannotOpenFile:
// 				case NSUrlError.CannotCloseFile:
// 				case NSUrlError.CannotWriteToFile:
// 				case NSUrlError.CannotRemoveFile:
// 				case NSUrlError.CannotMoveFile:
// 				case NSUrlError.DownloadDecodingFailedMidStream:
// 				case NSUrlError.DownloadDecodingFailedToComplete:
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.Socks4RequestFailed:
// 				case (NSUrlError) CFNetworkErrors.Socks4IdentdFailed:
// 				case (NSUrlError) CFNetworkErrors.Socks4IdConflict:
// 				case (NSUrlError) CFNetworkErrors.Socks4UnknownStatusCode:
// 				case (NSUrlError) CFNetworkErrors.Socks5BadState:
// 				case (NSUrlError) CFNetworkErrors.Socks5BadResponseAddr:
// 				case (NSUrlError) CFNetworkErrors.CannotParseCookieFile:
// 				case (NSUrlError) CFNetworkErrors.NetServiceUnknown:
// 				case (NSUrlError) CFNetworkErrors.NetServiceCollision:
// 				case (NSUrlError) CFNetworkErrors.NetServiceNotFound:
// 				case (NSUrlError) CFNetworkErrors.NetServiceInProgress:
// 				case (NSUrlError) CFNetworkErrors.NetServiceBadArgument:
// 				case (NSUrlError) CFNetworkErrors.NetServiceInvalid:
// #endif
// 					webExceptionStatus = WebExceptionStatus.ReceiveFailure;
// 					break;
// 				case NSUrlError.SecureConnectionFailed:
// 					webExceptionStatus = WebExceptionStatus.SecureChannelFailure;
// 					break;
// 				case NSUrlError.ServerCertificateHasBadDate:
// 				case NSUrlError.ServerCertificateHasUnknownRoot:
// 				case NSUrlError.ServerCertificateNotYetValid:
// 				case NSUrlError.ServerCertificateUntrusted:
// 				case NSUrlError.ClientCertificateRejected:
// 				case NSUrlError.ClientCertificateRequired:
// 					webExceptionStatus = WebExceptionStatus.TrustFailure;
// 					break;
// #if !MONOTOUCH_WATCH
// 				case (NSUrlError) CFNetworkErrors.HttpProxyConnectionFailure:
// 				case (NSUrlError) CFNetworkErrors.HttpBadProxyCredentials:
// 				case (NSUrlError) CFNetworkErrors.PacFileError:
// 				case (NSUrlError) CFNetworkErrors.PacFileAuth:
// 				case (NSUrlError) CFNetworkErrors.HttpsProxyConnectionFailure:
// 				case (NSUrlError) CFNetworkErrors.HttpsProxyFailureUnexpectedResponseToConnectMethod:
// 					webExceptionStatus = WebExceptionStatus.RequestProhibitedByProxy;
// 					break;
// #endif
				}
			} 

			// Always create a WebException so that it can be handled by the client.
			return new WebException(error.LocalizedDescription, innerException); //, webExceptionStatus, response: null);
		}
	}
}
