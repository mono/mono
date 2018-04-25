//------------------------------------------------------------------------------
// <copyright file="UpWmlMobileTextWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Security.Permissions;

using SR=System.Web.UI.MobileControls.Adapters.SR;

#if COMPILING_FOR_SHIPPED_SOURCE
using Adapters=System.Web.UI.MobileControls.ShippedAdapterSource;
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
using Adapters=System.Web.UI.MobileControls.Adapters;
namespace System.Web.UI.MobileControls.Adapters
#endif    

{
    /*
     * UpWmlMobileTextWriter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class UpWmlMobileTextWriter : WmlMobileTextWriter
    {
        private int       _screenWidth;
        private int       _screenHeight;
        private bool      _inHyperlink = false;
        private bool      _inPostBack = false;
        private bool      _inSoftkey = false;
        private Alignment _lastAlignment = Alignment.Left;
        private Wrapping  _lastWrapping = Wrapping.Wrap;
        private int       _currentCardIndex = -1;
        private ArrayList _cards = new ArrayList();
        private int       _currentCardAnchorCount = 0;
        private int       _currentCardPostBacks = 0;
        private int       _currentCardSubmits = 0;
        private bool      _canRenderMixedSelects = false;
        private bool      _requiresOptionSubmitCard = false;
        private int       _optionSubmitCardIndex = 0;
        private String    _optionMenuName = null;
        
        private String    _linkText = null;
        private String    _targetUrl = null;
        private String    _softkeyLabel = null;
        private bool      _encodeUrl = false;
        private bool      _useMenuOptionTitle = false;
        
        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.UpWmlMobileTextWriter"]/*' />
        public UpWmlMobileTextWriter(TextWriter writer, MobileCapabilities device, MobilePage page) 
            : base(writer, device, page)
        {
            _screenWidth = device.ScreenCharactersWidth;
            _screenHeight = device.ScreenCharactersHeight;
            _canRenderMixedSelects = device.CanRenderMixedSelects;
        }

        private UpCard CurrentCard
        {
            get
            {
                return (UpCard)_cards[_currentCardIndex];
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.BeginForm"]/*' />
        public override void BeginForm(Form form)
        {
            ResetState();
            if (AnalyzeMode)
            {
                AllocateNewCard();
                base.BeginForm(form);
            }
            else
            {
                if (form == form.MobilePage.ActiveForm)
                {
                    PreRenderActiveForm();
                }

                base.BeginForm(form);
                RenderCardOpening(0);
            }
        }

        private static readonly int _filePathSuffixLength = 
            Constants.UniqueFilePathSuffixVariableWithoutEqual.Length + 1;
        private int _sessionCount = -1;
        private int SessionCount
        {
            get
            {
                if (_sessionCount == -1)
                {
                    _sessionCount = 0;
                    String filePathSuffix = 
                        Page.Request.QueryString[Constants.UniqueFilePathSuffixVariableWithoutEqual];

                    if (filePathSuffix != null && filePathSuffix.Length == _filePathSuffixLength)
                    {
                        Char c = filePathSuffix[_filePathSuffixLength - 1];
                        if (Char.IsDigit(c))
                        {
                            _sessionCount = (int)Char.GetNumericValue(c);
                        }
                    }
                }

                return _sessionCount;
            }
        }

        private bool RequiresLoopDetectionCard
        {
            get
            {
                IDictionary dictionary = Page.Adapter.CookielessDataDictionary;
                if((dictionary != null) && (dictionary.Count > 0))
                {
                    return true;
                }
                return SessionCount == 9;
            }
        }

        private void PreRenderActiveForm()
        {
            if (Device.RequiresUniqueFilePathSuffix && RequiresLoopDetectionCard)
            {
                Debug.Assert(!AnalyzeMode);
                Write(String.Format(CultureInfo.InvariantCulture, _loopDetectionCard, Page.ActiveForm.ClientID));
            }
        }

        private String _cachedFormQueryString;
        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.CalculateFormQueryString"]/*' />
        protected override String CalculateFormQueryString()
        {
            if(_cachedFormQueryString != null)
            {
                return _cachedFormQueryString;
            }
            String queryString = null;
            if (CurrentForm.Method != FormMethod.Get)
            {
                queryString = Page.QueryStringText;
            }

            if (Device.RequiresUniqueFilePathSuffix)
            {
                String ufps = Page.UniqueFilePathSuffix;
                if(this.HasFormVariables)
                {
                    if (SessionCount == 9)
                    {
                        ufps += '0';
                    }
                    else
                    {
                        ufps += (SessionCount + 1).ToString(CultureInfo.InvariantCulture);
                    }
                }
                if (queryString != null && queryString.Length > 0)
                {
                    queryString = String.Concat(ufps, "&", queryString);
                }
                else
                {
                    queryString = ufps;
                }
            }
            _cachedFormQueryString = queryString;
            return queryString;
        }

        private const String _loopDetectionCard = 
            "<card ontimer=\"#{0}\"><onevent type=\"onenterbackward\"><prev /></onevent><timer value=\"1\" /></card>";

        internal override bool ShouldWriteFormID(Form form)
        {
            if (RequiresLoopDetectionCard)
            {
                return true;
            }
            return base.ShouldWriteFormID(form);
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.EndForm"]/*' />
        public override void EndForm()
        {
            if (AnalyzeMode)
            {
                CheckRawOutput();
                CurrentCard.AnchorCount = _currentCardAnchorCount;
                base.EndForm();
            }
            else
            {
                RenderCardClosing(_currentCardIndex);
                base.EndForm();
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderEndForm"]/*' />
        protected override void RenderEndForm()
        {
            base.RenderEndForm();
            if (_requiresOptionSubmitCard)
            {
                Write("<card id=\"");
                Write(_postBackCardPrefix);
                Write("0");
                Write(_optionSubmitCardIndex++);
                WriteLine("\">");

                Write("<onevent type=\"onenterforward\">");
                RenderGoAction(null, _postBackEventArgumentVarName, WmlPostFieldType.Variable, true);
                WriteLine("</onevent>");

                WriteLine("<onevent type=\"onenterbackward\"><prev /></onevent>");
                WriteLine("</card>");
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderText"]/*' />
        public override void RenderText(String text, bool breakAfter, bool encodeText)
        {
            if (AnalyzeMode)
            {
                if (CurrentCard.HasInputElements && !Device.CanRenderAfterInputOrSelectElement)
                {
                    BeginNextCard();
                }

                CheckRawOutput();
                if (_inHyperlink || _inPostBack)
                {
                    // When analyzing, accumulate link text for use in figuring
                    // out softkey.
                    if (_inSoftkey)
                    {
                        _linkText += text;
                    }
                }
                else
                {
                    // Text cannot come after a menu.
                    if (CurrentCard.RenderAsMenu)
                    {
                        CurrentCard.RenderAsMenu = false;
                        CurrentCard.MenuCandidate = false;
                    }
                    else if (CurrentCard.MenuCandidate)
                    {
                        // Calculate weight of static items before a menu.
                        // This is used to check for screens that scroll past their
                        // initial content.

                        int weight = text != null ? text.Length : 0;
                        if (breakAfter)
                        {
                            weight = ((weight - 1) / _screenWidth + 1) * _screenWidth;
                        }
                        CurrentCard.StaticItemsWeight += weight;
                    }
                }
            }
            else
            {
                bool willRenderText = false;
                if (_inHyperlink || _inPostBack)
                {
                    // If rendering in menu, simply accumulate text.
                    if (CurrentCard.RenderAsMenu)
                    {
                        _linkText += text;
                    }
                    else if (!CurrentCard.UsesDefaultSubmit)
                    {
                        willRenderText = true;
                    }
                }
                else
                {
                    willRenderText = true;
                }

                if (willRenderText)
                {
                    // Some browsers that
                    // RendersBreakBeforeWmlSelectAndInput have the odd behavior
                    // of *not* rendering a break if there is nothing on the
                    // card before it, and entering an explicit <br> creates two
                    // breaks.  Therefore, we just render a &nbsp; in this
                    // situation.
                    
                    if (!CurrentCard.RenderedTextElementYet &&
                        Device.RendersBreakBeforeWmlSelectAndInput &&
                        !((WmlPageAdapter)Page.Adapter).IsKDDIPhone())
                    {
                        CurrentCard.RenderedTextElementYet = true;
                        if (breakAfter && (text != null && text.Length == 0))
                        {
                            base.RenderText("&nbsp;", false, false);
                        } 
                    }
                    base.RenderText(text, breakAfter, encodeText);
                }
                
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderBeginHyperlink"]/*' />
        public override void RenderBeginHyperlink(String targetUrl, 
                                                  bool encodeUrl, 
                                                  String softkeyLabel, 
                                                  bool implicitSoftkeyLabel,
                                                  bool mapToSoftkey)
        {
            if (_inHyperlink || _inPostBack)
            {
                throw new Exception();
            }

            // AUI 4137
            if (targetUrl != null && targetUrl.Length > 0 && targetUrl[0] != '#')
            {
                targetUrl = Page.MakePathAbsolute(targetUrl);
            }

            if (AnalyzeMode)
            {
                if (CurrentCard.HasInputElements && !Device.CanRenderAfterInputOrSelectElement)
                {
                    BeginNextCard();
                }

                CheckRawOutput();

                // Try to map to softkey if possible.
                if (mapToSoftkey && CurrentCard.SoftkeysUsed < NumberOfSoftkeys)
                {
                    _inSoftkey = true;
                    _targetUrl = targetUrl;
                    _softkeyLabel = softkeyLabel;
                    _encodeUrl = encodeUrl;
                    _linkText = String.Empty;
                }
            }
            else
            {
                if (CurrentCard.RenderAsMenu)
                {
                    if (!CurrentCard.MenuOpened)
                    {
                        OpenMenu();
                    }

                    // In menu mode, actual rendering is done on RenderEndHyperlink,
                    // when we have all available info.

                    _targetUrl = targetUrl;
                    _softkeyLabel = softkeyLabel;
                    _useMenuOptionTitle = mapToSoftkey && !implicitSoftkeyLabel;
                    _encodeUrl = encodeUrl;
                    _linkText = String.Empty;
                }
                else if (!CurrentCard.UsesDefaultSubmit)
                {
                    base.RenderBeginHyperlink(targetUrl, 
                                              encodeUrl, 
                                              softkeyLabel, 
                                              implicitSoftkeyLabel, 
                                              mapToSoftkey);
                }
            }
            _inHyperlink = true;
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderEndHyperlink"]/*' />
        public override void RenderEndHyperlink(bool breakAfter)
        {
            if (!_inHyperlink)
            {
                throw new Exception();
            }

            _inHyperlink = false;
            if (AnalyzeMode)
            {
                CheckRawOutput();
                if (CurrentCard.MenuCandidate)
                {
                    CurrentCard.RenderAsMenu = true;
                }
                CurrentCard.HasNonStaticElements = true;

                if (_inSoftkey)
                {
                    // Add a softkey if possible.

                    _inSoftkey = false;
                    UpHyperlinkSoftkey softkey = new UpHyperlinkSoftkey();
                    softkey.TargetUrl = _targetUrl;
                    softkey.EncodeUrl = _encodeUrl;
                    if (_softkeyLabel == null || _softkeyLabel.Length == 0)
                    {
                        _softkeyLabel = _linkText;
                    }
                    softkey.Label = _softkeyLabel;
                    CurrentCard.Softkeys[CurrentCard.SoftkeysUsed++] = softkey;
                }
            }
            else
            {
                if (CurrentCard.RenderAsMenu)
                {
                    Write("<option onpick=\"");
                    if (_targetUrl.StartsWith(Constants.FormIDPrefix, StringComparison.Ordinal))
                    {
                        // no encoding needed if pointing to another form id
                        Write(_targetUrl);
                    }
                    else if (!_encodeUrl)
                    {
                        Write(EscapeAmpersand(_targetUrl));
                    }
                    else
                    {
                        WriteEncodedUrl(_targetUrl);
                    }
                    Write("\"");

                    if (_useMenuOptionTitle && IsValidSoftkeyLabel(_softkeyLabel))
                    {
                        WriteTextEncodedAttribute("title", _softkeyLabel);
                    }

                    Write(">");
                    WriteEncodedText(_linkText);
                    WriteEndTag("option");
                }
                else if (!CurrentCard.UsesDefaultSubmit)
                {
                    base.RenderEndHyperlink(breakAfter);
                }

            }
            _currentCardAnchorCount++;
            
            if (!AnalyzeMode &&
                    _currentCardAnchorCount == CurrentCard.AnchorCount && 
                    _currentCardIndex < _cards.Count - 1)
            {
                BeginNextCard();
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderTextBox"]/*' />
        public override void RenderTextBox(String id, 
                                           String value,
                                           String format, 
                                           String title,
                                           bool password, 
                                           int size, 
                                           int maxLength, 
                                           bool generateRandomID,
                                           bool breakAfter)
        {
            if (AnalyzeMode)
            {
                CheckRawOutput();

                // If an anchor precedes this control, then break to the
                // next card.
                if (_currentCardAnchorCount > 0)
                {
                    BeginNextCard();
                }
                else if (CurrentCard.HasInputElements && 
                            (!Device.CanRenderInputAndSelectElementsTogether ||
                                !Device.CanRenderAfterInputOrSelectElement))
                {
                    BeginNextCard();
                }

                CurrentCard.RenderAsMenu = false;
                CurrentCard.MenuCandidate = false;
                CurrentCard.HasNonStaticElements = true;
                CurrentCard.HasInputElements = true;
            }
            else
            {
                // Don't write breaks after textboxes on UP.
                base.RenderTextBox(id, value, format, title, password, size, maxLength, generateRandomID, false);

                // If we can't render more than one input element on this card, and
                // there are no anchors left to render, break here.

                if (!Device.CanRenderAfterInputOrSelectElement && _currentCardIndex < _cards.Count - 1)
                {
                    CurrentCard.NoOKLink = true;
                    BeginNextCard();
                }
                else if (CurrentCard.AnchorCount == 0 && _currentCardIndex < _cards.Count - 1 &&
                        !Device.CanRenderInputAndSelectElementsTogether)
                {
                    CurrentCard.NoOKLink = true;
                    BeginNextCard();
                }
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderImage"]/*' />
        public override void RenderImage(String source, 
                                         String localSource, 
                                         String alternateText, 
                                         bool breakAfter)
        {
            if (AnalyzeMode)
            {
                if (CurrentCard.HasInputElements && !Device.CanRenderAfterInputOrSelectElement)
                {
                    BeginNextCard();
                }

                CheckRawOutput();

                if (_inHyperlink || _inPostBack)
                {
                    CurrentCard.RenderAsMenu = false;
                    CurrentCard.MenuCandidate = false;
                }
                else
                {
                    if (CurrentCard.RenderAsMenu)
                    {
                        // Images cannot come after a menu on a card.
                        CurrentCard.RenderAsMenu = false;
                        CurrentCard.MenuCandidate = false;
                    }
                    else if (CurrentCard.MenuCandidate)
                    {
                        CurrentCard.StaticItemsWeight += _screenWidth;
                    }
                }
            }
            else
            {
                // AUI 4137
                if (source != null)
                {
                    source = Page.MakePathAbsolute(source);
                }

                if (_inHyperlink || _inPostBack)
                {
                    if (CurrentCard.RenderAsMenu)
                    {
                        _linkText += alternateText;
                    }
                    else if (!CurrentCard.UsesDefaultSubmit)
                    {
                        base.RenderImage(source, localSource, alternateText, breakAfter);
                    }
                }
                else
                {
                    base.RenderImage(source, localSource, alternateText, breakAfter);
                }
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderBeginPostBack"]/*' />
        public override void RenderBeginPostBack(String softkeyLabel, 
                                                 bool implicitSoftkeyLabel, 
                                                 bool mapToSoftkey)
        {
            if (_inHyperlink || _inPostBack)
            {
                throw new Exception();
            }

            if (AnalyzeMode)
            {
                if (CurrentCard.HasInputElements && !Device.CanRenderAfterInputOrSelectElement)
                {
                    BeginNextCard();
                }

                CheckRawOutput();

                // Try to map to softkey if possible.
                if (mapToSoftkey && CurrentCard.SoftkeysUsed < NumberOfSoftkeys)
                {
                    _inSoftkey = true;
                    _softkeyLabel = softkeyLabel;
                    _linkText = String.Empty;
                }

            }
            else
            {
                if (CurrentCard.RenderAsMenu)
                {
                    if (!CurrentCard.MenuOpened)
                    {
                        OpenMenu();
                    }

                    // In menu mode, actual rendering is done on RenderEndPostBack,
                    // when we have all available info.
                    _softkeyLabel = softkeyLabel;
                    _useMenuOptionTitle = mapToSoftkey && !implicitSoftkeyLabel;
                    _linkText = String.Empty;
                }
                else if (!CurrentCard.UsesDefaultSubmit)
                {
                    base.RenderBeginPostBack(softkeyLabel, implicitSoftkeyLabel, mapToSoftkey);
                }
            }
            _inPostBack = true;
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderEndPostBack"]/*' />
        public override void RenderEndPostBack(String target, String argument, WmlPostFieldType postBackType, bool includeVariables, bool breakAfter)
        {
            if (!_inPostBack)
            {
                throw new Exception();
            }

            _inPostBack = false;
            if (AnalyzeMode)
            {
                CheckRawOutput();

                if (CurrentCard.MenuCandidate)
                {
                    // If all postback menu items go to one target, we can write the do
                    // to hit that target. Otherwise, we must post back to the form.

                    if (CurrentCard.RenderAsMenu)
                    {
                        if (CurrentCard.MenuTarget != target)
                        {
                            CurrentCard.MenuTarget = null;
                        }
                    }
                    else
                    {
                        CurrentCard.MenuTarget = target;
                        CurrentCard.RenderAsMenu = true;
                    }
                }
                CurrentCard.HasNonStaticElements = true;

                if (_inSoftkey)
                {
                    // Map to softkey.
                    _inSoftkey = false;
                    UpPostBackSoftkey softkey = new UpPostBackSoftkey();
                    softkey.Target = target;
                    softkey.Argument = argument;
                    softkey.PostBackType = postBackType;
                    softkey.IncludeVariables = includeVariables;
                    if (_softkeyLabel == null || _softkeyLabel.Length == 0)
                    {
                        _softkeyLabel = _linkText;
                    }
                    softkey.Label = _softkeyLabel;
                    CurrentCard.Softkeys[CurrentCard.SoftkeysUsed++] = softkey;
                }
                AnalyzePostBack(includeVariables, postBackType);
            }
            else
            {
                if (CurrentCard.RenderAsMenu)
                {
                    // Render as a menu item.

                    WriteBeginTag("option");
                    if (!_canRenderMixedSelects)
                    {
                        if (_useMenuOptionTitle && IsValidSoftkeyLabel(_softkeyLabel))
                        {
                            WriteTextEncodedAttribute("title", _softkeyLabel);
                        }

                        _requiresOptionSubmitCard = true;
                        Write("><onevent type=\"onpick\"><go href=\"#");
                        Write(_postBackCardPrefix);
                        Write("0");
                        Write(_optionSubmitCardIndex);
                        Write("\">");
                        Write("<setvar name=\"");
                        Write(_postBackEventTargetVarName);
                        Write("\" value=\"");
                        if (_optionMenuName != null)
                        {
                            Write(_optionMenuName);
                        }
                        Write("\" />");
                        Write("<setvar name=\"");
                        Write(_postBackEventArgumentVarName);
                        Write("\" value=\"");
                    }
                    else
                    {
                        Write(" value=\"");
                    }

                    if (CurrentCard.MenuTarget != null)
                    {
                        if (argument != null)
                        {
                            WriteEncodedText(argument);
                        }
                    }
                    else
                    {
                        WriteEncodedText(target);
                        if (argument != null)
                        {
                            Write(",");
                            WriteEncodedText(argument);
                        }
                    }
                    if (!_canRenderMixedSelects)
                    {
                        Write("\" /></go></onevent>");
                    }
                    else
                    {
                        Write("\"");
                        if (_useMenuOptionTitle && IsValidSoftkeyLabel(_softkeyLabel))
                        {
                            WriteTextEncodedAttribute("title", _softkeyLabel);
                        }
                        Write(">");
                    }

                    WriteEncodedText(_linkText);
                    WriteEndTag("option");
                }
                else if (!CurrentCard.UsesDefaultSubmit)
                {
                    base.RenderEndPostBack(target, argument, postBackType, 
                            includeVariables, breakAfter);
                }
            }
            _currentCardAnchorCount++;

            if (!AnalyzeMode &&
                    _currentCardAnchorCount == CurrentCard.AnchorCount && 
                    _currentCardIndex < _cards.Count - 1)
            {
                BeginNextCard();
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.BeginCustomMarkup"]/*' />
        public override void BeginCustomMarkup()
        {
            if (!AnalyzeMode && !CurrentCard.MenuOpened)
            {
                EnsureLayout();
            }
        }


        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.AnalyzePostBack"]/*' />
        protected override void AnalyzePostBack(bool includeVariables, WmlPostFieldType postBackType)
        {
            base.AnalyzePostBack(includeVariables, postBackType);
            if (CurrentForm.Action.Length > 0)
            {
                if (postBackType == WmlPostFieldType.Submit)
                {
                    _currentCardSubmits++;
                    CurrentCard.ExternalSubmitMenu = true;
                }
                else
                {
                    _currentCardPostBacks++;
                }
                if (_currentCardPostBacks > 0 && _currentCardSubmits > 0)
                {
                    // Posts to more than one target, so we can't use a menu card.

                    CurrentCard.RenderAsMenu = false;
                    CurrentCard.MenuCandidate = false;
                }
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderBeginSelect"]/*' />
        public override void RenderBeginSelect(String name, String iname, String ivalue, String title, bool multiSelect)
        {
            if (AnalyzeMode)
            {
                CheckRawOutput();
                if (_currentCardAnchorCount > 0)
                {
                    // If an anchor precedes this control, then break to the
                    // next card.
                    BeginNextCard();
                }
                else if (CurrentCard.HasInputElements &&
                            (!Device.CanRenderInputAndSelectElementsTogether ||
                                !Device.CanRenderAfterInputOrSelectElement))
                {
                    BeginNextCard();
                }

                CurrentCard.RenderAsMenu = false;
                CurrentCard.MenuCandidate = false;
                CurrentCard.HasNonStaticElements = true;
                CurrentCard.HasInputElements = true;
            }
            else
            {
                base.RenderBeginSelect(name, iname, ivalue, title, multiSelect);
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderEndSelect"]/*' />
        public override void RenderEndSelect(bool breakAfter)
        {
            if (AnalyzeMode)
            {
                CheckRawOutput();
            }
            else
            {
                // Don't write breaks after selects on UP.
                base.RenderEndSelect(false);

                // If we can't render more than one input element on this card, and
                // there are no anchors left to render, break here.

                if (!Device.CanRenderAfterInputOrSelectElement && _currentCardIndex < _cards.Count - 1)
                {
                    CurrentCard.NoOKLink = true;
                    BeginNextCard();
                } 
                else if (CurrentCard.AnchorCount == 0 && _currentCardIndex < _cards.Count - 1 &&
                        !Device.CanRenderInputAndSelectElementsTogether)
                {
                    CurrentCard.NoOKLink = true;
                    BeginNextCard();
                }
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderSelectOption"]/*' />
        public override void RenderSelectOption(String text)
        {
            if (AnalyzeMode)
            {
                CheckRawOutput();
            }
            else
            {
                base.RenderSelectOption(text);
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.RenderSelectOption1"]/*' />
        public override void RenderSelectOption(String text, String value)
        {
            if (AnalyzeMode)
            {
                CheckRawOutput();
            }
            else
            {
                base.RenderSelectOption(text, value);
            }
        }

        private void OpenMenu()
        {
            // Close any character formatting tags before starting a <do>.
            CloseCharacterFormat();

            String menuTarget;
            String menuTargetClientID;
            WmlPostFieldType postFieldType;

            if (CurrentCard.ExternalSubmitMenu)
            {
                menuTarget = null;
                menuTargetClientID = null;
                postFieldType = WmlPostFieldType.Submit;
            }
            else
            {
                if (CurrentCard.MenuTarget != null)
                {
                    menuTarget = CurrentCard.MenuTarget;
                    if (menuTarget.IndexOf(":", StringComparison.Ordinal) >= 0) 
                    {
                        menuTargetClientID = menuTarget.Replace(":", "_");
                    }
                    else
                    {
                        menuTargetClientID = menuTarget;
                    }
                }
                else
                {
                    menuTarget = CurrentForm.UniqueID;
                    menuTargetClientID = CurrentForm.ClientID;
                }

                postFieldType = WmlPostFieldType.Variable;
            }

            if (!_canRenderMixedSelects)
            {
                _optionMenuName = menuTarget;
                menuTargetClientID = null;
            }
            else
            {
                String GoLabel = SR.GetString(SR.WmlMobileTextWriterGoLabel);
                RenderDoEvent("accept", 
                              menuTarget, 
                              menuTargetClientID != null ? MapClientIDToShortName(menuTargetClientID, false) : null,
                              postFieldType,
                              GoLabel,
                              true);
            }

            base.RenderBeginSelect(menuTargetClientID, null, null, null, false);

            CurrentCard.MenuOpened = true;
        }

        private void CloseMenu()
        {
            base.RenderEndSelect(false);
            CurrentCard.MenuOpened = false;
        }

        // Overriden to convert relative file paths to absolute file paths,
        // due to a redirection issue on UP phones.

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.CalculateFormPostBackUrl"]/*' />
        protected override String CalculateFormPostBackUrl(bool externalSubmit, ref bool encode)
        {
            String url = CurrentForm.Action;
            if (externalSubmit && url.Length > 0)
            {
                // Not only do we need to resolve the URL, but we need to make it absolute.
                url = Page.MakePathAbsolute(CurrentForm.ResolveUrl(url));
                encode = false;
            }
            else
            {
                url = Page.AbsoluteFilePath;
                encode = true;
            }
            return url;
        }

        // Captures raw output written to the writers during analyze mode.

        private void CheckRawOutput()
        {
            Debug.Assert(AnalyzeMode);

            EmptyTextWriter innerWriter = (EmptyTextWriter)InnerWriter;
            if (innerWriter.NonWhiteSpaceWritten)
            {
                CurrentCard.RenderAsMenu = false;
                CurrentCard.MenuCandidate = false;
            }
            innerWriter.Reset();
        }

        // Overriden to always write the "align" or "wrap" attributes when
        // they are changing, even if they are set to defaults.

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.OpenParagraph"]/*' />
        protected override void OpenParagraph(WmlLayout layout, bool writeAlignment, bool writeWrapping)
        {
            base.OpenParagraph(layout,
                               writeAlignment || layout.Align != _lastAlignment,
                               writeWrapping  || layout.Wrap != _lastWrapping);
            _lastAlignment = layout.Align;
            _lastWrapping = layout.Wrap;
        }

        // Resets writer state between forms.

        private void ResetState()
        {
            if (AnalyzeMode)
            {
                _currentCardPostBacks = 0;
                _currentCardSubmits = 0;
                _inHyperlink = false;
                _inPostBack = false;
                _inSoftkey = false;
                if (_cards.Count > 0)
                {
                    _cards.Clear();
                }
            }
            _currentCardAnchorCount = 0;
            _currentCardIndex = 0;
            _currentCardPostBacks = 0;
            _currentCardSubmits = 0;
            _requiresOptionSubmitCard = false;
        }

        private UpCard AllocateNewCard()
        {
            UpCard card = new UpCard();
            card.Softkeys = new UpSoftkey[NumberOfSoftkeys];
            _cards.Add(card);
            return card;
        }

        // Analyze an individual card.

        private void PostAnalyzeCard(int cardIndex)
        {
            UpCard card = (UpCard)_cards[cardIndex];
            if (card.RenderAsMenu)
            {
                // If the card is the last card, and has only
                // one anchor that can be mapped to a softkey, 
                // ignore the 

                if (card.AnchorCount == 1 && cardIndex == _cards.Count - 1 && card.SoftkeysUsed == 1)
                {
                    card.RenderAsMenu = false;
                }

                // If the card has a lot of static content followed by
                // a number of links, don't render it as a menu card, 
                // because the card would scroll off the static content
                // to get to the menu.

                else if (card.StaticItemsWeight >= 3 * _screenWidth)
                {
                    card.RenderAsMenu = false;
                }
            }
        }

        /// <include file='doc\UpWmlMobileTextWriter.uex' path='docs/doc[@for="UpWmlMobileTextWriter.PostAnalyzeForm"]/*' />
        protected override void PostAnalyzeForm()
        {
            base.PostAnalyzeForm();

            for(int i = 0; i < _cards.Count; i++)
            {
                PostAnalyzeCard(i);
            }

            // If the last card ends with an input element and an anchor
            // then use the anchor as a do tag to submit the form, and
            // don't render as a separate anchor (otherwise, the user would
            // see an extra screen at the end with a single anchor)
            UpCard lastCard = CurrentCard;
            if (lastCard.HasInputElements && _currentCardAnchorCount >= 1 && 
                    _currentCardAnchorCount <= Device.DefaultSubmitButtonLimit)
            {
                lastCard.UsesDefaultSubmit = true;
            }
        }

        private void RenderCardOpening(int cardIndex)
        {
            UpCard card = (UpCard)_cards[cardIndex];
            if (card.RenderAsMenu)
            {
            }
            else
            {
                // Render softkeys
                if (card.HasNonStaticElements)
                {
                    for (int i = 0; i < card.SoftkeysUsed; i++)
                    {
                        RenderSoftkey(i == 0 ? "accept" : "options", card.Softkeys[i]);
                    }
                }
                else if (cardIndex == _cards.Count - 1)
                {
                    // Render the last card with an extra <do>, so that
                    // it overrides the default function of the OK button,
                    // which is to go back a screen.
                    //EnsureLayout();
                    Write("<do type=\"accept\"><noop /></do>");
                }
            }
        }

        private void RenderCardClosing(int cardIndex)
        {
            UpCard card = (UpCard)_cards[cardIndex];

            if (cardIndex < _cards.Count - 1 && !card.NoOKLink)
            {
                // Add a link to go to the next card.

                UpCard nextCard = (UpCard)_cards[cardIndex + 1];
                String OkLabel = SR.GetString(SR.WmlMobileTextWriterOKLabel);
                RenderBeginHyperlink("#" + nextCard.Id, false, OkLabel, true, true);
                RenderText(OkLabel);
                RenderEndHyperlink(true);
            }

            if (card.RenderAsMenu)
            {
                CloseMenu();
            }
        }

        private void BeginNextCard()
        {
            if (AnalyzeMode)
            {
                // Add a softkey on the current card, to go to the new card.

                String nextCardId = "card" + (_currentCardIndex + 1).ToString(CultureInfo.InvariantCulture);

                UpHyperlinkSoftkey softkey = new UpHyperlinkSoftkey();
                softkey.TargetUrl = "#" + nextCardId;
                softkey.EncodeUrl = false;
                softkey.Label = "OK";
                SetPrimarySoftkey(softkey);

                CurrentCard.AnchorCount = _currentCardAnchorCount;

                UpCard card = AllocateNewCard();
                card.Id = nextCardId;
                _currentCardIndex++;
            }
            else
            {
                RenderCardClosing(_currentCardIndex);
                CloseParagraph();
                WriteEndTag("card");
                WriteLine();
    
                _currentCardIndex++;
                _lastAlignment = Alignment.Left;
                _lastWrapping = Wrapping.NoWrap;
    
                WriteBeginTag("card");
                WriteAttribute("id", CurrentCard.Id);
                String formTitle = CurrentForm.Title;
                if (formTitle != null && formTitle.Length > 0)
                {
                    WriteTextEncodedAttribute("title", formTitle);
                }
                WriteLine(">");
                RenderCardOpening(_currentCardIndex);
            }

            _currentCardAnchorCount = 0;
            _currentCardPostBacks = 0;
            _currentCardSubmits = 0;
        }

        private void SetPrimarySoftkey(UpSoftkey softkey)
        {
            for (int i = NumberOfSoftkeys - 1; i > 0; i--)
            {
                CurrentCard.Softkeys[i] = CurrentCard.Softkeys[i - 1];
            }
            CurrentCard.Softkeys[0] = softkey;
            if (CurrentCard.SoftkeysUsed < NumberOfSoftkeys)
            {
                CurrentCard.SoftkeysUsed++;
            }
        }

        private void RenderSoftkey(String doType, UpSoftkey softkey)
        {
            UpHyperlinkSoftkey linkSoftkey = softkey as UpHyperlinkSoftkey;
            if (linkSoftkey != null)
            {
                WriteBeginTag("do");
                WriteAttribute("type", doType);
                WriteTextEncodedAttribute("label", linkSoftkey.Label);
                Write(">");
                WriteBeginTag("go");
                Write(" href=\"");

                if (linkSoftkey.EncodeUrl)
                {
                    WriteEncodedUrl(linkSoftkey.TargetUrl);
                }
                else
                {
                    Write(EscapeAmpersand(linkSoftkey.TargetUrl));
                }
                Write("\" />");
                WriteEndTag("do");
                return;
            }

            UpPostBackSoftkey postBackSoftkey = softkey as UpPostBackSoftkey;
            if (postBackSoftkey != null)
            {
                RenderDoEvent(doType, 
                              postBackSoftkey.Target, 
                              postBackSoftkey.Argument, 
                              postBackSoftkey.PostBackType, 
                              postBackSoftkey.Label,
                              postBackSoftkey.IncludeVariables); 
                return;
            }
        }

        private class UpSoftkey
        {
            public String Label;
        }

        private class UpHyperlinkSoftkey : UpSoftkey
        {
            public String TargetUrl;
            public bool EncodeUrl;
        }

        private class UpPostBackSoftkey : UpSoftkey
        {
            public String Target;
            public String Argument;
            public WmlPostFieldType PostBackType;
            public bool IncludeVariables;
        }

        private class UpCard
        {
            public String Id;
            public bool MenuCandidate = true;
            public bool RenderAsMenu = false;
            public int StaticItemsWeight = 0;
            public bool HasNonStaticElements = false;
            public bool MenuOpened = false;
            public bool HasInputElements = false;
            public bool UsesDefaultSubmit = false;
            public int SoftkeysUsed = 0;
            public UpSoftkey[] Softkeys = null;
            public int AnchorCount = 0;
            public String MenuTarget = null;
            public bool ExternalSubmitMenu = false;
            public bool NoOKLink = false;
            public bool RenderedTextElementYet = false;
            
        }

    }
}
