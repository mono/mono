//
// System.ComponentModel.Design.ISelectionService.cs
//
// Authors:
//   Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Alejandro Sánchez Acosta
// (C) 2003 Andreas Nahr
// 

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public interface ISelectionService
	{
		bool GetComponentSelected (object component);

		ICollection GetSelectedComponents ();

		void SetSelectedComponents (ICollection components, SelectionTypes selectionType);

		void SetSelectedComponents (ICollection components);

		object PrimarySelection {get;}

		int SelectionCount {get;}

		event EventHandler SelectionChanged;

		event EventHandler SelectionChanging;
	}		
}
