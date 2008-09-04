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
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpBrowserCapabilitiesBase : IFilterResolutionService
	{
		[MonoTODO]
		public virtual bool ActiveXControls { get; private set; }
		[MonoTODO]
		public virtual IDictionary Adapters { get; private set; }
		[MonoTODO]
		public virtual bool AOL { get; private set; }
		[MonoTODO]
		public virtual bool BackgroundSounds { get; private set; }
		[MonoTODO]
		public virtual bool Beta { get; private set; }
		[MonoTODO]
		public virtual string Browser { get; private set; }
		[MonoTODO]
		public virtual ArrayList Browsers { get; private set; }
		[MonoTODO]
		public virtual bool CanCombineFormsInDeck { get; private set; }
		[MonoTODO]
		public virtual bool CanInitiateVoiceCall { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderAfterInputOrSelectElement { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderEmptySelects { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderInputAndSelectElementsTogether { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderMixedSelects { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderOneventAndPrevElementsTogether { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderPostBackCards { get; private set; }
		[MonoTODO]
		public virtual bool CanRenderSetvarZeroWithMultiSelectionList { get; private set; }
		[MonoTODO]
		public virtual bool CanSendMail { get; private set; }
		[MonoTODO]
		public virtual IDictionary Capabilities { get; set; }
		[MonoTODO]
		public virtual bool CDF { get; private set; }
		[MonoTODO]
		public virtual Version ClrVersion { get; private set; }
		[MonoTODO]
		public virtual bool Cookies { get; private set; }
		[MonoTODO]
		public virtual bool Crawler { get; private set; }
		[MonoTODO]
		public virtual int DefaultSubmitButtonLimit { get; private set; }
		[MonoTODO]
		public virtual Version EcmaScriptVersion { get; private set; }
		[MonoTODO]
		public virtual bool Frames { get; private set; }
		[MonoTODO]
		public virtual int GatewayMajorVersion { get; private set; }
		[MonoTODO]
		public virtual double GatewayMinorVersion { get; private set; }
		[MonoTODO]
		public virtual string GatewayVersion { get; private set; }
		[MonoTODO]
		public virtual bool HasBackButton { get; private set; }
		[MonoTODO]
		public virtual bool HidesRightAlignedMultiselectScrollbars { get; private set; }
		[MonoTODO]
		public virtual string HtmlTextWriter { get; set; }
		[MonoTODO]
		public virtual string Id { get; private set; }
		[MonoTODO]
		public virtual string InputType { get; private set; }
		[MonoTODO]
		public virtual bool IsColor { get; private set; }
		[MonoTODO]
		public virtual bool IsMobileDevice { get; private set; }
		[MonoTODO]
		public virtual string this [string key] {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual bool JavaApplets { get; private set; }
		[MonoTODO]
		public virtual Version JScriptVersion { get; private set; }
		[MonoTODO]
		public virtual int MajorVersion { get; private set; }
		[MonoTODO]
		public virtual int MaximumHrefLength { get; private set; }
		[MonoTODO]
		public virtual int MaximumRenderedPageSize { get; private set; }
		[MonoTODO]
		public virtual int MaximumSoftkeyLabelLength { get; private set; }
		[MonoTODO]
		public virtual double MinorVersion { get; private set; }
		[MonoTODO]
		public virtual string MinorVersionString { get; private set; }
		[MonoTODO]
		public virtual string MobileDeviceManufacturer { get; private set; }
		[MonoTODO]
		public virtual string MobileDeviceModel { get; private set; }
		[MonoTODO]
		public virtual Version MSDomVersion { get; private set; }
		[MonoTODO]
		public virtual int NumberOfSoftkeys { get; private set; }
		[MonoTODO]
		public virtual string Platform { get; private set; }
		[MonoTODO]
		public virtual string PreferredImageMime { get; private set; }
		[MonoTODO]
		public virtual string PreferredRenderingMime { get; private set; }
		[MonoTODO]
		public virtual string PreferredRenderingType { get; private set; }
		[MonoTODO]
		public virtual string PreferredRequestEncoding { get; private set; }
		[MonoTODO]
		public virtual string PreferredResponseEncoding { get; private set; }
		[MonoTODO]
		public virtual bool RendersBreakBeforeWmlSelectAndInput { get; private set; }
		[MonoTODO]
		public virtual bool RendersBreaksAfterHtmlLists { get; private set; }
		[MonoTODO]
		public virtual bool RendersBreaksAfterWmlAnchor { get; private set; }
		[MonoTODO]
		public virtual bool RendersBreaksAfterWmlInput { get; private set; }
		[MonoTODO]
		public virtual bool RendersWmlDoAcceptsInline { get; private set; }
		[MonoTODO]
		public virtual bool RendersWmlSelectsAsMenuCards { get; private set; }
		[MonoTODO]
		public virtual string RequiredMetaTagNameValue { get; private set; }
		[MonoTODO]
		public virtual bool RequiresAttributeColonSubstitution { get; private set; }
		[MonoTODO]
		public virtual bool RequiresContentTypeMetaTag { get; private set; }
		[MonoTODO]
		public virtual bool RequiresControlStateInSession { get; private set; }
		[MonoTODO]
		public virtual bool RequiresDBCSCharacter { get; private set; }
		[MonoTODO]
		public virtual bool RequiresHtmlAdaptiveErrorReporting { get; private set; }
		[MonoTODO]
		public virtual bool RequiresLeadingPageBreak { get; private set; }
		[MonoTODO]
		public virtual bool RequiresNoBreakInFormatting { get; private set; }
		[MonoTODO]
		public virtual bool RequiresOutputOptimization { get; private set; }
		[MonoTODO]
		public virtual bool RequiresPhoneNumbersAsPlainText { get; private set; }
		[MonoTODO]
		public virtual bool RequiresSpecialViewStateEncoding { get; private set; }
		[MonoTODO]
		public virtual bool RequiresUniqueFilePathSuffix { get; private set; }
		[MonoTODO]
		public virtual bool RequiresUniqueHtmlCheckboxNames { get; private set; }
		[MonoTODO]
		public virtual bool RequiresUniqueHtmlInputNames { get; private set; }
		[MonoTODO]
		public virtual bool RequiresUrlEncodedPostfieldValues { get; private set; }
		[MonoTODO]
		public virtual int ScreenBitDepth { get; private set; }
		[MonoTODO]
		public virtual int ScreenCharactersHeight { get; private set; }
		[MonoTODO]
		public virtual int ScreenCharactersWidth { get; private set; }
		[MonoTODO]
		public virtual int ScreenPixelsHeight { get; private set; }
		[MonoTODO]
		public virtual int ScreenPixelsWidth { get; private set; }
		[MonoTODO]
		public virtual bool SupportsAccesskeyAttribute { get; private set; }
		[MonoTODO]
		public virtual bool SupportsBodyColor { get; private set; }
		[MonoTODO]
		public virtual bool SupportsBold { get; private set; }
		[MonoTODO]
		public virtual bool SupportsCacheControlMetaTag { get; private set; }
		[MonoTODO]
		public virtual bool SupportsCallback { get; private set; }
		[MonoTODO]
		public virtual bool SupportsCss { get; private set; }
		[MonoTODO]
		public virtual bool SupportsDivAlign { get; private set; }
		[MonoTODO]
		public virtual bool SupportsDivNoWrap { get; private set; }
		[MonoTODO]
		public virtual bool SupportsEmptyStringInCookieValue { get; private set; }
		[MonoTODO]
		public virtual bool SupportsFontColor { get; private set; }
		[MonoTODO]
		public virtual bool SupportsFontName { get; private set; }
		[MonoTODO]
		public virtual bool SupportsFontSize { get; private set; }
		[MonoTODO]
		public virtual bool SupportsImageSubmit { get; private set; }
		[MonoTODO]
		public virtual bool SupportsIModeSymbols { get; private set; }
		[MonoTODO]
		public virtual bool SupportsInputIStyle { get; private set; }
		[MonoTODO]
		public virtual bool SupportsInputMode { get; private set; }
		[MonoTODO]
		public virtual bool SupportsItalic { get; private set; }
		[MonoTODO]
		public virtual bool SupportsJPhoneMultiMediaAttributes { get; private set; }
		[MonoTODO]
		public virtual bool SupportsJPhoneSymbols { get; private set; }
		[MonoTODO]
		public virtual bool SupportsQueryStringInFormAction { get; private set; }
		[MonoTODO]
		public virtual bool SupportsRedirectWithCookie { get; private set; }
		[MonoTODO]
		public virtual bool SupportsSelectMultiple { get; private set; }
		[MonoTODO]
		public virtual bool SupportsUncheck { get; private set; }
		[MonoTODO]
		public virtual bool SupportsXmlHttp { get; private set; }
		[MonoTODO]
		public virtual bool Tables { get; private set; }
		[MonoTODO]
		public virtual Type TagWriter { get; private set; }
		[MonoTODO]
		public virtual string Type { get; private set; }
		[MonoTODO]
		public virtual bool UseOptimizedCacheKey { get; private set; }
		[MonoTODO]
		public virtual bool VBScript { get; private set; }
		[MonoTODO]
		public virtual string Version { get; private set; }
		[MonoTODO]
		public virtual Version W3CDomVersion { get; private set; }
		[MonoTODO]
		public virtual bool Win16 { get; private set; }
		[MonoTODO]
		public virtual bool Win32 { get; private set; }

		[MonoTODO]
		public virtual void AddBrowser (string browserName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual int CompareFilters (string filter1, string filter2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual HtmlTextWriter CreateHtmlTextWriter (TextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void DisableOptimizedCacheKey ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool EvaluateFilter (string filterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual Version [] GetClrVersions ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsBrowser (string browserName)
		{
			throw new NotImplementedException ();
		}
	}
}
