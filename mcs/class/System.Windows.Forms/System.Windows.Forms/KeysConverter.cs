//
// System.Windows.Forms.KeysConverter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.Globalization;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// </summary>
    public class KeysConverter : TypeConverter, IComparer {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public KeysConverter()
		{
		
		}

		//
		//  --- Public Methods
		//

		[MonoTODO]
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool CanCompareTo(Type type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual bool CanCompareTo(ITypeDescriptorContext context, Type type)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Compare(object a, object b)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object o, Type t)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool GetProertiesSupported()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool GetProertiesSupported(ITypeDescriptorContext context)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetStandardValues(context);
		}

		[MonoTODO]
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetStandardValuesExclusive(context);
		}

		[MonoTODO]
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			//FIXME:
			return base.GetStandardValuesExclusive(context);
		}

		[MonoTODO]
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			//FIXME:
			return base.IsValid(context, value);
		}
	 }
}
