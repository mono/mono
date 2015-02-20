//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;

    internal class PropertyInspectorFontAndColorDictionary : ResourceDictionary 
    {
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        internal PropertyInspectorFontAndColorDictionary(Dictionary<string, object> fontAndColorData) 
        {
            try 
            {
                BeginInit();

                foreach (KeyValuePair<string, object> keyValuePair in fontAndColorData)
                {
                    if (keyValuePair.Value is System.Drawing.SolidBrush)
                    {
                        System.Drawing.SolidBrush drawingBrush = (System.Drawing.SolidBrush)(keyValuePair.Value);
                        Color color = new Color();
                        color.A = drawingBrush.Color.A;
                        color.R = drawingBrush.Color.R;
                        color.G = drawingBrush.Color.G;
                        color.B = drawingBrush.Color.B;

                        Add(keyValuePair.Key, new SolidColorBrush(color));
                    }
                    else if (keyValuePair.Value is System.Drawing.Drawing2D.LinearGradientBrush)
                    {
                        System.Drawing.Drawing2D.LinearGradientBrush drawingBrush = (System.Drawing.Drawing2D.LinearGradientBrush)keyValuePair.Value;
                        Color startingColor = new Color();
                        startingColor.A = drawingBrush.LinearColors[0].A;
                        startingColor.R = drawingBrush.LinearColors[0].R;
                        startingColor.G = drawingBrush.LinearColors[0].G;
                        startingColor.B = drawingBrush.LinearColors[0].B;
                        Color endingColor = new Color();
                        endingColor.A = drawingBrush.LinearColors[1].A;
                        endingColor.R = drawingBrush.LinearColors[1].R;
                        endingColor.G = drawingBrush.LinearColors[1].G;
                        endingColor.B = drawingBrush.LinearColors[1].B;
                        Add(keyValuePair.Key, new System.Windows.Media.LinearGradientBrush(startingColor, endingColor, 90));
                    }
                    else if (keyValuePair.Value is System.Drawing.FontFamily)
                    {
                        System.Drawing.FontFamily drawingFontFamily = (System.Drawing.FontFamily)keyValuePair.Value;
                        Add(keyValuePair.Key, new FontFamily(drawingFontFamily.Name));
                    }
                    else if (keyValuePair.Key == "FontWeightKey")
                    {
                        Add(keyValuePair.Key, System.Windows.FontWeights.Normal);
                    }
                    else
                    {
                        Add(keyValuePair.Key, keyValuePair.Value);
                    }
                }
            }
            finally 
            {
                EndInit();
            }
        }
    }
}
