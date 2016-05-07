namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Standard commands controling the PropertyValueEditing experience
    /// </summary>
    public static class PropertyValueEditorCommands {
        // PropertyContainer mode-switching commands
        private static RoutedCommand _showInlineEditor;
        private static RoutedCommand _showExtendedPopupEditor;
        private static RoutedCommand _showExtendedPinnedEditor;
        private static RoutedCommand _showDialogEditor;

        // Transaction commands
        private static RoutedCommand _beginTransaction;
        private static RoutedCommand _commitTransaction;
        private static RoutedCommand _abortTransaction;

        // Signal to the editor that PropertyContainer is done editing a particular value.
        // It is up to the host to interpret this command as it sees fit.  Cider may decide
        // to highlight the next property in the list.  Sparkle will return the focus back to
        // the design surface.
        private static RoutedCommand _finishEditing;

        /// <summary>
        /// Editors may raise this command to switch PropertyContainer mode Inline
        /// </summary>
        public static RoutedCommand ShowInlineEditor {
            get {
                if (_showInlineEditor == null)
                    _showInlineEditor = new RoutedCommand("ShowInlineEditor", typeof(PropertyValueEditorCommands));
                
                return _showInlineEditor; 
            }
        }

        /// <summary>
        /// Editors may raise this command to switch PropertyContainer mode ExtendedPopup
        /// </summary>
        public static RoutedCommand ShowExtendedPopupEditor {
            get {
                if (_showExtendedPopupEditor == null)
                    _showExtendedPopupEditor = new RoutedCommand("ShowExtendedPopupEditor", typeof(PropertyValueEditorCommands));
                
                return _showExtendedPopupEditor;
            }
        }

        /// <summary>
        /// Editors may raise this command to switch PropertyContainer mode ExtendedPinned
        /// </summary>
        public static RoutedCommand ShowExtendedPinnedEditor {
            get {
                if (_showExtendedPinnedEditor == null)
                    _showExtendedPinnedEditor = new RoutedCommand("ShowExtendedPinnedEditor", typeof(PropertyValueEditorCommands));
                
                return _showExtendedPinnedEditor;
            } 
        }

        /// <summary>
        /// Editors may raise this command to switch PropertyContainer mode Dialog
        /// </summary>
        public static RoutedCommand ShowDialogEditor {
            get {
                if (_showDialogEditor == null)
                    _showDialogEditor = new RoutedCommand("ShowDialogEditor", typeof(PropertyValueEditorCommands));
                
                return _showDialogEditor;
            }
        }

        /// <summary>
        /// Editors may raise this command to begin a transaction.
        /// </summary>
        public static RoutedCommand BeginTransaction {
            get {
                if (_beginTransaction == null)
                    _beginTransaction = new RoutedCommand("BeginTransaction", typeof(PropertyValueEditorCommands));
                
                return _beginTransaction; 
            }
        }

        /// <summary>
        /// Editors may raise this command to commit a transaction. If this command is
        /// executed when there aren't any open transactions, an exception will be thrown.
        /// </summary>
        public static RoutedCommand CommitTransaction {
            get {
                if (_commitTransaction == null)
                    _commitTransaction = new RoutedCommand("CommitTransaction", typeof(PropertyValueEditorCommands));
                
                return _commitTransaction; 
            } 
        }

        /// <summary>
        /// Editors may raise this command to abort a transaction. If this command is
        /// executed when there aren't any open transactions, an exception will be thrown.
        /// </summary>
        public static RoutedCommand AbortTransaction { 
            get {
                if (_abortTransaction == null)
                    _abortTransaction = new RoutedCommand("AbortTransaction", typeof(PropertyValueEditorCommands));
                
                return _abortTransaction;
            }
        }

        /// <summary>
        /// Editors may raise this command to indicate to the host that they have finished editing.
        /// This allows the host to do cleanup or potentially change the focus to a different UIElement.
        /// </summary>
        public static RoutedCommand FinishEditing {
            get {
                if (_finishEditing == null)
                    _finishEditing = new RoutedCommand("FinishEditing", typeof(PropertyValueEditorCommands));
                
                return _finishEditing;
            }
        }
    }
}
