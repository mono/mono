//
// System.Drawing.Imaging.PlayRecordCallback.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto
// eMail: everaldo.canuto@bol.com.br
//
using System;

namespace System.Drawing.Imaging {

	[Serializable]
	public delegate void PlayRecordCallback(
		EmfPlusRecordType recordType,
		int flags,
		int dataSize,
		IntPtr recordData
	);
}
