//------------------------------------------------------------------------------
// <copyright file="MobileCapabilities.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Mobile
{
    using System.Web;
    using System.Collections;
    using System.Configuration;
    using System.Reflection;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Globalization;
    using System.Security.Permissions;

    /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileCapabilities : HttpBrowserCapabilities
    {
        internal delegate bool EvaluateCapabilitiesDelegate(MobileCapabilities capabilities,
            String evalParameter);

        private Hashtable _evaluatorResults = Hashtable.Synchronized(new Hashtable());

        private const String _kDeviceFiltersConfig = "system.web/deviceFilters";
        private static readonly object _staticLock = new object();

        [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
		[AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
		[ConfigurationPermission(SecurityAction.Assert, Unrestricted = true)]
		private DeviceFilterDictionary GetCurrentFilters()
        {
            object config = ConfigurationManager.GetSection(_kDeviceFiltersConfig);
            DeviceFiltersSection controlSection = config as DeviceFiltersSection;
            if (controlSection != null)
            {
                return controlSection.GetDeviceFilters();
            }
            return (DeviceFilterDictionary)config;
        }

        private bool HasComparisonEvaluator(String evaluatorName, out bool result)
        {
            result = false;
            String evaluator;
            String argument;

            DeviceFilterDictionary currentFilters = GetCurrentFilters();
            if(currentFilters == null)
            {
                return false;
            }

            if(!currentFilters.FindComparisonEvaluator(evaluatorName, out evaluator, out argument))
            {
                return false;
            }

            result = HasCapability(evaluator, argument);

            return true;
        }


        private bool HasDelegatedEvaluator(String evaluatorName, String parameter,
            out bool result)
        {
            result = false;
            EvaluateCapabilitiesDelegate evaluator;

            DeviceFilterDictionary currentFilters = GetCurrentFilters();
            if(currentFilters == null)
            {
                return false;
            }

            if(!currentFilters.FindDelegateEvaluator(evaluatorName, out evaluator))
            {
                return false;
            }

            result = evaluator(this, parameter);

            return true;
        }


        private bool HasItem(String evaluatorName, String parameter,
            out bool result)
        {
            result = false;
            String item;

            item = this[evaluatorName];
            if(item == null)
            {
                return false;
            }

            result = (item == parameter);
            return true;
        }


        private bool HasProperty(String evaluatorName, String parameter,
            out bool result)
        {
            result = false;
            PropertyDescriptor propertyDescriptor =
                TypeDescriptor.GetProperties(this)[evaluatorName];
            if(propertyDescriptor == null)
            {
                return false;
            }

            String propertyValue = propertyDescriptor.GetValue(this).ToString();
            bool invariantCultureIgnoreCase = (propertyDescriptor.PropertyType == typeof(bool) && parameter != null);
            StringComparison compareOption = invariantCultureIgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.CurrentCulture;
            result = (String.Equals(propertyValue, parameter, compareOption));
            return true;
        }


        private bool IsComparisonEvaluator(String evaluatorName)
        {
            DeviceFilterDictionary currentFilters = GetCurrentFilters();

            if(currentFilters == null)
            {
                return false;
            }
            else
            {
                return currentFilters.IsComparisonEvaluator(evaluatorName) &&
                    !currentFilters.IsDelegateEvaluator(evaluatorName);
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.HasCapability"]/*' />
        public bool HasCapability(String delegateName, String optionalParameter)
        {   
            bool result;
            bool resultFound;

            if(String.IsNullOrEmpty(delegateName))
            {
                throw new ArgumentException(SR.GetString(SR.MobCap_DelegateNameNoValue),
                                            "delegateName");
            }

            // Check for cached results

            DeviceFilterDictionary currentFilters = GetCurrentFilters();
            String hashKey = ((currentFilters == null) ? "null" : currentFilters.GetHashCode().ToString(CultureInfo.InvariantCulture))
                + delegateName;

            if(optionalParameter != null && !IsComparisonEvaluator(delegateName))
            {
                hashKey += optionalParameter;
            }

            if (_evaluatorResults.Contains(hashKey))
            {
                return (bool)_evaluatorResults[hashKey];
            }

            lock (_staticLock)
            {
                if (_evaluatorResults.Contains(hashKey))
                {
                    return (bool)_evaluatorResults[hashKey];
                }

                // Note: The fact that delegate evaluators are checked before comparison evaluators
                // determines the implementation of IsComparisonEvaluator above.

                resultFound = HasDelegatedEvaluator(delegateName, optionalParameter, out result);

                if (!resultFound)
                {
                    resultFound = HasComparisonEvaluator(delegateName, out result);

                    if (!resultFound)
                    {
                        resultFound = HasProperty(delegateName, optionalParameter, out result);

                        if (!resultFound)
                        {
                            resultFound = HasItem(delegateName, optionalParameter, out result);
                        }
                    }
                }

                if (resultFound)
                {
                    _evaluatorResults.Add(hashKey, result);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        "delegateName",
                        SR.GetString(SR.MobCap_CantFindCapability, delegateName));
                }

                return result;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.MobileDeviceManufacturer"]/*' />
/*        public virtual String MobileDeviceManufacturer
        {
            get
            {
                if(!_haveMobileDeviceManufacturer)
                {
                    _mobileDeviceManufacturer = this["mobileDeviceManufacturer"];
                    _haveMobileDeviceManufacturer = true;
                }
                return _mobileDeviceManufacturer;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.MobileDeviceModel"]/*' />
        public virtual String MobileDeviceModel
        {
            get
            {
                if(!_haveMobileDeviceModel)
                {
                    _mobileDeviceModel = this["mobileDeviceModel"];
                    _haveMobileDeviceModel = true;
                }
                return _mobileDeviceModel;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.GatewayVersion"]/*' />
        public virtual String GatewayVersion
        {
            get
            {
                if(!_haveGatewayVersion)
                {
                    _gatewayVersion = this["gatewayVersion"];
                    _haveGatewayVersion = true;
                }
                return _gatewayVersion;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.GatewayMajorVersion"]/*' />
        public virtual int GatewayMajorVersion
        {
            get
            {
                if(!_haveGatewayMajorVersion)
                {
                    _gatewayMajorVersion = Convert.ToInt32(this["gatewayMajorVersion"]);
                    _haveGatewayMajorVersion = true;
                }
                return _gatewayMajorVersion;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.GatewayMinorVersion"]/*' />
        public virtual double GatewayMinorVersion
        {
            get
            {
                if(!_haveGatewayMinorVersion)
                {
                    // The conversion below does not use Convert.ToDouble()  
                    // because it depends on the current locale.  So a german machine it would look for 
                    // a comma as a seperator "1,5" where all user-agent strings use english
                    // decimal points "1.5".  URT11176
                    // 
                    _gatewayMinorVersion = double.Parse(
                                        this["gatewayMinorVersion"], 
                                        NumberStyles.Float | NumberStyles.AllowDecimalPoint, 
                                        NumberFormatInfo.InvariantInfo);
                    _haveGatewayMinorVersion = true;
                }
                return _gatewayMinorVersion;
            }
        }

*/
        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredRenderingTypeHtml32"]/*' />
        public static readonly String PreferredRenderingTypeHtml32 = "html32";
        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredRenderingTypeWml11"]/*' />
        public static readonly String PreferredRenderingTypeWml11 = "wml11";
        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredRenderingTypeWml12"]/*' />
        public static readonly String PreferredRenderingTypeWml12 = "wml12";
        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredRenderingTypeChtml10"]/*' />
        public static readonly String PreferredRenderingTypeChtml10 = "chtml10";

/*
        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredRenderingType"]/*' />
        public virtual String PreferredRenderingType
        {
            get
            {
                if(!_havePreferredRenderingType)
                {
                    _preferredRenderingType = this["preferredRenderingType"];
                    _havePreferredRenderingType = true;
                }
                return _preferredRenderingType;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredRenderingMime"]/*' />
        public virtual String PreferredRenderingMime
        {
            get
            {
                if(!_havePreferredRenderingMime)
                {
                    _preferredRenderingMime = this["preferredRenderingMime"];
                    _havePreferredRenderingMime = true;
                }
                return _preferredRenderingMime;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.PreferredImageMime"]/*' />
        public virtual String PreferredImageMime
        {
            get
            {
                if(!_havePreferredImageMime)
                {
                    _preferredImageMime = this["preferredImageMime"];
                    _havePreferredImageMime = true;
                }
                return _preferredImageMime;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.ScreenCharactersWidth"]/*' />
        public virtual int ScreenCharactersWidth
        {
            get
            {
                if(!_haveScreenCharactersWidth)
                {
                    if(this["screenCharactersWidth"] == null)
                    {
                        // calculate from best partial information

                        int screenPixelsWidthToUse = 640;
                        int characterWidthToUse = 8;

                        if(this["screenPixelsWidth"] != null && this["characterWidth"] != null)
                        {
                            screenPixelsWidthToUse = Convert.ToInt32(this["screenPixelsWidth"]);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"]);
                        }
                        else if(this["screenPixelsWidth"] != null)
                        {
                            screenPixelsWidthToUse = Convert.ToInt32(this["screenPixelsWidth"]);
                            characterWidthToUse = Convert.ToInt32(this["defaultCharacterWidth"]);
                        }
                        else if(this["characterWidth"] != null)
                        {
                            screenPixelsWidthToUse = Convert.ToInt32(this["defaultScreenPixelsWidth"]);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"]);
                        }
                        else if(this["defaultScreenCharactersWidth"] != null)
                        {
                            screenPixelsWidthToUse = Convert.ToInt32(this["defaultScreenCharactersWidth"]);
                            characterWidthToUse = 1;
                        }

                        _screenCharactersWidth = screenPixelsWidthToUse / characterWidthToUse;
                    }
                    else
                    {
                        _screenCharactersWidth = Convert.ToInt32(this["screenCharactersWidth"]);
                    }
                    _haveScreenCharactersWidth = true;
                }
                return _screenCharactersWidth;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.ScreenCharactersHeight"]/*' />
        public virtual int ScreenCharactersHeight
        {
            get
            {
                if(!_haveScreenCharactersHeight)
                {
                    if(this["screenCharactersHeight"] == null)
                    {
                        // calculate from best partial information

                        int screenPixelHeightToUse = 480;
                        int characterHeightToUse = 12;

                        if(this["screenPixelsHeight"] != null && this["characterHeight"] != null)
                        {
                            screenPixelHeightToUse = Convert.ToInt32(this["screenPixelsHeight"]);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"]);
                        }
                        else if(this["screenPixelsHeight"] != null)
                        {
                            screenPixelHeightToUse = Convert.ToInt32(this["screenPixelsHeight"]);
                            characterHeightToUse = Convert.ToInt32(this["defaultCharacterHeight"]);
                        }
                        else if(this["characterHeight"] != null)
                        {
                            screenPixelHeightToUse = Convert.ToInt32(this["defaultScreenPixelsHeight"]);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"]);
                        }
                        else if(this["defaultScreenCharactersHeight"] != null)
                        {
                            screenPixelHeightToUse = Convert.ToInt32(this["defaultScreenCharactersHeight"]);
                            characterHeightToUse = 1;
                        }

                        _screenCharactersHeight = screenPixelHeightToUse / characterHeightToUse;
                    }
                    else
                    {
                        _screenCharactersHeight = Convert.ToInt32(this["screenCharactersHeight"]);
                    }
                    _haveScreenCharactersHeight = true;
                }
                return _screenCharactersHeight;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.ScreenPixelsWidth"]/*' />
        public virtual int ScreenPixelsWidth
        {
            get
            {
                if(!_haveScreenPixelsWidth)
                {
                    if(this["screenPixelsWidth"] == null)
                    {
                        // calculate from best partial information

                        int screenCharactersWidthToUse = 80;
                        int characterWidthToUse = 8;

                        if(this["screenCharactersWidth"] != null && this["characterWidth"] != null)
                        {
                            screenCharactersWidthToUse = Convert.ToInt32(this["screenCharactersWidth"]);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"]);
                        }
                        else if(this["screenCharactersWidth"] != null)
                        {
                            screenCharactersWidthToUse = Convert.ToInt32(this["screenCharactersWidth"]);
                            characterWidthToUse = Convert.ToInt32(this["defaultCharacterWidth"]);
                        }
                        else if(this["characterWidth"] != null)
                        {
                            screenCharactersWidthToUse = Convert.ToInt32(this["defaultScreenCharactersWidth"]);
                            characterWidthToUse = Convert.ToInt32(this["characterWidth"]);
                        }
                        else if(this["defaultScreenPixelsWidth"] != null)
                        {
                            screenCharactersWidthToUse = Convert.ToInt32(this["defaultScreenPixelsWidth"]);
                            characterWidthToUse = 1;
                        }

                        _screenPixelsWidth = screenCharactersWidthToUse * characterWidthToUse;
                    }
                    else
                    {
                        _screenPixelsWidth = Convert.ToInt32(this["screenPixelsWidth"]);
                    }
                    _haveScreenPixelsWidth = true;
                }
                return _screenPixelsWidth;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.ScreenPixelsHeight"]/*' />
        public virtual int ScreenPixelsHeight
        {
            get
            {
                if(!_haveScreenPixelsHeight)
                {
                    if(this["screenPixelsHeight"] == null)
                    {
                        int screenCharactersHeightToUse = 480 / 12;
                        int characterHeightToUse = 12;

                        if(this["screenCharactersHeight"] != null && this["characterHeight"] != null)
                        {
                            screenCharactersHeightToUse = Convert.ToInt32(this["screenCharactersHeight"]);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"]);
                        }
                        else if(this["screenCharactersHeight"] != null)
                        {
                            screenCharactersHeightToUse = Convert.ToInt32(this["screenCharactersHeight"]);
                            characterHeightToUse = Convert.ToInt32(this["defaultCharacterHeight"]);
                        }
                        else if(this["characterHeight"] != null)
                        {
                            screenCharactersHeightToUse = Convert.ToInt32(this["defaultScreenCharactersHeight"]);
                            characterHeightToUse = Convert.ToInt32(this["characterHeight"]);
                        }
                        else if(this["defaultScreenPixelsHeight"] != null)
                        {
                            screenCharactersHeightToUse = Convert.ToInt32(this["defaultScreenPixelsHeight"]);
                            characterHeightToUse = 1;
                        }

                        _screenPixelsHeight = screenCharactersHeightToUse * characterHeightToUse;
                    }
                    else
                    {
                        _screenPixelsHeight = Convert.ToInt32(this["screenPixelsHeight"]);
                    }
                    _haveScreenPixelsHeight = true;
                }
                return _screenPixelsHeight;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.ScreenBitDepth"]/*' />
        public virtual int ScreenBitDepth
        {
            get
            {
                if(!_haveScreenBitDepth)
                {
                    _screenBitDepth = Convert.ToInt32(this["screenBitDepth"]);
                    _haveScreenBitDepth = true;
                }
                return _screenBitDepth;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.IsColor"]/*' />
        public virtual bool IsColor
        {
            get
            {
                if(!_haveIsColor)
                {
                    String isColorString = this["isColor"];
                    if(isColorString == null)
                    {
                        _isColor = false;
                    }
                    else
                    {
                        _isColor = Convert.ToBoolean(this["isColor"]);
                    }
                    _haveIsColor = true;
                }
                return _isColor;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.InputType"]/*' />
        public virtual String InputType
        {
            get
            {
                if(!_haveInputType)
                {
                    _inputType = this["inputType"];
                    _haveInputType = true;
                }
                return _inputType;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.NumberOfSoftkeys"]/*' />
        public virtual int NumberOfSoftkeys
        {
            get
            {
                if(!_haveNumberOfSoftkeys)
                {
                    _numberOfSoftkeys = Convert.ToInt32(this["numberOfSoftkeys"]);
                    _haveNumberOfSoftkeys = true;
                }
                return _numberOfSoftkeys;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.MaximumSoftkeyLabelLength"]/*' />
        public virtual int MaximumSoftkeyLabelLength
        {
            get
            {
                if(!_haveMaximumSoftkeyLabelLength)
                {
                    _maximumSoftkeyLabelLength = Convert.ToInt32(this["maximumSoftkeyLabelLength"]);
                    _haveMaximumSoftkeyLabelLength = true;
                }
                return _maximumSoftkeyLabelLength;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanInitiateVoiceCall"]/*' />
        public virtual bool CanInitiateVoiceCall
        {
            get
            {
                if(!_haveCanInitiateVoiceCall)
                {
                    String canInitiateVoiceCallString = this["canInitiateVoiceCall"];
                    if(canInitiateVoiceCallString == null)
                    {
                        _canInitiateVoiceCall = false;
                    }
                    else
                    {
                        _canInitiateVoiceCall = Convert.ToBoolean(canInitiateVoiceCallString);
                    }
                    _haveCanInitiateVoiceCall = true;
                }
                return _canInitiateVoiceCall;
            }
        }


        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanSendMail"]/*' />
        public virtual bool CanSendMail
        {
            get
            {
                if(!_haveCanSendMail)
                {
                    String canSendMailString = this["canSendMail"];
                    if(canSendMailString == null)
                    {
                        _canSendMail = true;
                    }
                    else
                    {
                        _canSendMail = Convert.ToBoolean(canSendMailString);
                    }
                    _haveCanSendMail = true;
                }
                return _canSendMail;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.HasBackButton"]/*' />
        public virtual bool HasBackButton
        {
            get
            {
                if(!_haveHasBackButton)
                {
                    String hasBackButtonString = this["hasBackButton"];
                    if(hasBackButtonString == null)
                    {
                        _hasBackButton = true;
                    }
                    else
                    {
                        _hasBackButton = Convert.ToBoolean(hasBackButtonString);
                    }
                    _haveHasBackButton = true;
                }
                return _hasBackButton;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RendersWmlDoAcceptsInline"]/*' />
        public virtual bool RendersWmlDoAcceptsInline
        {
            get
            {
                if(!_haveRendersWmlDoAcceptsInline)
                {
                    String rendersWmlDoAcceptsInlineString = this["rendersWmlDoAcceptsInline"];
                    if(rendersWmlDoAcceptsInlineString == null)
                    {
                        _rendersWmlDoAcceptsInline = true;
                    }
                    else
                    {
                        _rendersWmlDoAcceptsInline = Convert.ToBoolean(rendersWmlDoAcceptsInlineString);
                    }
                    _haveRendersWmlDoAcceptsInline = true;
                }
                return _rendersWmlDoAcceptsInline;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RendersWmlSelectsAsMenuCards"]/*' />
        public virtual bool RendersWmlSelectsAsMenuCards
        {
            get
            {
                if(!_haveRendersWmlSelectsAsMenuCards)
                {
                    String rendersWmlSelectsAsMenuCardsString = this["rendersWmlSelectsAsMenuCards"];
                    if(rendersWmlSelectsAsMenuCardsString == null)
                    {
                        _rendersWmlSelectsAsMenuCards = false;
                    }
                    else
                    {
                        _rendersWmlSelectsAsMenuCards = Convert.ToBoolean(rendersWmlSelectsAsMenuCardsString);
                    }
                    _haveRendersWmlSelectsAsMenuCards = true;
                }
                return _rendersWmlSelectsAsMenuCards;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RendersBreaksAfterWmlAnchor"]/*' />
        public virtual bool RendersBreaksAfterWmlAnchor
        {
            get
            {
                if(!_haveRendersBreaksAfterWmlAnchor)
                {
                    String rendersBreaksAfterWmlAnchorString = this["rendersBreaksAfterWmlAnchor"];
                    if(rendersBreaksAfterWmlAnchorString == null)
                    {
                        _rendersBreaksAfterWmlAnchor = true;
                    }
                    else
                    {
                        _rendersBreaksAfterWmlAnchor = Convert.ToBoolean(rendersBreaksAfterWmlAnchorString);
                    }
                    _haveRendersBreaksAfterWmlAnchor = true;
                }
                return _rendersBreaksAfterWmlAnchor;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RendersBreaksAfterWmlInput"]/*' />
        public virtual bool RendersBreaksAfterWmlInput
        {
            get
            {
                if(!_haveRendersBreaksAfterWmlInput)
                {
                    String rendersBreaksAfterWmlInputString = this["rendersBreaksAfterWmlInput"];
                    if(rendersBreaksAfterWmlInputString == null)
                    {
                        _rendersBreaksAfterWmlInput = true;
                    }
                    else
                    {
                        _rendersBreaksAfterWmlInput = Convert.ToBoolean(rendersBreaksAfterWmlInputString);
                    }
                    _haveRendersBreaksAfterWmlInput = true;
                }
                return _rendersBreaksAfterWmlInput;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RendersBreakBeforeWmlSelectAndInput"]/*' />
        public virtual bool RendersBreakBeforeWmlSelectAndInput
        {
            get
            {
                if(!_haveRendersBreakBeforeWmlSelectAndInput)
                {
                    String rendersBreaksBeforeWmlSelectAndInputString = this["rendersBreakBeforeWmlSelectAndInput"];
                    if(rendersBreaksBeforeWmlSelectAndInputString == null)
                    {
                        _rendersBreakBeforeWmlSelectAndInput = false;
                    }
                    else
                    {
                        _rendersBreakBeforeWmlSelectAndInput = Convert.ToBoolean(rendersBreaksBeforeWmlSelectAndInputString);
                    }
                    _haveRendersBreakBeforeWmlSelectAndInput = true;
                }
                return _rendersBreakBeforeWmlSelectAndInput;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresPhoneNumbersAsPlainText"]/*' />
        public virtual bool RequiresPhoneNumbersAsPlainText
        {
            get
            {
                if(!_haveRequiresPhoneNumbersAsPlainText)
                {
                    String requiresPhoneNumbersAsPlainTextString = this["requiresPhoneNumbersAsPlainText"];
                    if(requiresPhoneNumbersAsPlainTextString == null)
                    {
                        _requiresPhoneNumbersAsPlainText = false;
                    }
                    else
                    {
                        _requiresPhoneNumbersAsPlainText = Convert.ToBoolean(requiresPhoneNumbersAsPlainTextString);
                    }
                    _haveRequiresPhoneNumbersAsPlainText = true;
                }
                return _requiresPhoneNumbersAsPlainText;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresUrlEncodedPostfieldValues"]/*' />
        public virtual bool RequiresUrlEncodedPostfieldValues
        {
            get
            {
                if(!_haveRequiresUrlEncodedPostfieldValues)
                {
                    String requiresUrlEncodedPostfieldValuesString = this["requiresUrlEncodedPostfieldValues"];
                    if(requiresUrlEncodedPostfieldValuesString == null)
                    {
                        _requiresUrlEncodedPostfieldValues = true;
                    }
                    else
                    {
                        _requiresUrlEncodedPostfieldValues = Convert.ToBoolean(requiresUrlEncodedPostfieldValuesString);
                    }
                    _haveRequiresUrlEncodedPostfieldValues = true;
                }
                return _requiresUrlEncodedPostfieldValues;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiredMetaTagNameValue"]/*' />
        public virtual String RequiredMetaTagNameValue
        {
            get
            {
                if(!_haveRequiredMetaTagNameValue)
                {
                    String value = this["requiredMetaTagNameValue"];
                    if(value == null || value == String.Empty)
                    {
                        _requiredMetaTagNameValue = null;
                    }
                    else
                    {
                        _requiredMetaTagNameValue = value;
                    }
                    _haveRequiredMetaTagNameValue = true;
                }
                return _requiredMetaTagNameValue;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RendersBreaksAfterHtmlLists"]/*' />
        public virtual bool RendersBreaksAfterHtmlLists
        {
            get
            {
                if(!_haveRendersBreaksAfterHtmlLists)
                {
                    String rendersBreaksAfterHtmlListsString = this["rendersBreaksAfterHtmlLists"];
                    if(rendersBreaksAfterHtmlListsString == null)
                    {
                        _rendersBreaksAfterHtmlLists = true;
                    }
                    else
                    {
                        _rendersBreaksAfterHtmlLists = Convert.ToBoolean(rendersBreaksAfterHtmlListsString);
                    }
                    _haveRendersBreaksAfterHtmlLists = true;
                }
                return _rendersBreaksAfterHtmlLists;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresUniqueHtmlInputNames"]/*' />
        public virtual bool RequiresUniqueHtmlInputNames
        {
            get
            {
                if(!_haveRequiresUniqueHtmlInputNames)
                {
                    String requiresUniqueHtmlInputNamesString = this["requiresUniqueHtmlInputNames"];
                    if(requiresUniqueHtmlInputNamesString == null)
                    {
                        _requiresUniqueHtmlInputNames = false;
                    }
                    else
                    {
                        _requiresUniqueHtmlInputNames = Convert.ToBoolean(requiresUniqueHtmlInputNamesString);
                    }
                    _haveRequiresUniqueHtmlInputNames = true;
                }
                return _requiresUniqueHtmlInputNames;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresUniqueHtmlCheckboxNames"]/*' />
        public virtual bool RequiresUniqueHtmlCheckboxNames
        {
            get
            {
                if(!_haveRequiresUniqueHtmlCheckboxNames)
                {
                    String requiresUniqueHtmlCheckboxNamesString = this["requiresUniqueHtmlCheckboxNames"];
                    if(requiresUniqueHtmlCheckboxNamesString == null)
                    {
                        _requiresUniqueHtmlCheckboxNames = false;
                    }
                    else
                    {
                        _requiresUniqueHtmlCheckboxNames = Convert.ToBoolean(requiresUniqueHtmlCheckboxNamesString);
                    }
                    _haveRequiresUniqueHtmlCheckboxNames = true;
                }
                return _requiresUniqueHtmlCheckboxNames;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsCss"]/*' />
        public virtual bool SupportsCss
        {
            get
            {
                if(!_haveSupportsCss)
                {
                    String supportsCssString = this["supportsCss"];
                    if(supportsCssString == null)
                    {
                        _supportsCss = false;
                    }
                    else
                    {
                        _supportsCss = Convert.ToBoolean(supportsCssString);
                    }
                    _haveSupportsCss = true;
                }
                return _supportsCss;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.HidesRightAlignedMultiselectScrollbars"]/*' />
        public virtual bool HidesRightAlignedMultiselectScrollbars
        {
            get
            {
                if(!_haveHidesRightAlignedMultiselectScrollbars)
                {
                    String hidesRightAlignedMultiselectScrollbarsString = this["hidesRightAlignedMultiselectScrollbars"];
                    if(hidesRightAlignedMultiselectScrollbarsString == null)
                    {
                        _hidesRightAlignedMultiselectScrollbars = false;
                    }
                    else
                    {
                        _hidesRightAlignedMultiselectScrollbars = Convert.ToBoolean(hidesRightAlignedMultiselectScrollbarsString);
                    }
                    _haveHidesRightAlignedMultiselectScrollbars = true;
               }
               return _hidesRightAlignedMultiselectScrollbars;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.IsMobileDevice"]/*' />
        public virtual bool IsMobileDevice
        {
            get
            {
                if(!_haveIsMobileDevice)
                {
                    String isMobileDeviceString = this["isMobileDevice"];
                    if(isMobileDeviceString == null)
                    {
                        _isMobileDevice = false;
                    }
                    else
                    {
                        _isMobileDevice = Convert.ToBoolean(isMobileDeviceString);
                    }
                    _haveIsMobileDevice = true;
                }
                return _isMobileDevice;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresAttributeColonSubstitution"]/*' />
        public virtual bool RequiresAttributeColonSubstitution
        {
            get
            {
                if(!_haveRequiresAttributeColonSubstitution)
                {
                    String requiresAttributeColonSubstitution = this["requiresAttributeColonSubstitution"];
                    if(requiresAttributeColonSubstitution == null)
                    {
                        _requiresAttributeColonSubstitution = false;
                    }
                    else
                    {
                        _requiresAttributeColonSubstitution = Convert.ToBoolean(requiresAttributeColonSubstitution);
                    }
                    _haveRequiresAttributeColonSubstitution = true;
                }
                return _requiresAttributeColonSubstitution;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderOneventAndPrevElementsTogether"]/*' />
        public virtual bool CanRenderOneventAndPrevElementsTogether
        {
            get
            {
                if(!_haveCanRenderOneventAndPrevElementsTogether)
                {
                    String canRenderOneventAndPrevElementsTogetherString = this["canRenderOneventAndPrevElementsTogether"];
                    if(canRenderOneventAndPrevElementsTogetherString == null)
                    {
                        _canRenderOneventAndPrevElementsTogether = true;
                    }
                    else
                    {
                        _canRenderOneventAndPrevElementsTogether = Convert.ToBoolean(canRenderOneventAndPrevElementsTogetherString);
                    }
                    _haveCanRenderOneventAndPrevElementsTogether = true;
                }
                return _canRenderOneventAndPrevElementsTogether;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderInputAndSelectElementsTogether"]/*' />
        public virtual bool CanRenderInputAndSelectElementsTogether
        {
            get
            {
                if(!_haveCanRenderInputAndSelectElementsTogether)
                {
                    String canRenderInputAndSelectElementsTogetherString = this["canRenderInputAndSelectElementsTogether"];
                    if(canRenderInputAndSelectElementsTogetherString == null)
                    {
                        _canRenderInputAndSelectElementsTogether = true;
                    }
                    else
                    {
                        _canRenderInputAndSelectElementsTogether = Convert.ToBoolean(canRenderInputAndSelectElementsTogetherString);
                    }
                    _haveCanRenderInputAndSelectElementsTogether = true;
                }
                return _canRenderInputAndSelectElementsTogether;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderAfterInputOrSelectElement"]/*' />
        public virtual bool CanRenderAfterInputOrSelectElement
        {
            get
            {
                if(!_haveCanRenderAfterInputOrSelectElement)
                {
                    String canRenderAfterInputOrSelectElementString = this["canRenderAfterInputOrSelectElement"];
                    if(canRenderAfterInputOrSelectElementString == null)
                    {
                        _canRenderAfterInputOrSelectElement = true;
                    }
                    else
                    {
                        _canRenderAfterInputOrSelectElement = Convert.ToBoolean(canRenderAfterInputOrSelectElementString);
                    }
                    _haveCanRenderAfterInputOrSelectElement = true;
                }
                return _canRenderAfterInputOrSelectElement;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderPostBackCards"]/*' />
        public virtual bool CanRenderPostBackCards
        {
            get
            {
                if(!_haveCanRenderPostBackCards)
                {
                    String canRenderPostBackCardsString = this["canRenderPostBackCards"];
                    if(canRenderPostBackCardsString == null)
                    {
                        _canRenderPostBackCards = true;
                    }
                    else
                    {
                        _canRenderPostBackCards = Convert.ToBoolean(canRenderPostBackCardsString);
                    }
                    _haveCanRenderPostBackCards = true;
                }
                return _canRenderPostBackCards;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderMixedSelects"]/*' />
        public virtual bool CanRenderMixedSelects
        {
            get
            {
                if(!_haveCanRenderMixedSelects)
                {
                    String canRenderMixedSelectsString = this["canRenderMixedSelects"];
                    if(canRenderMixedSelectsString == null)
                    {
                        _canRenderMixedSelects = true;
                    }
                    else
                    {
                        _canRenderMixedSelects = Convert.ToBoolean(canRenderMixedSelectsString);
                    }
                    _haveCanRenderMixedSelects = true;
                }
                return _canRenderMixedSelects;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanCombineFormsInDeck"]/*' />
        public virtual bool CanCombineFormsInDeck
        {
            get
            {
                if(!_haveCanCombineFormsInDeck)
                {
                    String canCombineFormsInDeckString = this["canCombineFormsInDeck"];
                    if(canCombineFormsInDeckString == null)
                    {
                        _canCombineFormsInDeck = true;
                    }
                    else
                    {
                        _canCombineFormsInDeck = Convert.ToBoolean(canCombineFormsInDeckString);
                    }
                    _haveCanCombineFormsInDeck = true;
                }
                return _canCombineFormsInDeck;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderSetvarZeroWithMultiSelectionList"]/*' />
        public virtual bool CanRenderSetvarZeroWithMultiSelectionList
        {
            get
            {
                if(!_haveCanRenderSetvarZeroWithMultiSelectionList)
                {
                    String canRenderSetvarZeroWithMultiSelectionListString = this["canRenderSetvarZeroWithMultiSelectionList"];
                    if(canRenderSetvarZeroWithMultiSelectionListString == null)
                    {
                        _canRenderSetvarZeroWithMultiSelectionList = true;
                    }
                    else
                    {
                        _canRenderSetvarZeroWithMultiSelectionList = Convert.ToBoolean(canRenderSetvarZeroWithMultiSelectionListString);
                    }
                    _haveCanRenderSetvarZeroWithMultiSelectionList = true;
                }
                return _canRenderSetvarZeroWithMultiSelectionList;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsImageSubmit"]/*' />
        public virtual bool SupportsImageSubmit
        {
            get
            {
                if(!_haveSupportsImageSubmit)
                {
                    String supportsImageSubmitString = this["supportsImageSubmit"];
                    if(supportsImageSubmitString == null)
                    {
                        _supportsImageSubmit = false;
                    }
                    else
                    {
                        _supportsImageSubmit = Convert.ToBoolean(supportsImageSubmitString);
                    }
                    _haveSupportsImageSubmit = true;
                }
                return _supportsImageSubmit;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresUniqueFilePathSuffix"]/*' />
        public virtual bool RequiresUniqueFilePathSuffix
        {
            get
            {
                if(!_haveRequiresUniqueFilePathSuffix)
                {
                    String requiresUniqueFilePathSuffixString = this["requiresUniqueFilePathSuffix"];
                    if(requiresUniqueFilePathSuffixString == null)
                    {
                        _requiresUniqueFilePathSuffix = false;
                    }
                    else
                    {
                        _requiresUniqueFilePathSuffix = Convert.ToBoolean(requiresUniqueFilePathSuffixString);
                    }
                    _haveRequiresUniqueFilePathSuffix = true;
                }
                return _requiresUniqueFilePathSuffix;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresNoBreakInFormatting"]/*' />
        public virtual bool RequiresNoBreakInFormatting
        {
            get
            {
                if(!_haveRequiresNoBreakInFormatting)
                {
                    String requiresNoBreakInFormatting = this["requiresNoBreakInFormatting"];
                    if(requiresNoBreakInFormatting == null)
                    {
                        _requiresNoBreakInFormatting = false;
                    }
                    else
                    {
                        _requiresNoBreakInFormatting = Convert.ToBoolean(requiresNoBreakInFormatting);
                    }
                    _haveRequiresNoBreakInFormatting = true;
                }
                return _requiresNoBreakInFormatting;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresLeadingPageBreak"]/*' />
        public virtual bool RequiresLeadingPageBreak
        {
            get
            {
                if(!_haveRequiresLeadingPageBreak)
                {
                    String requiresLeadingPageBreak = this["requiresLeadingPageBreak"];
                    if(requiresLeadingPageBreak == null)
                    {
                        _requiresLeadingPageBreak = false;
                    }
                    else
                    {
                        _requiresLeadingPageBreak = Convert.ToBoolean(requiresLeadingPageBreak);
                    }
                    _haveRequiresLeadingPageBreak = true;
                }
                return _requiresLeadingPageBreak;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsSelectMultiple"]/*' />
        public virtual bool SupportsSelectMultiple
        {
            get
            {
                if(!_haveSupportsSelectMultiple)
                {
                    String supportsSelectMultipleString = this["supportsSelectMultiple"];
                    if(supportsSelectMultipleString == null)
                    {
                        _supportsSelectMultiple = false;
                    }
                    else
                    {
                        _supportsSelectMultiple = Convert.ToBoolean(supportsSelectMultipleString);
                    }
                    _haveSupportsSelectMultiple = true;
                }
                return _supportsSelectMultiple;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsBold"]/*' />
        public new virtual bool SupportsBold
        {
            get
            {
                if(!_haveSupportsBold)
                {
                    String supportsBold = this["supportsBold"];
                    if(supportsBold == null)
                    {
                        _supportsBold = false;
                    }
                    else
                    {
                        _supportsBold = Convert.ToBoolean(supportsBold);
                    }
                    _haveSupportsBold = true;
                }
                return _supportsBold;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsItalic"]/*' />
        public new virtual bool SupportsItalic
        {
            get
            {
                if(!_haveSupportsItalic)
                {
                    String supportsItalic = this["supportsItalic"];
                    if(supportsItalic == null)
                    {
                        _supportsItalic = false;
                    }
                    else
                    {
                        _supportsItalic = Convert.ToBoolean(supportsItalic);
                    }
                    _haveSupportsItalic = true;
                }
                return _supportsItalic;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsFontSize"]/*' />
        public virtual bool SupportsFontSize
        {
            get
            {
                if(!_haveSupportsFontSize)
                {
                    String supportsFontSize = this["supportsFontSize"];
                    if(supportsFontSize == null)
                    {
                        _supportsFontSize = false;
                    }
                    else
                    {
                        _supportsFontSize = Convert.ToBoolean(supportsFontSize);
                    }
                    _haveSupportsFontSize = true;
                }
                return _supportsFontSize;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsFontName"]/*' />
        public virtual bool SupportsFontName
        {
            get
            {
                if(!_haveSupportsFontName)
                {
                    String supportsFontName = this["supportsFontName"];
                    if(supportsFontName == null)
                    {
                        _supportsFontName = false;
                    }
                    else
                    {
                        _supportsFontName = Convert.ToBoolean(supportsFontName);
                    }
                    _haveSupportsFontName = true;
                }
                return _supportsFontName;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsFontColor"]/*' />
        public virtual bool SupportsFontColor
        {
            get
            {
                if(!_haveSupportsFontColor)
                {
                    String supportsFontColor = this["supportsFontColor"];
                    if(supportsFontColor == null)
                    {
                        _supportsFontColor = false;
                    }
                    else
                    {
                        _supportsFontColor = Convert.ToBoolean(supportsFontColor);
                    }
                    _haveSupportsFontColor = true;
                }
                return _supportsFontColor;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsBodyColor"]/*' />
        public virtual bool SupportsBodyColor
        {
            get
            {
                if(!_haveSupportsBodyColor)
                {
                    String supportsBodyColor = this["supportsBodyColor"];
                    if(supportsBodyColor == null)
                    {
                        _supportsBodyColor = false;
                    }
                    else
                    {
                        _supportsBodyColor = Convert.ToBoolean(supportsBodyColor);
                    }
                    _haveSupportsBodyColor = true;
                }
                return _supportsBodyColor;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsDivAlign"]/*' />
        public virtual bool SupportsDivAlign
        {
            get
            {
                if(!_haveSupportsDivAlign)
                {
                    String supportsDivAlign = this["supportsDivAlign"];
                    if(supportsDivAlign == null)
                    {
                        _supportsDivAlign = false;
                    }
                    else
                    {
                        _supportsDivAlign = Convert.ToBoolean(supportsDivAlign);
                    }
                    _haveSupportsDivAlign = true;
                }
                return _supportsDivAlign;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsDivNoWrap"]/*' />
        public virtual bool SupportsDivNoWrap
        {
            get
            {
                if(!_haveSupportsDivNoWrap)
                {
                    String supportsDivNoWrap = this["supportsDivNoWrap"];
                    if(supportsDivNoWrap == null)
                    {
                        _supportsDivNoWrap = false;
                    }
                    else
                    {
                        _supportsDivNoWrap = Convert.ToBoolean(supportsDivNoWrap);
                    }
                    _haveSupportsDivNoWrap = true;
                }
                return _supportsDivNoWrap;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresContentTypeMetaTag"]/*' />
        public virtual bool RequiresContentTypeMetaTag
        {
            get
            {
                if(!_haveRequiresContentTypeMetaTag)
                {
                    String requiresContentTypeMetaTag = this["requiresContentTypeMetaTag"];
                    if(requiresContentTypeMetaTag == null)
                    {
                        _requiresContentTypeMetaTag = false;
                    }
                    else
                    {
                        _requiresContentTypeMetaTag = 
                            Convert.ToBoolean(requiresContentTypeMetaTag);
                    }
                    _haveRequiresContentTypeMetaTag = true;
                }
                return _requiresContentTypeMetaTag;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresDBCSCharacter"]/*' />
        public virtual bool RequiresDBCSCharacter
        {
            get
            {
                if(!_haveRequiresDBCSCharacter)
                {
                    String requiresDBCSCharacter = this["requiresDBCSCharacter"];
                    if(requiresDBCSCharacter == null)
                    {
                        _requiresDBCSCharacter = false;
                    }
                    else
                    {
                        _requiresDBCSCharacter = 
                            Convert.ToBoolean(requiresDBCSCharacter);
                    }
                    _haveRequiresDBCSCharacter = true;
                }
                return _requiresDBCSCharacter;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresHtmlAdaptiveErrorReporting"]/*' />
        public virtual bool RequiresHtmlAdaptiveErrorReporting
        {
            get
            {
                if(!_haveRequiresHtmlAdaptiveErrorReporting)
                {
                    String requiresHtmlAdaptiveErrorReporting = this["requiresHtmlAdaptiveErrorReporting"];
                    if(requiresHtmlAdaptiveErrorReporting == null)
                    {
                        _requiresHtmlAdaptiveErrorReporting = false;
                    }
                    else
                    {
                        _requiresHtmlAdaptiveErrorReporting = 
                            Convert.ToBoolean(requiresHtmlAdaptiveErrorReporting);
                    }
                    _haveRequiresHtmlAdaptiveErrorReporting = true;
                }
                return _requiresHtmlAdaptiveErrorReporting;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresOutputOptimization"]/*' />
        public virtual bool RequiresOutputOptimization
        {
            get
            {
                if(!_haveRequiresOutputOptimization)
                {
                    String RequiresOutputOptimizationString = this["requiresOutputOptimization"];
                    if(RequiresOutputOptimizationString == null)
                    {
                        _requiresOutputOptimization = false;
                    }
                    else
                    {
                        _requiresOutputOptimization = Convert.ToBoolean(RequiresOutputOptimizationString);
                    }
                    _haveRequiresOutputOptimization = true;
                }
                return _requiresOutputOptimization;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsAccesskeyAttribute"]/*' />
        public virtual bool SupportsAccesskeyAttribute
        {
            get
            {
                if(!_haveSupportsAccesskeyAttribute)
                {
                    String SupportsAccesskeyAttributeString = this["supportsAccesskeyAttribute"];
                    if(SupportsAccesskeyAttributeString == null)
                    {
                        _supportsAccesskeyAttribute = false;
                    }
                    else
                    {
                        _supportsAccesskeyAttribute = Convert.ToBoolean(SupportsAccesskeyAttributeString);
                    }
                    _haveSupportsAccesskeyAttribute = true;
                }
                return _supportsAccesskeyAttribute;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsInputIStyle"]/*' />
        public virtual bool SupportsInputIStyle
        {
            get
            {
                if(!_haveSupportsInputIStyle)
                {
                    String SupportsInputIStyleString = this["supportsInputIStyle"];
                    if(SupportsInputIStyleString == null)
                    {
                        _supportsInputIStyle = false;
                    }
                    else
                    {
                        _supportsInputIStyle = Convert.ToBoolean(SupportsInputIStyleString);
                    }
                    _haveSupportsInputIStyle = true;
                }
                return _supportsInputIStyle;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsInputMode"]/*' />
        public virtual bool SupportsInputMode
        {
            get
            {
                if(!_haveSupportsInputMode)
                {
                    String SupportsInputModeString = this["supportsInputMode"];
                    if(SupportsInputModeString == null)
                    {
                        _supportsInputMode = false;
                    }
                    else
                    {
                        _supportsInputMode = Convert.ToBoolean(SupportsInputModeString);
                    }
                    _haveSupportsInputMode = true;
                }
                return _supportsInputMode;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsIModeSymbols"]/*' />
        public virtual bool SupportsIModeSymbols
        {
            get
            {
                if(!_haveSupportsIModeSymbols)
                {
                    String SupportsIModeSymbolsString = this["supportsIModeSymbols"];
                    if(SupportsIModeSymbolsString == null)
                    {
                        _supportsIModeSymbols = false;
                    }
                    else
                    {
                        _supportsIModeSymbols = Convert.ToBoolean(SupportsIModeSymbolsString);
                    }
                    _haveSupportsIModeSymbols = true;
                }
                return _supportsIModeSymbols;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsJPhoneSymbols"]/*' />
        public virtual bool SupportsJPhoneSymbols
        {
            get
            {
                if(!_haveSupportsJPhoneSymbols)
                {
                    String SupportsJPhoneSymbolsString = this["supportsJPhoneSymbols"];
                    if(SupportsJPhoneSymbolsString == null)
                    {
                        _supportsJPhoneSymbols = false;
                    }
                    else
                    {
                        _supportsJPhoneSymbols = Convert.ToBoolean(SupportsJPhoneSymbolsString);
                    }
                    _haveSupportsJPhoneSymbols = true;
                }
                return _supportsJPhoneSymbols;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsJPhoneMultiMediaAttributes"]/*' />
        public virtual bool SupportsJPhoneMultiMediaAttributes
        {
            get
            {
                if(!_haveSupportsJPhoneMultiMediaAttributes)
                {
                    String SupportsJPhoneMultiMediaAttributesString = this["supportsJPhoneMultiMediaAttributes"];
                    if(SupportsJPhoneMultiMediaAttributesString == null)
                    {
                        _supportsJPhoneMultiMediaAttributes = false;
                    }
                    else
                    {
                        _supportsJPhoneMultiMediaAttributes = Convert.ToBoolean(SupportsJPhoneMultiMediaAttributesString);
                    }
                    _haveSupportsJPhoneMultiMediaAttributes = true;
                }
                return _supportsJPhoneMultiMediaAttributes;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.MaximumRenderedPageSize"]/*' />
        public virtual int MaximumRenderedPageSize
        {
            get
            {
                if(!_haveMaximumRenderedPageSize)
                {
                    _maximumRenderedPageSize = Convert.ToInt32(this["maximumRenderedPageSize"]);
                    _haveMaximumRenderedPageSize = true;
                }
                return _maximumRenderedPageSize;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.RequiresSpecialViewStateEncoding"]/*' />
        public virtual bool RequiresSpecialViewStateEncoding
        {
            get
            {
                if(!_haveRequiresSpecialViewStateEncoding)
                {
                    String RequiresSpecialViewStateEncodingString = this["requiresSpecialViewStateEncoding"];
                    if(RequiresSpecialViewStateEncodingString == null)
                    {
                        _requiresSpecialViewStateEncoding = false;
                    }
                    else
                    {
                        _requiresSpecialViewStateEncoding = Convert.ToBoolean(RequiresSpecialViewStateEncodingString);
                    }
                    _haveRequiresSpecialViewStateEncoding = true;
                }
                return _requiresSpecialViewStateEncoding;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsQueryStringInFormAction"]/*' />
        public virtual bool SupportsQueryStringInFormAction
        {
            get
            {
                if(!_haveSupportsQueryStringInFormAction)
                {
                    String SupportsQueryStringInFormActionString = this["supportsQueryStringInFormAction"];
                    if(SupportsQueryStringInFormActionString == null)
                    {
                        _supportsQueryStringInFormAction = true;
                    }
                    else
                    {
                        _supportsQueryStringInFormAction = Convert.ToBoolean(SupportsQueryStringInFormActionString);
                    }
                    _haveSupportsQueryStringInFormAction = true;
                }
                return _supportsQueryStringInFormAction;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsCacheControlMetaTag"]/*' />
        public virtual bool SupportsCacheControlMetaTag
        {
            get
            {
                if(!_haveSupportsCacheControlMetaTag)
                {
                    String SupportsCacheControlMetaTagString = this["supportsCacheControlMetaTag"];
                    if(SupportsCacheControlMetaTagString == null)
                    {
                        _supportsCacheControlMetaTag = true;
                    }
                    else
                    {
                        _supportsCacheControlMetaTag = Convert.ToBoolean(SupportsCacheControlMetaTagString);
                    }
                    _haveSupportsCacheControlMetaTag = true;
                }
                return _supportsCacheControlMetaTag;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsUncheck"]/*' />
        public virtual bool SupportsUncheck
        {
            get
            {
                if(!_haveSupportsUncheck)
                {
                    String SupportsUncheckString = this["supportsUncheck"];
                    if(SupportsUncheckString == null)
                    {
                        _supportsUncheck = true;
                    }
                    else
                    {
                        _supportsUncheck = Convert.ToBoolean(SupportsUncheckString);
                    }
                    _haveSupportsUncheck = true;
                }
                return _supportsUncheck;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.CanRenderEmptySelects"]/*' />
        public virtual bool CanRenderEmptySelects
        {
            get
            {
                if(!_haveCanRenderEmptySelects)
                {
                    String CanRenderEmptySelectsString = this["canRenderEmptySelects"];
                    if(CanRenderEmptySelectsString == null)
                    {
                        _canRenderEmptySelects = true;
                    }
                    else
                    {
                        _canRenderEmptySelects = Convert.ToBoolean(CanRenderEmptySelectsString);
                    }
                    _haveCanRenderEmptySelects = true;
                }
                return _canRenderEmptySelects;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsRedirectWithCookie"]/*' />
        public virtual bool SupportsRedirectWithCookie
        {
            get
            {
                if(!_haveSupportsRedirectWithCookie)
                {
                    String supportsRedirectWithCookie = this["supportsRedirectWithCookie"];
                    if(supportsRedirectWithCookie == null)
                    {
                        _supportsRedirectWithCookie = true;
                    }
                    else
                    {
                        _supportsRedirectWithCookie = Convert.ToBoolean(supportsRedirectWithCookie);
                    }
                    _haveSupportsRedirectWithCookie = true;
                }
                return _supportsRedirectWithCookie;
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.SupportsEmptyStringInCookieValue"]/*' />
        public virtual bool SupportsEmptyStringInCookieValue
        {
            get
            {
                if (!_haveSupportsEmptyStringInCookieValue)
                {
                    String supportsEmptyStringInCookieValue = this["supportsEmptyStringInCookieValue"];
                    if (supportsEmptyStringInCookieValue == null)
                    {
                        _supportsEmptyStringInCookieValue = true;
                    }
                    else
                    {
                        _supportsEmptyStringInCookieValue = 
                            Convert.ToBoolean (supportsEmptyStringInCookieValue);
                    }
                    _haveSupportsEmptyStringInCookieValue = true;
                }
                return _supportsEmptyStringInCookieValue;                
            }
        }

        /// <include file='doc\MobileCapabilities.uex' path='docs/doc[@for="MobileCapabilities.DefaultSubmitButtonLimit"]/*' />
        public virtual int DefaultSubmitButtonLimit
        {
            get
            {
                if(!_haveDefaultSubmitButtonLimit)
                {
                    String s = this["defaultSubmitButtonLimit"];
                    _defaultSubmitButtonLimit = s != null ? Convert.ToInt32(this["defaultSubmitButtonLimit"]) : 1;
                    _haveDefaultSubmitButtonLimit = true;
                }
                return _defaultSubmitButtonLimit;
            }
        }


        private String _mobileDeviceManufacturer;
        private String _mobileDeviceModel;
        private String _gatewayVersion;
        private int _gatewayMajorVersion;
        private double _gatewayMinorVersion;
        private String _preferredRenderingType;     // 
















































































































































*/
    }
}

