//
// System.Drawing.Drawing2D.RegionData.cs
//
// Authors:
//   Dennis Hayes (dennish@Raytek.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002/3 Ximian, Inc
//

using System;

namespace System.Drawing.Drawing2D
{
	/// <summary>
	/// Summary description for RegionData.
	/// </summary>
	public sealed class RegionData
	{

		byte[] data;

		internal RegionData()
		{
		}

		public byte[] Data {
			get {return data;} 
			set {data = value;}
		}
	}
}
