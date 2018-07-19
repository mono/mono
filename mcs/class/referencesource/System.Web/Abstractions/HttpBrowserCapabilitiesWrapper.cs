//------------------------------------------------------------------------------
// <copyright file="HttpBrowserCapabilitiesWrapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpBrowserCapabilitiesWrapper : HttpBrowserCapabilitiesBase {
        private HttpBrowserCapabilities _browser;

        public HttpBrowserCapabilitiesWrapper(HttpBrowserCapabilities httpBrowserCapabilities) {
            if (httpBrowserCapabilities == null) {
                throw new ArgumentNullException("httpBrowserCapabilities");
            }
            _browser = httpBrowserCapabilities;
        }

        public override string Browser {
            get {
                return _browser.Browser;
            }
        }

        public override Version EcmaScriptVersion {
            get {
                return _browser.EcmaScriptVersion;
            }
        }

        public override Version JScriptVersion {
            get {
                return _browser.JScriptVersion;
            }
        }

        public override bool SupportsCallback {
            get {
                return _browser.SupportsCallback;
            }
        }

        public override Version W3CDomVersion {
            get {
                return _browser.W3CDomVersion;
            }
        }

        public override bool ActiveXControls {
            get {
                return _browser.ActiveXControls;
            }
        }

        public override IDictionary Adapters {
            get {
                return _browser.Adapters;
            }
        }

        public override bool AOL {
            get {
                return _browser.AOL;
            }
        }

        public override bool BackgroundSounds {
            get {
                return _browser.BackgroundSounds;
            }
        }

        public override bool Beta {
            get {
                return _browser.Beta;
            }
        }

        public override ArrayList Browsers {
            get {
                return _browser.Browsers;
            }
        }

        public override bool CanCombineFormsInDeck {
            get {
                return _browser.CanCombineFormsInDeck;
            }
        }

        public override bool CanInitiateVoiceCall {
            get {
                return _browser.CanInitiateVoiceCall;
            }
        }

        public override bool CanRenderAfterInputOrSelectElement {
            get {
                return _browser.CanRenderAfterInputOrSelectElement;
            }
        }

        public override bool CanRenderEmptySelects {
            get {
                return _browser.CanRenderEmptySelects;
            }
        }

        public override bool CanRenderInputAndSelectElementsTogether {
            get {
                return _browser.CanRenderInputAndSelectElementsTogether;
            }
        }

        public override bool CanRenderMixedSelects {
            get {
                return _browser.CanRenderMixedSelects;
            }
        }

        public override bool CanRenderOneventAndPrevElementsTogether {
            get {
                return _browser.CanRenderOneventAndPrevElementsTogether;
            }
        }

        public override bool CanRenderPostBackCards {
            get {
                return _browser.CanRenderPostBackCards;
            }
        }

        public override bool CanRenderSetvarZeroWithMultiSelectionList {
            get {
                return _browser.CanRenderSetvarZeroWithMultiSelectionList;
            }
        }

        public override bool CanSendMail {
            get {
                return _browser.CanSendMail;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "This has to match the System.Web.dll API.")]
        public override IDictionary Capabilities {
            get {
                return _browser.Capabilities;
            }
            set {
                _browser.Capabilities = value;
            }
        }

        public override bool CDF {
            get {
                return _browser.CDF;
            }
        }

        public override Version ClrVersion {
            get {
                return _browser.ClrVersion;
            }
        }

        public override bool Cookies {
            get {
                return _browser.Cookies;
            }
        }

        public override bool Crawler {
            get {
                return _browser.Crawler;
            }
        }

        public override int DefaultSubmitButtonLimit {
            get {
                return _browser.DefaultSubmitButtonLimit;
            }
        }

        public override bool Frames {
            get {
                return _browser.Frames;
            }
        }

        public override int GatewayMajorVersion {
            get {
                return _browser.GatewayMajorVersion;
            }
        }

        public override double GatewayMinorVersion {
            get {
                return _browser.GatewayMinorVersion;
            }
        }

        public override string GatewayVersion {
            get {
                return _browser.GatewayVersion;
            }
        }

        public override bool HasBackButton {
            get {
                return _browser.HasBackButton;
            }
        }

        public override bool HidesRightAlignedMultiselectScrollbars {
            get {
                return _browser.HidesRightAlignedMultiselectScrollbars;
            }
        }

        public override string HtmlTextWriter {
            get {
                return _browser.HtmlTextWriter;
            }
            set {
                _browser.HtmlTextWriter = value;
            }
        }

        public override string Id {
            get {
                return _browser.Id;
            }
        }

        public override string InputType {
            get {
                return _browser.InputType;
            }
        }

        public override bool IsColor {
            get {
                return _browser.IsColor;
            }
        }

        public override bool IsMobileDevice {
            get {
                return _browser.IsMobileDevice;
            }
        }

        public override bool JavaApplets {
            get {
                return _browser.JavaApplets;
            }
        }

        public override int MajorVersion {
            get {
                return _browser.MajorVersion;
            }
        }

        public override int MaximumHrefLength {
            get {
                return _browser.MaximumHrefLength;
            }
        }

        public override int MaximumRenderedPageSize {
            get {
                return _browser.MaximumRenderedPageSize;
            }
        }

        public override int MaximumSoftkeyLabelLength {
            get {
                return _browser.MaximumSoftkeyLabelLength;
            }
        }

        public override double MinorVersion {
            get {
                return _browser.MinorVersion;
            }
        }

        public override string MinorVersionString {
            get {
                return _browser.MinorVersionString;
            }
        }

        public override string MobileDeviceManufacturer {
            get {
                return _browser.MobileDeviceManufacturer;
            }
        }

        public override string MobileDeviceModel {
            get {
                return _browser.MobileDeviceModel;
            }
        }

        public override Version MSDomVersion {
            get {
                return _browser.MSDomVersion;
            }
        }

        public override int NumberOfSoftkeys {
            get {
                return _browser.NumberOfSoftkeys;
            }
        }

        public override string Platform {
            get {
                return _browser.Platform;
            }
        }

        public override string PreferredImageMime {
            get {
                return _browser.PreferredImageMime;
            }
        }

        public override string PreferredRenderingMime {
            get {
                return _browser.PreferredRenderingMime;
            }
        }

        public override string PreferredRenderingType {
            get {
                return _browser.PreferredRenderingType;
            }
        }

        public override string PreferredRequestEncoding {
            get {
                return _browser.PreferredRequestEncoding;
            }
        }

        public override string PreferredResponseEncoding {
            get {
                return _browser.PreferredResponseEncoding;
            }
        }

        public override bool RendersBreakBeforeWmlSelectAndInput {
            get {
                return _browser.RendersBreakBeforeWmlSelectAndInput;
            }
        }

        public override bool RendersBreaksAfterHtmlLists {
            get {
                return _browser.RendersBreaksAfterHtmlLists;
            }
        }

        public override bool RendersBreaksAfterWmlAnchor {
            get {
                return _browser.RendersBreaksAfterWmlAnchor;
            }
        }

        public override bool RendersBreaksAfterWmlInput {
            get {
                return _browser.RendersBreaksAfterWmlInput;
            }
        }

        public override bool RendersWmlDoAcceptsInline {
            get {
                return _browser.RendersWmlDoAcceptsInline;
            }
        }

        public override bool RendersWmlSelectsAsMenuCards {
            get {
                return _browser.RendersWmlSelectsAsMenuCards;
            }
        }

        public override string RequiredMetaTagNameValue {
            get {
                return _browser.RequiredMetaTagNameValue;
            }
        }

        public override bool RequiresAttributeColonSubstitution {
            get {
                return _browser.RequiresAttributeColonSubstitution;
            }
        }

        public override bool RequiresContentTypeMetaTag {
            get {
                return _browser.RequiresContentTypeMetaTag;
            }
        }

        public override bool RequiresControlStateInSession {
            get {
                return _browser.RequiresControlStateInSession;
            }
        }

        public override bool RequiresDBCSCharacter {
            get {
                return _browser.RequiresDBCSCharacter;
            }
        }

        public override bool RequiresHtmlAdaptiveErrorReporting {
            get {
                return _browser.RequiresHtmlAdaptiveErrorReporting;
            }
        }

        public override bool RequiresLeadingPageBreak {
            get {
                return _browser.RequiresLeadingPageBreak;
            }
        }

        public override bool RequiresNoBreakInFormatting {
            get {
                return _browser.RequiresNoBreakInFormatting;
            }
        }

        public override bool RequiresOutputOptimization {
            get {
                return _browser.RequiresOutputOptimization;
            }
        }

        public override bool RequiresPhoneNumbersAsPlainText {
            get {
                return _browser.RequiresPhoneNumbersAsPlainText;
            }
        }

        public override bool RequiresSpecialViewStateEncoding {
            get {
                return _browser.RequiresSpecialViewStateEncoding;
            }
        }

        public override bool RequiresUniqueFilePathSuffix {
            get {
                return _browser.RequiresUniqueFilePathSuffix;
            }
        }

        public override bool RequiresUniqueHtmlCheckboxNames {
            get {
                return _browser.RequiresUniqueHtmlCheckboxNames;
            }
        }

        public override bool RequiresUniqueHtmlInputNames {
            get {
                return _browser.RequiresUniqueHtmlInputNames;
            }
        }

        public override bool RequiresUrlEncodedPostfieldValues {
            get {
                return _browser.RequiresUrlEncodedPostfieldValues;
            }
        }

        public override int ScreenBitDepth {
            get {
                return _browser.ScreenBitDepth;
            }
        }

        public override int ScreenCharactersHeight {
            get {
                return _browser.ScreenCharactersHeight;
            }
        }

        public override int ScreenCharactersWidth {
            get {
                return _browser.ScreenCharactersWidth;
            }
        }

        public override int ScreenPixelsHeight {
            get {
                return _browser.ScreenPixelsHeight;
            }
        }

        public override int ScreenPixelsWidth {
            get {
                return _browser.ScreenPixelsWidth;
            }
        }

        public override bool SupportsAccesskeyAttribute {
            get {
                return _browser.SupportsAccesskeyAttribute;
            }
        }

        public override bool SupportsBodyColor {
            get {
                return _browser.SupportsBodyColor;
            }
        }

        public override bool SupportsBold {
            get {
                return _browser.SupportsBold;
            }
        }

        public override bool SupportsCacheControlMetaTag {
            get {
                return _browser.SupportsCacheControlMetaTag;
            }
        }

        public override bool SupportsCss {
            get {
                return _browser.SupportsCss;
            }
        }

        public override bool SupportsDivAlign {
            get {
                return _browser.SupportsDivAlign;
            }
        }

        public override bool SupportsDivNoWrap {
            get {
                return _browser.SupportsDivNoWrap;
            }
        }

        public override bool SupportsEmptyStringInCookieValue {
            get {
                return _browser.SupportsEmptyStringInCookieValue;
            }
        }

        public override bool SupportsFontColor {
            get {
                return _browser.SupportsFontColor;
            }
        }

        public override bool SupportsFontName {
            get {
                return _browser.SupportsFontName;
            }
        }

        public override bool SupportsFontSize {
            get {
                return _browser.SupportsFontSize;
            }
        }

        public override bool SupportsImageSubmit {
            get {
                return _browser.SupportsImageSubmit;
            }
        }

        public override bool SupportsIModeSymbols {
            get {
                return _browser.SupportsIModeSymbols;
            }
        }

        public override bool SupportsInputIStyle {
            get {
                return _browser.SupportsInputIStyle;
            }
        }

        public override bool SupportsInputMode {
            get {
                return _browser.SupportsInputMode;
            }
        }

        public override bool SupportsItalic {
            get {
                return _browser.SupportsItalic;
            }
        }

        public override bool SupportsJPhoneMultiMediaAttributes {
            get {
                return _browser.SupportsJPhoneMultiMediaAttributes;
            }
        }

        public override bool SupportsJPhoneSymbols {
            get {
                return _browser.SupportsJPhoneSymbols;
            }
        }

        public override bool SupportsQueryStringInFormAction {
            get {
                return _browser.SupportsQueryStringInFormAction;
            }
        }

        public override bool SupportsRedirectWithCookie {
            get {
                return _browser.SupportsRedirectWithCookie;
            }
        }

        public override bool SupportsSelectMultiple {
            get {
                return _browser.SupportsSelectMultiple;
            }
        }

        public override bool SupportsUncheck {
            get {
                return _browser.SupportsUncheck;
            }
        }

        public override bool SupportsXmlHttp {
            get {
                return _browser.SupportsXmlHttp;
            }
        }

        public override bool Tables {
            get {
                return _browser.Tables;
            }
        }

        public override Type TagWriter {
            get {
                return _browser.TagWriter;
            }
        }

        public override string Type {
            get {
                return _browser.Type;
            }
        }

        public override bool UseOptimizedCacheKey {
            get {
                return _browser.UseOptimizedCacheKey;
            }
        }

        public override bool VBScript {
            get {
                return _browser.VBScript;
            }
        }

        public override string Version {
            get {
                return _browser.Version;
            }
        }

        public override bool Win16 {
            get {
                return _browser.Win16;
            }
        }

        public override bool Win32 {
            get {
                return _browser.Win32;
            }
        }

        public override string this[string key] {
            get {
                return _browser[key];
            }
        }

        public override void AddBrowser(string browserName) {
            _browser.AddBrowser(browserName);
        }

        public override HtmlTextWriter CreateHtmlTextWriter(TextWriter w) {
            return _browser.CreateHtmlTextWriter(w);
        }

        public override void DisableOptimizedCacheKey() {
            _browser.DisableOptimizedCacheKey();
        }

        public override Version[] GetClrVersions() {
            return _browser.GetClrVersions();
        }

        public override bool IsBrowser(string browserName) {
            return _browser.IsBrowser(browserName);
        }

        public override int CompareFilters(string filter1, string filter2) {
            return ((IFilterResolutionService)_browser).CompareFilters(filter1, filter2);
        }

        public override bool EvaluateFilter(string filterName) {
            return ((IFilterResolutionService)_browser).EvaluateFilter(filterName);
        }
    }
}
