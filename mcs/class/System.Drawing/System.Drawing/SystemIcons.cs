//
// System.Drawing.SystemIcons.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc
//

using System;

namespace System.Drawing
{
	[MonoTODO ("not implemented")]
	public sealed class SystemIcons
	{
		private SystemIcons()
		{
		}

		public static Icon Application { get { return LoadIcon();} }
		public static Icon Asterisk { get { return LoadIcon();} }
		public static Icon Error { get { return LoadIcon();} }
		public static Icon Exclamation { get { return LoadIcon();} }
		public static Icon Hand { get { return LoadIcon();} }
		public static Icon Information { get { return LoadIcon();} }
		public static Icon Question { get { return LoadIcon();} }
		public static Icon Warning { get { return LoadIcon();} }
		public static Icon WinLogo { get { return LoadIcon();} }

		[MonoTODO]
		private static Icon LoadIcon ()
		{
			return null;
		}
	}
}
