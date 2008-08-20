//
// DataContractSerializerTest_FrameworkTypes_System.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft.co http://www.mainsoft.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
// This test code contains tests for attributes in System.Runtime.Serialization
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	[Category ("NotWorking")]
	public partial class DataContractSerializerTest_FrameworkTypes_System
		: DataContractSerializerTest_FrameworkTypes
	{
		[Test]
		public void System_Text_RegularExpressions_RegexOptions () {
			Test<global::System.Text.RegularExpressions.RegexOptions> ();
		}
		[Test]
		public void System_ComponentModel_BindableSupport () {
			Test<global::System.ComponentModel.BindableSupport> ();
		}
		[Test]
		public void System_ComponentModel_BindingDirection () {
			Test<global::System.ComponentModel.BindingDirection> ();
		}
		[Test]
		public void System_ComponentModel_DataObjectMethodType () {
			Test<global::System.ComponentModel.DataObjectMethodType> ();
		}
		[Test]
		public void System_ComponentModel_DesignerSerializationVisibility () {
			Test<global::System.ComponentModel.DesignerSerializationVisibility> ();
		}
		[Test]
		public void System_ComponentModel_EditorBrowsableState () {
			Test<global::System.ComponentModel.EditorBrowsableState> ();
		}
		[Test]
		public void System_ComponentModel_InvalidAsynchronousStateException () {
			Test<global::System.ComponentModel.InvalidAsynchronousStateException> ();
		}
		[Test]
		public void System_ComponentModel_InvalidEnumArgumentException () {
			Test<global::System.ComponentModel.InvalidEnumArgumentException> ();
		}
		[Test]
		public void System_ComponentModel_LicenseUsageMode () {
			Test<global::System.ComponentModel.LicenseUsageMode> ();
		}
		[Test]
		public void System_ComponentModel_ListChangedType () {
			Test<global::System.ComponentModel.ListChangedType> ();
		}
		[Test]
		public void System_ComponentModel_ListSortDirection () {
			Test<global::System.ComponentModel.ListSortDirection> ();
		}
		[Test]
		public void System_ComponentModel_MaskedTextResultHint () {
			Test<global::System.ComponentModel.MaskedTextResultHint> ();
		}
		[Test]
		public void System_ComponentModel_ToolboxItemFilterType () {
			Test<global::System.ComponentModel.ToolboxItemFilterType> ();
		}
		[Test]
		public void System_ComponentModel_WarningException () {
			Test<global::System.ComponentModel.WarningException> ();
		}
		[Test]
		public void System_ComponentModel_Win32Exception () {
			Test<global::System.ComponentModel.Win32Exception> ();
		}
		[Test]
		public void System_ComponentModel_Design_CheckoutException () {
			Test<global::System.ComponentModel.Design.CheckoutException> ();
		}
		[Test]
		public void System_ComponentModel_Design_HelpContextType () {
			Test<global::System.ComponentModel.Design.HelpContextType> ();
		}
		[Test]
		public void System_ComponentModel_Design_HelpKeywordAttribute () {
			Test<global::System.ComponentModel.Design.HelpKeywordAttribute> ();
		}
		[Test]
		public void System_ComponentModel_Design_HelpKeywordType () {
			Test<global::System.ComponentModel.Design.HelpKeywordType> ();
		}
		[Test]
		public void System_ComponentModel_PropertyTabScope () {
			Test<global::System.ComponentModel.PropertyTabScope> ();
		}
		[Test]
		public void System_ComponentModel_RefreshProperties () {
			Test<global::System.ComponentModel.RefreshProperties> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_ComponentModel_Design_SelectionTypes () {
			Test<global::System.ComponentModel.Design.SelectionTypes> ();
		}
		[Test]
		public void System_ComponentModel_Design_ViewTechnology () {
			Test<global::System.ComponentModel.Design.ViewTechnology> ();
		}
		[Test]
		public void System_Diagnostics_SourceLevels () {
			Test<global::System.Diagnostics.SourceLevels> ();
		}
		[Test]
		public void System_Diagnostics_TraceLevel () {
			Test<global::System.Diagnostics.TraceLevel> ();
		}
		[Test]
		public void System_Diagnostics_TraceOptions () {
			Test<global::System.Diagnostics.TraceOptions> ();
		}
		[Test]
		public void System_IO_Compression_CompressionMode () {
			Test<global::System.IO.Compression.CompressionMode> ();
		}
		[Test]
		public void System_IO_InvalidDataException () {
			Test<global::System.IO.InvalidDataException> ();
		}
		[Test]
		public void System_Threading_SemaphoreFullException () {
			Test<global::System.Threading.SemaphoreFullException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Media_SoundPlayer () {
			Test<global::System.Media.SoundPlayer> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Security_AccessControl_SemaphoreRights () {
			Test<global::System.Security.AccessControl.SemaphoreRights> ();
		}
		[Test]
		public void System_Collections_Specialized_HybridDictionary () {
			Test<global::System.Collections.Specialized.HybridDictionary> ();
		}
		[Test]
		public void System_Collections_Specialized_ListDictionary () {
			Test<global::System.Collections.Specialized.ListDictionary> ();
		}
		[Test]
		public void System_Collections_Specialized_OrderedDictionary () {
			Test<global::System.Collections.Specialized.OrderedDictionary> ();
		}
		[Test]
		public void System_Collections_Specialized_StringCollection () {
			Test<global::System.Collections.Specialized.StringCollection> ();
		}
		[Test]
		public void System_Collections_Specialized_StringDictionary () {
			Test<global::System.Collections.Specialized.StringDictionary> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_InteropServices_ComTypes_ADVF () {
			Test<global::System.Runtime.InteropServices.ComTypes.ADVF> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Runtime_InteropServices_ComTypes_DVASPECT () {
			Test<global::System.Runtime.InteropServices.ComTypes.DVASPECT> ();
		}
		[Test]
		public void System_Runtime_InteropServices_ComTypes_TYMED () {
			Test<global::System.Runtime.InteropServices.ComTypes.TYMED> ();
		}
		[Test]
		public void System_Security_Permissions_StorePermissionFlags () {
			Test<global::System.Security.Permissions.StorePermissionFlags> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X500DistinguishedNameFlags () {
			Test<global::System.Security.Cryptography.X509Certificates.X500DistinguishedNameFlags> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509NameType () {
			Test<global::System.Security.Cryptography.X509Certificates.X509NameType> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509IncludeOption () {
			Test<global::System.Security.Cryptography.X509Certificates.X509IncludeOption> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509FindType () {
			Test<global::System.Security.Cryptography.X509Certificates.X509FindType> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509CertificateCollection () {
			Test<global::System.Security.Cryptography.X509Certificates.X509CertificateCollection> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509ChainStatusFlags () {
			Test<global::System.Security.Cryptography.X509Certificates.X509ChainStatusFlags> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509RevocationMode () {
			Test<global::System.Security.Cryptography.X509Certificates.X509RevocationMode> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509RevocationFlag () {
			Test<global::System.Security.Cryptography.X509Certificates.X509RevocationFlag> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509VerificationFlags () {
			Test<global::System.Security.Cryptography.X509Certificates.X509VerificationFlags> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509KeyUsageFlags () {
			Test<global::System.Security.Cryptography.X509Certificates.X509KeyUsageFlags> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_X509SubjectKeyIdentifierHashAlgorithm () {
			Test<global::System.Security.Cryptography.X509Certificates.X509SubjectKeyIdentifierHashAlgorithm> ();
		}
		[Test]
		public void System_Security_Cryptography_X509Certificates_OpenFlags () {
			Test<global::System.Security.Cryptography.X509Certificates.OpenFlags> ();
		}
		[Test]
		public void System_UriFormatException () {
			Test<global::System.UriFormatException> ();
		}
		[Test]
		public void System_UriHostNameType () {
			Test<global::System.UriHostNameType> ();
		}
		[Test]
		public void System_UriPartial () {
			Test<global::System.UriPartial> ();
		}
		[Test]
		public void System_UriKind () {
			Test<global::System.UriKind> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_UriComponents () {
			Test<global::System.UriComponents> ();
		}
		[Test]
		public void System_UriIdnScope () {
			Test<global::System.UriIdnScope> ();
		}
		[Test]
		public void System_GenericUriParserOptions () {
			Test<global::System.GenericUriParserOptions> ();
		}
		[Test]
		public void System_Net_AuthenticationSchemes () {
			Test<global::System.Net.AuthenticationSchemes> ();
		}
		//[Test]
		//[Category ("NotWorking")]
		//public void System_Net_Cookie () {
		//    Test<global::System.Net.Cookie> ();
		//}
		[Test]
		public void System_Net_CookieCollection () {
			Test<global::System.Net.CookieCollection> ();
		}
		[Test]
		public void System_Net_CookieContainer () {
			Test<global::System.Net.CookieContainer> ();
		}
		[Test]
		public void System_Net_CookieException () {
			Test<global::System.Net.CookieException> ();
		}
		[Test]
		public void System_Net_FtpStatusCode () {
			Test<global::System.Net.FtpStatusCode> ();
		}
		[Test]
		public void System_Net_HttpListenerException () {
			Test<global::System.Net.HttpListenerException> ();
		}
		[Test]
		public void System_Net_HttpRequestHeader () {
			Test<global::System.Net.HttpRequestHeader> ();
		}
		[Test]
		public void System_Net_HttpResponseHeader () {
			Test<global::System.Net.HttpResponseHeader> ();
		}
		[Test]
		public void System_Net_DecompressionMethods () {
			Test<global::System.Net.DecompressionMethods> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Net_NetworkAccess () {
			Test<global::System.Net.NetworkAccess> ();
		}
		[Test]
		public void System_Net_ProtocolViolationException () {
			Test<global::System.Net.ProtocolViolationException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Net_SecurityProtocolType () {
			Test<global::System.Net.SecurityProtocolType> ();
		}
		[Test]
		public void System_Net_Sockets_SocketException () {
			Test<global::System.Net.Sockets.SocketException> ();
		}
		[Test]
		public void System_Net_WebException () {
			Test<global::System.Net.WebException> ();
		}
		[Test]
		public void System_Net_WebExceptionStatus () {
			Test<global::System.Net.WebExceptionStatus> ();
		}
		[Test]
		public void System_Net_WebHeaderCollection () {
			Test<global::System.Net.WebHeaderCollection> ();
		}
		[Test]
		public void System_Net_WebPermission () {
			Test<global::System.Net.WebPermission> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Net_WebProxy () {
			Test<global::System.Net.WebProxy> ();
		}
		[Test]
		public void System_Net_Cache_RequestCacheLevel () {
			Test<global::System.Net.Cache.RequestCacheLevel> ();
		}
		[Test]
		public void System_Net_Cache_HttpRequestCacheLevel () {
			Test<global::System.Net.Cache.HttpRequestCacheLevel> ();
		}
		[Test]
		public void System_Net_Cache_HttpCacheAgeControl () {
			Test<global::System.Net.Cache.HttpCacheAgeControl> ();
		}
		[Test]
		public void System_Security_Authentication_AuthenticationException () {
			Test<global::System.Security.Authentication.AuthenticationException> ();
		}
		[Test]
		public void System_Security_Authentication_InvalidCredentialException () {
			Test<global::System.Security.Authentication.InvalidCredentialException> ();
		}
		[Test]
		public void System_Net_Security_AuthenticationLevel () {
			Test<global::System.Net.Security.AuthenticationLevel> ();
		}
		[Test]
		public void System_Net_Security_ProtectionLevel () {
			Test<global::System.Net.Security.ProtectionLevel> ();
		}
		[Test]
		public void System_Security_Authentication_SslProtocols () {
			Test<global::System.Security.Authentication.SslProtocols> ();
		}
		[Test]
		public void System_Security_Authentication_ExchangeAlgorithmType () {
			Test<global::System.Security.Authentication.ExchangeAlgorithmType> ();
		}
		[Test]
		public void System_Security_Authentication_CipherAlgorithmType () {
			Test<global::System.Security.Authentication.CipherAlgorithmType> ();
		}
		[Test]
		public void System_Security_Authentication_HashAlgorithmType () {
			Test<global::System.Security.Authentication.HashAlgorithmType> ();
		}
		[Test]
		public void System_Net_Security_SslPolicyErrors () {
			Test<global::System.Net.Security.SslPolicyErrors> ();
		}
		[Test]
		public void System_Net_Sockets_AddressFamily () {
			Test<global::System.Net.Sockets.AddressFamily> ();
		}
		[Test]
		public void System_Net_Sockets_ProtocolFamily () {
			Test<global::System.Net.Sockets.ProtocolFamily> ();
		}
		[Test]
		public void System_Net_Sockets_ProtocolType () {
			Test<global::System.Net.Sockets.ProtocolType> ();
		}
		[Test]
		public void System_Net_Sockets_SelectMode () {
			Test<global::System.Net.Sockets.SelectMode> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Net_Sockets_SocketInformationOptions () {
			Test<global::System.Net.Sockets.SocketInformationOptions> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Net_Sockets_SocketInformation () {
			Test<global::System.Net.Sockets.SocketInformation> ();
		}
		//[Test]
		//public void System_Net_Sockets_SocketAsyncOperation () {
		//    Test<global::System.Net.Sockets.SocketAsyncOperation> ();
		//}
		[Test]
		public void System_Net_Sockets_SocketError () {
			Test<global::System.Net.Sockets.SocketError> ();
		}
		[Test]
		public void System_Net_Sockets_SocketFlags () {
			Test<global::System.Net.Sockets.SocketFlags> ();
		}
		[Test]
		public void System_Net_Sockets_SocketOptionLevel () {
			Test<global::System.Net.Sockets.SocketOptionLevel> ();
		}
		[Test]
		public void System_Net_Sockets_SocketShutdown () {
			Test<global::System.Net.Sockets.SocketShutdown> ();
		}
		[Test]
		public void System_Net_Sockets_TransmitFileOptions () {
			Test<global::System.Net.Sockets.TransmitFileOptions> ();
		}
		[Test]
		public void System_Net_NetworkInformation_DuplicateAddressDetectionState () {
			Test<global::System.Net.NetworkInformation.DuplicateAddressDetectionState> ();
		}
		[Test]
		public void System_Net_NetworkInformation_IPStatus () {
			Test<global::System.Net.NetworkInformation.IPStatus> ();
		}
		[Test]
		public void System_Net_NetworkInformation_NetworkInformationException () {
			Test<global::System.Net.NetworkInformation.NetworkInformationException> ();
		}
		[Test]
		public void System_Net_NetworkInformation_NetworkInformationAccess () {
			Test<global::System.Net.NetworkInformation.NetworkInformationAccess> ();
		}
		[Test]
		public void System_Net_NetworkInformation_NetworkInterfaceComponent () {
			Test<global::System.Net.NetworkInformation.NetworkInterfaceComponent> ();
		}
		[Test]
		public void System_Net_NetworkInformation_NetBiosNodeType () {
			Test<global::System.Net.NetworkInformation.NetBiosNodeType> ();
		}
		[Test]
		public void System_Net_NetworkInformation_PrefixOrigin () {
			Test<global::System.Net.NetworkInformation.PrefixOrigin> ();
		}
		[Test]
		public void System_Net_NetworkInformation_SuffixOrigin () {
			Test<global::System.Net.NetworkInformation.SuffixOrigin> ();
		}
		[Test]
		public void System_Net_NetworkInformation_TcpState () {
			Test<global::System.Net.NetworkInformation.TcpState> ();
		}
		[Test]
		public void System_Net_Configuration_ProxyElement_BypassOnLocalValues () {
			Test<global::System.Net.Configuration.ProxyElement.BypassOnLocalValues> ();
		}
		[Test]
		public void System_Net_Configuration_ProxyElement_UseSystemDefaultValues () {
			Test<global::System.Net.Configuration.ProxyElement.UseSystemDefaultValues> ();
		}
		[Test]
		public void System_Net_Configuration_ProxyElement_AutoDetectValues () {
			Test<global::System.Net.Configuration.ProxyElement.AutoDetectValues> ();
		}
		[Test]
		public void System_Net_Mail_DeliveryNotificationOptions () {
			Test<global::System.Net.Mail.DeliveryNotificationOptions> ();
		}
		[Test]
		public void System_Net_Mail_MailPriority () {
			Test<global::System.Net.Mail.MailPriority> ();
		}
		[Test]
		public void System_Net_Mail_SmtpDeliveryMethod () {
			Test<global::System.Net.Mail.SmtpDeliveryMethod> ();
		}
		[Test]
		public void System_Net_Mail_SmtpException () {
			Test<global::System.Net.Mail.SmtpException> ();
		}
		[Test]
		public void System_Net_Mail_SmtpFailedRecipientException () {
			Test<global::System.Net.Mail.SmtpFailedRecipientException> ();
		}
		[Test]
		public void System_Net_Mail_SmtpAccess () {
			Test<global::System.Net.Mail.SmtpAccess> ();
		}
		[Test]
		public void System_Net_Mime_TransferEncoding () {
			Test<global::System.Net.Mime.TransferEncoding> ();
		}
		[Test]
		public void System_Configuration_ConfigurationException () {
			Test<global::System.Configuration.ConfigurationException> ();
		}
		[Test]
		public void System_Configuration_SettingsAttributeDictionary () {
			Test<global::System.Configuration.SettingsAttributeDictionary> ();
		}
		[Test]
		public void System_Configuration_SettingsManageability () {
			Test<global::System.Configuration.SettingsManageability> ();
		}
		[Test]
		public void System_Configuration_SpecialSetting () {
			Test<global::System.Configuration.SpecialSetting> ();
		}
		[Test]
		public void System_Configuration_SettingsContext () {
			Test<global::System.Configuration.SettingsContext> ();
		}
		[Test]
		public void System_Configuration_SettingsPropertyIsReadOnlyException () {
			Test<global::System.Configuration.SettingsPropertyIsReadOnlyException> ();
		}
		[Test]
		public void System_Configuration_SettingsPropertyNotFoundException () {
			Test<global::System.Configuration.SettingsPropertyNotFoundException> ();
		}
		[Test]
		public void System_Configuration_SettingsPropertyWrongTypeException () {
			Test<global::System.Configuration.SettingsPropertyWrongTypeException> ();
		}
		[Test]
		public void System_Configuration_SettingsSerializeAs () {
			Test<global::System.Configuration.SettingsSerializeAs> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_IO_NotifyFilters () {
			Test<global::System.IO.NotifyFilters> ();
		}
		[Test]
		public void System_IO_InternalBufferOverflowException () {
			Test<global::System.IO.InternalBufferOverflowException> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_IO_WatcherChangeTypes () {
			Test<global::System.IO.WatcherChangeTypes> ();
		}
		[Test]
		public void System_Security_Permissions_ResourcePermissionBaseEntry () {
			Test<global::System.Security.Permissions.ResourcePermissionBaseEntry> ();
		}
		[Test]
		public void System_Diagnostics_CounterCreationData () {
			Test<global::System.Diagnostics.CounterCreationData> ();
		}
		[Test]
		public void System_Diagnostics_CounterCreationDataCollection () {
			Test<global::System.Diagnostics.CounterCreationDataCollection> ();
		}
		[Test]
		public void System_Diagnostics_EventLogPermissionAccess () {
			Test<global::System.Diagnostics.EventLogPermissionAccess> ();
		}
		[Test]
		public void System_Diagnostics_OverflowAction () {
			Test<global::System.Diagnostics.OverflowAction> ();
		}
		[Test]
		public void System_Diagnostics_PerformanceCounterCategoryType () {
			Test<global::System.Diagnostics.PerformanceCounterCategoryType> ();
		}
		[Test]
		public void System_Diagnostics_PerformanceCounterInstanceLifetime () {
			Test<global::System.Diagnostics.PerformanceCounterInstanceLifetime> ();
		}
		[Test]
		public void System_Diagnostics_PerformanceCounterPermissionAccess () {
			Test<global::System.Diagnostics.PerformanceCounterPermissionAccess> ();
		}
		[Test]
		public void System_Diagnostics_PerformanceCounterType () {
			Test<global::System.Diagnostics.PerformanceCounterType> ();
		}
		[Test]
		public void System_Diagnostics_ProcessWindowStyle () {
			Test<global::System.Diagnostics.ProcessWindowStyle> ();
		}
		[Test]
		public void System_Diagnostics_ThreadPriorityLevel () {
			Test<global::System.Diagnostics.ThreadPriorityLevel> ();
		}
		[Test]
		public void System_Diagnostics_ThreadState () {
			Test<global::System.Diagnostics.ThreadState> ();
		}
		[Test]
		public void System_Diagnostics_ThreadWaitReason () {
			Test<global::System.Diagnostics.ThreadWaitReason> ();
		}
		[Test]
		public void System_IO_Ports_Handshake () {
			Test<global::System.IO.Ports.Handshake> ();
		}
		[Test]
		public void System_IO_Ports_Parity () {
			Test<global::System.IO.Ports.Parity> ();
		}
		[Test]
		public void System_IO_Ports_StopBits () {
			Test<global::System.IO.Ports.StopBits> ();
		}
	}
}
