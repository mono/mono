//------------------------------------------------------------------------------
// <copyright file="HttpBrowserCapabilitiesBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web {
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Web.UI;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public abstract class HttpBrowserCapabilitiesBase : IFilterResolutionService {
        public virtual bool ActiveXControls {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual IDictionary Adapters {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool AOL {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool BackgroundSounds {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Beta {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string Browser {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual ArrayList Browsers {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanCombineFormsInDeck {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanInitiateVoiceCall {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanRenderAfterInputOrSelectElement {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanRenderEmptySelects {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanRenderInputAndSelectElementsTogether {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanRenderMixedSelects {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool CanRenderOneventAndPrevElementsTogether {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanRenderPostBackCards {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool CanRenderSetvarZeroWithMultiSelectionList {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool CanSendMail {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual IDictionary Capabilities {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool CDF {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Version ClrVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Cookies {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Crawler {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int DefaultSubmitButtonLimit {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual Version EcmaScriptVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Frames {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int GatewayMajorVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual double GatewayMinorVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string GatewayVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool HasBackButton {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool HidesRightAlignedMultiselectScrollbars {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string HtmlTextWriter {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public virtual string Id {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string InputType {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsColor {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool IsMobileDevice {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool JavaApplets {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Version JScriptVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int MajorVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int MaximumHrefLength {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int MaximumRenderedPageSize {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual int MaximumSoftkeyLabelLength {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual double MinorVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string MinorVersionString {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string MobileDeviceManufacturer {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string MobileDeviceModel {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Version MSDomVersion {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual int NumberOfSoftkeys {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string Platform {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string PreferredImageMime {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string PreferredRenderingMime {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string PreferredRenderingType {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string PreferredRequestEncoding {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string PreferredResponseEncoding {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RendersBreakBeforeWmlSelectAndInput {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RendersBreaksAfterHtmlLists {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RendersBreaksAfterWmlAnchor {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RendersBreaksAfterWmlInput {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RendersWmlDoAcceptsInline {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RendersWmlSelectsAsMenuCards {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string RequiredMetaTagNameValue {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresAttributeColonSubstitution {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresContentTypeMetaTag {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresControlStateInSession {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1705:LongAcronymsShouldBePascalCased",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool RequiresDBCSCharacter {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresHtmlAdaptiveErrorReporting {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresLeadingPageBreak {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresNoBreakInFormatting {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresOutputOptimization {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool RequiresPhoneNumbersAsPlainText {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresSpecialViewStateEncoding {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresUniqueFilePathSuffix {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool RequiresUniqueHtmlCheckboxNames {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool RequiresUniqueHtmlInputNames {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool RequiresUrlEncodedPostfieldValues {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int ScreenBitDepth {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int ScreenCharactersHeight {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int ScreenCharactersWidth {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int ScreenPixelsHeight {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual int ScreenPixelsWidth {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool SupportsAccesskeyAttribute {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsBodyColor {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsBold {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsCacheControlMetaTag {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsCallback {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsCss {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsDivAlign {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsDivNoWrap {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsEmptyStringInCookieValue {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsFontColor {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsFontName {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsFontSize {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsImageSubmit {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsIModeSymbols {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsInputIStyle {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsInputMode {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsItalic {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual bool SupportsJPhoneMultiMediaAttributes {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsJPhoneSymbols {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsQueryStringInFormAction {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsRedirectWithCookie {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsSelectMultiple {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsUncheck {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool SupportsXmlHttp {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Tables {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Type TagWriter {
            get {
                throw new NotImplementedException();
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods",
            Justification = "Matches the base class that we're abstracting.")]
        public virtual string Type {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool UseOptimizedCacheKey {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool VBScript {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string Version {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual Version W3CDomVersion {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Win16 {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual bool Win32 {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual string this[string key] {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual void AddBrowser(string browserName) {
            throw new NotImplementedException();
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Matches HttpBrowserCapabilities class")]
        public virtual HtmlTextWriter CreateHtmlTextWriter(TextWriter w) {
            throw new NotImplementedException();
        }

        public virtual void DisableOptimizedCacheKey() {
            throw new NotImplementedException();
        }

        public virtual Version[] GetClrVersions() {
            throw new NotImplementedException();
        }

        public virtual bool IsBrowser(string browserName) {
            throw new NotImplementedException();
        }

        #region IFilterResolutionService Members
        public virtual int CompareFilters(string filter1, string filter2) {
            throw new NotImplementedException();
        }

        public virtual bool EvaluateFilter(string filterName) {
            throw new NotImplementedException();
        }
        #endregion
    }
}
