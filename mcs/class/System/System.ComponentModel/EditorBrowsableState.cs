//
// ProjectData.cs
//
// Author:
//   Martin Adoue (martin@cwanet.com)
//
// (C) 2002 Martin Adoue
//

using System;

namespace System.ComponentModel
{

	/// <summary>
	/// Specifies the browsable state of a property or method from within an editor.
	/// </summary>
	public enum EditorBrowsableState 
	{
		/// <summary>
		/// The property or method is always browsable from within an editor.
		/// </summary>
		Always = 0,
		/// <summary>
		/// The property or method is never browsable from within an editor.
		/// </summary>
		Never = 1,
		/// <summary>
		/// The property or method is a feature that only advanced users should see. An editor can either show or hide such properties.
		/// </summary>
		Advanced = 2
	}

}
