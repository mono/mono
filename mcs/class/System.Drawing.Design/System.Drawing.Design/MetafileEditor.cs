//
// System.Drawing.Design.MetafileEditor.cs
// 
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
// 
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// 
using System;
using System.IO;
using System.Drawing.Imaging;
namespace System.Drawing.Design
{
	public class MetafileEditor : ImageEditor
	{

		public MetafileEditor()
		{
		}

		protected override string[] GetExtensions()
		{
			return new string[] {"*.emf", "*.wmf"};
		}

		protected override string GetFileDialogDescription()
		{
			// FIXME: Add multilanguage support
			return "All metafile files";
		}

		protected override Image LoadFromStream (Stream stream)
		{
			return new Metafile (stream);
		}
	}
}
