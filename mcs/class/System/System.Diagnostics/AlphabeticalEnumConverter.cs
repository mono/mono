//
// System.Diagnostics.AlphabeticalEnumConverter.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System.Diagnostics
{

	internal sealed class AlphabeticalEnumConverter : EnumConverter
	{

		public AlphabeticalEnumConverter (Type type)
			: base (type)
		{
		}

		[MonoTODO ("Create sorted standart values")]
		public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
		{
			return Values;
		}
	}
}
