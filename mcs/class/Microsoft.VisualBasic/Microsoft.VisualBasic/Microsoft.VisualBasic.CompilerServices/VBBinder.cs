//
// LateBinding.cs
//
// Author:
//   Marco Ridoni    (marco.ridoni@virgilio.it)
//
// (C) 2003 Marco Ridoni
//
using System;
using System.Reflection;
using System.Globalization;
using Microsoft.VisualBasic;

namespace Microsoft.VisualBasic.CompilerServices
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute]
	public class VBBinder : Binder
	{
		public VBBinder() : base()
		{
		}

		private class BinderState
		{
			public object[] args;
		}

		public override FieldInfo BindToField(
			BindingFlags bindingAttr,
			FieldInfo[] match,
			object value,
			CultureInfo culture
			)
		{
			return null;
		}

		public override MethodBase BindToMethod(
			BindingFlags bindingAttr,
			MethodBase[] match,
			ref object[] args,
			ParameterModifier[] modifiers,
			CultureInfo culture,
			string[] names,
			out object state
			)
		{
			// Store the arguments to the method in a state object.
			BinderState binderState = new BinderState();
			object[] arguments = new Object[args.Length];
			args.CopyTo(arguments, 0);
			binderState.args = arguments;
			state = binderState;
			if(match == null)
				throw new ArgumentNullException();
				
			for(int x = 0; x < match.Length; x++)
			{
				int count = 0;
				ParameterInfo[] parameters = match[x].GetParameters();
				if(args.Length != parameters.Length)
					continue;

				for(int y = 0; y < args.Length; y++)
				{
					if(ChangeType(args[y], parameters[y].ParameterType, culture) != null)
						count++;
					else
						break;
				}
				if(count == args.Length)
					return match[x];
			}
			return null;
		}

		public override object ChangeType(
			object value,
			Type myChangeType,
			CultureInfo culture
			)
		{		
			TypeCode src_type = Type.GetTypeCode (value.GetType());			
			TypeCode dest_type = Type.GetTypeCode (myChangeType);
			
			switch (dest_type) {
				case TypeCode.String:
					switch (src_type) {
						case TypeCode.SByte:						
						case TypeCode.Byte:
							return (StringType.FromByte ((byte)value));
						case TypeCode.UInt16:
						case TypeCode.Int16:
							return (StringType.FromShort ((short)value));	
						case TypeCode.UInt32:					
						case TypeCode.Int32:
							return (StringType.FromInteger ((int)value));						
						case TypeCode.UInt64:	
						case TypeCode.Int64:
							return (StringType.FromLong ((long)value));						
						case TypeCode.Char:
							return (StringType.FromChar ((char)value));							
						case TypeCode.Single:
							return (StringType.FromSingle ((float)value));	
						case TypeCode.Double:
							return (StringType.FromDouble ((double)value));																		
						case TypeCode.Boolean:
							return (StringType.FromBoolean ((bool)value));	
						case TypeCode.Object:
							return (StringType.FromObject (value));																												
					}
					break;
					
				case TypeCode.Int32:
				case TypeCode.UInt32:	
					switch (src_type) {						
						case TypeCode.String:				
							return (IntegerType.FromString ((string)value));	
						case TypeCode.Object:				
							return (IntegerType.FromObject (value));										
					}
					break;	

				case TypeCode.Int16:
				case TypeCode.UInt16:	
					switch (src_type) {						
						case TypeCode.String:				
							return (ShortType.FromString ((string)value));		
						case TypeCode.Object:				
							return (ShortType.FromObject (value));										
					}
					break;	
				case TypeCode.Object:
					return ((Object) value);												
			}
			return null;
		}

		public override void ReorderArgumentArray(
			ref object[] args,
			object state
			)
		{

		}

		public override MethodBase SelectMethod(
			BindingFlags bindingAttr,
			MethodBase[] match,
			Type[] types,
			ParameterModifier[] modifiers
			)
		{
			return null;
		}

		public override PropertyInfo SelectProperty(
			BindingFlags bindingAttr,
			PropertyInfo[] match,
			Type returnType,
			Type[] indexes,
			ParameterModifier[] modifiers
			)		
		{
			return null;
		}
		

	}
}
