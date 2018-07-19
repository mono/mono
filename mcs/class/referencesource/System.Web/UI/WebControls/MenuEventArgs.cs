//------------------------------------------------------------------------------
// <copyright file="MenuEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System;

    public sealed class MenuEventArgs : CommandEventArgs {
        private MenuItem _item;
        private object _commandSource;

        public MenuEventArgs(MenuItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
            _item = item;
            _commandSource = commandSource;
        }

        public MenuEventArgs(MenuItem item) : this(item, null, new CommandEventArgs(String.Empty, null)) {
        }

        public object CommandSource {
            get {
                return _commandSource;
            }
        }
        
        public MenuItem Item {
            get {
                return _item;
            }
        }
    }
}
