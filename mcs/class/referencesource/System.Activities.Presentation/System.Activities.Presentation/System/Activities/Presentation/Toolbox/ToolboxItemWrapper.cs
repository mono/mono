//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System;
    using System.Activities.Presentation.Utility;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    // This class is a wrapper for ToolboxItem objects. It adds support
    // for cate----zation of toolbox items. ResolveToolboxItem method
    // establishes link between actual ToolboxItem instance and Tool representation
    // in the toolbox (via qualified assembly and type name properties)

    public sealed class ToolboxItemWrapper : INotifyPropertyChanged
    {
        string toolName;
        string assemblyName;
        string bitmapName;
        string customDisplayName;
        string defaultDisplayName;
        Bitmap defaultBitmap;
        ToolboxItem toolboxItem;
        Type toolType;
        IDataObject dataObject;
        //LogPixelsXIndex and LogPixelsYIndex are the parameters you pass to GetDeviceCaps to get the current monitor resolution, they are constant.
        private const int LogPixelsXIndex = 88;
        private const int LogPixelsYIndex = 90;
        private const int defaultDpi = 96;

        public ToolboxItemWrapper()
            : this(string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }

        public ToolboxItemWrapper(Type toolType)
            : this(toolType, string.Empty)
        {
        }

        public ToolboxItemWrapper(Type toolType, string displayName)
            : this(toolType, string.Empty, displayName)
        {
        }

        public ToolboxItemWrapper(Type toolType, string bitmapName, string displayName)
            : this(toolType.FullName, toolType.Assembly.FullName, bitmapName, displayName)
        {
        }

        public ToolboxItemWrapper(string toolName, string assemblyName, string bitmapName, string displayName)
            : this(toolName, assemblyName, bitmapName, displayName, null)
        {
        }

        internal ToolboxItemWrapper(string toolName, string assemblyName, string bitmapName, string displayName, IDataObject dataObject)
        {
            this.ToolName = toolName;
            this.AssemblyName = assemblyName;
            this.BitmapName = bitmapName;
            this.DisplayName = displayName;
            this.defaultDisplayName = string.Empty;
            this.dataObject = dataObject;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        internal ToolboxItem ToolboxItem
        {
            get { return this.toolboxItem; }
            private set
            {
                if (this.toolboxItem != value)
                {
                    this.toolboxItem = value;
                    RaisePropertyChanged("IsValid");
                    RaisePropertyChanged("ToolboxItem");
                }
            }
        }

        public bool IsValid
        {
            get
            {
                return (null != this.toolboxItem);
            }
        }

        public string ToolName
        {
            get { return this.toolName; }
            set
            {
                if (null != this.toolboxItem)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ToolboxItemFrozenDescription));
                }
                bool isChanged = !string.Equals(value, this.toolName);
                if (isChanged)
                {
                    this.toolName = value;
                    RaisePropertyChanged("ToolName");
                    this.ToolboxItem = null;
                }
            }
        }

        public string AssemblyName
        {
            get { return this.assemblyName; }
            set
            {
                if (null != this.toolboxItem)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ToolboxItemFrozenDescription));
                }
                bool isChanged = !string.Equals(value, this.assemblyName);
                if (isChanged)
                {
                    this.assemblyName = value;
                    RaisePropertyChanged("AssemblyName");
                    this.ToolboxItem = null;
                }
            }
        }

        public string BitmapName
        {
            get { return this.bitmapName; }
            set
            {
                bool isChanged = !string.Equals(value, this.bitmapName);
                if (isChanged)
                {
                    this.bitmapName = value;
                    RaisePropertyChanged("BitmapName");
                    LoadBitmap();
                }
            }
        }

        internal IDataObject DataObject
        {
            get
            {
                return this.dataObject;
            }

            set
            {
                this.dataObject = value;
            }
        }

        public Bitmap Bitmap
        {
            get { return this.ToolboxItem.Bitmap; }
        }

        public string DisplayName
        {
            get
            {
                return this.ToolboxItem != null ? this.ToolboxItem.DisplayName : this.customDisplayName;
            }
            set
            {
                bool isChanged = !string.Equals(value, this.customDisplayName);
                if (isChanged)
                {
                    this.customDisplayName = value;
                    ChangeToolboxDisplayName();
                    RaisePropertyChanged("DisplayName");
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "By design.")]
        public Type Type
        {
            get { return this.toolType; }
            private set
            {
                bool isChanged = !Type.Equals(this.toolType, value);
                if (isChanged)
                {
                    this.toolType = value;
                    RaisePropertyChanged("Type");
                }
            }
        }

        internal bool ResolveToolboxItem()
        {
            if (null != this.ToolboxItem)
            {
                return true;
            }
            try
            {
                if (null == this.AssemblyName || null == this.ToolName)
                {
                    throw FxTrace.Exception.AsError(new ArgumentNullException(null == AssemblyName ? "AssemblyName" : "ToolName"));
                }

                Assembly toolAssembly = Assembly.Load(this.AssemblyName);
                Type discoveredToolType = toolAssembly.GetType(this.ToolName, true, true);
                ValidateTool(discoveredToolType);
                ToolboxItemAttribute[] toolboxItemAttributes
                    = discoveredToolType.GetCustomAttributes(typeof(ToolboxItemAttribute), true) as ToolboxItemAttribute[];
                ToolboxItem instance = null;
                if (0 != toolboxItemAttributes.Length)
                {
                    instance =
                        Activator.CreateInstance(toolboxItemAttributes[0].ToolboxItemType) as ToolboxItem;
                }
                else
                {
                    instance = new ToolboxItem(discoveredToolType);
                }
                this.ToolboxItem = instance;
                this.defaultDisplayName = instance.DisplayName;
                this.defaultBitmap = instance.Bitmap;
                LoadBitmap();
                ChangeToolboxDisplayName();
                this.Type = discoveredToolType;
                return true;
            }
            catch
            {
                this.ToolboxItem = null;
                this.Type = null;
                throw;
            }
        }

        internal static Bitmap CreateBitmapFromDrawingBrush(DrawingBrush resource)
        {
            if (resource == null)
            {
                return null;
            }

            const int defaultPixelWidth = 16;
            const int defaultPixelHeight = 16;
            double dpiX = GetSystemDpi(LogPixelsXIndex);
            double dpiY = GetSystemDpi(LogPixelsYIndex);
            int pixelWidth = (int)(defaultPixelWidth * dpiX / defaultDpi);
            int pixelHeight = (int)(defaultPixelHeight * dpiY / defaultDpi);
            var renderTargetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawRectangle(((DrawingBrush)resource), null, new Rect(0, 0, defaultPixelWidth, defaultPixelHeight));
            }

            renderTargetBitmap.Render(drawingVisual);
            MemoryStream bitmapStream = new MemoryStream();
            BitmapEncoder bitmapEncode = new PngBitmapEncoder
            {
                Interlace = PngInterlaceOption.Off,
            };
            bitmapEncode.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            bitmapEncode.Save(bitmapStream);
            //Reposition the MemoryStream pointer to the start of the stream after the save
            bitmapStream.Position = 0;
            Bitmap bitmap = new Bitmap(bitmapStream);
            return bitmap;
        }

        internal static Bitmap GetBitmapFromResource(string resourceString)
        {
            DrawingBrush drawingBrush = IconHelper.GetBrushFromResource(resourceString);
            return CreateBitmapFromDrawingBrush(drawingBrush);
        }

        void LoadBitmap()
        {
            try
            {
                if (this.toolboxItem != null)
                {
                    if (!string.IsNullOrEmpty(this.BitmapName))
                    {
                        this.toolboxItem.Bitmap = new Bitmap(this.BitmapName);
                    }
                    else
                    {
                        Bitmap bitmap = GetBitmapFromResource(this.toolboxItem.TypeName);
                        if (bitmap != null)
                        {
                            this.toolboxItem.Bitmap = bitmap;
                        }
                        else
                        {
                            if (WorkflowDesignerIcons.IsDefaultCutomActivitySetByUser)
                            {
                                this.toolboxItem.Bitmap = CreateBitmapFromDrawingBrush(WorkflowDesignerIcons.Activities.DefaultCustomActivity);
                            }
                            else if (WorkflowDesignerIcons.Activities.ToolboxDefaultCustomActivity != null)
                            {
                                this.toolboxItem.Bitmap = CreateBitmapFromDrawingBrush(WorkflowDesignerIcons.Activities.ToolboxDefaultCustomActivity);
                            }
                            else
                            {
                                this.toolboxItem.Bitmap = this.defaultBitmap;
                            }
                        }
                    }
                }
            }
            catch (ArgumentException)
            {
                this.toolboxItem.Bitmap = this.defaultBitmap;
            }

            RaisePropertyChanged("ToolboxItem");
            RaisePropertyChanged("Bitmap");
        }

        void ChangeToolboxDisplayName()
        {
            if (null != this.toolboxItem)
            {
                if (!string.IsNullOrEmpty(this.customDisplayName))
                {
                    this.toolboxItem.DisplayName = this.customDisplayName;
                }
                else
                {
                    this.toolboxItem.DisplayName = this.defaultDisplayName;
                }
                RaisePropertyChanged("ToolboxItem");
            }
        }

        void RaisePropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void ValidateTool(Type toolType)
        {
            bool isInvalid = toolType.IsAbstract;
            isInvalid |= toolType.IsInterface;
            isInvalid |= !toolType.IsVisible;
            ConstructorInfo ctor = toolType.GetConstructor(Type.EmptyTypes);
            isInvalid |= (null == ctor);
            if (isInvalid)
            {
                string reason = string.Empty;
                if (toolType.IsAbstract)
                {
                    reason = "IsAbstract == true ";
                }
                if (toolType.IsInterface)
                {
                    reason += "IsInterface == true ";
                }
                if (!toolType.IsVisible)
                {
                    reason += "IsVisible == false ";
                }
                if (null == ctor)
                {
                    reason += SR.NoDefaultCtorError;
                }

                string error = string.Format(CultureInfo.CurrentCulture, SR.NotSupportedToolboxTypeFormatString, toolType.Name, reason);
                throw FxTrace.Exception.AsError(new NotSupportedException(error));
            }
        }

        public override string ToString()
        {
            return this.ToolName;
        }

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        private static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("Gdi32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        internal static int GetSystemDpi(int index)
        {
            IntPtr dc = GetDC(IntPtr.Zero);
            //if dc is null, just return the common Dpi value
            if (dc == null)
            {
                return defaultDpi;
            }

            try
            {
                return GetDeviceCaps(dc, index);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, dc);
            }
        }
    }
}
