//
// IArrangedElement.cs
//
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
// Copyright (c) 2018 Filip Navara
//

using System.Collections;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms.Layout
{
	interface IArrangedElement : IComponent
	{
		bool Visible { get; }
		bool AutoSize { get; }
		AutoSizeMode GetAutoSizeMode();
		Rectangle Bounds { get; }
		Rectangle ExplicitBounds { get; }
		Padding Padding { get; }
		Padding Margin { get; }
		Size MinimumSize { get; }
		AnchorStyles Anchor { get; }
		DockStyle Dock { get; }
		Rectangle DisplayRectangle { get; }
		IArrangedContainer Parent { get; }
		string Name { get; }
		void SetBounds(int x, int y, int width, int height, BoundsSpecified specified);
		Size GetPreferredSize(Size proposedSize);
		int DistanceRight { get; set; }
		int DistanceBottom { get; set; }
	}
}
