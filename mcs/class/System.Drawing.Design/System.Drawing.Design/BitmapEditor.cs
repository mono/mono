//
// System.Drawing.Design.BitmapEditor.cs
// 
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 
using System;
using System.IO;
using System.Drawing;
namespace System.Drawing.Design
{
	public class BitmapEditor : ImageEditor
	{

		public BitmapEditor()
		{
		}

		protected override string[] GetExtensions()
		{
			return new string[] {"*.bmp", "*.gif", "*.jpg", "*.jpeg", "*.png", "*.ico"};
		}

		protected override string GetFileDialogDescription()
		{
			return Locale.GetText("All bitmap files");
		}

		protected override Image LoadFromStream (Stream stream)
		{
			return new Bitmap (stream);
		}
	}
}
