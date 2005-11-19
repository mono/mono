//
// System.Web.Configuration.HttpCapabilitiesBase
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.Security.Permissions;
using System.IO;
using System.Web.UI;

namespace System.Web.Configuration
{
	public partial class HttpCapabilitiesBase: IFilterResolutionService
	{
		int IFilterResolutionService.CompareFilters (string filter1, string filter2)
		{
			throw new NotImplementedException ();
		}

		bool IFilterResolutionService.EvaluateFilter (string filterName)
		{
			throw new NotImplementedException ();
		}
		
		public void AddBrowser (string browserName)
		{
		}

		public HtmlTextWriter CreateHtmlTextWriter (TextWriter w)
		{
			throw new NotImplementedException ();
		}

		public void DisableOptimizedCacheKey ()
		{
		}

		public HttpCapabilitiesBase GetConfigCapabilities ()
		{
			throw new NotImplementedException ();
		}

		public IDictionary Adapters {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanCombineFormsInDeck {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanInitiateVoiceCall {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderAfterInputOrSelectElement {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderEmptySelects {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderInputAndSelectElementsTogether {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderMixedSelects {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderOneventAndPrevElementsTogether {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderPostBackCards {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanRenderSetvarZeroWithMultiSelectionList {
			get { throw new NotImplementedException (); }
		}

		public virtual bool CanSendMail {
			get { throw new NotImplementedException (); }
		}

		public IDictionary Capabilities {
			get { throw new NotImplementedException (); }
			set { }
		}

		public virtual int DefaultSubmitButtonLimit {
			get { throw new NotImplementedException (); }
		}

		public virtual int GatewayMajorVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual Double GatewayMinorVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual string GatewayVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual bool HasBackButton {
			get { throw new NotImplementedException (); }
		}

		public virtual bool HidesRightAlignedMultiselectScrollbars {
			get { throw new NotImplementedException (); }
		}

		public string HtmlTextWriter {
			get { throw new NotImplementedException (); }
			set { }
		}

		public string Id {
			get { throw new NotImplementedException (); }
		}

		public virtual string InputType {
			get { throw new NotImplementedException (); }
		}

		public virtual bool IsColor {
			get { throw new NotImplementedException (); }
		}

		public virtual bool IsMobileDevice {
			get { throw new NotImplementedException (); }
		}

		public virtual string Item {
			get { throw new NotImplementedException (); }
		}

		public Version JScriptVersion {
			get { throw new NotImplementedException (); }
		}

		public virtual int MaximumHrefLength {
			get { throw new NotImplementedException (); }
		}

		public virtual int MaximumRenderedPageSize {
			get { throw new NotImplementedException (); }
		}

		public virtual int MaximumSoftkeyLabelLength {
			get { throw new NotImplementedException (); }
		}

		public string MinorVersionString {
			get { throw new NotImplementedException (); }
		}

		public virtual string MobileDeviceManufacturer {
			get { throw new NotImplementedException (); }
		}

		public virtual string MobileDeviceModel {
			get { throw new NotImplementedException (); }
		}

		public virtual int NumberOfSoftkeys {
			get { throw new NotImplementedException (); }
		}

		public virtual string PreferredImageMime {
			get { throw new NotImplementedException (); }
		}

		public virtual string PreferredRenderingMime {
			get { throw new NotImplementedException (); }
		}

		public virtual string PreferredRenderingType {
			get { throw new NotImplementedException (); }
		}

		public virtual string PreferredRequestEncoding {
			get { throw new NotImplementedException (); }
		}

		public virtual string PreferredResponseEncoding {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RendersBreakBeforeWmlSelectAndInput {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RendersBreaksAfterHtmlLists {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RendersBreaksAfterWmlAnchor {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RendersBreaksAfterWmlInput {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RendersWmlDoAcceptsInline {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RendersWmlSelectsAsMenuCards {
			get { throw new NotImplementedException (); }
		}

		public virtual string RequiredMetaTagNameValue {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresAttributeColonSubstitution {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresContentTypeMetaTag {
			get { throw new NotImplementedException (); }
		}

		public bool RequiresControlStateInSession {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresDBCSCharacter {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresHtmlAdaptiveErrorReporting {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresLeadingPageBreak {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresNoBreakInFormatting {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresOutputOptimization {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresPhoneNumbersAsPlainText {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresSpecialViewStateEncoding {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresUniqueFilePathSuffix {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresUniqueHtmlCheckboxNames {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresUniqueHtmlInputNames {
			get { throw new NotImplementedException (); }
		}

		public virtual bool RequiresUrlEncodedPostfieldValues {
			get { throw new NotImplementedException (); }
		}

		public virtual int ScreenBitDepth {
			get { throw new NotImplementedException (); }
		}

		public virtual int ScreenCharactersHeight {
			get { throw new NotImplementedException (); }
		}

		public virtual int ScreenCharactersWidth {
			get { throw new NotImplementedException (); }
		}

		public virtual int ScreenPixelsHeight {
			get { throw new NotImplementedException (); }
		}

		public virtual int ScreenPixelsWidth {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsAccesskeyAttribute {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsBodyColor {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsBold {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsCacheControlMetaTag {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsCallback {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsCss {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsDivAlign {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsDivNoWrap {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsEmptyStringInCookieValue {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsFontColor {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsFontName {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsFontSize {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsImageSubmit {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsIModeSymbols {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsInputIStyle {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsInputMode {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsItalic {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsJPhoneMultiMediaAttributes {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsJPhoneSymbols {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsQueryStringInFormAction {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsRedirectWithCookie {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsSelectMultiple {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsUncheck {
			get { throw new NotImplementedException (); }
		}

		public virtual bool SupportsXmlHttp {
			get { throw new NotImplementedException (); }
		}

		public bool UseOptimizedCacheKey {
			get { throw new NotImplementedException (); }
		}
	}
}

#endif
