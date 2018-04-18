using System;
using System.Collections.Generic;
using System.Text;

#if WINFORMS_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{
    internal static class Editors
    {
        #region Assembly configuration strings

#if WINFORMS_CONTROL
        private const string AssemblyName = "System.Windows.Forms.DataVisualization.Design";
        internal const string Version = ThisAssembly.Version;
        private const string Culture = "neutral";
        private const string PublicKeyToken = AssemblyRef.SharedLibPublicKeyToken;
        private const string Namespace = "System.Windows.Forms.Design.DataVisualization.Charting";
#else
        private const string AssemblyName = "System.Web.DataVisualization.Design";
        internal const string Version = ThisAssembly.Version;
        private const string Culture = "neutral";
        private const string PublicKeyToken = AssemblyRef.SharedLibPublicKeyToken;
        private const string Namespace = "System.Web.UI.Design.DataVisualization.Charting";
#endif
        #endregion Assembly configuration strings

        public const string UITypeEditorBase = "System.Drawing.Design.UITypeEditor, System.Drawing, Version=" + Version + ", Culture=neutral, PublicKeyToken=" + AssemblyRef.MicrosoftPublicKeyToken;

        internal static class ChartColorEditor
        {
            private const string ClassName = "ChartColorEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class FlagsEnumUITypeEditor
        {
            private const string ClassName = "FlagsEnumUITypeEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class SeriesDataSourceMemberValueAxisUITypeEditor
        {
            private const string ClassName = "SeriesDataSourceMemberValueAxisUITypeEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class SeriesDataFieldValueAxisUITypeEditor
        {
            private const string ClassName = "SeriesDataFieldValueAxisUITypeEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class ChartTypeEditor
        {
            private const string ClassName = "ChartTypeEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class ChartCollectionEditor
        {
            private const string ClassName = "ChartCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class DataPointCollectionEditor
        {
            private const string ClassName = "DataPointCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class SeriesCollectionEditor
        {
            private const string ClassName = "SeriesCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class AreaCollectionEditor
        {
            private const string ClassName = "AreaCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class AnnotationCollectionEditor
        {
            private const string ClassName = "AnnotationCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class AnchorPointUITypeEditor
        {
            private const string ClassName = "AnchorPointUITypeEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class AnnotationAxisUITypeEditor
        {
            private const string ClassName = "AnnotationAxisUITypeEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class ImageValueEditor
        {
            private const string ClassName = "ImageValueEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class HatchStyleEditor
        {
            private const string ClassName = "HatchStyleEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class UITypeEditorProxy
        {
            private const string ClassName = "UITypeEditorProxy";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class UITypeEditorProxyEx
        {
            private const string ClassName = "UITypeEditorProxyEx";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class ColorPaletteEditor
        {
            private const string ClassName = "ColorPaletteEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class UIPropertyEditor
        {
            private const string ClassName = "UIPropertyEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class MarkerStyleEditor
        {
            private const string ClassName = "MarkerStyleEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class GradientEditor
        {
            private const string ClassName = "GradientEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class AxesArrayEditor
        {
            private const string ClassName = "AxesArrayEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class LegendItemCollectionEditor
        {
            private const string ClassName = "LegendItemCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class LegendCollectionEditor
        {
            private const string ClassName = "LegendCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class LegendCellColumnCollectionEditor
        {
            private const string ClassName = "LegendCellColumnCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class LegendCellCollectionEditor
        {
            private const string ClassName = "LegendCellCollectionEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class KeywordsStringEditor
        {
            private const string ClassName = "KeywordsStringEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class UrlValueEditor
        {
            private const string ClassName = "UrlValueEditor";
            public const string Editor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = UITypeEditorBase;
        }

        internal static class SeriesDataFieldXConvertor
        {
            private const string ClassName = "SeriesDataFieldXConvertor";
            public const string Convertor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
        }

        internal static class SeriesDataFieldYConvertor
        {
            private const string ClassName = "SeriesDataFieldYConvertor";
            public const string Convertor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
        }


        internal static class DataPointCustomPropertiesConverter
        {
            private const string ClassName = "DataPointCustomPropertiesConverter";
            public const string Convertor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
        }

        internal static class DataPointConverter
        {
            private const string ClassName = "DataPointConverter";
            public const string Convertor = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
        }


        #region Designers

        public const string ChartWinDesigner = Namespace + ".ChartWinDesigner, " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;

        internal static class ChartWinDesignerSerializer
        {
            private const string ClassName = "ChartWinDesignerSerializer";
            public const string Designer = Namespace + "." + ClassName + ", " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;
            public const string Base = "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        }

        public const string ChartWebDesigner = Namespace + ".ChartWebDesigner, " + AssemblyName + ", Version=" + Version + ", Culture=" + Culture + ", PublicKeyToken=" + PublicKeyToken;


        #endregion Designers
    }
}
