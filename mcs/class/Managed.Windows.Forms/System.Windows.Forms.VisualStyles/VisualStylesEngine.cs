namespace System.Windows.Forms.VisualStyles
{
	class VisualStylesEngine
	{
		static IVisualStyles instance = Initialize ();
		public static IVisualStyles Instance {
			get { return instance; }
		}
		static IVisualStyles Initialize ()
		{
			if (
#if !VISUAL_STYLES_USE_GTKPLUS_ON_WINDOWS
				!VisualStylesNative.IsSupported () &&
#endif
				VisualStylesGtkPlus.Initialize ())
				return new VisualStylesGtkPlus ();
			return new VisualStylesNative ();
		}
	}
}
