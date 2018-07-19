//------------------------------------------------------------------------------
// <copyright file="PageStatePersister.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Collections;

    public abstract class PageStatePersister {
        
        private Page _page;
        private object _viewState;
        private object _controlState;
        private IStateFormatter2 _stateFormatter;

        protected PageStatePersister (Page page) {
            if (page == null) {
                throw new ArgumentNullException("page", SR.GetString(SR.PageStatePersister_PageCannotBeNull));
            }
            _page = page;
        }

        public object ControlState {
            get {
                return _controlState;
            }
            set {
                _controlState = value;
            }
        }

        /// <devdoc>
        /// Provides the formatter used to serialize and deserialize the object graph representing the
        /// state to be persisted.
        /// </devdoc>
        protected IStateFormatter StateFormatter {
            get { return StateFormatter2; }
        }

        internal IStateFormatter2 StateFormatter2 {
            get {
                if (_stateFormatter == null) {
                    _stateFormatter = Page.CreateStateFormatter();
                }
                return _stateFormatter;
            }
        }

        protected Page Page {
            get {
                return _page;
            }
            set {
                _page = value;
            }
        }

        public object ViewState {
            get {
                return _viewState;
            }
            set {
                _viewState = value;
            }
        }
        
        public abstract void Load();
        
        public abstract void Save();
    }
}
