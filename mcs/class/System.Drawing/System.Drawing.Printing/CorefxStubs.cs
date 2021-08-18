// Stubs to make Unix compilation happy with our PrintingHelper hooks in Graphics.cs

namespace System.Drawing.Internal
{
	internal class DeviceContext
	{
		internal void Dispose() { }
	}

	internal class PrintPreviewGraphics
	{
		internal RectangleF VisibleClipBounds { get; set; }
	}
}
