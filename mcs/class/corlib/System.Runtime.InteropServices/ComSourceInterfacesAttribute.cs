//
// System.Runtime.InteropServices.ComSourceInterfacesAttribute.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.InteropServices
{
	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class ComSourceInterfacesAttribute : Attribute
	{
		private string internalValue;

		public ComSourceInterfacesAttribute (string sourceInterfaces)
		{
			internalValue = sourceInterfaces;
		}

		public ComSourceInterfacesAttribute (Type sourceInterface)
		{
			internalValue = sourceInterface.ToString ();
		}

		public ComSourceInterfacesAttribute (Type sourceInterface1, Type sourceInterface2)
		{
			internalValue = sourceInterface1.ToString () + sourceInterface2.ToString ();
		}

		public ComSourceInterfacesAttribute (Type sourceInterface1, Type sourceInterface2, Type sourceInterface3)
		{
			internalValue = sourceInterface1.ToString () + sourceInterface2.ToString () +
				sourceInterface3.ToString ();
		}

		public ComSourceInterfacesAttribute (Type sourceInterface1, Type sourceInterface2, Type sourceInterface3, Type sourceInterface4)
		{
			internalValue = sourceInterface1.ToString () + sourceInterface2.ToString () +
				sourceInterface3.ToString () + sourceInterface4.ToString ();
		}
		
		public string Value {
			get {return internalValue; } 
		}
	}
}
