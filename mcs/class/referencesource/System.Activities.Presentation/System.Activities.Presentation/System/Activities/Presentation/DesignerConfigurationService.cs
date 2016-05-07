//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Versioning;
    using Microsoft.Activities.Presentation;

    internal enum WorkflowDesignerHostId
    {
        Rehost,
        Dev10,
        Dev11
    }

    /// <summary>
    /// Stores configuration information of designer
    /// </summary>
    public sealed class DesignerConfigurationService
    {
        private static FrameworkName defaultTargetFrameworkName = FrameworkNameConstants.NetFramework40;

        private bool annotationEnabled;
        private bool autoConnectEnabled;
        private bool autoSplitEnabled;
        private bool autoSurroundWithSequenceEnabled;
        private bool backgroundValidationEnabled;
        private bool isAnnotationEnabledSetByUser;
        private bool isAutoConnectEnabledSetByUser;
        private bool isAutoSplitEnabledSetByUser;
        private bool isAutoSurroundWithSequenceEnabledSetByUser;
        private bool isBackgroundValidationEnabledSetByUser;
        private bool isLoadingFromUntrustedSourceEnabledSetByUser;
        private bool isMultipleItemsDragDropEnabledSetByUser;
        private bool isMultipleItemsContextMenuEnabledSetByUser;
        private bool isPanModeEnabledSetByUser;
        private bool isRubberBandSelectionEnabledSetByUser;
        private bool loadingFromUntrustedSourceEnabled;
        private bool multipleItemsDragDropEnabled;
        private bool multipleItemsContextMenuEnabled;
        private bool namespaceConversionEnabled;
        private bool panModeEnabled;
        private bool rubberBandSelectionEnabled;
        private FrameworkName targetFrameworkName;
        private WorkflowDesignerHostId workflowDesignerHostId;

        /// <summary>
        /// Default constructor
        /// </summary>
        internal DesignerConfigurationService()
        {
            this.namespaceConversionEnabled = true;
            this.loadingFromUntrustedSourceEnabled = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether annotation feature is enabled.
        /// Can only be set before WorkflowDesigner.Load()
        /// Can only be enabled when target framework is 4.5 or higher
        /// </summary>
        public bool AnnotationEnabled
        {
            get
            {
                return this.annotationEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isAnnotationEnabledSetByUser = true;
                this.annotationEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto connect feature is enabled.
        /// </summary>
        public bool AutoConnectEnabled
        {
            get
            {
                return this.autoConnectEnabled;
            }

            set
            {
                this.isAutoConnectEnabledSetByUser = true;
                this.autoConnectEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto split feature is enabled.
        /// </summary>
        public bool AutoSplitEnabled
        {
            get
            {
                return this.autoSplitEnabled;
            }

            set
            {
                this.isAutoSplitEnabledSetByUser = true;
                this.autoSplitEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether auto surround with sequence is enabled.
        /// </summary>
        public bool AutoSurroundWithSequenceEnabled
        {
            get
            {
                return this.autoSurroundWithSequenceEnabled;
            }

            set
            {
                this.isAutoSurroundWithSequenceEnabledSetByUser = true;
                this.autoSurroundWithSequenceEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether background validation is enabled.
        /// </summary>
        public bool BackgroundValidationEnabled
        {
            get
            {
                return this.backgroundValidationEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isBackgroundValidationEnabledSetByUser = true;
                this.backgroundValidationEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether allow loading from untrusted source is enabled. 
        /// If false, loading from untrusted source will cause a SecurityException thrown out when WorkflowDesigner.Load(string fileName).
        /// Can only be set before WorkflowDesigner.Load()
        /// </summary>
        public bool LoadingFromUntrustedSourceEnabled
        {
            get
            {
                return this.loadingFromUntrustedSourceEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isLoadingFromUntrustedSourceEnabledSetByUser = true;
                this.loadingFromUntrustedSourceEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Context Menu can be displayed when selecting multiple items.
        /// Can only be set before WorkflowDesigner.Load()
        /// </summary>
        public bool MultipleItemsContextMenuEnabled
        {
            get
            {
                return this.multipleItemsContextMenuEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isMultipleItemsContextMenuEnabledSetByUser = true;
                this.multipleItemsContextMenuEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether dragging multiple items is enabled.
        /// Can only be set before WorkflowDesigner.Load()
        /// </summary>
        public bool MultipleItemsDragDropEnabled
        {
            get
            {
                return this.multipleItemsDragDropEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isMultipleItemsDragDropEnabledSetByUser = true;
                this.multipleItemsDragDropEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether namespace conversion is enabled.
        /// Can only be set before WorkflowDesigner.Load()
        /// </summary>
        public bool NamespaceConversionEnabled
        {
            get
            {
                return this.namespaceConversionEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.namespaceConversionEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether pan mode is enabled.
        /// Can only be set before WorkflowDesigner.Load()
        /// </summary>
        public bool PanModeEnabled
        {
            get
            {
                return this.panModeEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isPanModeEnabledSetByUser = true;
                this.panModeEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether rubberband selection is enabled.
        /// Can only be set before WorkflowDesigner.Load()
        /// </summary>
        public bool RubberBandSelectionEnabled
        {
            get
            {
                return this.rubberBandSelectionEnabled;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.isRubberBandSelectionEnabledSetByUser = true;
                this.rubberBandSelectionEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the target framework.
        /// Can only be set before WorkflowDesigner.Load()
        /// The framework identifier only supports '.NET Framework' and '.NETFramework'.
        /// The framework profile only supports string.Empty or 'Client'.
        /// </summary>
        public FrameworkName TargetFrameworkName
        {
            get
            {
                if (this.targetFrameworkName == null)
                {
                    this.targetFrameworkName = defaultTargetFrameworkName;
                }

                return this.targetFrameworkName;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("TargetFrameworkName");
                }

                if (value.Identifier != FrameworkNameConstants.NetFramework
                 && value.Identifier != FrameworkNameConstants.NetFrameworkWithSpace)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, SR.NotSupportedFrameworkIdentifier, value.Identifier)));
                }

                if (!value.IsProfileSupported())
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, SR.NotSupportedFrameworkProfile, value.Profile)));
                }

                if (value.IsLessThan40())
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, SR.NotSupportedFrameworkVersion, value.Version.ToString())));
                }

                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.targetFrameworkName = value;
            }
        }

        internal static FrameworkName DefaultTargetFrameworkName
        {
            get
            {
                return defaultTargetFrameworkName;
            }
        }

        internal WorkflowDesignerHostId WorkflowDesignerHostId
        {
            get
            {
                return this.workflowDesignerHostId;
            }

            set
            {
                if (this.IsWorkflowLoaded)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CouldNotSetPropertyAfterLoad));
                }

                this.workflowDesignerHostId = value;
            }
        }

        internal bool IsWorkflowLoaded { get; set; }

        /// <summary>
        /// Apply default preference based on TargetFramework and hoster.
        /// If some flags were already set by user, don't set it to default.
        /// </summary>
        internal void ApplyDefaultPreference()
        {
            Fx.Assert(!this.IsWorkflowLoaded, "Cannot be invoked after WorkflowDesigner.Load");

            if (this.TargetFrameworkName.Is45OrHigher())
            {
                // Dev11 + 4.5 and Rehost + 4.5, open all features.
                // Dev10 + 4.5, invalid case.
                this.SetAnnotationIfNotSetByUser(true);
                this.SetAutoConnectIfNotSetByUser(true);
                this.SetAutoSplitIfNotSetByUser(true);
                this.SetAutoSurroundWithSequenceIfNotSetByUser(true);
                this.SetBackgroundValidationIfNotSetByUser(true);
                this.SetMultipleItemsContextMenuIfNotSetByUser(true);
                this.SetMultipleItemsDragDropIfNotSetByUser(true);
                this.SetPanModeIfNotSetByUser(true);
                this.SetRubberBandSelectionIfNotSetByUser(true);
            }
            else
            {
                switch (this.workflowDesignerHostId)
                {
                    case WorkflowDesignerHostId.Rehost:
                    case WorkflowDesignerHostId.Dev10:
                        this.SetAnnotationIfNotSetByUser(false);
                        this.SetAutoConnectIfNotSetByUser(false);
                        this.SetAutoSplitIfNotSetByUser(false);
                        this.SetAutoSurroundWithSequenceIfNotSetByUser(false);
                        this.SetBackgroundValidationIfNotSetByUser(false);
                        this.SetMultipleItemsContextMenuIfNotSetByUser(false);
                        this.SetMultipleItemsDragDropIfNotSetByUser(false);
                        this.SetPanModeIfNotSetByUser(false);
                        this.SetRubberBandSelectionIfNotSetByUser(false);
                        break;
                    case WorkflowDesignerHostId.Dev11:
                        this.SetAnnotationIfNotSetByUser(false);
                        this.SetAutoConnectIfNotSetByUser(true);
                        this.SetAutoSplitIfNotSetByUser(true);
                        this.SetAutoSurroundWithSequenceIfNotSetByUser(true);
                        this.SetBackgroundValidationIfNotSetByUser(true);
                        this.SetMultipleItemsContextMenuIfNotSetByUser(true);
                        this.SetMultipleItemsDragDropIfNotSetByUser(true);
                        this.SetPanModeIfNotSetByUser(true);
                        this.SetRubberBandSelectionIfNotSetByUser(true);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// The method should be invoked at WorkflowDesigner.Load(string fileName)
        /// </summary>
        internal void SetDefaultOfLoadingFromUntrustedSourceEnabled()
        {
            Fx.Assert(!this.IsWorkflowLoaded, "Cannot be invoked after WorkflowDesigner.Load");

            // Only Rehost need to apply this flag's default value.
            // Dev11 will opt-off this feature and handle this by itself.
            if (this.WorkflowDesignerHostId == Presentation.WorkflowDesignerHostId.Rehost)
            {
                if (this.TargetFrameworkName.Is45OrHigher())
                {
                    this.SetLoadingFromUntrustedSourceEnabledIfNotSetByUser(false);
                }
            }
        }

        internal void Validate()
        {
            if (this.TargetFrameworkName.IsLessThan45())
            {
                if (this.annotationEnabled)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CantEnableAnnotationBefore45));
                }
            }
        }

        private void SetAnnotationIfNotSetByUser(bool annotationEnable)
        {
            if (!this.isAnnotationEnabledSetByUser)
            {
                this.annotationEnabled = annotationEnable;
            }
        }

        private void SetAutoConnectIfNotSetByUser(bool autoConnectEnabled)
        {
            if (!this.isAutoConnectEnabledSetByUser)
            {
                this.autoConnectEnabled = autoConnectEnabled;
            }
        }

        private void SetAutoSplitIfNotSetByUser(bool autoSplitEnabled)
        {
            if (!this.isAutoSplitEnabledSetByUser)
            {
                this.autoSplitEnabled = autoSplitEnabled;
            }
        }

        private void SetAutoSurroundWithSequenceIfNotSetByUser(bool autoSurroundWithSequenceEnabled)
        {
            if (!this.isAutoSurroundWithSequenceEnabledSetByUser)
            {
                this.autoSurroundWithSequenceEnabled = autoSurroundWithSequenceEnabled;
            }
        }

        private void SetBackgroundValidationIfNotSetByUser(bool backgroundValidationEnabled)
        {
            if (!this.isBackgroundValidationEnabledSetByUser)
            {
                this.backgroundValidationEnabled = backgroundValidationEnabled;
            }
        }

        private void SetMultipleItemsContextMenuIfNotSetByUser(bool multipleItemsContextMenuEnabled)
        {
            if (!this.isMultipleItemsContextMenuEnabledSetByUser)
            {
                this.multipleItemsContextMenuEnabled = multipleItemsContextMenuEnabled;
            }
        }

        private void SetMultipleItemsDragDropIfNotSetByUser(bool multipleItemsDragDropEnabled)
        {
            if (!this.isMultipleItemsDragDropEnabledSetByUser)
            {
                this.multipleItemsDragDropEnabled = multipleItemsDragDropEnabled;
            }
        }

        private void SetPanModeIfNotSetByUser(bool panModeEnabled)
        {
            if (!this.isPanModeEnabledSetByUser)
            {
                this.panModeEnabled = panModeEnabled;
            }
        }

        private void SetRubberBandSelectionIfNotSetByUser(bool rubberBandSelectionEnabled)
        {
            if (!this.isRubberBandSelectionEnabledSetByUser)
            {
                this.rubberBandSelectionEnabled = rubberBandSelectionEnabled;
            }
        }

        private void SetLoadingFromUntrustedSourceEnabledIfNotSetByUser(bool loadingFromUntrustedSourceEnabled)
        {
            if (!this.isLoadingFromUntrustedSourceEnabledSetByUser)
            {
                this.loadingFromUntrustedSourceEnabled = loadingFromUntrustedSourceEnabled;
            }
        }
    }
}
