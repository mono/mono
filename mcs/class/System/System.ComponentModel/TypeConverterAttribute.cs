//
// System.ComponentModel.TypeConverterAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//
using System;
namespace System.ComponentModel 
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class TypeConverterAttribute : Attribute
	{
		public static readonly TypeConverterAttribute Default = new TypeConverterAttribute ();

		private string converter_type;

		public TypeConverterAttribute ()
		{
			converter_type = "";
		}

		public TypeConverterAttribute (string typeName)
		{
			converter_type = typeName;
		}

		public TypeConverterAttribute (Type type)
		{
			converter_type = type.AssemblyQualifiedName;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is TypeConverterAttribute))
				return false;

			return ((TypeConverterAttribute) obj).ConverterTypeName == converter_type;
		}

		public override int GetHashCode ()
		{
			return converter_type.GetHashCode ();
		}

		public string ConverterTypeName {
			get { return converter_type; }
		}
	}
}

