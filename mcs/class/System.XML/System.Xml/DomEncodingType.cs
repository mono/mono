// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.DomEncodingType.cs
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
//
// (C) 2001 Daniel Weber
//

using System;

namespace System.Xml
{
	/// <summary>
	/// Define encoding types for the DOM
	/// </summary>
	internal enum DomEncodingType
	{
		etUTF8,
		etUTF16BE,
		etUTF16LE,
		etLatin1,
		etLatin2,
		etLatin3,
		etLatin4,
		etCyrillic,
		etArabic,
		etGreek,
		etHebrew,
		etLatin5,
		etLatin6,
		etLatin7,
		etLatin8,
		etLatin9,
		etKOI8R,
		etcp10000_MacRoman,
		etcp1250,
		etcp1251,
		etcp1252,
	}
}
