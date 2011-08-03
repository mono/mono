// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

namespace System.Windows.Forms {
	public class OSFeature : FeatureSupport {
		#region Local Variables
		private static OSFeature	feature = new OSFeature();
		#endregion	// Local Variables

		#region Protected Constructors
		protected OSFeature() {
			
		}
		#endregion	// Protected Constructors

		#region Public Static Fields
		public static readonly object LayeredWindows;
		public static readonly object Themes;
		#endregion	// Public Static Fields

		#region Public Static Properties
		public static OSFeature Feature {
			get {
				return  feature;
			}
		}
		
		public static bool IsPresent (SystemParameter enumVal)
		{
#pragma warning disable 219			
			object o;
#pragma warning restore 219			

			switch (enumVal) {
				case SystemParameter.DropShadow:
					try {
						o = SystemInformation.IsDropShadowEnabled;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.FlatMenu:
					try {
						o = SystemInformation.IsFlatMenuEnabled;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.FontSmoothingContrastMetric:
					try {
						o = SystemInformation.FontSmoothingContrast;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.FontSmoothingTypeMetric:
					try {
						o = SystemInformation.FontSmoothingType;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.MenuFadeEnabled:
					try {
						o = SystemInformation.IsMenuFadeEnabled;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.SelectionFade:
					try {
						o = SystemInformation.IsSelectionFadeEnabled;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.ToolTipAnimationMetric:
					try {
						o = SystemInformation.IsToolTipAnimationEnabled;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.UIEffects:
					try {
						o = SystemInformation.UIEffectsEnabled;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.CaretWidthMetric:
					try {
						o = SystemInformation.CaretWidth;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.VerticalFocusThicknessMetric:
					try {
						o = SystemInformation.VerticalFocusThickness;
						return true;
					} catch (Exception) { return false; }
				case SystemParameter.HorizontalFocusThicknessMetric:
					try {
						o = SystemInformation.HorizontalFocusThickness;
						return true;
					} catch (Exception) { return false; }
			}
			
			return false;
		}
		#endregion	// Public Static Properties

		#region Public Instance Methods
		public override Version GetVersionPresent(object feature) {
			if (feature == Themes) {
				return ThemeEngine.Current.Version;
			}
			return null;
		}
		#endregion	// Public Instance Methods
	}
}
