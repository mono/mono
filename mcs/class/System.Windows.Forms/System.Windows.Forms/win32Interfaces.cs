/*
 * Copyright (C) 5/11/2002 Carlos Harvey Perez 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject
 * to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL CARLOS HARVEY PEREZ BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
 * THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Except as contained in this notice, the name of Carlos Harvey Perez
 * shall not be used in advertising or otherwise to promote the sale,
 * use or other dealings in this Software without prior written
 * authorization from Carlos Harvey Perez.
 */

using System;
using System.Runtime.InteropServices;


//namespace UtilityLibrary.Win32
namespace System.Windows.Forms{

	#region IUnknown
	[ComImport, Guid("00000000-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IUnknown
	{
		[PreserveSig]
		IntPtr QueryInterface(REFIID riid, out IntPtr pVoid);
		
		[PreserveSig]
		IntPtr AddRef();

		[PreserveSig]
		IntPtr Release();
	}
	#endregion

	#region IMalloc
	[ComImport, Guid("00000002-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMalloc
	{
		[PreserveSig]
		IntPtr Alloc(int cb);

		[PreserveSig]
		IntPtr Realloc(IntPtr pv, int cb);

		[PreserveSig]
		void Free(IntPtr pv);

		[PreserveSig]
		int GetSize(IntPtr pv);

		[PreserveSig]
		int DidAlloc(IntPtr pv);

		[PreserveSig]
		void HeapMinimize();
	}
	#endregion

	#region IShellFolder
	[ComImport, Guid("000214E6-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IShellFolder
	{
		[PreserveSig]
		int ParseDisplayName(IntPtr hWnd, IntPtr bindingContext, 
			IntPtr OLEString, out int chEaten, ref IntPtr idList, ref int attributes);
		
		[PreserveSig]
		int EnumObjects(IntPtr hWnd, ShellEnumFlags flags,  ref IEnumIDList enumList);

		[PreserveSig]
        int BindToObject(IntPtr idList, IntPtr bindingContext, ref REFIID refiid, ref IShellFolder folder);
        
		[PreserveSig]
		int BindToStorage(ref IntPtr idList, IntPtr bindingContext, ref REFIID riid, IntPtr pVoid);

		[PreserveSig]
		int CompareIDs(int lparam, IntPtr idList1, IntPtr idList2);
        
		[PreserveSig]
		int CreateViewObject(IntPtr hWnd, REFIID riid, IntPtr pVoid);
        
		[PreserveSig]
		int GetAttributesOf(int count, ref IntPtr idList, out GetAttributeOfFlags attributes);

		[PreserveSig]
		int GetUIObjectOf(IntPtr hWnd, int count, ref IntPtr idList, 
			ref REFIID riid, out int arrayInOut, ref IUnknown iUnknown);

		[PreserveSig]
		int GetDisplayNameOf(IntPtr idList, ShellGetDisplayNameOfFlags flags, ref STRRET strRet);

		[PreserveSig]
		int SetNameOf(IntPtr hWnd, ref IntPtr idList,
			IntPtr pOLEString, int flags, ref IntPtr pItemIDList);
        
	}
    #endregion

	#region IEnumIDList
	[ComImport, Guid("000214f2-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IEnumIDList
	{
		[PreserveSig]
        int Next(int count, ref IntPtr idList, out int fetched);
 
		[PreserveSig]
		int Skip(int count);

		[PreserveSig]
		int Reset();

		[PreserveSig]
		int Clone(ref IEnumIDList list);
	}
	#endregion

	#region IContextMenu
	[ComImport, Guid("000214e4-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IContextMenu
	{
		[PreserveSig]
        int QueryContextMenu(IntPtr hMenu, int indexMenu, int idFirstCommand, int idLastCommand, QueryContextMenuFlags flags);
    
		[PreserveSig]
		int InvokeCommand(ref CMINVOKECOMMANDINFO ici);

		[PreserveSig]
        int GetCommandString(int idCommand, int type, int reserved, string commandName, int cchMax);
	}
	#endregion

	#region IContextMenu2
	[ComImport, Guid("000214f4-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IContextMenu2
	{

		[PreserveSig]
		int HandleMenuMsg(int message, int wParam, int lParam);
	}
	#endregion
    
}