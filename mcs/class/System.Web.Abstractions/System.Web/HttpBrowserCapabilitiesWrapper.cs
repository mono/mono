//
// HttpBrowserCapabilitiesWrapper.cs
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
	public class HttpBrowserCapabilitiesWrapper : HttpBrowserCapabilitiesBase
	{
		HttpBrowserCapabilities w;

		public HttpBrowserCapabilitiesWrapper (HttpBrowserCapabilities httpBrowserCapabilities)
		{
			if (httpBrowserCapabilities == null)
				throw new ArgumentNullException ("httpBrowserCapabilities");
			w = httpBrowserCapabilities;
		}


		public override bool ActiveXControls {
			get { return w.ActiveXControls; }
		}

		public override IDictionary Adapters {
			get { return w.Adapters; }
		}

		public override bool AOL {
			get { return w.AOL; }
		}

		public override bool BackgroundSounds {
			get { return w.BackgroundSounds; }
		}

		public override bool Beta {
			get { return w.Beta; }
		}

		public override string Browser {
			get { return w.Browser; }
		}

		public override ArrayList Browsers {
			get { return w.Browsers; }
		}

		public override bool CanCombineFormsInDeck {
			get { return w.CanCombineFormsInDeck; }
		}

		public override bool CanInitiateVoiceCall {
			get { return w.CanInitiateVoiceCall; }
		}

		public override bool CanRenderAfterInputOrSelectElement {
			get { return w.CanRenderAfterInputOrSelectElement; }
		}

		public override bool CanRenderEmptySelects {
			get { return w.CanRenderEmptySelects; }
		}

		public override bool CanRenderInputAndSelectElementsTogether {
			get { return w.CanRenderInputAndSelectElementsTogether; }
		}

		public override bool CanRenderMixedSelects {
			get { return w.CanRenderMixedSelects; }
		}

		public override bool CanRenderOneventAndPrevElementsTogether {
			get { return w.CanRenderOneventAndPrevElementsTogether; }
		}

		public override bool CanRenderPostBackCards {
			get { return w.CanRenderPostBackCards; }
		}

		public override bool CanRenderSetvarZeroWithMultiSelectionList {
			get { return w.CanRenderSetvarZeroWithMultiSelectionList; }
		}

		public override bool CanSendMail {
			get { return w.CanSendMail; }
		}

		public override IDictionary Capabilities {
			get { return w.Capabilities; } set { w.Capabilities = value; }
		}

		public override bool CDF {
			get { return w.CDF; }
		}

		public override Version ClrVersion {
			get { return w.ClrVersion; }
		}

		public override bool Cookies {
			get { return w.Cookies; }
		}

		public override bool Crawler {
			get { return w.Crawler; }
		}

		public override int DefaultSubmitButtonLimit {
			get { return w.DefaultSubmitButtonLimit; }
		}

		public override Version EcmaScriptVersion {
			get { return w.EcmaScriptVersion; }
		}

		public override bool Frames {
			get { return w.Frames; }
		}

		public override int GatewayMajorVersion {
			get { return w.GatewayMajorVersion; }
		}

		public override double GatewayMinorVersion {
			get { return w.GatewayMinorVersion; }
		}

		public override string GatewayVersion {
			get { return w.GatewayVersion; }
		}

		public override bool HasBackButton {
			get { return w.HasBackButton; }
		}

		public override bool HidesRightAlignedMultiselectScrollbars {
			get { return w.HidesRightAlignedMultiselectScrollbars; }
		}

		public override string HtmlTextWriter {
			get { return w.HtmlTextWriter; } set { w.HtmlTextWriter = value; }
		}

		public override string Id {
			get { return w.Id; }
		}

		public override string InputType {
			get { return w.InputType; }
		}

		public override bool IsColor {
			get { return w.IsColor; }
		}

		public override bool IsMobileDevice {
			get { return w.IsMobileDevice; }
		}

		public override string this [string key] {
			get { throw new NotImplementedException (); }
		}

		public override bool JavaApplets {
			get { return w.JavaApplets; }
		}

		public override Version JScriptVersion {
			get { return w.JScriptVersion; }
		}

		public override int MajorVersion {
			get { return w.MajorVersion; }
		}

		public override int MaximumHrefLength {
			get { return w.MaximumHrefLength; }
		}

		public override int MaximumRenderedPageSize {
			get { return w.MaximumRenderedPageSize; }
		}

		public override int MaximumSoftkeyLabelLength {
			get { return w.MaximumSoftkeyLabelLength; }
		}

		public override double MinorVersion {
			get { return w.MinorVersion; }
		}

		public override string MinorVersionString {
			get { return w.MinorVersionString; }
		}

		public override string MobileDeviceManufacturer {
			get { return w.MobileDeviceManufacturer; }
		}

		public override string MobileDeviceModel {
			get { return w.MobileDeviceModel; }
		}

		public override Version MSDomVersion {
			get { return w.MSDomVersion; }
		}

		public override int NumberOfSoftkeys {
			get { return w.NumberOfSoftkeys; }
		}

		public override string Platform {
			get { return w.Platform; }
		}

		public override string PreferredImageMime {
			get { return w.PreferredImageMime; }
		}

		public override string PreferredRenderingMime {
			get { return w.PreferredRenderingMime; }
		}

		public override string PreferredRenderingType {
			get { return w.PreferredRenderingType; }
		}

		public override string PreferredRequestEncoding {
			get { return w.PreferredRequestEncoding; }
		}

		public override string PreferredResponseEncoding {
			get { return w.PreferredResponseEncoding; }
		}

		public override bool RendersBreakBeforeWmlSelectAndInput {
			get { return w.RendersBreakBeforeWmlSelectAndInput; }
		}

		public override bool RendersBreaksAfterHtmlLists {
			get { return w.RendersBreaksAfterHtmlLists; }
		}

		public override bool RendersBreaksAfterWmlAnchor {
			get { return w.RendersBreaksAfterWmlAnchor; }
		}

		public override bool RendersBreaksAfterWmlInput {
			get { return w.RendersBreaksAfterWmlInput; }
		}

		public override bool RendersWmlDoAcceptsInline {
			get { return w.RendersWmlDoAcceptsInline; }
		}

		public override bool RendersWmlSelectsAsMenuCards {
			get { return w.RendersWmlSelectsAsMenuCards; }
		}

		public override string RequiredMetaTagNameValue {
			get { return w.RequiredMetaTagNameValue; }
		}

		public override bool RequiresAttributeColonSubstitution {
			get { return w.RequiresAttributeColonSubstitution; }
		}

		public override bool RequiresContentTypeMetaTag {
			get { return w.RequiresContentTypeMetaTag; }
		}

		public override bool RequiresControlStateInSession {
			get { return w.RequiresControlStateInSession; }
		}

		public override bool RequiresDBCSCharacter {
			get { return w.RequiresDBCSCharacter; }
		}

		public override bool RequiresHtmlAdaptiveErrorReporting {
			get { return w.RequiresHtmlAdaptiveErrorReporting; }
		}

		public override bool RequiresLeadingPageBreak {
			get { return w.RequiresLeadingPageBreak; }
		}

		public override bool RequiresNoBreakInFormatting {
			get { return w.RequiresNoBreakInFormatting; }
		}

		public override bool RequiresOutputOptimization {
			get { return w.RequiresOutputOptimization; }
		}

		public override bool RequiresPhoneNumbersAsPlainText {
			get { return w.RequiresPhoneNumbersAsPlainText; }
		}

		public override bool RequiresSpecialViewStateEncoding {
			get { return w.RequiresSpecialViewStateEncoding; }
		}

		public override bool RequiresUniqueFilePathSuffix {
			get { return w.RequiresUniqueFilePathSuffix; }
		}

		public override bool RequiresUniqueHtmlCheckboxNames {
			get { return w.RequiresUniqueHtmlCheckboxNames; }
		}

		public override bool RequiresUniqueHtmlInputNames {
			get { return w.RequiresUniqueHtmlInputNames; }
		}

		public override bool RequiresUrlEncodedPostfieldValues {
			get { return w.RequiresUrlEncodedPostfieldValues; }
		}

		public override int ScreenBitDepth {
			get { return w.ScreenBitDepth; }
		}

		public override int ScreenCharactersHeight {
			get { return w.ScreenCharactersHeight; }
		}

		public override int ScreenCharactersWidth {
			get { return w.ScreenCharactersWidth; }
		}

		public override int ScreenPixelsHeight {
			get { return w.ScreenPixelsHeight; }
		}

		public override int ScreenPixelsWidth {
			get { return w.ScreenPixelsWidth; }
		}

		public override bool SupportsAccesskeyAttribute {
			get { return w.SupportsAccesskeyAttribute; }
		}

		public override bool SupportsBodyColor {
			get { return w.SupportsBodyColor; }
		}

		public override bool SupportsBold {
			get { return w.SupportsBold; }
		}

		public override bool SupportsCacheControlMetaTag {
			get { return w.SupportsCacheControlMetaTag; }
		}

		public override bool SupportsCallback {
			get { return w.SupportsCallback; }
		}

		public override bool SupportsCss {
			get { return w.SupportsCss; }
		}

		public override bool SupportsDivAlign {
			get { return w.SupportsDivAlign; }
		}

		public override bool SupportsDivNoWrap {
			get { return w.SupportsDivNoWrap; }
		}

		public override bool SupportsEmptyStringInCookieValue {
			get { return w.SupportsEmptyStringInCookieValue; }
		}

		public override bool SupportsFontColor {
			get { return w.SupportsFontColor; }
		}

		public override bool SupportsFontName {
			get { return w.SupportsFontName; }
		}

		public override bool SupportsFontSize {
			get { return w.SupportsFontSize; }
		}

		public override bool SupportsImageSubmit {
			get { return w.SupportsImageSubmit; }
		}

		public override bool SupportsIModeSymbols {
			get { return w.SupportsIModeSymbols; }
		}

		public override bool SupportsInputIStyle {
			get { return w.SupportsInputIStyle; }
		}

		public override bool SupportsInputMode {
			get { return w.SupportsInputMode; }
		}

		public override bool SupportsItalic {
			get { return w.SupportsItalic; }
		}

		public override bool SupportsJPhoneMultiMediaAttributes {
			get { return w.SupportsJPhoneMultiMediaAttributes; }
		}

		public override bool SupportsJPhoneSymbols {
			get { return w.SupportsJPhoneSymbols; }
		}

		public override bool SupportsQueryStringInFormAction {
			get { return w.SupportsQueryStringInFormAction; }
		}

		public override bool SupportsRedirectWithCookie {
			get { return w.SupportsRedirectWithCookie; }
		}

		public override bool SupportsSelectMultiple {
			get { return w.SupportsSelectMultiple; }
		}

		public override bool SupportsUncheck {
			get { return w.SupportsUncheck; }
		}

		public override bool SupportsXmlHttp {
			get { return w.SupportsXmlHttp; }
		}

		public override bool Tables {
			get { return w.Tables; }
		}

		public override Type TagWriter {
			get { return w.TagWriter; }
		}

		public override string Type {
			get { return w.Type; }
		}

		public override bool UseOptimizedCacheKey {
			get { return w.UseOptimizedCacheKey; }
		}

		public override bool VBScript {
			get { return w.VBScript; }
		}

		public override string Version {
			get { return w.Version; }
		}

		public override Version W3CDomVersion {
			get { return w.W3CDomVersion; }
		}

		public override bool Win16 {
			get { return w.Win16; }
		}

		public override bool Win32 {
			get { return w.Win32; }
		}

		public override void AddBrowser (string browserName)
		{
			w.AddBrowser (browserName);
		}

		[MonoTODO]
		public override int CompareFilters (string filter1, string filter2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override HtmlTextWriter CreateHtmlTextWriter (TextWriter w)
		{
			throw new NotImplementedException ();
		}

		public override void DisableOptimizedCacheKey ()
		{
			w.DisableOptimizedCacheKey ();
		}

		[MonoTODO]
		public override bool EvaluateFilter (string filterName)
		{
			throw new NotImplementedException ();
		}

		public override Version [] GetClrVersions ()
		{
			return w.GetClrVersions ();
		}

		public override bool IsBrowser (string browserName)
		{
			return w.IsBrowser (browserName);
		}
	}
}
