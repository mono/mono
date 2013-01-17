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

using System.Collections;
using System.Security.Permissions;
using System.IO;
using System.Web.UI;

namespace System.Web.Configuration
{
	public partial class HttpCapabilitiesBase: IFilterResolutionService
	{
		internal IDictionary capabilities;

		public HttpCapabilitiesBase () { }

		public virtual string this [string key]
		{
			get { return capabilities [key] as string; }
		}

		internal static string GetUserAgentForDetection (HttpRequest request)
		{
			string ua = null;
			if (request.Context.CurrentHandler is System.Web.UI.Page)
				ua = ((System.Web.UI.Page) request.Context.CurrentHandler).ClientTarget;
			
			if (String.IsNullOrEmpty (ua)) {
				ua = request.ClientTarget;

				if (String.IsNullOrEmpty (ua))
					ua = request.UserAgent;
			}

			return ua;
		}

		static HttpBrowserCapabilities GetHttpBrowserCapabilitiesFromBrowscapini(string ua)
		{
			HttpBrowserCapabilities bcap = new HttpBrowserCapabilities();
			bcap.capabilities = CapabilitiesLoader.GetCapabilities (ua);
			return bcap;
		}
		
		public static HttpCapabilitiesBase GetConfigCapabilities (string configKey, HttpRequest request)
		{
			string ua = GetUserAgentForDetection (request);
			HttpBrowserCapabilities bcap = GetHttpBrowserCapabilitiesFromBrowscapini(ua);
			GetConfigCapabilities_called = true;
			if (HttpApplicationFactory.AppBrowsersFiles.Length > 0)
				bcap = HttpApplicationFactory.CapabilitiesProcessor.Process(request, bcap.Capabilities);
			bcap.useragent = ua;
			bcap.Init ();
			return bcap;
		}

		// Used by unit tests to determine whether GetConfigCapabilities was called.
		static internal bool GetConfigCapabilities_called;

		protected virtual void Init ()
		{
		}

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
			return (HtmlTextWriter) Activator.CreateInstance (TagWriter, new object[] {w});
		}

		public void DisableOptimizedCacheKey ()
		{
			throw new NotImplementedException ();
		}

		IDictionary adapters = null;
		public IDictionary Adapters {
			get {
				if (!Get (HaveAdapters)) {
					adapters = GetAdapters();
					Set (HaveAdapters);
				}

				return adapters;
			}
		}
		
		internal virtual IDictionary GetAdapters ()
		{
			return new Hashtable();
		}

		bool canCombineFormsInDeck;
		public virtual bool CanCombineFormsInDeck {
			get {
				if (!Get (HaveCanCombineFormsInDeck)) {
					canCombineFormsInDeck = ReadBoolean ("cancombineformsindeck");
					Set (HaveCanCombineFormsInDeck);
				}

				return canCombineFormsInDeck;
			}
		}

		bool canInitiateVoiceCall;
		public virtual bool CanInitiateVoiceCall {
			get {
				if (!Get (HaveCanInitiateVoiceCall)) {
					canInitiateVoiceCall = ReadBoolean ("caninitiatevoicecall");
					Set (HaveCanInitiateVoiceCall);
				}

				return canInitiateVoiceCall;
			}
		}

		bool canRenderAfterInputOrSelectElement;
		public virtual bool CanRenderAfterInputOrSelectElement {
			get {
				if (!Get (HaveCanRenderAfterInputOrSelectElement)) {
					canRenderAfterInputOrSelectElement = ReadBoolean ("canrenderafterinputorselectelement");
					Set (HaveCanRenderAfterInputOrSelectElement);
				}

				return canRenderAfterInputOrSelectElement;
			}
		}

		bool canRenderEmptySelects;
		public virtual bool CanRenderEmptySelects {
			get {
				if (!Get (HaveCanRenderEmptySelects)) {
					canRenderEmptySelects = ReadBoolean ("canrenderemptyselects");
					Set (HaveCanRenderEmptySelects);
				}

				return canRenderEmptySelects;
			}
		}

		bool canRenderInputAndSelectElementsTogether;
		public virtual bool CanRenderInputAndSelectElementsTogether {
			get {
				if (!Get (HaveCanRenderInputAndSelectElementsTogether)) {
					canRenderInputAndSelectElementsTogether = ReadBoolean ("canrenderinputandselectelementstogether");
					Set (HaveCanRenderInputAndSelectElementsTogether);
				}

				return canRenderInputAndSelectElementsTogether;
			}
		}

		bool canRenderMixedSelects;
		public virtual bool CanRenderMixedSelects {
			get {
				if (!Get (HaveCanRenderMixedSelects)) {
					canRenderMixedSelects = ReadBoolean ("canrendermixedselects");
					Set (HaveCanRenderMixedSelects);
				}

				return canRenderMixedSelects;
			}
		}

		bool canRenderOneventAndPrevElementsTogether;
		public virtual bool CanRenderOneventAndPrevElementsTogether {
			get {
				if (!Get (HaveCanRenderOneventAndPrevElementsTogether)) {
					canRenderOneventAndPrevElementsTogether = ReadBoolean ("canrenderoneventandprevelementstogether");
					Set (HaveCanRenderOneventAndPrevElementsTogether);
				}

				return canRenderOneventAndPrevElementsTogether;
			}
		}

		bool canRenderPostBackCards;
		public virtual bool CanRenderPostBackCards {
			get {
				if (!Get (HaveCanRenderPostBackCards)) {
					canRenderPostBackCards = ReadBoolean ("canrenderpostbackcards");
					Set (HaveCanRenderPostBackCards);
				}

				return canRenderPostBackCards;
			}
		}

		bool canRenderSetvarZeroWithMultiSelectionList;
		public virtual bool CanRenderSetvarZeroWithMultiSelectionList {
			get {
				if (!Get (HaveCanRenderSetvarZeroWithMultiSelectionList)) {
					canRenderSetvarZeroWithMultiSelectionList = ReadBoolean ("canrendersetvarzerowithmultiselectionlist");
					Set (HaveCanRenderSetvarZeroWithMultiSelectionList);
				}

				return canRenderSetvarZeroWithMultiSelectionList;
			}
		}

		bool canSendMail;
		public virtual bool CanSendMail {
			get {
				if (!Get (HaveCanSendMail)) {
					canSendMail = ReadBoolean ("cansendmail");
					Set (HaveCanSendMail);
				}

				return canSendMail;
			}
		}

		public IDictionary Capabilities {
			get { return capabilities; }
			set { capabilities = new Hashtable(value, StringComparer.OrdinalIgnoreCase); }
		}

		int defaultSubmitButtonLimit;
		public virtual int DefaultSubmitButtonLimit {
			get {
				if (!Get (HaveDefaultSubmitButtonLimit)) {
					defaultSubmitButtonLimit = ReadInt32 ("defaultsubmitbuttonlimit");
					Set (HaveDefaultSubmitButtonLimit);
				}

				return defaultSubmitButtonLimit;
			}
		}

		int gatewayMajorVersion;
		public virtual int GatewayMajorVersion {
			get {
				if (!Get (HaveGatewayMajorVersion)) {
					gatewayMajorVersion = ReadInt32 ("gatewaymajorversion");
					Set (HaveGatewayMajorVersion);
				}

				return gatewayMajorVersion;
			}
		}

		Double gatewayMinorVersion;
		public virtual Double GatewayMinorVersion {
			get {
				if (!Get (HaveGatewayMinorVersion)) {
					gatewayMinorVersion = ReadDouble ("gatewayminorversion");
					Set (HaveGatewayMinorVersion);
				}

				return gatewayMinorVersion;
			}
		}

		string gatewayVersion;
		public virtual string GatewayVersion {
			get {
				if (!Get (HaveGatewayVersion)) {
					gatewayVersion = ReadString ("gatewayversion");
					Set (HaveGatewayVersion);
				}

				return gatewayVersion;
			}
		}

		bool hasBackButton;
		public virtual bool HasBackButton {
			get {
				if (!Get (HaveHasBackButton)) {
					hasBackButton = ReadBoolean ("hasbackbutton");
					Set (HaveHasBackButton);
				}

				return hasBackButton;
			}
		}

		bool hidesRightAlignedMultiselectScrollbars;
		public virtual bool HidesRightAlignedMultiselectScrollbars {
			get {
				if (!Get (HaveHidesRightAlignedMultiselectScrollbars)) {
					hidesRightAlignedMultiselectScrollbars = ReadBoolean ("hidesrightalignedmultiselectscrollbars");
					Set (HaveHidesRightAlignedMultiselectScrollbars);
				}
				
				return hidesRightAlignedMultiselectScrollbars;
			}
		}

		string htmlTextWriter;
		public string HtmlTextWriter {
			get {
				if (!Get (HaveHtmlTextWriter)) {
					htmlTextWriter = ReadString ("htmlTextWriter");
					Set (HaveHtmlTextWriter);
				}

				return htmlTextWriter;
			}
			set {
				Set (HaveHtmlTextWriter);
				htmlTextWriter = value;
			}
		}

		public string Id {
			get { return this.Browser; }
		}

		string inputType;
		public virtual string InputType {
			get {
				if (!Get (HaveInputType)) {
					inputType = ReadString ("inputtype");
					Set (HaveInputType);
				}

				return inputType;
			}
		}

		bool isColor;
		public virtual bool IsColor {
			get {
				if (!Get (HaveIsColor)) {
					isColor = ReadBoolean ("iscolor");
					Set (HaveIsColor);
				}

				return isColor;
			}
		}

		bool isMobileDevice;
		public virtual bool IsMobileDevice {
			get {
				if (!Get (HaveIsMobileDevice)) {
					isMobileDevice = ReadBoolean ("ismobiledevice");
					Set (HaveIsMobileDevice);
				}

				return isMobileDevice;
			}
		}

		Version jscriptVersion;
		public Version JScriptVersion {
			get {
				if (!Get (HaveJScriptVersion)) {
					jscriptVersion = ReadVersion ("jscriptversion");
					Set (HaveJScriptVersion);
				}

				return jscriptVersion;
			}
		}

		int maximumHrefLength;
		public virtual int MaximumHrefLength {
			get {
				if (!Get (HaveMaximumHrefLength)) {
					maximumHrefLength = ReadInt32 ("maximumhreflength");
					Set (HaveMaximumHrefLength);
				}

				return maximumHrefLength;
			}
		}

		int maximumRenderedPageSize;
		public virtual int MaximumRenderedPageSize {
			get {
				if (!Get (HaveMaximumRenderedPageSize)) {
					maximumRenderedPageSize = ReadInt32 ("maximumrenderedpagesize");
					Set (HaveMaximumRenderedPageSize);
				}

				return maximumRenderedPageSize;
			}
		}

		int maximumSoftkeyLabelLength;
		public virtual int MaximumSoftkeyLabelLength {
			get {
				if (!Get (HaveMaximumSoftkeyLabelLength)) {
					maximumSoftkeyLabelLength = ReadInt32 ("maximumsoftkeylabellength");
					Set (HaveMaximumSoftkeyLabelLength);
				}

				return maximumSoftkeyLabelLength;
			}
		}

		string minorVersionString;
		public string MinorVersionString {
			get {
				if (!Get (HaveMinorVersionString)) {
					minorVersionString = ReadString ("minorversionstring");
					Set (HaveMinorVersionString);
				}

				return minorVersionString;
			}
		}

		string mobileDeviceManufacturer;
		public virtual string MobileDeviceManufacturer {
			get {
				if (!Get (HaveMobileDeviceManufacturer)) {
					mobileDeviceManufacturer = ReadString ("mobiledevicemanufacturer");
					Set (HaveMobileDeviceManufacturer);
				}

				return mobileDeviceManufacturer;
			}
		}

		string mobileDeviceModel;
		public virtual string MobileDeviceModel {
			get {
				if (!Get (HaveMobileDeviceModel)) {
					mobileDeviceModel = ReadString ("mobiledevicemodel");
					Set (HaveMobileDeviceModel);
				}

				return mobileDeviceModel;
			}
		}

		int numberOfSoftkeys;
		public virtual int NumberOfSoftkeys {
			get {
				if (!Get (HaveNumberOfSoftkeys)) {
					numberOfSoftkeys = ReadInt32 ("numberofsoftkeys");
					Set (HaveNumberOfSoftkeys);
				}

				return numberOfSoftkeys;
			}
		}

		string preferredImageMime;
		public virtual string PreferredImageMime {
			get {
				if (!Get (HavePreferredImageMime)) {
					preferredImageMime = ReadString ("preferredimagemime");
					Set (HavePreferredImageMime);
				}

				return preferredImageMime;
			}
		}

		string preferredRenderingMime;
		public virtual string PreferredRenderingMime {
			get {
				if (!Get (HavePreferredRenderingMime)) {
					preferredRenderingMime = ReadString ("preferredrenderingmime");
					Set (HavePreferredRenderingMime);
				}

				return preferredRenderingMime;
			}
		}

		string preferredRenderingType;
		public virtual string PreferredRenderingType {
			get {
				if (!Get (HavePreferredRenderingType)) {
					preferredRenderingType = ReadString ("preferredrenderingtype");
					Set (HavePreferredRenderingType);
				}

				return preferredRenderingType;
			}
		}

		string preferredRequestEncoding;
		public virtual string PreferredRequestEncoding {
			get {
				if (!Get (HavePreferredRequestEncoding)) {
					preferredRequestEncoding = ReadString ("preferredrequestencoding");
					Set (HavePreferredRequestEncoding);
				}

				return preferredRequestEncoding;
			}
		}

		string preferredResponseEncoding;
		public virtual string PreferredResponseEncoding {
			get {
				if (!Get (HavePreferredResponseEncoding)) {
					preferredResponseEncoding = ReadString ("preferredresponseencoding");
					Set (HavePreferredResponseEncoding);
				}

				return preferredResponseEncoding;
			}
		}

		bool rendersBreakBeforeWmlSelectAndInput;
		public virtual bool RendersBreakBeforeWmlSelectAndInput {
			get {
				if (!Get (HaveRendersBreakBeforeWmlSelectAndInput)) {
					rendersBreakBeforeWmlSelectAndInput = ReadBoolean ("rendersbreakbeforewmlselectandinput");
					Set (HaveRendersBreakBeforeWmlSelectAndInput);
				}

				return rendersBreakBeforeWmlSelectAndInput;
			}
		}

		bool rendersBreaksAfterHtmlLists;
		public virtual bool RendersBreaksAfterHtmlLists {
			get {
				if (!Get (HaveRendersBreaksAfterHtmlLists)) {
					rendersBreaksAfterHtmlLists = ReadBoolean ("rendersbreaksafterhtmllists");
					Set (HaveRendersBreaksAfterHtmlLists);
				}

				return rendersBreaksAfterHtmlLists;
			}
		}

		bool rendersBreaksAfterWmlAnchor;
		public virtual bool RendersBreaksAfterWmlAnchor {
			get {
				if (!Get (HaveRendersBreaksAfterWmlAnchor)) {
					rendersBreaksAfterWmlAnchor = ReadBoolean ("rendersbreaksafterwmlanchor");
					Set (HaveRendersBreaksAfterWmlAnchor);
				}

				return rendersBreaksAfterWmlAnchor;
			}
		}

		bool rendersBreaksAfterWmlInput;
		public virtual bool RendersBreaksAfterWmlInput {
			get {
				if (!Get (HaveRendersBreaksAfterWmlInput)) {
					rendersBreaksAfterWmlInput = ReadBoolean ("rendersbreaksafterwmlinput");
					Set (HaveRendersBreaksAfterWmlInput);
				}

				return rendersBreaksAfterWmlInput;
			}
		}

		bool rendersWmlDoAcceptsInline;
		public virtual bool RendersWmlDoAcceptsInline {
			get {
				if (!Get (HaveRendersWmlDoAcceptsInline)) {
					rendersWmlDoAcceptsInline = ReadBoolean ("renderswmldoacceptsinline");
					Set (HaveRendersWmlDoAcceptsInline);
				}

				return rendersWmlDoAcceptsInline;
			}
		}

		bool rendersWmlSelectsAsMenuCards;
		public virtual bool RendersWmlSelectsAsMenuCards {
			get {
				if (!Get (HaveRendersWmlSelectsAsMenuCards)) {
					rendersWmlSelectsAsMenuCards = ReadBoolean ("renderswmlselectsasmenucards");
					Set (HaveRendersWmlSelectsAsMenuCards);
				}

				return rendersWmlSelectsAsMenuCards;
			}
		}

		string requiredMetaTagNameValue;
		public virtual string RequiredMetaTagNameValue {
			get {
				if (!Get (HaveRequiredMetaTagNameValue)) {
					requiredMetaTagNameValue = ReadString ("requiredmetatagnamevalue");
					Set (HaveRequiredMetaTagNameValue);
				}

				return requiredMetaTagNameValue;
			}
		}

		bool requiresAttributeColonSubstitution;
		public virtual bool RequiresAttributeColonSubstitution {
			get {
				if (!Get (HaveRequiresAttributeColonSubstitution)) {
					requiresAttributeColonSubstitution = ReadBoolean ("requiresattributecolonsubstitution");
					Set (HaveRequiresAttributeColonSubstitution);
				}

				return requiresAttributeColonSubstitution;
			}
		}

		bool requiresContentTypeMetaTag;
		public virtual bool RequiresContentTypeMetaTag {
			get {
				if (!Get (HaveRequiresContentTypeMetaTag)) {
					requiresContentTypeMetaTag = ReadBoolean ("requiresContentTypeMetaTag");
					Set (HaveRequiresContentTypeMetaTag);
				}

				return requiresContentTypeMetaTag;
			}
		}

		bool requiresControlStateInSession;
		public bool RequiresControlStateInSession {
			get {
				if (!Get (HaveRequiresControlStateInSession)) {
					requiresControlStateInSession = ReadBoolean ("requiresControlStateInSession");
					Set (HaveRequiresControlStateInSession);
				}

				return requiresControlStateInSession;
			}
		}

		bool requiresDBCSCharacter;
		public virtual bool RequiresDBCSCharacter {
			get {
				if (!Get (HaveRequiresDBCSCharacter)) {
					requiresDBCSCharacter = ReadBoolean ("requiresdbcscharacter");
					Set (HaveRequiresDBCSCharacter);
				}

				return requiresDBCSCharacter;
			}
		}

		bool requiresHtmlAdaptiveErrorReporting;
		public virtual bool RequiresHtmlAdaptiveErrorReporting {
			get {
				if (!Get (HaveRequiresHtmlAdaptiveErrorReporting)) {
					requiresHtmlAdaptiveErrorReporting = ReadBoolean ("requireshtmladaptiveerrorreporting");
					Set (HaveRequiresHtmlAdaptiveErrorReporting);
				}

				return requiresHtmlAdaptiveErrorReporting;
			}
		}

		bool requiresLeadingPageBreak;
		public virtual bool RequiresLeadingPageBreak {
			get {
				if (!Get (HaveRequiresLeadingPageBreak)) {
					requiresLeadingPageBreak = ReadBoolean ("requiresleadingpagebreak");
					Set (HaveRequiresLeadingPageBreak);
				}

				return requiresLeadingPageBreak;
			}
		}

		bool requiresNoBreakInFormatting;
		public virtual bool RequiresNoBreakInFormatting {
			get {
				if (!Get (HaveRequiresNoBreakInFormatting)) {
					requiresNoBreakInFormatting = ReadBoolean ("requiresnobreakinformatting");
					Set (HaveRequiresNoBreakInFormatting);
				}

				return requiresNoBreakInFormatting;
			}
		}

		bool requiresOutputOptimization;
		public virtual bool RequiresOutputOptimization {
			get {
				if (!Get (HaveRequiresOutputOptimization)) {
					requiresOutputOptimization = ReadBoolean ("requiresoutputoptimization");
					Set (HaveRequiresOutputOptimization);
				}

				return requiresOutputOptimization;
			}
		}

		bool requiresPhoneNumbersAsPlainText;
		public virtual bool RequiresPhoneNumbersAsPlainText {
			get {
				if (!Get (HaveRequiresPhoneNumbersAsPlainText)) {
					requiresPhoneNumbersAsPlainText = ReadBoolean ("requiresphonenumbersasplaintext");
					Set (HaveRequiresPhoneNumbersAsPlainText);
				}

				return requiresPhoneNumbersAsPlainText;
			}
		}

		bool requiresSpecialViewStateEncoding;
		public virtual bool RequiresSpecialViewStateEncoding {
			get {
				if (!Get (HaveRequiresSpecialViewStateEncoding)) {
					requiresSpecialViewStateEncoding = ReadBoolean ("requiresspecialviewstateencoding");
					Set (HaveRequiresSpecialViewStateEncoding);
				}

				return requiresSpecialViewStateEncoding;
			}
		}

		bool requiresUniqueFilePathSuffix;
		public virtual bool RequiresUniqueFilePathSuffix {
			get {
				if (!Get (HaveRequiresUniqueFilePathSuffix)) {
					requiresUniqueFilePathSuffix = ReadBoolean ("requiresuniquefilepathsuffix");
					Set (HaveRequiresUniqueFilePathSuffix);
				}

				return requiresUniqueFilePathSuffix;
			}
		}

		bool requiresUniqueHtmlCheckboxNames;
		public virtual bool RequiresUniqueHtmlCheckboxNames {
			get {
				if (!Get (HaveRequiresUniqueHtmlCheckboxNames)) {
					requiresUniqueHtmlCheckboxNames = ReadBoolean ("requiresuniquehtmlcheckboxnames");
					Set (HaveRequiresUniqueHtmlCheckboxNames);
				}

				return requiresUniqueHtmlCheckboxNames;
			}
		}

		bool requiresUniqueHtmlInputNames;
		public virtual bool RequiresUniqueHtmlInputNames {
			get {
				if (!Get (HaveRequiresUniqueHtmlInputNames)) {
					requiresUniqueHtmlInputNames = ReadBoolean ("requiresuniquehtmlinputnames");
					Set (HaveRequiresUniqueHtmlInputNames);
				}

				return requiresUniqueHtmlInputNames;
			}
		}

		bool requiresUrlEncodedPostfieldValues;
		public virtual bool RequiresUrlEncodedPostfieldValues {
			get {
				if (!Get (HaveRequiresUrlEncodedPostfieldValues)) {
					requiresUrlEncodedPostfieldValues = ReadBoolean ("requiresurlencodedpostfieldvalues");
					Set (HaveRequiresUrlEncodedPostfieldValues);
				}

				return requiresUrlEncodedPostfieldValues;
			}
		}

		int screenBitDepth;
		public virtual int ScreenBitDepth {
			get {
				if (!Get (HaveScreenBitDepth)) {
					screenBitDepth = ReadInt32 ("screenbitdepth");
					Set (HaveScreenBitDepth);
				}

				return screenBitDepth;
			}
		}

		int screenCharactersHeight;
		public virtual int ScreenCharactersHeight {
			get {
				if (!Get (HaveScreenCharactersHeight)) {
					screenCharactersHeight = ReadInt32 ("screencharactersheight");
					Set (HaveScreenCharactersHeight);
				}

				return screenCharactersHeight;
			}
		}

		int screenCharactersWidth;
		public virtual int ScreenCharactersWidth {
			get {
				if (!Get (HaveScreenCharactersWidth)) {
					screenCharactersWidth = ReadInt32 ("screencharacterswidth");
					Set (HaveScreenCharactersWidth);
				}

				return screenCharactersWidth;
			}
		}

		int screenPixelsHeight;
		public virtual int ScreenPixelsHeight {
			get {
				if (!Get (HaveScreenPixelsHeight)) {
					screenPixelsHeight = ReadInt32 ("screenpixelsheight");
					Set (HaveScreenPixelsHeight);
				}

				return screenPixelsHeight;
			}
		}

		int screenPixelsWidth;
		public virtual int ScreenPixelsWidth {
			get {
				if (!Get (HaveScreenPixelsWidth)) {
					screenPixelsWidth = ReadInt32 ("screenpixelswidth");
					Set (HaveScreenPixelsWidth);
				}

				return screenPixelsWidth;
			}
		}

		bool supportsAccesskeyAttribute;
		public virtual bool SupportsAccesskeyAttribute {
			get {
				if (!Get (HaveSupportsAccesskeyAttribute)) {
					supportsAccesskeyAttribute = ReadBoolean ("supportsaccesskeyattribute");
					Set (HaveSupportsAccesskeyAttribute);
				}

				return supportsAccesskeyAttribute;
			}
		}

		bool supportsBodyColor;
		public virtual bool SupportsBodyColor {
			get {
				if (!Get (HaveSupportsBodyColor)) {
					supportsBodyColor = ReadBoolean ("supportsbodycolor");
					Set (HaveSupportsBodyColor);
				}

				return supportsBodyColor;
			}
		}

		bool supportsBold;
		public virtual bool SupportsBold {
			get {
				if (!Get (HaveSupportsBold)) {
					supportsBold = ReadBoolean ("supportsbold");
					Set (HaveSupportsBold);
				}

				return supportsBold;
			}
		}

		bool supportsCacheControlMetaTag;
		public virtual bool SupportsCacheControlMetaTag {
			get {
				if (!Get (HaveSupportsCacheControlMetaTag)) {
					supportsCacheControlMetaTag = ReadBoolean ("supportscachecontrolmetatag");
					Set (HaveSupportsCacheControlMetaTag);
				}

				return supportsCacheControlMetaTag;
			}
		}

		bool supportsCallback;
		public virtual bool SupportsCallback {
			get {
				if (!Get (HaveSupportsCallback)) {
					supportsCallback = ReadBoolean ("supportscallback");
					Set (HaveSupportsCallback);
				}

				return supportsCallback;
			}
		}

		bool supportsCss;
		public virtual bool SupportsCss {
			get {
				if (!Get (HaveSupportsCss)) {
					supportsCss = ReadBoolean ("supportscss");
					Set (HaveSupportsCss);
				}

				return supportsCss;
			}
		}

		bool supportsDivAlign;
		public virtual bool SupportsDivAlign {
			get {
				if (!Get (HaveSupportsDivAlign)) {
					supportsDivAlign = ReadBoolean ("supportsdivalign");
					Set (HaveSupportsDivAlign);
				}

				return supportsDivAlign;
			}
		}

		bool supportsDivNoWrap;
		public virtual bool SupportsDivNoWrap {
			get {
				if (!Get (HaveSupportsDivNoWrap)) {
					supportsDivNoWrap = ReadBoolean ("supportsdivnowrap");
					Set (HaveRequiresDBCSCharacter);
				}

				return supportsDivNoWrap;
			}
		}

		bool supportsEmptyStringInCookieValue;
		public virtual bool SupportsEmptyStringInCookieValue {
			get {
				if (!Get (HaveSupportsEmptyStringInCookieValue)) {
					supportsEmptyStringInCookieValue = ReadBoolean ("supportsemptystringincookievalue");
					Set (HaveSupportsEmptyStringInCookieValue);
				}

				return supportsEmptyStringInCookieValue;
			}
		}

		bool supportsFontColor;
		public virtual bool SupportsFontColor {
			get {
				if (!Get (HaveSupportsFontColor)) {
					supportsFontColor = ReadBoolean ("supportsfontcolor");
					Set (HaveSupportsFontColor);
				}

				return supportsFontColor;
			}
		}

		bool supportsFontName;
		public virtual bool SupportsFontName {
			get {
				if (!Get (HaveSupportsFontName)) {
					supportsFontName = ReadBoolean ("supportsfontname");
					Set (HaveSupportsFontName);
				}

				return supportsFontName;
			}
		}

		bool supportsFontSize;
		public virtual bool SupportsFontSize {
			get {
				if (!Get (HaveSupportsFontSize)) {
					supportsFontSize = ReadBoolean ("supportsfontsize");
					Set (HaveSupportsFontSize);
				}

				return supportsFontSize;
			}
		}

		bool supportsImageSubmit;
		public virtual bool SupportsImageSubmit {
			get {
				if (!Get (HaveSupportsImageSubmit)) {
					supportsImageSubmit = ReadBoolean ("supportsimagesubmit");
					Set (HaveSupportsImageSubmit);
				}

				return supportsImageSubmit;
			}
		}

		bool supportsIModeSymbols;
		public virtual bool SupportsIModeSymbols {
			get {
				if (!Get (HaveSupportsIModeSymbols)) {
					supportsIModeSymbols = ReadBoolean ("supportsimodesymbols");
					Set (HaveSupportsIModeSymbols);
				}

				return supportsIModeSymbols;
			}
		}

		bool supportsInputIStyle;
		public virtual bool SupportsInputIStyle {
			get {
				if (!Get (HaveSupportsInputIStyle)) {
					supportsInputIStyle = ReadBoolean ("supportsinputistyle");
					Set (HaveSupportsInputIStyle);
				}

				return supportsInputIStyle;
			}
		}

		bool supportsInputMode;
		public virtual bool SupportsInputMode {
			get {
				if (!Get (HaveSupportsInputMode)) {
					supportsInputMode = ReadBoolean ("supportsinputmode");
					Set (HaveSupportsInputMode);
				}

				return supportsInputMode;
			}
		}

		bool supportsItalic;
		public virtual bool SupportsItalic {
			get {
				if (!Get (HaveSupportsItalic)) {
					supportsItalic = ReadBoolean ("supportsitalic");
					Set (HaveSupportsItalic);
				}

				return supportsItalic;
			}
		}

		bool supportsJPhoneMultiMediaAttributes;
		public virtual bool SupportsJPhoneMultiMediaAttributes {
			get {
				if (!Get (HaveSupportsJPhoneMultiMediaAttributes)) {
					supportsJPhoneMultiMediaAttributes = ReadBoolean ("supportsjphonemultimediaattributes");
					Set (HaveSupportsJPhoneMultiMediaAttributes);
				}

				return supportsJPhoneMultiMediaAttributes;
			}
		}

		bool supportsJPhoneSymbols;
		public virtual bool SupportsJPhoneSymbols {
			get {
				if (!Get (HaveSupportsJPhoneSymbols)) {
					supportsJPhoneSymbols = ReadBoolean ("supportsjphonesymbols");
					Set (HaveSupportsJPhoneSymbols);
				}

				return supportsJPhoneSymbols;
			}
		}

		bool supportsQueryStringInFormAction;
		public virtual bool SupportsQueryStringInFormAction {
			get {
				if (!Get (HaveSupportsQueryStringInFormAction)) {
					supportsQueryStringInFormAction = ReadBoolean ("supportsquerystringinformaction");
					Set (HaveSupportsQueryStringInFormAction);
				}

				return supportsQueryStringInFormAction;
			}
		}

		bool supportsRedirectWithCookie;
		public virtual bool SupportsRedirectWithCookie {
			get {
				if (!Get (HaveSupportsRedirectWithCookie)) {
					supportsRedirectWithCookie = ReadBoolean ("supportsredirectwithcookie");
					Set (HaveSupportsRedirectWithCookie);
				}

				return supportsRedirectWithCookie;
			}
		}

		bool supportsSelectMultiple;
		public virtual bool SupportsSelectMultiple {
			get {
				if (!Get (HaveSupportsSelectMultiple)) {
					supportsSelectMultiple = ReadBoolean ("supportsselectmultiple");
					Set (HaveSupportsSelectMultiple);
				}

				return supportsSelectMultiple;
			}
		}

		bool supportsUncheck;
		public virtual bool SupportsUncheck {
			get {
				if (!Get (HaveSupportsUncheck)) {
					supportsUncheck = ReadBoolean ("supportsuncheck");
					Set (HaveSupportsUncheck);
				}

				return supportsUncheck;
			}
		}

		bool supportsXmlHttp;
		public virtual bool SupportsXmlHttp {
			get {
				if (!Get (HaveSupportsXmlHttp)) {
					supportsXmlHttp = ReadBoolean ("supportsxmlhttp");
					Set (HaveSupportsXmlHttp);
				}

				return supportsXmlHttp;
			}
		}

		bool useOptimizedCacheKey;
		public bool UseOptimizedCacheKey {
			get {
				if (!Get (HaveUseOptimizedCacheKey)) {
					useOptimizedCacheKey = ReadBoolean ("useoptimizedcachekey");
					Set (HaveUseOptimizedCacheKey);
				}

				return useOptimizedCacheKey;
			}
		}
		
#if NET_4_0
		static HttpCapabilitiesProvider _provider = new HttpCapabilitiesDefaultProvider();
		public static HttpCapabilitiesProvider BrowserCapabilitiesProvider { 
			get { return _provider; }
			set { _provider = value; }
		}
#endif
	}
}

