//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
    
    /// <summary>
    /// Exposes members representing an icon for a workflow out-of-box items.
    /// </summary>
    public static class WorkflowDesignerIcons
    {
        private static string addToCollectionIconName = "AddToCollectionIcon";
        private static string annotationIndicatorIconName = "AnnotationIndicatorBrush";
        private static string assignIconName = "AssignIcon";
        private static string cancellationScopeIconName = "CancellationScopeIcon";
        private static string clearCollectionIconName = "ClearCollectionIcon";
        private static string compensableActivityIconName = "CompensableActivityIcon";
        private static string compensateIconName = "CompensateIcon";
        private static string confirmIconName = "ConfirmIcon";
        private static string correlationScopeIconName = "CorrelationScopeIcon";
        private static string delayIconName = "DelayIcon";
        private static string deleteIconName = "DeleteIcon";
        private static string deleteDisabledIconName = "DeleteDisabledIcon";
        private static string doWhileIconName = "DoWhileIcon";
        private static string entryIconName = "EntryIcon";
        private static string errorValidationIconName = "ErrorValidationIcon";
        private static string existsInCollectionIconName = "ExistsInCollectionIcon";
        private static string exitIconName = "ExitIcon";
        private static string extensionWindowHeaderCloseButtonName = "ExtensionWindowHeaderCloseButton";
        private static string finalStateIconName = "FinalStateIcon";
        private static string fitToScreenIconName = "FitToScreenIcon";
        private static string flowchartIconName = "FlowchartIcon";
        private static string flowDecisionIconName = "FlowDecisionIcon";
        private static string flowDecisionIconBrushName = "FlowDecisionIconBrush";
        private static string flowSwitchIconName = "FlowSwitchIcon";
        private static string flowSwitchIconBrushName = "FlowSwitchIconBrush";
        private static string forEachIconName = "ForEachIcon";
        private static string genericLeafActivityIconName = "GenericLeafActivityIcon";
        private static string ifIconName = "IfIcon";
        private static string initializeCorrelationIconName = "InitializeCorrelationIcon";
        private static string interopIconName = "InteropIcon";
        private static string invokeDelegateIconName = "InvokeDelegateIcon";
        private static string invokeMethodIconName = "InvokeMethodIcon";
        private static string minimapIconName = "MinimapIcon";
        private static string moveDownIconName = "MoveDownIcon";
        private static string moveDownDisabledIconName = "MoveDownDisabledIcon";
        private static string moveUpIconName = "MoveUpIcon";
        private static string moveUpDisabledIconName = "MoveUpDisabledIcon";
        private static string noPersistScopeIconName = "NoPersistScopeIcon";
        private static string operationCopyIconName = "OperationCopyIcon";
        private static string operationCopyDisabledIconName = "OperationCopyDisabledIcon";
        private static string operationCutIconName = "OperationCutIcon";
        private static string operationCutDisabledIconName = "OperationCutDisabledIcon";
        private static string operationDeleteIconName = "OperationDeleteIcon";
        private static string operationDeleteDisabledIconName = "OperationDeleteDisabledIcon";
        private static string operationPasteIconName = "OperationPasteIcon";
        private static string operationPasteDisabledIconName = "OperationPasteDisabledIcon";
        private static string panModeIconName = "PanModeIcon";
        private static string parallelForEachIconName = "ParallelForEachIcon";
        private static string parallelIconName = "ParallelIcon";
        private static string persistIconName = "PersistIcon";
        private static string pickBranchIconName = "PickBranchIcon";
        private static string pickIconName = "PickIcon";
        private static string receiveAndSendReplyIconName = "ReceiveAndSendReplyIcon";
        private static string receiveIconName = "ReceiveIcon";
        private static string receiveReplyIconName = "ReceiveReplyIcon";
        private static string removeFromCollectionIconName = "RemoveFromCollectionIcon";
        private static string resizeGripIconName = "ResizeGripIcon";
        private static string rethrowIconName = "RethrowIcon";
        private static string sendAndReceiveReplyIconName = "SendAndReceiveReplyIcon";
        private static string sendIconName = "SendIcon";
        private static string sendReplyIconName = "SendReplyIcon";
        private static string sequenceIconName = "SequenceIcon";
        private static string startSymbolIconName = "StartSymbolIconBrush";
        private static string stateIconName = "StateIcon";
        private static string stateMachineIconName = "StateMachineIcon";
        private static string switchIconName = "SwitchIcon";
        private static string terminateWorkflowIconName = "TerminateWorkflowIcon";
        private static string textBoxErrorIconName = "TextBoxErrorIcon";
        private static string throwIconName = "ThrowIcon";
        private static string toolboxDefaultCustomActivityName = "ToolboxDefaultCustomActivityIcon";
        private static string transactedReceiveScopeIconName = "TransactedReceiveScopeIcon";
        private static string transactionScopeIconName = "TransactionScopeIcon";
        private static string transitionIconName = "TransitionIcon";
        private static string tryCatchIconName = "TryCatchIcon";
        private static string validationErrorIconName = "ValidationErrorIcon";
        private static string warningValidationIconName = "WarningValidationIcon";
        private static string whileIconName = "WhileIcon";
        private static string writeLineIconName = "WriteLineIcon";
        private static string zoomIconName = "ZoomIcon";

        private static ResourceDictionary iconResourceDictionary;

        internal static ResourceDictionary IconResourceDictionary
        {
            get
            {
                if (WorkflowDesignerIcons.iconResourceDictionary == null)
                {
                    InitializeDefaultResourceDictionary();
                }

                return WorkflowDesignerIcons.iconResourceDictionary;
            }
        }

        internal static bool IsDefaultCutomActivitySetByUser { get; set; }

        /// <summary>
        /// Applies the WindowsApp style icons to all workflow out-of-box items.
        /// </summary>
        public static void UseWindowsStoreAppStyleIcons()
        {
            Uri resourceUri = new Uri(string.Concat(typeof(WorkflowDesignerIcons).Assembly.GetName().Name, @";component/Themes/Icons.WindowsApp.xaml"), UriKind.RelativeOrAbsolute);
            ResourceDictionary windowsAppIcons = (ResourceDictionary)Application.LoadComponent(resourceUri);

            if (WorkflowDesignerIcons.iconResourceDictionary == null)
            {
                WorkflowDesignerIcons.iconResourceDictionary = new ResourceDictionary();
            }

            foreach (string key in windowsAppIcons.Keys)
            {
                WorkflowDesignerIcons.iconResourceDictionary[key] = windowsAppIcons[key];
            }

            ImageSource enabledImagaSource = null;
            ImageSource disabledImagaSource = null;

            WorkflowDesignerIcons.LoadImageSourceFromResource("Copy.WindowsApp", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCopyIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCopyDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            WorkflowDesignerIcons.LoadImageSourceFromResource("Cut.WindowsApp", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCutIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCutDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            WorkflowDesignerIcons.LoadImageSourceFromResource("Delete.WindowsApp", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationDeleteIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationDeleteDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            WorkflowDesignerIcons.LoadImageSourceFromResource("Paste.WindowsApp", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationPasteIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationPasteDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            // reset the flag so that we display the toolbox default custom activity icons in toolbox.
            WorkflowDesignerIcons.IsDefaultCutomActivitySetByUser = false;
        }

        private static void InitializeDefaultResourceDictionary()
        {
            Uri resourceUri = new Uri(string.Concat(typeof(WorkflowDesignerIcons).Assembly.GetName().Name, @";component/Themes/Icons.Default.xaml"), UriKind.RelativeOrAbsolute);
            ResourceDictionary defaultIcons = (ResourceDictionary)Application.LoadComponent(resourceUri);

            WorkflowDesignerIcons.iconResourceDictionary = new ResourceDictionary();

            foreach (object key in defaultIcons.Keys)
            {
                WorkflowDesignerIcons.iconResourceDictionary[key] = defaultIcons[key];
            }

            ImageSource enabledImagaSource = null;
            ImageSource disabledImagaSource = null;

            WorkflowDesignerIcons.LoadImageSourceFromResource("Copy.Default", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCopyIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCopyDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            WorkflowDesignerIcons.LoadImageSourceFromResource("Cut.Default", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCutIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationCutDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            WorkflowDesignerIcons.LoadImageSourceFromResource("Delete.Default", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationDeleteIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationDeleteDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);

            WorkflowDesignerIcons.LoadImageSourceFromResource("Paste.Default", out enabledImagaSource, out disabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationPasteIconName] = WorkflowDesignerIcons.MakeIcon(enabledImagaSource);
            WorkflowDesignerIcons.iconResourceDictionary[WorkflowDesignerIcons.operationPasteDisabledIconName] = WorkflowDesignerIcons.MakeIcon(disabledImagaSource);
        }

        private static void LoadImageSourceFromResource(string iconName, out ImageSource enabledImageSource, out ImageSource disabledImageSource)
        {
            string uri = string.Format(CultureInfo.InvariantCulture, "pack://application:,,,/System.Activities.Presentation;component/Resources/{0}.png", iconName);
            BitmapImage image = new BitmapImage(new Uri(uri, UriKind.Absolute));
            PixelFormat format = PixelFormats.Bgra32;
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            int stride = ((width * format.BitsPerPixel) + 7) / 8;
            uint[] pixels = new uint[stride * height];
            
            image.CopyPixels(pixels, stride, 0);

            enabledImageSource = BitmapSource.Create(width, height, image.DpiX, image.DpiY, format, null, pixels, stride);

            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = WorkflowDesignerIcons.MakePixelGray(pixels[i]);
            }

            disabledImageSource = BitmapSource.Create(width, height, image.DpiX, image.DpiY, format, null, pixels, stride);
        }

        private static DrawingBrush MakeIcon(ImageSource imageSource)
        {
            DrawingBrush icon = new DrawingBrush();
            icon.Stretch = Stretch.Uniform;
            icon.Drawing = new ImageDrawing(imageSource, new Rect(new Size(16, 16)));
            return icon;
        }

        private static uint MakePixelGray(uint pixel)
        {
            byte blue = (byte)pixel;
            byte green = (byte)(pixel >> 8);
            byte red = (byte)(pixel >> 16);
            byte alpha = (byte)(pixel >> 24);

            byte gray = (byte)(((red * 77) + (green * 150) + (blue * 29) + 128) / 256);
            return (uint)(alpha << 24 | gray << 16 | gray << 8 | gray);
        }

        /// <summary>
        /// Exposes members representing an icon associating with workflow out-of-box activities.
        /// These icons can be found in the toolbox and/or in the workflow designer.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034", Justification = "This is what our design is.")]
        [SuppressMessage("Microsoft.Naming", "CA1724", Justification = "This is what our design is.")]
        public static class Activities
        {
            /// <summary>
            /// Gets or sets the icon for AddToCollection activity.
            /// </summary>
            public static DrawingBrush AddToCollection
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.addToCollectionIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.addToCollectionIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Assign activity.
            /// </summary>
            public static DrawingBrush Assign
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.assignIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.assignIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for CancellationScope activity.
            /// </summary>
            public static DrawingBrush CancellationScope
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.cancellationScopeIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.cancellationScopeIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for ClearCollection activity.
            /// </summary>
            public static DrawingBrush ClearCollection
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.clearCollectionIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.clearCollectionIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for CompensableActivity activity.
            /// </summary>
            public static DrawingBrush CompensableActivity
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.compensableActivityIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.compensableActivityIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Compensate activity.
            /// </summary>
            public static DrawingBrush Compensate
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.compensateIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.compensateIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Confirm activity.
            /// </summary>
            public static DrawingBrush Confirm
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.confirmIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.confirmIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for CorrelationScope activity.
            /// </summary>
            public static DrawingBrush CorrelationScope
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.correlationScopeIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.correlationScopeIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Delay activity.
            /// </summary>
            public static DrawingBrush Delay
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.delayIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.delayIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for DoWhile activity.
            /// </summary>
            public static DrawingBrush DoWhile
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.doWhileIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.doWhileIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for ExistsInCollection activity.
            /// </summary>
            public static DrawingBrush ExistsInCollection
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.existsInCollectionIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.existsInCollectionIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for FinalState.
            /// </summary>
            public static DrawingBrush FinalState
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.finalStateIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.finalStateIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Flowchart.
            /// </summary>
            public static DrawingBrush Flowchart
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowchartIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowchartIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the toolbox icon for FlowDecision.
            /// </summary>
            public static DrawingBrush FlowDecision
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowDecisionIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowDecisionIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the FlowDecision node in the designer.
            /// </summary>
            public static DrawingBrush FlowDecisionNode
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowDecisionIconBrushName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowDecisionIconBrushName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the toolbox icon for FlowSwitch.
            /// </summary>
            public static DrawingBrush FlowSwitch
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowSwitchIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowSwitchIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the FlowSwitch node in the designer.
            /// </summary>
            public static DrawingBrush FlowSwitchNode
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowSwitchIconBrushName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.flowSwitchIconBrushName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for ForEach activity.
            /// </summary>
            public static DrawingBrush ForEach
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.forEachIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.forEachIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the default icon for custom activities.
            /// </summary>
            public static DrawingBrush DefaultCustomActivity
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.genericLeafActivityIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IsDefaultCutomActivitySetByUser = true;
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.genericLeafActivityIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for If activity.
            /// </summary>
            public static DrawingBrush If
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.ifIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.ifIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for InitializeCorrelation activity.
            /// </summary>
            public static DrawingBrush InitializeCorrelation
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.initializeCorrelationIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.initializeCorrelationIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Interop activity.
            /// </summary>
            public static DrawingBrush Interop
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.interopIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.interopIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for InvokeDelegate activity.
            /// </summary>
            public static DrawingBrush InvokeDelegate
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.invokeDelegateIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.invokeDelegateIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for InvokeMethod activity.
            /// </summary>
            public static DrawingBrush InvokeMethod
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.invokeMethodIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.invokeMethodIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for NoPersistScope activity.
            /// </summary>
            public static DrawingBrush NoPersistScope
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.noPersistScopeIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.noPersistScopeIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for ParallelForEach activity.
            /// </summary>
            public static DrawingBrush ParallelForEach
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.parallelForEachIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.parallelForEachIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Parallel activity.
            /// </summary>
            public static DrawingBrush Parallel
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.parallelIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.parallelIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Persist activity.
            /// </summary>
            public static DrawingBrush Persist
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.persistIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.persistIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for PickBranch.
            /// </summary>
            public static DrawingBrush PickBranch
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.pickBranchIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.pickBranchIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Pick activity.
            /// </summary>
            public static DrawingBrush Pick
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.pickIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.pickIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for ReceiveAndSendReply.
            /// </summary>
            public static DrawingBrush ReceiveAndSendReply
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.receiveAndSendReplyIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.receiveAndSendReplyIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Receive activity.
            /// </summary>
            public static DrawingBrush Receive
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.receiveIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.receiveIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for ReceiveReply activity.
            /// </summary>
            public static DrawingBrush ReceiveReply
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.receiveReplyIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.receiveReplyIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for RemoveFromCollection activity.
            /// </summary>
            public static DrawingBrush RemoveFromCollection
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.removeFromCollectionIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.removeFromCollectionIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Rethrow activity.
            /// </summary>
            public static DrawingBrush Rethrow
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.rethrowIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.rethrowIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for SendAndReceiveReply.
            /// </summary>
            public static DrawingBrush SendAndReceiveReply
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sendAndReceiveReplyIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sendAndReceiveReplyIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Send activity.
            /// </summary>
            public static DrawingBrush Send
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sendIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sendIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for SendReply activity.
            /// </summary>
            public static DrawingBrush SendReply
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sendReplyIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sendReplyIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Sequence activity.
            /// </summary>
            public static DrawingBrush Sequence
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sequenceIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.sequenceIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the Start node. This node is used in StateMachine and Flowchart.
            /// </summary>
            public static DrawingBrush StartNode
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.startSymbolIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.startSymbolIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for State.
            /// </summary>
            public static DrawingBrush State
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.stateIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.stateIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for a state entry in a state machine.
            /// </summary>
            public static DrawingBrush StateEntry
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.entryIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.entryIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for a state exit in a state machine.
            /// </summary>
            public static DrawingBrush StateExit
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.exitIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.exitIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for StateMachine activity.
            /// </summary>
            public static DrawingBrush StateMachine
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.stateMachineIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.stateMachineIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Switch activity.
            /// </summary>
            public static DrawingBrush Switch
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.switchIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.switchIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for TerminateWorkflow activity.
            /// </summary>
            public static DrawingBrush TerminateWorkflow
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.terminateWorkflowIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.terminateWorkflowIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for Throw activity.
            /// </summary>
            public static DrawingBrush Throw
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.throwIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.throwIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for TransactedReceiveScope activity.
            /// </summary>
            public static DrawingBrush TransactedReceiveScope
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.transactedReceiveScopeIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.transactedReceiveScopeIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for TransactionScope activity.
            /// </summary>
            public static DrawingBrush TransactionScope
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.transactionScopeIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.transactionScopeIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for a state transition in a state machine.
            /// </summary>
            public static DrawingBrush StateTransition
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.transitionIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.transitionIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for TryCatch activity.
            /// </summary>
            public static DrawingBrush TryCatch
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.tryCatchIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.tryCatchIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for While activity.
            /// </summary>
            public static DrawingBrush While
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.whileIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.whileIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for WriteLine activity.
            /// </summary>
            public static DrawingBrush WriteLine
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.writeLineIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.writeLineIconName] = value;
                }
            }

            internal static DrawingBrush ToolboxDefaultCustomActivity
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.toolboxDefaultCustomActivityName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.toolboxDefaultCustomActivityName] = value;
                }
            }
        }

        /// <summary>
        /// Exposes members representing an icon for the context menu items (right click menu) used in the workflow designer.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034", Justification = "This is what our design is.")]
        public static class ContextMenuItems
        {
            /// <summary>
            /// Gets or sets the icon for the context menu Copy for when the menu is enabled.
            /// </summary>
            public static DrawingBrush Copy
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCopyIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCopyIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Copy for when the menu is disabled.
            /// </summary>
            public static DrawingBrush CopyDisabled
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCopyDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCopyDisabledIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Cut for when the menu is enabled.
            /// </summary>
            public static DrawingBrush Cut
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCutIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCutIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Cut for when the menu is disabled.
            /// </summary>
            public static DrawingBrush CutDisabled
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCutDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationCutDisabledIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Delete for when the menu is enabled.
            /// </summary>
            public static DrawingBrush Delete
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationDeleteIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationDeleteIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Delete for when the menu is disabled.
            /// </summary>
            public static DrawingBrush DeleteDisabled
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationDeleteDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationDeleteDisabledIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Paste for when the menu is enabled.
            /// </summary>
            public static DrawingBrush Paste
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationPasteIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationPasteIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the context menu Paste for when the menu is disabled.
            /// </summary>
            public static DrawingBrush PasteDisabled
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationPasteDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.operationPasteDisabledIconName] = value;
                }
            }
        }

        /// <summary>
        /// Exposes members representing an icon associated with the context menu items (right click menu) used
        /// in the workflow designer.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034", Justification = "This is what our design is.")]
        public static class DesignerItems
        {
            /// <summary>
            /// Gets or sets the icon for an annotation.
            /// </summary>
            public static DrawingBrush Annotation
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.annotationIndicatorIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.annotationIndicatorIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon mainly for indicating errors in an activity definition.
            /// Note that there are 3 properties for error validation icons.
            /// These include ActivityErrorValidation, TextBoxErrorValidation and WorkflowErrorValidation.
            /// For consistency, the same icon should be used for all of them.
            /// </summary>
            public static DrawingBrush ActivityErrorValidation
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.errorValidationIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.errorValidationIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the delete button used in DynamicArgumentDialog and TypeCollectionDesigner.
            /// The icon is used for when the delete button is enabled.
            /// </summary>
            public static DrawingBrush DeleteButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.deleteIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.deleteIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the delete button used in DynamicArgumentDialog and TypeCollectionDesigner.
            /// The icon is used for when the delete button is disabled.
            /// </summary>
            public static DrawingBrush DeleteDisabledButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.deleteDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.deleteDisabledIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the Fit-To-Screen button located at the bottom right of the designer.
            /// </summary>
            public static DrawingBrush FitToScreen
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.fitToScreenIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.fitToScreenIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the Overview control  button (also known as MiniMap) located at the bottom right of the designer.
            /// </summary>
            public static DrawingBrush Overview
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.minimapIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.minimapIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the close button used in the Overview control (also known as MiniMap) window.
            /// </summary>
            public static DrawingBrush OverviewWindowCloseButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.extensionWindowHeaderCloseButtonName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.extensionWindowHeaderCloseButtonName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the move-down button used in DynamicArgumentDialog and TypeCollectionDesigner.
            /// The icon is used for when the delete button is enabled.
            /// </summary>
            public static DrawingBrush MoveDownButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveDownIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveDownIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the move-down button used in DynamicArgumentDialog and TypeCollectionDesigner.
            /// The icon is used for when the delete button is disabled.
            /// </summary>
            public static DrawingBrush MoveDownDisabledButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveDownDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveDownDisabledIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the move-up button used in DynamicArgumentDialog and TypeCollectionDesigner.
            /// The icon is used for when the delete button is enabled.
            /// </summary>
            public static DrawingBrush MoveUpButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveUpIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveUpIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the move-up button used in DynamicArgumentDialog and TypeCollectionDesigner.
            /// The icon is used for when the delete button is disabled.
            /// </summary>
            public static DrawingBrush MoveUpDisabledButton
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveUpDisabledIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.moveUpDisabledIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the Pan control button located at the bottom right of the designer.
            /// </summary>
            public static DrawingBrush PanMode
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.panModeIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.panModeIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the resize grip control found in FlowChart and State.
            /// </summary>
            public static DrawingBrush ResizeGrip
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.resizeGripIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.resizeGripIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon mainly for indicating errors in an expression textbox (e.g. textboxes inside an activity designer, property grid, arguments window, etc).
            /// Note that there are 3 properties for error validation icons.
            /// These include ActivityErrorValidation, TextBoxErrorValidation and WorkflowErrorValidation.
            /// For consistency, the same icon should be used for all of them.
            /// </summary>
            public static DrawingBrush TextBoxErrorValidation
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.textBoxErrorIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.textBoxErrorIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon mainly for indicating errors occurred when loading an invalid workflow Xaml into the designer.
            /// Note that there are 3 properties for error validation icons.
            /// These include ActivityErrorValidation, TextBoxErrorValidation and WorkflowErrorValidation.
            /// For consistency, the same icon should be used for all of them.
            /// </summary>
            public static DrawingBrush WorkflowErrorValidation
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.validationErrorIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.validationErrorIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for indicating warnings.
            /// </summary>
            public static DrawingBrush WarningValidation
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.warningValidationIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.warningValidationIconName] = value;
                }
            }

            /// <summary>
            /// Gets or sets the icon for the Zoom control button located at the bottom right of the designer.
            /// </summary>
            public static DrawingBrush Zoom
            {
                get
                {
                    return WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.zoomIconName] as DrawingBrush;
                }

                set
                {
                    WorkflowDesignerIcons.IconResourceDictionary[WorkflowDesignerIcons.zoomIconName] = value;
                }
            }
        }
    }
}
