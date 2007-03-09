//
// System.Web.Management.WebEventFormatter.cs
//
// Authors:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0
namespace System.Web.Management
{
	public sealed class WebEventCodes
	{
		public const int InvalidEventCode = -1;
		
		public const int UndefinedEventCode = 0;
		public const int UndefinedEventDetailCode = 0;
		
		public const int ApplicationCodeBase = 0x003E8;
		public const int ApplicationStart = ApplicationCodeBase + 0x01;
		public const int ApplicationShutdown = ApplicationCodeBase + 0x02;
		public const int ApplicationCompilationStart = ApplicationCodeBase + 0x03;
		public const int ApplicationCompilationEnd = ApplicationCodeBase + 0x04;
		public const int ApplicationHeartbeat = ApplicationCodeBase + 0x05;
		
		public const int RequestCodeBase = 0x007D0;
		public const int RequestTransactionComplete = RequestCodeBase + 0x01;
		public const int RequestTransactionAbort = RequestCodeBase + 0x02;
		
		public const int ErrorCodeBase = 0x00BB8;
		public const int RuntimeErrorRequestAbort = ErrorCodeBase + 0x01;
		public const int RuntimeErrorViewStateFailure = ErrorCodeBase + 0x02;
		public const int RuntimeErrorValidationFailure = ErrorCodeBase + 0x03;
		public const int RuntimeErrorPostTooLarge = ErrorCodeBase + 0x04;
		public const int RuntimeErrorUnhandledException = ErrorCodeBase + 0x05;
		public const int WebErrorParserError = ErrorCodeBase + 0x06;
		public const int WebErrorCompilationError = ErrorCodeBase + 0x07;
		public const int WebErrorConfigurationError = ErrorCodeBase + 0x08;
		public const int WebErrorOtherError = ErrorCodeBase + 0x09;
		public const int WebErrorPropertyDeserializationError = ErrorCodeBase + 0x0A;
		public const int WebErrorObjectStateFormatterDeserializationError = ErrorCodeBase + 0x0B;
		
		public const int AuditCodeBase = 0x00FA0;
		public const int AuditFormsAuthenticationSuccess = AuditCodeBase + 0x01;
		public const int AuditMembershipAuthenticationSuccess = AuditCodeBase + 0x02;
		public const int AuditUrlAuthorizationSuccess = AuditCodeBase + 0x03;
		public const int AuditFileAuthorizationSuccess = AuditCodeBase + 0x04;
		public const int AuditFormsAuthenticationFailure = AuditCodeBase + 0x05;
		public const int AuditMembershipAuthenticationFailure = AuditCodeBase + 0x06;
		public const int AuditUrlAuthorizationFailure = AuditCodeBase + 0x07;
		public const int AuditFileAuthorizationFailure = AuditCodeBase + 0x08;
		public const int AuditInvalidViewStateFailure = AuditCodeBase + 0x09;
		public const int AuditUnhandledSecurityException = AuditCodeBase + 0x0A;
		public const int AuditUnhandledAccessException = AuditCodeBase + 0x0B;
		
		public const int MiscCodeBase = 0x01770;
		public const int WebEventProviderInformation = MiscCodeBase + 0x01;
		
		public const int ApplicationDetailCodeBase = 0x0C350;
		public const int ApplicationShutdownUnknown = ApplicationDetailCodeBase + 0x01;
		public const int ApplicationShutdownHostingEnvironment = ApplicationDetailCodeBase + 0x02;
		public const int ApplicationShutdownChangeInGlobalAsax = ApplicationDetailCodeBase + 0x03;
		public const int ApplicationShutdownConfigurationChange = ApplicationDetailCodeBase + 0x04;
		public const int ApplicationShutdownUnloadAppDomainCalled = ApplicationDetailCodeBase + 0x05;
		public const int ApplicationShutdownChangeInSecurityPolicyFile = ApplicationDetailCodeBase + 0x06;
		public const int ApplicationShutdownBinDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 0x07;
		public const int ApplicationShutdownBrowsersDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 0x08;
		public const int ApplicationShutdownCodeDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 0x09;
		public const int ApplicationShutdownResourcesDirChangeOrDirectoryRename = ApplicationDetailCodeBase + 0x0A;
		public const int ApplicationShutdownIdleTimeout = ApplicationDetailCodeBase + 0x0B;
		public const int ApplicationShutdownPhysicalApplicationPathChanged = ApplicationDetailCodeBase + 0x0C;
		public const int ApplicationShutdownHttpRuntimeClose = ApplicationDetailCodeBase + 0x0D;
		public const int ApplicationShutdownInitializationError = ApplicationDetailCodeBase + 0x0E;
		public const int ApplicationShutdownMaxRecompilationsReached = ApplicationDetailCodeBase + 0x0F;
		public const int StateServerConnectionError = ApplicationDetailCodeBase + 0x10;
		
		public const int AuditDetailCodeBase = 0x0C418;
		public const int InvalidTicketFailure = AuditDetailCodeBase + 0x01;
		public const int ExpiredTicketFailure = AuditDetailCodeBase + 0x02;
		public const int InvalidViewStateMac = AuditDetailCodeBase + 0x03;
		public const int InvalidViewState = AuditDetailCodeBase + 0x04;
		
		public const int WebEventDetailCodeBase = 0x0C47C;
		public const int SqlProviderEventsDropped = WebEventDetailCodeBase + 0x01;
		
		public const int WebExtendedBase = 0x186A0;
	}
}
#endif