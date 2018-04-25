//------------------------------------------------------------------------------
// <copyright file="PostBackOptions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * PostBackOptions class definition
 *
 * Copyright (c) 2003 Microsoft Corporation
 */
namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Security.Permissions;

    public sealed class PostBackOptions {

        private string _actionUrl;
        private string _argument;
        private string _validationGroup;

        private bool _autoPostBack;
        private bool _requiresJavaScriptProtocol;
        private bool _performValidation;
        private bool _trackFocus;
        private bool _clientSubmit = true;

        private Control _targetControl;


        public PostBackOptions(Control targetControl) :
            this(targetControl, null, null, false, false, false, true, false, null) {
        }


        public PostBackOptions(Control targetControl, string argument) :
            this(targetControl, argument, null, false, false, false, true, false, null) {
        }


        public PostBackOptions(Control targetControl, string argument, string actionUrl, bool autoPostBack,
             bool requiresJavaScriptProtocol, bool trackFocus, bool clientSubmit, bool performValidation, string validationGroup) {

            if (targetControl == null)
                throw new ArgumentNullException("targetControl");

            _actionUrl = actionUrl;
            _argument = argument;
            _autoPostBack = autoPostBack;
            _clientSubmit = clientSubmit;
            _requiresJavaScriptProtocol = requiresJavaScriptProtocol;
            _performValidation = performValidation;
            _trackFocus = trackFocus;
            _targetControl = targetControl;
            _validationGroup = validationGroup;
        }


        [DefaultValue("")]
        public string ActionUrl {
            get {
                return _actionUrl;
            }
            set {
                _actionUrl = value;
            }
        }


        [DefaultValue("")]
        public string Argument {
            get {
                return _argument;
            }
            set {
                _argument = value;
            }
        }


        [DefaultValue(false)]
        public bool AutoPostBack {
            get {
                return _autoPostBack;
            }
            set {
                _autoPostBack = value;
            }
        }


        [DefaultValue(true)]
        public bool ClientSubmit {
            get {
                return _clientSubmit;
            }
            set {
                _clientSubmit = value;
            }
        }


        [DefaultValue(true)]
        public bool RequiresJavaScriptProtocol {
            get {
                return _requiresJavaScriptProtocol;
            }
            set {
                _requiresJavaScriptProtocol = value;
            }
        }


        [DefaultValue(false)]
        public bool PerformValidation {
            get {
                return _performValidation;
            }
            set {
                _performValidation = value;
            }
        }


        [DefaultValue("")]
        public string ValidationGroup {
            get {
                return _validationGroup;
            }
            set {
                _validationGroup = value;
            }
        }


        [DefaultValue(null)]
        public Control TargetControl {
            get {
                return _targetControl;
            }
        }


        [DefaultValue(false)]
        public bool TrackFocus {
            get {
                return _trackFocus;
            }
            set {
                _trackFocus = value;
            }
        }
    }
}
