//
// HttpBrowserCapabilitiesBase.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;

namespace System.Web
{
#if NET_4_0
        [TypeForwardedFrom ("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
#endif
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpBrowserCapabilitiesBase : IFilterResolutionService
	{
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual bool ActiveXControls { get { NotImplemented (); return false; } }

		public virtual IDictionary Adapters { get { NotImplemented (); return null; } }

		public virtual bool AOL { get { NotImplemented (); return false; } }

		public virtual bool BackgroundSounds { get { NotImplemented (); return false; } }

		public virtual bool Beta { get { NotImplemented (); return false; } }

		public virtual string Browser { get { NotImplemented (); return null; } }

		public virtual ArrayList Browsers { get { NotImplemented (); return null; } }

		public virtual bool CanCombineFormsInDeck { get { NotImplemented (); return false; } }

		public virtual bool CanInitiateVoiceCall { get { NotImplemented (); return false; } }

		public virtual bool CanRenderAfterInputOrSelectElement { get { NotImplemented (); return false; } }

		public virtual bool CanRenderEmptySelects { get { NotImplemented (); return false; } }

		public virtual bool CanRenderInputAndSelectElementsTogether { get { NotImplemented (); return false; } }

		public virtual bool CanRenderMixedSelects { get { NotImplemented (); return false; } }

		public virtual bool CanRenderOneventAndPrevElementsTogether { get { NotImplemented (); return false; } }

		public virtual bool CanRenderPostBackCards { get { NotImplemented (); return false; } }

		public virtual bool CanRenderSetvarZeroWithMultiSelectionList { get { NotImplemented (); return false; } }

		public virtual bool CanSendMail { get { NotImplemented (); return false; } }

		public virtual IDictionary Capabilities { get; set; }

		public virtual bool CDF { get { NotImplemented (); return false; } }

		public virtual Version ClrVersion { get { NotImplemented (); return null; } }

		public virtual bool Cookies { get { NotImplemented (); return false; } }

		public virtual bool Crawler { get { NotImplemented (); return false; } }

		public virtual int DefaultSubmitButtonLimit { get { NotImplemented (); return 0; } }

		public virtual Version EcmaScriptVersion { get { NotImplemented (); return null; } }

		public virtual bool Frames { get { NotImplemented (); return false; } }

		public virtual int GatewayMajorVersion { get { NotImplemented (); return 0; } }

		public virtual double GatewayMinorVersion { get { NotImplemented (); return 0; } }

		public virtual string GatewayVersion { get { NotImplemented (); return null; } }

		public virtual bool HasBackButton { get { NotImplemented (); return false; } }

		public virtual bool HidesRightAlignedMultiselectScrollbars { get { NotImplemented (); return false; } }

		public virtual string HtmlTextWriter { get; set; }

		public virtual string Id { get { NotImplemented (); return null; } }

		public virtual string InputType { get { NotImplemented (); return null; } }

		public virtual bool IsColor { get { NotImplemented (); return false; } }

		public virtual bool IsMobileDevice { get { NotImplemented (); return false; } }

		public virtual string this [string key] {
			get { throw new NotImplementedException (); }
		}

		public virtual bool JavaApplets { get { NotImplemented (); return false; } }

		public virtual Version JScriptVersion { get { NotImplemented (); return null; } }

		public virtual int MajorVersion { get { NotImplemented (); return 0; } }

		public virtual int MaximumHrefLength { get { NotImplemented (); return 0; } }

		public virtual int MaximumRenderedPageSize { get { NotImplemented (); return 0; } }

		public virtual int MaximumSoftkeyLabelLength { get { NotImplemented (); return 0; } }

		public virtual double MinorVersion { get { NotImplemented (); return 0; } }

		public virtual string MinorVersionString { get { NotImplemented (); return null; } }

		public virtual string MobileDeviceManufacturer { get { NotImplemented (); return null; } }

		public virtual string MobileDeviceModel { get { NotImplemented (); return null; } }

		public virtual Version MSDomVersion { get { NotImplemented (); return null; } }

		public virtual int NumberOfSoftkeys { get { NotImplemented (); return 0; } }

		public virtual string Platform { get { NotImplemented (); return null; } }

		public virtual string PreferredImageMime { get { NotImplemented (); return null; } }

		public virtual string PreferredRenderingMime { get { NotImplemented (); return null; } }

		public virtual string PreferredRenderingType { get { NotImplemented (); return null; } }

		public virtual string PreferredRequestEncoding { get { NotImplemented (); return null; } }

		public virtual string PreferredResponseEncoding { get { NotImplemented (); return null; } }

		public virtual bool RendersBreakBeforeWmlSelectAndInput { get { NotImplemented (); return false; } }

		public virtual bool RendersBreaksAfterHtmlLists { get { NotImplemented (); return false; } }

		public virtual bool RendersBreaksAfterWmlAnchor { get { NotImplemented (); return false; } }

		public virtual bool RendersBreaksAfterWmlInput { get { NotImplemented (); return false; } }

		public virtual bool RendersWmlDoAcceptsInline { get { NotImplemented (); return false; } }

		public virtual bool RendersWmlSelectsAsMenuCards { get { NotImplemented (); return false; } }

		public virtual string RequiredMetaTagNameValue { get { NotImplemented (); return null; } }

		public virtual bool RequiresAttributeColonSubstitution { get { NotImplemented (); return false; } }

		public virtual bool RequiresContentTypeMetaTag { get { NotImplemented (); return false; } }

		public virtual bool RequiresControlStateInSession { get { NotImplemented (); return false; } }

		public virtual bool RequiresDBCSCharacter { get { NotImplemented (); return false; } }

		public virtual bool RequiresHtmlAdaptiveErrorReporting { get { NotImplemented (); return false; } }

		public virtual bool RequiresLeadingPageBreak { get { NotImplemented (); return false; } }

		public virtual bool RequiresNoBreakInFormatting { get { NotImplemented (); return false; } }

		public virtual bool RequiresOutputOptimization { get { NotImplemented (); return false; } }

		public virtual bool RequiresPhoneNumbersAsPlainText { get { NotImplemented (); return false; } }

		public virtual bool RequiresSpecialViewStateEncoding { get { NotImplemented (); return false; } }

		public virtual bool RequiresUniqueFilePathSuffix { get { NotImplemented (); return false; } }

		public virtual bool RequiresUniqueHtmlCheckboxNames { get { NotImplemented (); return false; } }

		public virtual bool RequiresUniqueHtmlInputNames { get { NotImplemented (); return false; } }

		public virtual bool RequiresUrlEncodedPostfieldValues { get { NotImplemented (); return false; } }

		public virtual int ScreenBitDepth { get { NotImplemented (); return 0; } }

		public virtual int ScreenCharactersHeight { get { NotImplemented (); return 0; } }

		public virtual int ScreenCharactersWidth { get { NotImplemented (); return 0; } }

		public virtual int ScreenPixelsHeight { get { NotImplemented (); return 0; } }

		public virtual int ScreenPixelsWidth { get { NotImplemented (); return 0; } }

		public virtual bool SupportsAccesskeyAttribute { get { NotImplemented (); return false; } }

		public virtual bool SupportsBodyColor { get { NotImplemented (); return false; } }

		public virtual bool SupportsBold { get { NotImplemented (); return false; } }

		public virtual bool SupportsCacheControlMetaTag { get { NotImplemented (); return false; } }

		public virtual bool SupportsCallback { get { NotImplemented (); return false; } }

		public virtual bool SupportsCss { get { NotImplemented (); return false; } }

		public virtual bool SupportsDivAlign { get { NotImplemented (); return false; } }

		public virtual bool SupportsDivNoWrap { get { NotImplemented (); return false; } }

		public virtual bool SupportsEmptyStringInCookieValue { get { NotImplemented (); return false; } }

		public virtual bool SupportsFontColor { get { NotImplemented (); return false; } }

		public virtual bool SupportsFontName { get { NotImplemented (); return false; } }

		public virtual bool SupportsFontSize { get { NotImplemented (); return false; } }

		public virtual bool SupportsImageSubmit { get { NotImplemented (); return false; } }

		public virtual bool SupportsIModeSymbols { get { NotImplemented (); return false; } }

		public virtual bool SupportsInputIStyle { get { NotImplemented (); return false; } }

		public virtual bool SupportsInputMode { get { NotImplemented (); return false; } }

		public virtual bool SupportsItalic { get { NotImplemented (); return false; } }

		public virtual bool SupportsJPhoneMultiMediaAttributes { get { NotImplemented (); return false; } }

		public virtual bool SupportsJPhoneSymbols { get { NotImplemented (); return false; } }

		public virtual bool SupportsQueryStringInFormAction { get { NotImplemented (); return false; } }

		public virtual bool SupportsRedirectWithCookie { get { NotImplemented (); return false; } }

		public virtual bool SupportsSelectMultiple { get { NotImplemented (); return false; } }

		public virtual bool SupportsUncheck { get { NotImplemented (); return false; } }

		public virtual bool SupportsXmlHttp { get { NotImplemented (); return false; } }

		public virtual bool Tables { get { NotImplemented (); return false; } }

		public virtual Type TagWriter { get { NotImplemented (); return null; } }

		public virtual string Type { get { NotImplemented (); return null; } }

		public virtual bool UseOptimizedCacheKey { get { NotImplemented (); return false; } }

		public virtual bool VBScript { get { NotImplemented (); return false; } }

		public virtual string Version { get { NotImplemented (); return null; } }

		public virtual Version W3CDomVersion { get { NotImplemented (); return null; } }

		public virtual bool Win16 { get { NotImplemented (); return false; } }

		public virtual bool Win32 { get { NotImplemented (); return false; } }

		public virtual void AddBrowser (string browserName)
		{
			NotImplemented ();
		}

		public virtual int CompareFilters (string filter1, string filter2)
		{
			NotImplemented ();
			return 0;
		}

		public virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter w)
		{
			NotImplemented ();
			return null;
		}

		public virtual void DisableOptimizedCacheKey ()
		{
			NotImplemented ();
		}

		public virtual bool EvaluateFilter (string filterName)
		{
			NotImplemented ();
			return false;
		}

		public virtual Version [] GetClrVersions ()
		{
			NotImplemented ();
			return null;
		}

		public virtual bool IsBrowser (string browserName)
		{
			NotImplemented ();
			return false;
		}
	}
}
