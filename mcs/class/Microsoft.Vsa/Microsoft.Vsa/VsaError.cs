//
// VsaError.cs:
//
// Author: 
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

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

using System;

namespace Microsoft.Vsa {

	[Serializable]
	public enum VsaError : int {

		AppDomainCannotBeSet        = -2146226176,
		AppDomainInvalid            = -2146226175,
		ApplicationBaseCannotBeSet  = -2146226174,
		ApplicationBaseInvalid      = -2146226173,
		AssemblyExpected            = -2146226172,
		AssemblyNameInvalid         = -2146226171,
		BadAssembly                 = -2146226170,
		BrowserNotExist             = -2146226115,
		CachedAssemblyInvalid       = -2146226169,
		CallbackUnexpected          = -2146226168,
		CannotAttachToWebServer     = -2146226100,
		CodeDOMNotAvailable         = -2146226167,
		CompiledStateNotFound       = -2146226166,
		DebuggeeNotStarted          = -2146226114,
		DebugInfoNotSupported       = -2146226165,
		ElementNameInvalid          = -2146226164,
		ElementNotFound             = -2146226163,
		EngineBusy                  = -2146226162,
		EngineCannotClose           = -2146226161,
		EngineCannotReset           = -2146226160,
		EngineClosed                = -2146226159,
		EngineEmpty                 = -2146226159,
		EngineInitialized           = -2146226157,
		EngineNameInUse             = -2146226156,
		EngineNameInvalid           = -2146226113,
		EngineNameNotSet            = -2146226099,
		EngineNotCompiled           = -2146226155,
		EngineNotExist              = -2146226112,
		EngineNotInitialized        = -2146226154,
		EngineNotRunning            = -2146226153,
		EngineRunning               = -2146226152,
		EventSourceInvalid          = -2146226151,
		EventSourceNameInUse        = -2146226150,
		EventSourceNameInvalid      = -2146226149,
		EventSourceNotFound         = -2146226148,
		EventSourceTypeInvalid      = -2146226147,
		FileFormatUnsupported       = -2146226111,
		FileTypeUnknown             = -2146226110,
		GetCompiledStateFailed      = -2146226146,
		GlobalInstanceInvalid       = -2146226145,
		GlobalInstanceTypeInvalid   = -2146226144,
		InternalCompilerError       = -2146226143,
		ItemCannotBeRemoved         = -2146226142,
		ItemCannotBeRenamed         = -2146226109,
		ItemFlagNotSupported        = -2146226141,
		ItemNameInUse               = -2146226140,
		ItemNameInvalid             = -2146226139,
		ItemNotFound                = -2146226138,
		ItemTypeNotSupported        = -2146226137,
		LCIDNotSupported            = -2146226136,
		LoadElementFailed           = -2146226135,
		MissingPdb                  = -2146226102,
		MissingSource               = -2146226108,
		NameTooLong                 = -2146226106,
		NotClientSideAndNoUrl       = -2146226101,
		NotificationInvalid         = -2146226134,
		NotInitCompleted            = -2146226107,
		OptionInvalid               = -2146226133,
		OptionNotSupported          = -2146226132,
		ProcNameInUse               = -2146226105,
		ProcNameInvalid             = -2146226104,
		RevokeFailed                = -2146226131,
		RootMonikerAlreadySet       = -2146226130,
		RootMonikerInUse            = -2146226129,
		RootMonikerInvalid          = -2146226128,
		RootMonikerNotSet           = -2146226127,
		RootMonikerProtocolInvalid  = -2146226126,
		RootNamespaceInvalid        = -2146226125,
		RootNamespaceNotSet         = -2146226124,
		SaveCompiledStateFailed     = -2146226123,
		SaveElementFailed           = -2146226122,
		SiteAlreadySet              = -2146226121,
		SiteInvalid                 = -2146226120,
		SiteNotSet                  = -2146226119,
		SourceItemNotAvailable      = -2146226118,
		SourceMonikerNotAvailable   = -2146226117,
		UnknownError                = -2146225921,
		URLInvalid                  = -2146226116,
		VsaServerDown               = -2146226103,
	}
}