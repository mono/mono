/**
 * Project   : Mono
 * Namespace : System.Web.Mobile
 * Class     : MobileCapabilities
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.Mobile
{
	public class MobileCapabilities : HttpBrowserCapabilities
	{
		internal delegate void
		       EvaluateCapabilitiesHandler(MobileCapabilities capabilities,
		                                    string evaluationParam);

		private bool canCombineFormsInDeck = false;

		public MobileCapabilities()
		{
		}

		public bool CanCombineFormsInDeck
		{
			get
			{
				if(!this.canCombineFormsInDeck)
				{
					if(this["CanCombineFormsInDeck"] != null)
					{
						this.canCombineFormsInDeck =
						        Convert.ToBoolean(this["CanCombineFormsInDeck"]);
					}
				}
				return this.canCombineFormsInDeck;
			}
		}

		private bool canInitiateVoiceCall = false;

		public bool CanInitiateVoiceCall
		{
			get
			{
				if(!this.canInitiateVoiceCall)
				{
					if(this["CanInitiateVoiceCall"] != null)
					{
						this.canInitiateVoiceCall =
						     Convert.ToBoolean(this["CanInitiateVoiceCall"]);
					}
				}
				return this.canInitiateVoiceCall;
			}
		}

		private bool canRenderAfterInputOrSelectElement = false;

		public bool CanRenderAfterInputOrSelectElement
		{
			get
			{
				if(!this.canRenderAfterInputOrSelectElement)
				{
					if(this["CanRenderAfterInputOrSelectElement"] != null)
					{
						this.canRenderAfterInputOrSelectElement =
						     Convert.ToBoolean(this["CanRenderAfterInputOrSelectElement"]);
					}
				}
				return this.canRenderAfterInputOrSelectElement;
			}
		}

		private bool canRenderEmptySelects = false;

		public bool CanRenderEmptySelects
		{
			get
			{
				if(!this.canRenderEmptySelects)
				{
					if(this["CanRenderEmptySelects"] != null)
					{
						this.canRenderEmptySelects =
						     Convert.ToBoolean(this["CanRenderEmptySelects"]);
					}
				}
				return this.canRenderEmptySelects;
			}
		}

		private bool canRenderInputAndSelectElementsTogether = false;

		public bool CanRenderInputAndSelectElementsTogether
		{
			get
			{
				if(!this.canRenderInputAndSelectElementsTogether)
				{
					if(this["CanRenderInputAndSelectElementsTogether"] != null)
					{
						this.canRenderInputAndSelectElementsTogether =
						     Convert.ToBoolean(this["CanRenderInputAndSelectElementsTogether"]);
					}
				}
				return this.canRenderInputAndSelectElementsTogether;
			}
		}

		private bool canRenderMixedSelects = false;

		public bool CanRenderMixedSelects
		{
			get
			{
				if(!this.canRenderMixedSelects)
				{
					if(this["CanRenderMixedSelects"] != null)
					{
						this.canRenderMixedSelects =
						     Convert.ToBoolean(this["CanRenderMixedSelects"]);
					}
				}
				return this.canRenderMixedSelects;
			}
		}

		private bool canRenderOneventAndPrevElementsTogether = false;

		public bool CanRenderOneventAndPrevElementsTogether
		{
			get
			{
				if(canRenderOneventAndPrevElementsTogether == false)
				{
					if(this["CanRenderOneventAndPrevElementsTogether"] != null)
					{
						this.canRenderOneventAndPrevElementsTogether =
						     Convert.ToBoolean(this["CanRenderOneventAndPrevElementsTogether"]);
					}
				}
				return canRenderOneventAndPrevElementsTogether;
			}
		}

		private bool canRenderPostBackCards = false;

		public bool CanRenderPostBackCards
		{
			get
			{
				if(canRenderPostBackCards == false)
				{
					if(this["CanRenderPostBackCards"] != null)
					{
						this.canRenderPostBackCards =
						     Convert.ToBoolean(this["CanRenderPostBackCards"]);
					}
				}
				return canRenderPostBackCards;
			}
		}

		private bool canRenderSetvarZeroWithMultiSelectionList = false;

		public bool CanRenderSetvarZeroWithMultiSelectionList
		{
			get
			{
				if(canRenderSetvarZeroWithMultiSelectionList == false)
				{
					if(this["CanRenderSetvarZeroWithMultiSelectionList"] != null)
					{
						this.canRenderSetvarZeroWithMultiSelectionList =
						     Convert.ToBoolean(this["CanRenderSetvarZeroWithMultiSelectionList"]);
					}
				}
				return canRenderSetvarZeroWithMultiSelectionList;
			}
		}

		private bool canSendMail = false;

		public bool CanSendMail
		{
			get
			{
				if(canSendMail == false)
				{
					if(this["CanSendMail"] != null)
					{
						this.canSendMail =
						     Convert.ToBoolean(this["CanSendMail"]);
					}
				}
				return canSendMail;
			}
		}

		private string gatewayVersion = String.Empty;

		public string GatewayVersion
		{
			get
			{
				if(gatewayVersion == String.Empty)
				{
					if(this["GatewayVersion"] != null)
					{
						this.gatewayVersion =
						     this["GatewayVersion"];
					}
				}
				return gatewayVersion;
			}
		}

		private bool hasBackButton = false;

		public bool HasBackButton
		{
			get
			{
				if(hasBackButton == false)
				{
					if(this["HasBackButton"] != null)
					{
						this.hasBackButton =
						     Convert.ToBoolean(this["HasBackButton"]);
					}
				}
				return hasBackButton;
			}
		}

		private bool hidesRightAlignedMultiselectScrollbars = false;

		public bool HidesRightAlignedMultiselectScrollbars
		{
			get
			{
				if(hidesRightAlignedMultiselectScrollbars == false)
				{
					if(this["HidesRightAlignedMultiselectScrollbars"] != null)
					{
						this.hidesRightAlignedMultiselectScrollbars =
						     Convert.ToBoolean(this["HidesRightAlignedMultiselectScrollbars"]);
					}
				}
				return hidesRightAlignedMultiselectScrollbars;
			}
		}

		private string inputType = String.Empty;

		public string InputType
		{
			get
			{
				if(inputType == String.Empty)
				{
					if(this["InputType"] != null)
					{
						this.inputType =
						     this["InputType"];
					}
				}
				return inputType;
			}
		}

		private bool isColor = false;

		public bool IsColor
		{
			get
			{
				if(isColor == false)
				{
					if(this["IsColor"] != null)
					{
						this.isColor =
						     Convert.ToBoolean(this["IsColor"]);
					}
				}
				return isColor;
			}
		}

		private bool isMobileDevice = false;

		public bool IsMobileDevice
		{
			get
			{
				if(isMobileDevice == false)
				{
					if(this["IsMobileDevice"] != null)
					{
						this.isMobileDevice =
						     Convert.ToBoolean(this["IsMobileDevice"]);
					}
				}
				return isMobileDevice;
			}
		}

		private string mobileDeviceManufacturer = String.Empty;

		public string MobileDeviceManufacturer
		{
			get
			{
				if(mobileDeviceManufacturer == String.Empty)
				{
					if(this["MobileDeviceManufacturer"] != null)
					{
						this.mobileDeviceManufacturer =
						     this["MobileDeviceManufacturer"];
					}
				}
				return mobileDeviceManufacturer;
			}
		}

		private string mobileDeviceModel = String.Empty;

		public string MobileDeviceModel
		{
			get
			{
				if(mobileDeviceModel == String.Empty)
				{
					if(this["MobileDeviceModel"] != null)
					{
						this.mobileDeviceModel =
						     this["MobileDeviceModel"];
					}
				}
				return mobileDeviceModel;
			}
		}

		private string preferredImageMime = String.Empty;

		public string PreferredImageMime
		{
			get
			{
				if(preferredImageMime == String.Empty)
				{
					if(this["PreferredImageMime"] != null)
					{
						this.preferredImageMime =
						     this["PreferredImageMime"];
					}
				}
				return preferredImageMime;
			}
		}

		private string preferredRenderingMime = String.Empty;

		public string PreferredRenderingMime
		{
			get
			{
				if(preferredRenderingMime == String.Empty)
				{
					if(this["PreferredRenderingMime"] != null)
					{
						this.preferredRenderingMime =
						     this["PreferredRenderingMime"];
					}
				}
				return preferredRenderingMime;
			}
		}

		private string preferredRenderingType = String.Empty;

		public string PreferredRenderingType
		{
			get
			{
				if(preferredRenderingType == String.Empty)
				{
					if(this["PreferredRenderingType"] != null)
					{
						this.preferredRenderingType =
						     this["PreferredRenderingType"];
					}
				}
				return preferredRenderingType;
			}
		}

		private bool rendersBreakBeforeWmlSelectAndInput = false;

		public bool RendersBreakBeforeWmlSelectAndInput
		{
			get
			{
				if(rendersBreakBeforeWmlSelectAndInput == false)
				{
					if(this["RendersBreakBeforeWmlSelectAndInput"] != null)
					{
						this.rendersBreakBeforeWmlSelectAndInput =
						     Convert.ToBoolean(this["RendersBreakBeforeWmlSelectAndInput"]);
					}
				}
				return rendersBreakBeforeWmlSelectAndInput;
			}
		}

		private bool rendersBreaksAfterHtmlLists = false;

		public bool RendersBreaksAfterHtmlLists
		{
			get
			{
				if(rendersBreaksAfterHtmlLists == false)
				{
					if(this["RendersBreaksAfterHtmlLists"] != null)
					{
						this.rendersBreaksAfterHtmlLists =
						     Convert.ToBoolean(this["RendersBreaksAfterHtmlLists"]);
					}
				}
				return rendersBreaksAfterHtmlLists;
			}
		}

		private bool rendersBreaksAfterWmlAnchor = false;

		public bool RendersBreaksAfterWmlAnchor
		{
			get
			{
				if(rendersBreaksAfterWmlAnchor == false)
				{
					if(this["RendersBreaksAfterWmlAnchor"] != null)
					{
						this.rendersBreaksAfterWmlAnchor =
						     Convert.ToBoolean(this["RendersBreaksAfterWmlAnchor"]);
					}
				}
				return rendersBreaksAfterWmlAnchor;
			}
		}

		private bool rendersBreaksAfterWmlInput = false;

		public bool RendersBreaksAfterWmlInput
		{
			get
			{
				if(rendersBreaksAfterWmlInput == false)
				{
					if(this["RendersBreaksAfterWmlInput"] != null)
					{
						this.rendersBreaksAfterWmlInput =
						     Convert.ToBoolean(this["RendersBreaksAfterWmlInput"]);
					}
				}
				return rendersBreaksAfterWmlInput;
			}
		}

		private bool rendersWmlDoAcceptsInline = false;

		public bool RendersWmlDoAcceptsInline
		{
			get
			{
				if(rendersWmlDoAcceptsInline == false)
				{
					if(this["RendersWmlDoAcceptsInline"] != null)
					{
						this.rendersWmlDoAcceptsInline =
						     Convert.ToBoolean(this["RendersWmlDoAcceptsInline"]);
					}
				}
				return rendersWmlDoAcceptsInline;
			}
		}

		private bool rendersWmlSelectsAsMenuCards = false;

		public bool RendersWmlSelectsAsMenuCards
		{
			get
			{
				if(rendersWmlSelectsAsMenuCards == false)
				{
					if(this["RendersWmlSelectsAsMenuCards"] != null)
					{
						this.rendersWmlSelectsAsMenuCards =
						     Convert.ToBoolean(this["RendersWmlSelectsAsMenuCards"]);
					}
				}
				return rendersWmlSelectsAsMenuCards;
			}
		}

		private string requiredMetaTagNameValue = String.Empty;

		public string RequiredMetaTagNameValue
		{
			get
			{
				if(requiredMetaTagNameValue == String.Empty)
				{
					if(this["RequiredMetaTagNameValue"] != null)
					{
						this.requiredMetaTagNameValue =
						     this["RequiredMetaTagNameValue"];
					}
				}
				return requiredMetaTagNameValue;
			}
		}

		private bool requiresAttributeColonSubstitution = false;

		public bool RequiresAttributeColonSubstitution
		{
			get
			{
				if(requiresAttributeColonSubstitution == false)
				{
					if(this["RequiresAttributeColonSubstitution"] != null)
					{
						this.requiresAttributeColonSubstitution =
						     Convert.ToBoolean(this["RequiresAttributeColonSubstitution"]);
					}
				}
				return requiresAttributeColonSubstitution;
			}
		}

		private bool requiresContentTypeMetaTag = false;

		public bool RequiresContentTypeMetaTag
		{
			get
			{
				if(requiresContentTypeMetaTag == false)
				{
					if(this["RequiresContentTypeMetaTag"] != null)
					{
						this.requiresContentTypeMetaTag =
						     Convert.ToBoolean(this["RequiresContentTypeMetaTag"]);
					}
				}
				return requiresContentTypeMetaTag;
			}
		}

		private bool requiresDBCSCharacter = false;

		public bool RequiresDBCSCharacter
		{
			get
			{
				if(requiresDBCSCharacter == false)
				{
					if(this["RequiresDBCSCharacter"] != null)
					{
						this.requiresDBCSCharacter =
						     Convert.ToBoolean(this["RequiresDBCSCharacter"]);
					}
				}
				return requiresDBCSCharacter;
			}
		}

		private bool requiresHtmlAdaptiveErrorReporting = false;

		public bool RequiresHtmlAdaptiveErrorReporting
		{
			get
			{
				if(requiresHtmlAdaptiveErrorReporting == false)
				{
					if(this["RequiresHtmlAdaptiveErrorReporting"] != null)
					{
						this.requiresHtmlAdaptiveErrorReporting =
						     Convert.ToBoolean(this["RequiresHtmlAdaptiveErrorReporting"]);
					}
				}
				return requiresHtmlAdaptiveErrorReporting;
			}
		}

		private bool requiresLeadingPageBreak = false;

		public bool RequiresLeadingPageBreak
		{
			get
			{
				if(requiresLeadingPageBreak == false)
				{
					if(this["RequiresLeadingPageBreak"] != null)
					{
						this.requiresLeadingPageBreak =
						     Convert.ToBoolean(this["RequiresLeadingPageBreak"]);
					}
				}
				return requiresLeadingPageBreak;
			}
		}

		private bool requiresNoBreakInFormatting = false;

		public bool RequiresNoBreakInFormatting
		{
			get
			{
				if(requiresNoBreakInFormatting == false)
				{
					if(this["RequiresNoBreakInFormatting"] != null)
					{
						this.requiresNoBreakInFormatting =
						     Convert.ToBoolean(this["RequiresNoBreakInFormatting"]);
					}
				}
				return requiresNoBreakInFormatting;
			}
		}

		private bool requiresOutputOptimization = false;

		public bool RequiresOutputOptimization
		{
			get
			{
				if(requiresOutputOptimization == false)
				{
					if(this["RequiresOutputOptimization"] != null)
					{
						this.requiresOutputOptimization =
						     Convert.ToBoolean(this["RequiresOutputOptimization"]);
					}
				}
				return requiresOutputOptimization;
			}
		}

		private bool requiresPhoneNumbersAsPlainText = false;

		public bool RequiresPhoneNumbersAsPlainText
		{
			get
			{
				if(requiresPhoneNumbersAsPlainText == false)
				{
					if(this["RequiresPhoneNumbersAsPlainText"] != null)
					{
						this.requiresPhoneNumbersAsPlainText =
						     Convert.ToBoolean(this["RequiresPhoneNumbersAsPlainText"]);
					}
				}
				return requiresPhoneNumbersAsPlainText;
			}
		}

		private bool requiresSpecialViewStateEncoding = false;

		public bool RequiresSpecialViewStateEncoding
		{
			get
			{
				if(requiresSpecialViewStateEncoding == false)
				{
					if(this["RequiresSpecialViewStateEncoding"] != null)
					{
						this.requiresSpecialViewStateEncoding =
						     Convert.ToBoolean(this["RequiresSpecialViewStateEncoding"]);
					}
				}
				return requiresSpecialViewStateEncoding;
			}
		}

		private bool requiresUniqueFilePathSuffix = false;

		public bool RequiresUniqueFilePathSuffix
		{
			get
			{
				if(requiresUniqueFilePathSuffix == false)
				{
					if(this["RequiresUniqueFilePathSuffix"] != null)
					{
						this.requiresUniqueFilePathSuffix =
						     Convert.ToBoolean(this["RequiresUniqueFilePathSuffix"]);
					}
				}
				return requiresUniqueFilePathSuffix;
			}
		}

		private bool requiresUniqueHtmlCheckboxNames = false;

		public bool RequiresUniqueHtmlCheckboxNames
		{
			get
			{
				if(requiresUniqueHtmlCheckboxNames == false)
				{
					if(this["RequiresUniqueHtmlCheckboxNames"] != null)
					{
						this.requiresUniqueHtmlCheckboxNames =
						     Convert.ToBoolean(this["RequiresUniqueHtmlCheckboxNames"]);
					}
				}
				return requiresUniqueHtmlCheckboxNames;
			}
		}

		private bool requiresUrlEncodedPostfieldValues = false;

		public bool RequiresUrlEncodedPostfieldValues
		{
			get
			{
				if(requiresUrlEncodedPostfieldValues == false)
				{
					if(this["RequiresUrlEncodedPostfieldValues"] != null)
					{
						this.requiresUrlEncodedPostfieldValues =
						     Convert.ToBoolean(this["RequiresUrlEncodedPostfieldValues"]);
					}
				}
				return requiresUrlEncodedPostfieldValues;
			}
		}

		private bool supportsAccesskeyAttribute = false;

		public bool SupportsAccesskeyAttribute
		{
			get
			{
				if(supportsAccesskeyAttribute == false)
				{
					if(this["SupportsAccesskeyAttribute"] != null)
					{
						this.supportsAccesskeyAttribute =
						     Convert.ToBoolean(this["SupportsAccesskeyAttribute"]);
					}
				}
				return supportsAccesskeyAttribute;
			}
		}

		private bool supportsBodyColor = false;

		public bool SupportsBodyColor
		{
			get
			{
				if(supportsBodyColor == false)
				{
					if(this["SupportsBodyColor"] != null)
					{
						this.supportsBodyColor =
						     Convert.ToBoolean(this["SupportsBodyColor"]);
					}
				}
				return supportsBodyColor;
			}
		}

		private bool supportsBold = false;

		public bool SupportsBold
		{
			get
			{
				if(supportsBold == false)
				{
					if(this["SupportsBold"] != null)
					{
						this.supportsBold =
						     Convert.ToBoolean(this["SupportsBold"]);
					}
				}
				return supportsBold;
			}
		}

		private bool supportsCacheControlMetaTag = false;

		public bool SupportsCacheControlMetaTag
		{
			get
			{
				if(supportsCacheControlMetaTag == false)
				{
					if(this["SupportsCacheControlMetaTag"] != null)
					{
						this.supportsCacheControlMetaTag =
						     Convert.ToBoolean(this["SupportsCacheControlMetaTag"]);
					}
				}
				return supportsCacheControlMetaTag;
			}
		}

		private bool supportsCss = false;

		public bool SupportsCss
		{
			get
			{
				if(supportsCss == false)
				{
					if(this["SupportsCss"] != null)
					{
						this.supportsCss =
						     Convert.ToBoolean(this["SupportsCss"]);
					}
				}
				return supportsCss;
			}
		}

		private bool supportsDivAlign = false;

		public bool SupportsDivAlign
		{
			get
			{
				if(supportsDivAlign == false)
				{
					if(this["SupportsDivAlign"] != null)
					{
						this.supportsDivAlign =
						     Convert.ToBoolean(this["SupportsDivAlign"]);
					}
				}
				return supportsDivAlign;
			}
		}

		private bool supportsDivNoWrap = false;

		public bool SupportsDivNoWrap
		{
			get
			{
				if(supportsDivNoWrap == false)
				{
					if(this["SupportsDivNoWrap"] != null)
					{
						this.supportsDivNoWrap =
						     Convert.ToBoolean(this["SupportsDivNoWrap"]);
					}
				}
				return supportsDivNoWrap;
			}
		}

		private bool supportsFontColor = false;

		public bool SupportsFontColor
		{
			get
			{
				if(supportsFontColor == false)
				{
					if(this["SupportsFontColor"] != null)
					{
						this.supportsFontColor =
						     Convert.ToBoolean(this["SupportsFontColor"]);
					}
				}
				return supportsFontColor;
			}
		}

		private bool supportsFontName = false;

		public bool SupportsFontName
		{
			get
			{
				if(supportsFontName == false)
				{
					if(this["SupportsFontName"] != null)
					{
						this.supportsFontName =
						     Convert.ToBoolean(this["SupportsFontName"]);
					}
				}
				return supportsFontName;
			}
		}

		private bool supportsFontSize = false;

		public bool SupportsFontSize
		{
			get
			{
				if(supportsFontSize == false)
				{
					if(this["SupportsFontSize"] != null)
					{
						this.supportsFontSize =
						     Convert.ToBoolean(this["SupportsFontSize"]);
					}
				}
				return supportsFontSize;
			}
		}

		private bool supportsModeSymbols = false;

		public bool SupportsModeSymbols
		{
			get
			{
				if(supportsModeSymbols == false)
				{
					if(this["SupportsModeSymbols"] != null)
					{
						this.supportsModeSymbols =
						     Convert.ToBoolean(this["SupportsModeSymbols"]);
					}
				}
				return supportsModeSymbols;
			}
		}

		private bool supportsImageSubmit = false;

		public bool SupportsImageSubmit
		{
			get
			{
				if(supportsImageSubmit == false)
				{
					if(this["SupportsImageSubmit"] != null)
					{
						this.supportsImageSubmit =
						     Convert.ToBoolean(this["SupportsImageSubmit"]);
					}
				}
				return supportsImageSubmit;
			}
		}

		private bool supportsInputStyle = false;

		public bool SupportsInputStyle
		{
			get
			{
				if(supportsInputStyle == false)
				{
					if(this["SupportsInputStyle"] != null)
					{
						this.supportsInputStyle =
						     Convert.ToBoolean(this["SupportsInputStyle"]);
					}
				}
				return supportsInputStyle;
			}
		}

		private bool supportsInputMode = false;

		public bool SupportsInputMode
		{
			get
			{
				if(supportsInputMode == false)
				{
					if(this["SupportsInputMode"] != null)
					{
						this.supportsInputMode =
						     Convert.ToBoolean(this["SupportsInputMode"]);
					}
				}
				return supportsInputMode;
			}
		}

		private bool supportsItalic = false;

		public bool SupportsItalic
		{
			get
			{
				if(supportsItalic == false)
				{
					if(this["SupportsItalic"] != null)
					{
						this.supportsItalic =
						     Convert.ToBoolean(this["SupportsItalic"]);
					}
				}
				return supportsItalic;
			}
		}

		private bool supportsJPhoneMultiMediaAttributes = false;

		public bool SupportsJPhoneMultiMediaAttributes
		{
			get
			{
				if(supportsJPhoneMultiMediaAttributes == false)
				{
					if(this["SupportsJPhoneMultiMediaAttributes"] != null)
					{
						this.supportsJPhoneMultiMediaAttributes =
						     Convert.ToBoolean(this["SupportsJPhoneMultiMediaAttributes"]);
					}
				}
				return supportsJPhoneMultiMediaAttributes;
			}
		}

		private bool supportsJPhoneSymbols = false;

		public bool SupportsJPhoneSymbols
		{
			get
			{
				if(supportsJPhoneSymbols == false)
				{
					if(this["SupportsJPhoneSymbols"] != null)
					{
						this.supportsJPhoneSymbols =
						     Convert.ToBoolean(this["SupportsJPhoneSymbols"]);
					}
				}
				return supportsJPhoneSymbols;
			}
		}

		private bool supportsQueryStringInFormAction = false;

		public bool SupportsQueryStringInFormAction
		{
			get
			{
				if(supportsQueryStringInFormAction == false)
				{
					if(this["SupportsQueryStringInFormAction"] != null)
					{
						this.supportsQueryStringInFormAction =
						     Convert.ToBoolean(this["SupportsQueryStringInFormAction"]);
					}
				}
				return supportsQueryStringInFormAction;
			}
		}

		private bool supportsRedirectWithCookie = false;

		public bool SupportsRedirectWithCookie
		{
			get
			{
				if(supportsRedirectWithCookie == false)
				{
					if(this["SupportsRedirectWithCookie"] != null)
					{
						this.supportsRedirectWithCookie =
						     Convert.ToBoolean(this["SupportsRedirectWithCookie"]);
					}
				}
				return supportsRedirectWithCookie;
			}
		}

		private bool supportsSelectMultiple = false;

		public bool SupportsSelectMultiple
		{
			get
			{
				if(supportsSelectMultiple == false)
				{
					if(this["SupportsSelectMultiple"] != null)
					{
						this.supportsSelectMultiple =
						     Convert.ToBoolean(this["SupportsSelectMultiple"]);
					}
				}
				return supportsSelectMultiple;
			}
		}

		private bool supportsUncheck = false;

		public bool SupportsUncheck
		{
			get
			{
				if(supportsUncheck == false)
				{
					if(this["SupportsUncheck"] != null)
					{
						this.supportsUncheck =
						     Convert.ToBoolean(this["SupportsUncheck"]);
					}
				}
				return supportsUncheck;
			}
		}

		private int gatewayMajorVersion = 0;

		public int GatewayMajorVersion
		{
			get
			{
				if(gatewayMajorVersion == 0)
				{
					if(this["GatewayMajorVersion"] != null)
					{
						this.gatewayMajorVersion =
						     Convert.ToInt32(this["GatewayMajorVersion"]);
					}
				}
				return gatewayMajorVersion;
			}
		}

		private int maximumRenderedPageSize = 0;

		public int MaximumRenderedPageSize
		{
			get
			{
				if(maximumRenderedPageSize == 0)
				{
					if(this["MaximumRenderedPageSize"] != null)
					{
						this.maximumRenderedPageSize =
						     Convert.ToInt32(this["MaximumRenderedPageSize"]);
					}
				}
				return maximumRenderedPageSize;
			}
		}

		private int maximumSoftkeyLabelLength = 0;

		public int MaximumSoftkeyLabelLength
		{
			get
			{
				if(maximumSoftkeyLabelLength == 0)
				{
					if(this["MaximumSoftkeyLabelLength"] != null)
					{
						this.maximumSoftkeyLabelLength =
						     Convert.ToInt32(this["MaximumSoftkeyLabelLength"]);
					}
				}
				return maximumSoftkeyLabelLength;
			}
		}

		private int numberOfSoftkeys = 0;

		public int NumberOfSoftkeys
		{
			get
			{
				if(numberOfSoftkeys == 0)
				{
					if(this["NumberOfSoftkeys"] != null)
					{
						this.numberOfSoftkeys =
						     Convert.ToInt32(this["NumberOfSoftkeys"]);
					}
				}
				return numberOfSoftkeys;
			}
		}

		private int screenBitDepth = 0;

		public int ScreenBitDepth
		{
			get
			{
				if(screenBitDepth == 0)
				{
					if(this["ScreenBitDepth"] != null)
					{
						this.screenBitDepth =
						     Convert.ToInt32(this["ScreenBitDepth"]);
					}
				}
				return screenBitDepth;
			}
		}

		private int screenCharactersHeight = 0;

		public int ScreenCharactersHeight
		{
			get
			{
				if(screenCharactersHeight == 0)
				{
					if(this["ScreenCharactersHeight"] != null)
					{
						this.screenCharactersHeight =
						     Convert.ToInt32(this["ScreenCharactersHeight"]);
					}
				}
				return screenCharactersHeight;
			}
		}

		private int screenCharactersWidth = 0;

		public int ScreenCharactersWidth
		{
			get
			{
				if(screenCharactersWidth == 0)
				{
					if(this["ScreenCharactersWidth"] != null)
					{
						this.screenCharactersWidth =
						     Convert.ToInt32(this["ScreenCharactersWidth"]);
					}
				}
				return screenCharactersWidth;
			}
		}

		private int screenPixelsHeight = 0;

		public int ScreenPixelsHeight
		{
			get
			{
				if(screenPixelsHeight == 0)
				{
					if(this["ScreenPixelsHeight"] != null)
					{
						this.screenPixelsHeight =
						     Convert.ToInt32(this["ScreenPixelsHeight"]);
					}
				}
				return screenPixelsHeight;
			}
		}

		private int screenPixelsWidth = 0;

		public int ScreenPixelsWidth
		{
			get
			{
				if(screenPixelsWidth == 0)
				{
					if(this["ScreenPixelsWidth"] != null)
					{
						this.screenPixelsWidth =
						     Convert.ToInt32(this["ScreenPixelsWidth"]);
					}
				}
				return screenPixelsWidth;
			}
		}

		private double gatewayMinorVersion = 0.0;

		public double GatewayMinorVersion
		{
			get
			{
				if(gatewayMinorVersion == 0.0)
				{
					if(this["GatewayMinorVersion"] != null)
					{
						this.gatewayMinorVersion =
						     Convert.ToDouble(this["GatewayMinorVersion"]);
					}
				}
				return gatewayMinorVersion;
			}
		}

		public bool HasCapability(string delegateName, string optParams)
		{
			if(delegateName == null || delegateName.Trim() == String.Empty)
			{
				throw new ArgumentException("MobCap_DelegateNameNoValue");
			}
			throw new NotImplementedException();
		}
	}
}
