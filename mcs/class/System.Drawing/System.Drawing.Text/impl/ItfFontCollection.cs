//
// System.Drawing.Text.IFontCollection.cs
//
// Author: Alexandre Pigolkine (pigolkine@gmx.de)
//
using System;

namespace System.Drawing.Text {
	internal interface IFontCollectionFactory {
		IFontCollection InstalledFontCollection();
		IFontCollection PrivateFontCollection();
	}
	
	internal interface IFontCollection : IDisposable {
		FontFamily[] Families { get; }
	}
}
