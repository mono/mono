//
// System.ComponentModel.ListChangedEventHandler.cs
//
// Author: Duncan Mak (duncan@ximian.com)
// 
// (C) Ximian, Inc.
//

using System;

namespace System.ComponentModel {
	[Serializable]
	public delegate void ListChangedEventHandler(object sender, 
					     ListChangedEventArgs e);
}						    
