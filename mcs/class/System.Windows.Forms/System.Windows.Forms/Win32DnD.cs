// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//	Srikanth Madikeri	(csri_1986@yahoo.com) - Win32 Drop files.
// 

// NOT COMPLETE

using System;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace System.Windows.Forms {
	internal class Win32DnD {
		#region Local Variables
		private const uint DATADIR_GET			= 1;
		private const uint S_OK				= 0x00000000;
		private const uint S_FALSE			= 0x00000001;
		private const uint DRAGDROP_S_DROP		= 0x00040100;
		private const uint DRAGDROP_S_CANCEL		= 0x00040101;
		private const uint DRAGDROP_S_USEDEFAULTCURSORS	= 0x00040102;
		private const uint E_NOTIMPL			= unchecked((uint)0x80004001);
		private const uint E_NOINTERFACE		= unchecked((uint)0x80004002);
		private const uint E_FAIL			= unchecked((uint)0x80004005);
		private const uint OLE_E_ADVISENOTSUPPORTED	= unchecked((uint)0x80040003);
		private const uint DV_E_FORMATETC		= unchecked((uint)0x80040064);

		// To call function pointers
		//private static object[]				GetDataArgs;

		// IDataObject Delegates
		private static QueryInterfaceDelegate		DOQueryInterface;
		private static AddRefDelegate			DOAddRef;
		private static ReleaseDelegate			DORelease;
		private static GetDataDelegate			GetData;
		private static GetDataHereDelegate		GetDataHere;
		private static QueryGetDataDelegate		QueryGetData;
		private static GetCanonicalFormatEtcDelegate	GetCanonicalFormatEtc;
		private static SetDataDelegate			SetData;
		private static EnumFormatEtcDelegate		EnumFormatEtc;
		private static DAdviseDelegate			DAdvise;
		private static DUnadviseDelegate		DUnadvise;
		private static EnumDAdviseDelegate		EnumDAdvise;

		// IDropSource Delegates
		private static QueryInterfaceDelegate		DSQueryInterface;
		private static AddRefDelegate			DSAddRef;
		private static ReleaseDelegate			DSRelease;
		private static QueryContinueDragDelegate	QueryContinueDrag;
		private static GiveFeedbackDelegate		GiveFeedback;

		// IDropTarget Delegates
		private static QueryInterfaceDelegate		DTQueryInterface;
		private static AddRefDelegate			DTAddRef;
		private static ReleaseDelegate			DTRelease;
		private static DragEnterDelegate		DragEnter;
		private static DragOverDelegate			DragOver;
		private static DragLeaveDelegate		DragLeave;
		private static DropDelegate			Drop;

		private static DragEventArgs			DragDropEventArgs;
		private static GiveFeedbackEventArgs		DragFeedbackEventArgs;
		private static QueryContinueDragEventArgs	DragContinueEventArgs;
		private static ArrayList			DragFormats;
		private static FORMATETC[]			DragFormatArray;
		private static ArrayList			DragMediums;
		#endregion	// Local Variables

		#region	Delegate Definitions
		// IUnknown
		internal delegate uint QueryInterfaceDelegate(IntPtr @this, ref Guid riid, IntPtr ppvObject);
		internal delegate uint AddRefDelegate(IntPtr @this);
		internal delegate uint ReleaseDelegate(IntPtr @this);

		// IDataObject
		internal delegate uint GetDataDelegate(IntPtr @this, ref FORMATETC pformatetcIn, IntPtr pmedium);
		internal delegate uint GetDataHereDelegate(IntPtr @this, ref FORMATETC pformatetc, ref STGMEDIUM pmedium);
		internal delegate uint QueryGetDataDelegate(IntPtr @this, ref FORMATETC pformatetc);
		internal delegate uint GetCanonicalFormatEtcDelegate(IntPtr @this, ref FORMATETC pformatetcIn, IntPtr pformatetcOut);
		internal delegate uint SetDataDelegate(IntPtr @this, ref FORMATETC pformatetc, ref STGMEDIUM pmedium, bool release);
		internal delegate uint EnumFormatEtcDelegate(IntPtr @this, uint direction, IntPtr ppenumFormatEtc);
		internal delegate uint DAdviseDelegate(IntPtr @this, ref FORMATETC pformatetc, uint advf, IntPtr pAdvSink, ref uint pdwConnection);
		internal delegate uint DUnadviseDelegate(IntPtr @this, uint pdwConnection);
		internal delegate uint EnumDAdviseDelegate(IntPtr @this, IntPtr ppenumAdvise);

		// IDropSource
		internal delegate uint QueryContinueDragDelegate(IntPtr @this, bool fEscapePressed, uint grfkeyState);
		internal delegate uint GiveFeedbackDelegate(IntPtr @this, uint pdwEffect);

		// IDropTarget
		internal delegate uint DragEnterDelegate(IntPtr @this, IntPtr pDataObj, uint grfkeyState, IntPtr pt_x, IntPtr pt_y, IntPtr pdwEffect);
		internal delegate uint DragOverDelegate(IntPtr @this, uint grfkeyState, IntPtr pt_x, IntPtr pt_y, IntPtr pdwEffect);
		internal delegate uint DragLeaveDelegate(IntPtr @this);
		internal delegate uint DropDelegate(IntPtr @this, IntPtr pDataObj, uint grfkeyState, IntPtr pt_x, IntPtr pt_y, IntPtr pdwEffect);
		#endregion	// Delegate Definitions

		[StructLayout(LayoutKind.Sequential)]
			internal struct FORMATETC {
			[MarshalAs(UnmanagedType.U2)]
			internal ClipboardFormats	cfFormat;
			internal IntPtr			ptd;
			internal DVASPECT		dwAspect;
			internal int			lindex;
			internal TYMED			tymed;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct STGMEDIUM {
			internal TYMED		tymed;
			internal IntPtr		hHandle;
			internal IntPtr		pUnkForRelease;
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct DROPFILES { 
			internal uint		pFiles;
			internal uint		pt_x;
			internal uint		pt_y;
			internal bool		fNC;
			internal bool		fWide;
			internal string		pText;
		}

		internal enum DVASPECT {
			DVASPECT_CONTENT	= 1,
			DVASPECT_THUMBNAIL	= 2,
			DVASPECT_ICON		= 4,
			DVASPECT_DOCPRINT	= 8
		}

		internal enum TYMED {
			TYMED_HGLOBAL		= 1,
			TYMED_FILE		= 2,
			TYMED_ISTREAM		= 4,
			TYMED_ISTORAGE		= 8,
			TYMED_GDI		= 16,
			TYMED_MFPICT		= 32,
			TYMED_ENHMF		= 64,
			TYMED_NULL		= 0
		}

		private static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");
		private static readonly Guid IID_IDataObject = new Guid("0000010e-0000-0000-C000-000000000046");
		private static readonly Guid IID_IDropSource = new Guid("00000121-0000-0000-C000-000000000046");
		private static readonly Guid IID_IDropTarget = new Guid("00000122-0000-0000-C000-000000000046");

		static Win32DnD()
		{
			// Required for all other OLE functions to work
			Win32OleInitialize(IntPtr.Zero);

			// We reuse those
			DragDropEventArgs = new DragEventArgs(new DataObject(DataFormats.FileDrop, new string[0]), 0, 0, 0, DragDropEffects.None, DragDropEffects.None);
			DragFeedbackEventArgs = new GiveFeedbackEventArgs(DragDropEffects.None, true);
			DragContinueEventArgs = new QueryContinueDragEventArgs(0, false, DragAction.Continue);
			DragFormats = new ArrayList();
			DragFormatArray = new FORMATETC[0];
			DragMediums = new ArrayList();

			// Set up delegates
			// IDataObject
			DOQueryInterface = new QueryInterfaceDelegate(ComIDataObject.QueryInterface);
			DOAddRef = new AddRefDelegate(ComIDataObject.AddRef);
			DORelease = new ReleaseDelegate(ComIDataObject.Release);
			GetData = new GetDataDelegate(ComIDataObject.GetData);
			GetDataHere = new GetDataHereDelegate(ComIDataObject.GetDataHere);
			QueryGetData = new QueryGetDataDelegate(ComIDataObject.QueryGetData);
			GetCanonicalFormatEtc = new GetCanonicalFormatEtcDelegate(ComIDataObject.GetCanonicalFormatEtc);
			SetData = new SetDataDelegate(ComIDataObject.SetData);
			EnumFormatEtc = new EnumFormatEtcDelegate(ComIDataObject.EnumFormatEtc);
			DAdvise = new DAdviseDelegate(ComIDataObject.DAdvise);
			DUnadvise = new DUnadviseDelegate(ComIDataObject.DUnadvise);
			EnumDAdvise = new EnumDAdviseDelegate(ComIDataObject.EnumDAdvise);

			// IDropSource
			DSQueryInterface = new QueryInterfaceDelegate(ComIDropSource.QueryInterface);
			DSAddRef = new AddRefDelegate(ComIDropSource.AddRef);
			DSRelease = new ReleaseDelegate(ComIDropSource.Release);
			QueryContinueDrag = new QueryContinueDragDelegate(ComIDropSource.QueryContinueDrag);
			GiveFeedback = new GiveFeedbackDelegate(ComIDropSource.GiveFeedback);

			// IDropTarget
			DTQueryInterface = new QueryInterfaceDelegate(ComIDropTarget.QueryInterface);
			DTAddRef = new AddRefDelegate(ComIDropTarget.AddRef);
			DTRelease = new ReleaseDelegate(ComIDropTarget.Release);
			DragEnter = new DragEnterDelegate(ComIDropTarget.DragEnter);
			DragOver = new DragOverDelegate(ComIDropTarget.DragOver);
			DragLeave = new DragLeaveDelegate(ComIDropTarget.DragLeave);
			Drop = new DropDelegate(ComIDropTarget.Drop);
		}

		internal class ComIDataObject {
			[StructLayout(LayoutKind.Sequential)]
			internal struct DataObjectStruct {
				internal IntPtr				vtbl;
				internal QueryInterfaceDelegate		QueryInterface;
				internal AddRefDelegate			AddRef;
				internal ReleaseDelegate		Release;
				internal GetDataDelegate		GetData;
				internal GetDataHereDelegate		GetDataHere;
				internal QueryGetDataDelegate		QueryGetData;
				internal GetCanonicalFormatEtcDelegate	GetCanonicalFormatEtc;
				internal SetDataDelegate		SetData;
				internal EnumFormatEtcDelegate		EnumFormatEtc;
				internal DAdviseDelegate		DAdvise;
				internal DUnadviseDelegate		DUnadvise;
				internal EnumDAdviseDelegate		EnumDAdvise;
			}

			internal static IntPtr GetUnmanaged() {
				DataObjectStruct	data_object;
				IntPtr			data_object_ptr;
				long			offset;

				data_object = new DataObjectStruct();

				data_object.QueryInterface = Win32DnD.DOQueryInterface;
				data_object.AddRef = Win32DnD.DOAddRef;
				data_object.Release = Win32DnD.DORelease;
				data_object.GetData = Win32DnD.GetData;
				data_object.GetDataHere = Win32DnD.GetDataHere;
				data_object.QueryGetData = Win32DnD.QueryGetData;
				data_object.GetCanonicalFormatEtc = Win32DnD.GetCanonicalFormatEtc;
				data_object.SetData = Win32DnD.SetData;
				data_object.EnumFormatEtc = Win32DnD.EnumFormatEtc;
				data_object.DAdvise = Win32DnD.DAdvise;
				data_object.DUnadvise = Win32DnD.DUnadvise;
				data_object.EnumDAdvise = Win32DnD.EnumDAdvise;

				data_object_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(DataObjectStruct)));
				Marshal.StructureToPtr(data_object, data_object_ptr, false);

				// Update vtbl pointer
				offset = data_object_ptr.ToInt64();
				offset += Marshal.SizeOf(typeof(IntPtr));
				Marshal.WriteIntPtr(data_object_ptr, new IntPtr(offset));
				
				return data_object_ptr;
			}

			internal static void ReleaseUnmanaged(IntPtr data_object_ptr) {
				Marshal.FreeHGlobal(data_object_ptr);
			}

			internal static uint QueryInterface(IntPtr @this, ref Guid riid, IntPtr ppvObject) {
				try {
					if (IID_IUnknown.Equals(riid) || IID_IDataObject.Equals(riid)) {
						Marshal.WriteIntPtr(ppvObject, @this);
						return S_OK;
					}
				}

				catch (Exception e) {
					Console.WriteLine("Got exception {0}", e.Message);
				}

				Marshal.WriteIntPtr(ppvObject, IntPtr.Zero);
				return E_NOINTERFACE;
			}

			internal static uint AddRef(IntPtr @this) {
				// We only use this for DnD, try and fake it
				return 1;
			}

			internal static uint Release(IntPtr @this) {
				// We only use this for DnD, try and fake it
				return 0;
			}

			internal static 	STGMEDIUM	medium = new STGMEDIUM();
			internal static uint GetData(IntPtr this_, ref FORMATETC pformatetcIn, IntPtr pmedium) {
				int		index;

				index = FindFormat(pformatetcIn);
				if (index != -1) {
					medium.tymed = TYMED.TYMED_HGLOBAL;
					medium.hHandle = XplatUIWin32.DupGlobalMem(((STGMEDIUM)DragMediums[index]).hHandle);
					medium.pUnkForRelease = IntPtr.Zero;
					try {
						Marshal.StructureToPtr(medium, pmedium, false);
					}
					catch (Exception e) {
						Console.WriteLine("Error: {0}", e.Message);
					}
					return S_OK;
				}

				return DV_E_FORMATETC;
			}

			internal static uint GetDataHere(IntPtr @this, ref FORMATETC pformatetc, ref STGMEDIUM pmedium) {
				return DV_E_FORMATETC;
			}

			internal static uint QueryGetData(IntPtr @this, ref FORMATETC pformatetc) {
				if (FindFormat(pformatetc) != -1) {
					return S_OK;
				}
				return DV_E_FORMATETC;
			}

			internal static uint GetCanonicalFormatEtc(IntPtr @this, ref FORMATETC pformatetcIn, IntPtr pformatetcOut) {
				Marshal.WriteIntPtr(pformatetcOut, Marshal.SizeOf(typeof(IntPtr)), IntPtr.Zero);
				return E_NOTIMPL;
			}

			internal static uint SetData(IntPtr this_, ref FORMATETC pformatetc, ref STGMEDIUM pmedium, bool release) {
				return E_NOTIMPL;
			}

			internal static uint EnumFormatEtc(IntPtr this_, uint direction, IntPtr ppenumFormatEtc) {
				if (direction == DATADIR_GET) {
					IntPtr	ppenum_ptr;

					ppenum_ptr = IntPtr.Zero;
					DragFormatArray = new FORMATETC[DragFormats.Count];

					for (int i = 0; i < DragFormats.Count; i++) {
						DragFormatArray[i] = (FORMATETC)DragFormats[i];
					}
					Win32SHCreateStdEnumFmtEtc((uint)DragFormatArray.Length, DragFormatArray, ref ppenum_ptr);
					Marshal.WriteIntPtr(ppenumFormatEtc, ppenum_ptr);
					return S_OK;
				}
				return E_NOTIMPL;
			}

			internal static uint DAdvise(IntPtr this_, ref FORMATETC pformatetc, uint advf, IntPtr pAdvSink, ref uint pdwConnection) {
				return OLE_E_ADVISENOTSUPPORTED;
			}

			internal static uint DUnadvise(IntPtr this_, uint pdwConnection) {
				return OLE_E_ADVISENOTSUPPORTED;
			}

			internal static uint EnumDAdvise(IntPtr this_, IntPtr ppenumAdvise) {
				return OLE_E_ADVISENOTSUPPORTED;
			}
		}

		internal class ComIDataObjectUnmanaged {
			[StructLayout(LayoutKind.Sequential)]
				internal struct IDataObjectUnmanaged {
				internal IntPtr		QueryInterface;
				internal IntPtr		AddRef;
				internal IntPtr		Release;
				internal IntPtr		GetData;
				internal IntPtr		GetDataHere;
				internal IntPtr		QueryGetData;
				internal IntPtr		GetCanonicalFormatEtc;
				internal IntPtr		SetData;
				internal IntPtr		EnumFormatEtc;
				internal IntPtr		DAdvise;
				internal IntPtr		DUnadvise;
				internal IntPtr		EnumDAdvise;
			}

			private static bool		Initialized;
			private static MethodInfo	GetDataMethod;
			//private static MethodInfo	GetDataHereMethod;
			private static MethodInfo	QueryGetDataMethod;
			//private static MethodInfo	GetCanonicalFormatEtcMethod;
			//private static MethodInfo	SetDataMethod;
			//private static MethodInfo	EnumFormatEtcMethod;
			//private static MethodInfo	DAdviseMethod;
			//private static MethodInfo	DUnadviseMethod;
			//private static MethodInfo	EnumDAdviseMethod;
			private static object[]		MethodArguments;

			private IDataObjectUnmanaged	vtbl;
			private IntPtr			@this;

			internal ComIDataObjectUnmanaged(IntPtr data_object_ptr) {
				if (!Initialized) {
					Initialize();
				}

				vtbl = new IDataObjectUnmanaged();
				@this = data_object_ptr;
				try {
					vtbl = (IDataObjectUnmanaged)Marshal.PtrToStructure(Marshal.ReadIntPtr(data_object_ptr), typeof(IDataObjectUnmanaged));
				}

				catch (Exception e) {
					Console.WriteLine("Exception {0}", e.Message);
				}
			}

			private static void Initialize() {
				AssemblyName	assembly;
				AssemblyBuilder	assembly_builder;

				if (Initialized) {
					return;
				}

				assembly = new AssemblyName();
				assembly.Name = "XplatUIWin32.FuncPtrInterface";
				assembly_builder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);

				MethodArguments = new object[6];
				GetDataMethod = CreateFuncPtrInterface(assembly_builder, "GetData", typeof(uint), 3);
				//GetDataHereMethod = CreateFuncPtrInterface(assembly_builder, "GetDataHere", typeof(uint), 3);
				QueryGetDataMethod = CreateFuncPtrInterface(assembly_builder, "QueryGetData", typeof(uint), 2);
				//GetCanonicalFormatEtcMethod = CreateFuncPtrInterface(assembly_builder, "GetCanonicalFormatEtc", typeof(uint), 3);
				//SetDataMethod = CreateFuncPtrInterface(assembly_builder, "SetData", typeof(uint), 4);
				//EnumFormatEtcMethod = CreateFuncPtrInterface(assembly_builder, "EnumFormatEtc", typeof(uint), 3);
				//DAdviseMethod = CreateFuncPtrInterface(assembly_builder, "DAdvise", typeof(uint), 5);
				//DUnadviseMethod = CreateFuncPtrInterface(assembly_builder, "DUnadvise", typeof(uint), 2);
				//EnumDAdviseMethod = CreateFuncPtrInterface(assembly_builder, "EnumDAdvise", typeof(uint), 2);

				Initialized = true;
			}

			internal uint QueryInterface(Guid riid, IntPtr ppvObject) {
				uint	ret;
				IntPtr	riid_ptr;

				riid_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Guid)));
				Marshal.StructureToPtr(riid, riid_ptr, false);

				MethodArguments[0] = vtbl.QueryInterface;
				MethodArguments[1] = this.@this;
				MethodArguments[2] = riid_ptr;
				MethodArguments[3] = ppvObject;

				try {
					ret = (uint)GetDataMethod.Invoke(null, MethodArguments);
				}

				catch (Exception e) {
					Console.WriteLine("Caught exception {0}", e.Message);
					ret = E_FAIL;
				}

				Marshal.FreeHGlobal(riid_ptr);

				return ret;
			}

			internal uint AddRef() {
				// We only use this for DnD, try and fake it
				return 1;
			}

			internal uint Release() {
				// We only use this for DnD, try and fake it
				return 0;
			}

			internal uint GetData(FORMATETC pformatetcIn, ref STGMEDIUM pmedium) {
				uint	ret;
				IntPtr	pformatetcIn_ptr;
				IntPtr	pmedium_ptr;

				pformatetcIn_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FORMATETC)));
				Marshal.StructureToPtr(pformatetcIn, pformatetcIn_ptr, false);

				pmedium_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(STGMEDIUM)));

				MethodArguments[0] = vtbl.GetData;
				MethodArguments[1] = this.@this;
				MethodArguments[2] = pformatetcIn_ptr;
				MethodArguments[3] = pmedium_ptr;

				try {
					ret = (uint)GetDataMethod.Invoke(null, MethodArguments);
					Marshal.PtrToStructure(pmedium_ptr, pmedium);
				}

				catch (Exception e) {
					Console.WriteLine("Caught exception {0}", e.Message);
					ret = E_FAIL;
				}

				Marshal.FreeHGlobal(pformatetcIn_ptr);
				Marshal.FreeHGlobal(pmedium_ptr);

				return ret;
			}

			internal uint GetDataHere(FORMATETC pformatetc, ref STGMEDIUM pmedium) {
				return E_NOTIMPL;
			}

			internal uint QueryGetData(FORMATETC pformatetc) {
				uint	ret;
				IntPtr	pformatetc_ptr;

				pformatetc_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FORMATETC)));
				Marshal.StructureToPtr(pformatetc, pformatetc_ptr, false);

				MethodArguments[0] = vtbl.GetData;
				MethodArguments[1] = this.@this;
				MethodArguments[2] = pformatetc_ptr;

				try {
					ret = (uint)QueryGetDataMethod.Invoke(null, MethodArguments);
				}

				catch (Exception e) {
					Console.WriteLine("Caught exception {0}", e.Message);
					ret = E_FAIL;
				}

				Marshal.FreeHGlobal(pformatetc_ptr);

				return ret;
			}

			internal uint GetCanonicalFormatEtc(FORMATETC pformatetcIn, ref FORMATETC pformatetcOut) {
				return E_NOTIMPL;
			}

			internal uint SetData(FORMATETC pformatetc, STGMEDIUM pmedium, bool release) {
				return E_NOTIMPL;
			}

			internal uint EnumFormatEtc(uint direction, IntPtr ppenumFormatEtc) {
				return E_NOTIMPL;
			}

			internal uint DAdvise(FORMATETC pformatetc, uint advf, IntPtr pAdvSink, ref uint pdwConnection) {
				return OLE_E_ADVISENOTSUPPORTED;
			}

			internal uint DUnadvise(uint pdwConnection) {
				return OLE_E_ADVISENOTSUPPORTED;
			}

			internal uint EnumDAdvise(IntPtr ppenumAdvise) {
				return OLE_E_ADVISENOTSUPPORTED;
			}
		}


		internal class ComIDropSource {
			[StructLayout(LayoutKind.Sequential)]
				internal struct IDropSource {
				internal IntPtr				vtbl;
				internal IntPtr				Window;
				internal QueryInterfaceDelegate		QueryInterface;
				internal AddRefDelegate			AddRef;
				internal ReleaseDelegate		Release;
				internal QueryContinueDragDelegate	QueryContinueDrag;
				internal GiveFeedbackDelegate		GiveFeedback;
			}

			internal static IntPtr GetUnmanaged(IntPtr Window) {
				IDropSource	drop_source;
				IntPtr		drop_source_ptr;
				long		offset;

				drop_source = new IDropSource();
				drop_source.QueryInterface = Win32DnD.DSQueryInterface;
				drop_source.AddRef = Win32DnD.DSAddRef;
				drop_source.Release = Win32DnD.DSRelease;
				drop_source.QueryContinueDrag = Win32DnD.QueryContinueDrag;
				drop_source.GiveFeedback = Win32DnD.GiveFeedback;
				drop_source.Window = Window;

				drop_source_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(drop_source));
				Marshal.StructureToPtr(drop_source, drop_source_ptr, false);

				// Update vtbl pointer
				offset = drop_source_ptr.ToInt64();
				offset += 2 * Marshal.SizeOf(typeof(IntPtr));
				Marshal.WriteIntPtr(drop_source_ptr, new IntPtr(offset));
				
				return drop_source_ptr;
			}

			internal static void ReleaseUnmanaged(IntPtr drop_source_ptr) {
				Marshal.FreeHGlobal(drop_source_ptr);
			}

			internal static uint QueryInterface(IntPtr @this, ref Guid riid, IntPtr ppvObject) {
				try {
					if (IID_IUnknown.Equals(riid) || IID_IDropSource.Equals(riid)) {
						Marshal.WriteIntPtr(ppvObject, @this);
						return S_OK;
					}
				}

				catch (Exception e) {
					Console.WriteLine("Got exception {0}", e.Message);
				}

				Marshal.WriteIntPtr(ppvObject, IntPtr.Zero);
				return E_NOINTERFACE;
			}

			internal static uint AddRef(IntPtr @this) {
				// We only use this for DnD, try and fake it
				return 1;
			}

			internal static uint Release(IntPtr @this) {
				// We only use this for DnD, try and fake it
				return 0;
			}

			internal static uint QueryContinueDrag(IntPtr @this, bool fEscapePressed, uint grfkeyState) {
				IntPtr		window;

				window = Marshal.ReadIntPtr(@this, Marshal.SizeOf(typeof(IntPtr)));

				// LAMESPEC? - according to MSDN, when the any mousebutton is *pressed* it defaults to Drop.
				// According to COM customary behaviour it's the other way round; which is what we do here
				if (fEscapePressed) {
					DragContinueEventArgs.drag_action = DragAction.Cancel;
				} else if ((grfkeyState & (1+2+16)) == 0) {		// Left, middle and right mouse button not pressed
					DragContinueEventArgs.drag_action = DragAction.Drop;
				} else {
					DragContinueEventArgs.drag_action = DragAction.Continue;
				}

				DragContinueEventArgs.escape_pressed = fEscapePressed;
				DragContinueEventArgs.key_state = (int)grfkeyState;

				Control.FromHandle(window).DndContinueDrag(DragContinueEventArgs);

				if (DragContinueEventArgs.drag_action == DragAction.Cancel) {
					return DRAGDROP_S_CANCEL;
				} else if (DragContinueEventArgs.drag_action == DragAction.Drop) {
					return DRAGDROP_S_DROP;
				}
				return S_OK;
			}

			internal static uint GiveFeedback(IntPtr @this, uint pdwEffect) {
				IntPtr		window;

				window = Marshal.ReadIntPtr(@this, Marshal.SizeOf(typeof(IntPtr)));

				DragFeedbackEventArgs.effect = (DragDropEffects)pdwEffect;
				DragFeedbackEventArgs.use_default_cursors = true;

				Control.FromHandle(window).DndFeedback(DragFeedbackEventArgs);

				if (DragFeedbackEventArgs.use_default_cursors) {
					return DRAGDROP_S_USEDEFAULTCURSORS;
				}
				return S_OK;
			}
		}

		internal class ComIDropTarget {
			[StructLayout(LayoutKind.Sequential)]
				internal struct IDropTarget {
				internal IntPtr				vtbl;
				internal IntPtr				Window;
				internal uint				ref_count;
				internal QueryInterfaceDelegate		QueryInterface;
				internal AddRefDelegate			AddRef;
				internal ReleaseDelegate		Release;

				internal DragEnterDelegate		DragEnter;
				internal DragOverDelegate		DragOver;
				internal DragLeaveDelegate		DragLeave;
				internal DropDelegate			Drop;
			}

			internal static IntPtr GetUnmanaged(IntPtr Window) {
				IDropTarget	drop_target;
				IntPtr		drop_target_ptr;
				long		offset;

				drop_target = new IDropTarget();
				drop_target.QueryInterface = Win32DnD.DTQueryInterface;
				drop_target.AddRef = Win32DnD.DTAddRef;
				drop_target.Release = Win32DnD.DTRelease;
				drop_target.DragEnter = Win32DnD.DragEnter;
				drop_target.DragOver = Win32DnD.DragOver;
				drop_target.DragLeave = Win32DnD.DragLeave;
				drop_target.Drop = Win32DnD.Drop;
				drop_target.Window = Window;

				drop_target_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(drop_target));
				Marshal.StructureToPtr(drop_target, drop_target_ptr, false);

				// Update vtbl pointer
				offset = drop_target_ptr.ToInt64();
				offset += 2 * Marshal.SizeOf(typeof(IntPtr)) + Marshal.SizeOf(typeof(uint));
				Marshal.WriteIntPtr(drop_target_ptr, new IntPtr(offset));
				
				return drop_target_ptr;
			}

			internal static void ReleaseUnmanaged(IntPtr drop_target_ptr) {
				Marshal.FreeHGlobal(drop_target_ptr);
			}

			internal static uint QueryInterface(IntPtr @this, ref Guid riid, IntPtr ppvObject) {
				try {
					if (IID_IUnknown.Equals(riid) || IID_IDropTarget.Equals(riid)) {
						Marshal.WriteIntPtr(ppvObject, @this);
						return S_OK;
					}
				}

				catch (Exception e) {
					Console.WriteLine("Got exception {0}", e.Message);
				}

				Marshal.WriteIntPtr(ppvObject, IntPtr.Zero);
				return E_NOINTERFACE;
			}

			internal static uint AddRef(IntPtr @this) {
				var ref_count = (uint)Marshal.ReadInt32(@this, Marshal.SizeOf(typeof(IntPtr)) * 2);
				Marshal.WriteInt32(@this, Marshal.SizeOf(typeof(IntPtr)) * 2, (int)(ref_count + 1));
				return ref_count + 1;
			}

			internal static uint Release(IntPtr @this) {
				var ref_count = (uint)Marshal.ReadInt32(@this, Marshal.SizeOf(typeof(IntPtr)) * 2);
				Marshal.WriteInt32(@this, Marshal.SizeOf(typeof(IntPtr)) * 2, (int)(ref_count - 1));
				if (ref_count == 1) {
					ReleaseUnmanaged(@this);
				}
				return ref_count - 1;
			}

			internal static uint DragEnter(IntPtr @this, IntPtr pDataObj, uint grfkeyState, IntPtr pt_x, IntPtr pt_y, IntPtr pdwEffect) {
				IntPtr		window;

				window = Marshal.ReadIntPtr(@this, Marshal.SizeOf(typeof(IntPtr)));

				DragDropEventArgs.x = pt_x.ToInt32();
				DragDropEventArgs.y = pt_y.ToInt32();
				DragDropEventArgs.allowed_effect = (DragDropEffects)Marshal.ReadIntPtr(pdwEffect).ToInt32();
				DragDropEventArgs.current_effect = DragDropEventArgs.AllowedEffect;
				DragDropEventArgs.keystate = (int)grfkeyState;

				Control.FromHandle(window).DndEnter(DragDropEventArgs);

				Marshal.WriteInt32(pdwEffect, (int)DragDropEventArgs.Effect);

				return S_OK;
			}

			internal static uint DragOver(IntPtr @this, uint grfkeyState, IntPtr pt_x, IntPtr pt_y, IntPtr pdwEffect) {
				IntPtr window;

				window = Marshal.ReadIntPtr(@this, Marshal.SizeOf(typeof(IntPtr)));

				DragDropEventArgs.x = pt_x.ToInt32();
				DragDropEventArgs.y = pt_y.ToInt32();
				DragDropEventArgs.allowed_effect = (DragDropEffects)Marshal.ReadIntPtr(pdwEffect).ToInt32();
				DragDropEventArgs.current_effect = DragDropEventArgs.AllowedEffect;
				DragDropEventArgs.keystate = (int)grfkeyState;

				Control.FromHandle(window).DndOver(DragDropEventArgs);

				Marshal.WriteInt32(pdwEffect, (int)DragDropEventArgs.Effect);

				return S_OK;
			}

			internal static uint DragLeave(IntPtr @this) {
				IntPtr window;

				window = Marshal.ReadIntPtr(@this, Marshal.SizeOf(typeof(IntPtr)));

				Control.FromHandle(window).DndLeave(EventArgs.Empty);

				return S_OK;
			}

			internal static uint Drop(IntPtr @this, IntPtr pDataObj, uint grfkeyState, IntPtr pt_x, IntPtr pt_y, IntPtr pdwEffect)
			{
		  		IntPtr window;
				
				window = Marshal.ReadIntPtr (@this, Marshal.SizeOf (typeof (IntPtr)));

				DragDropEventArgs.x = pt_x.ToInt32 ();
				DragDropEventArgs.y = pt_y.ToInt32 ();
				DragDropEventArgs.allowed_effect = (DragDropEffects) Marshal.ReadIntPtr (pdwEffect).ToInt32();
				DragDropEventArgs.current_effect = DragDropEventArgs.AllowedEffect;
				DragDropEventArgs.keystate = (int) grfkeyState;

				Control control = Control.FromHandle (window);
				if (control != null) {
					control.DndDrop (DragDropEventArgs);
					return S_FALSE;
				}
 
				Marshal.WriteInt32 (pdwEffect, (int) DragDropEventArgs.Effect);

				return S_OK;
			}
		}

		internal static bool HandleWMDropFiles(ref MSG msg) {
			IntPtr		hDrop;
			int		count;
			StringBuilder	sb;
			string[]	dropfiles;

			hDrop = msg.wParam;
			count = Win32DragQueryFile(hDrop, -1, IntPtr.Zero, 0);

			dropfiles = new string[count];

			sb = new StringBuilder(256);
			for (int i = 0; i < count; i++) {
				Win32DragQueryFile(hDrop, i, sb, sb.Capacity);
				dropfiles[i] = sb.ToString();
			}

			DragDropEventArgs.Data.SetData(DataFormats.FileDrop, dropfiles);

			Control.FromHandle(msg.hwnd).DndDrop(DragDropEventArgs);

			return true;
		}

		private static bool AddFormatAndMedium(ClipboardFormats cfFormat, object data) {
			STGMEDIUM	medium;
			FORMATETC	format;
			IntPtr		hmem;
			IntPtr		hmem_ptr;
			byte[]		b;

			switch(cfFormat) {
				case ClipboardFormats.CF_TEXT: {
					b = XplatUIWin32.StringToAnsi ((string)data);
					hmem = XplatUIWin32.CopyToMoveableMemory (b);
					break;
				}

				case ClipboardFormats.CF_UNICODETEXT: {
					b = XplatUIWin32.StringToUnicode ((string)data);
					hmem = XplatUIWin32.CopyToMoveableMemory (b);
					break;
				}

				case ClipboardFormats.CF_HDROP: {
					IEnumerator	e;
					StringBuilder	sb;
					long		hmem_string_ptr;
					IntPtr		string_buffer;
					int		string_buffer_size;

					sb = new StringBuilder();

					// Make sure object is enumerable; otherwise
					if ((data is string) || !(data is IEnumerable)) {
						sb.Append(data.ToString());
						sb.Append('\0');
						sb.Append('\0');
					} else {
						e = ((IEnumerable)data).GetEnumerator();
						while (e.MoveNext()) {
							sb.Append(e.Current.ToString());
							sb.Append('\0');
						}
						sb.Append('\0');
					}

					string_buffer = Marshal.StringToHGlobalUni(sb.ToString());
					string_buffer_size = (int)XplatUIWin32.Win32GlobalSize(string_buffer);

					// Write DROPFILES structure
					hmem = XplatUIWin32.Win32GlobalAlloc(XplatUIWin32.GAllocFlags.GMEM_MOVEABLE | XplatUIWin32.GAllocFlags.GMEM_DDESHARE, 0x14 + string_buffer_size);
					hmem_ptr = XplatUIWin32.Win32GlobalLock(hmem);
					Marshal.WriteInt32(hmem_ptr, 0x14);					// pFiles
					Marshal.WriteInt32(hmem_ptr, 1 * Marshal.SizeOf(typeof(uint)), 0);	// point.x
					Marshal.WriteInt32(hmem_ptr, 2 * Marshal.SizeOf(typeof(uint)), 0);	// point.y
					Marshal.WriteInt32(hmem_ptr, 3 * Marshal.SizeOf(typeof(uint)), 0);	// fNc
					Marshal.WriteInt32(hmem_ptr, 4 * Marshal.SizeOf(typeof(uint)), 1);	// fWide

					hmem_string_ptr = (long)hmem_ptr;
					hmem_string_ptr += 0x14;

					XplatUIWin32.Win32CopyMemory(new IntPtr(hmem_string_ptr), string_buffer, string_buffer_size);
					Marshal.FreeHGlobal(string_buffer);
					XplatUIWin32.Win32GlobalUnlock(hmem_ptr);

					break;
				}

				case ClipboardFormats.CF_DIB: {
					b = XplatUIWin32.ImageToDIB((Image)data);
					hmem = XplatUIWin32.CopyToMoveableMemory (b);
					break;
				}

				default: {
					hmem = IntPtr.Zero;
					break;
				}
			}

			if (hmem != IntPtr.Zero) {
				medium = new STGMEDIUM();
				medium.tymed = TYMED.TYMED_HGLOBAL;
				medium.hHandle = hmem;
				medium.pUnkForRelease = IntPtr.Zero;
				DragMediums.Add(medium);

				format = new FORMATETC();
				format.ptd = IntPtr.Zero;
				format.dwAspect = DVASPECT.DVASPECT_CONTENT;
				format.lindex = -1;
				format.tymed = TYMED.TYMED_HGLOBAL;
				format.cfFormat = cfFormat;
				DragFormats.Add(format);

				return true;
			}

			return false;
		}

		private static int FindFormat(FORMATETC pformatetc) {
			for (int i = 0; i < DragFormats.Count; i++) {
				if ((((FORMATETC)DragFormats[i]).cfFormat == pformatetc.cfFormat) &&
					(((FORMATETC)DragFormats[i]).dwAspect  == pformatetc.dwAspect) &&
					((((FORMATETC)DragFormats[i]).tymed & pformatetc.tymed)) != 0) {
					return i;
				}
			}
			return -1;
		}

		private static void BuildFormats(object data) {

			DragFormats.Clear();
			DragMediums.Clear();

			// Build our formats based on object data
			if (data is String) {
				AddFormatAndMedium(ClipboardFormats.CF_TEXT, data);
				AddFormatAndMedium(ClipboardFormats.CF_UNICODETEXT, data);
				AddFormatAndMedium(ClipboardFormats.CF_HDROP, data);
			} else if (data is Bitmap) {
				AddFormatAndMedium(ClipboardFormats.CF_DIB, data);
			} else if (data is ICollection) {
				// FIXME - test with .Net and found how this is handled
				AddFormatAndMedium(ClipboardFormats.CF_HDROP, data);
			} else if (data is ISerializable) {
				// FIXME - test with .Net and found how this is handled
			}
		}

		internal static DragDropEffects StartDrag(IntPtr Window, object data, DragDropEffects allowed) {
			IntPtr	result;
			IntPtr	data_object;
			IntPtr	drop_source;

			BuildFormats(data);

			data_object = ComIDataObject.GetUnmanaged();
			drop_source = ComIDropSource.GetUnmanaged(Window);

			result = (IntPtr)DragDropEffects.None;

			Win32DoDragDrop(data_object, drop_source, (IntPtr)allowed, ref result);

			// Cleanup again
			ComIDataObject.ReleaseUnmanaged(data_object);
			ComIDropSource.ReleaseUnmanaged(drop_source);
			DragFormats.Clear();
			DragFormatArray = null;
			DragMediums.Clear();

			return (DragDropEffects)result.ToInt32();
		}

		internal static bool UnregisterDropTarget(IntPtr Window) {
			Win32RevokeDragDrop(Window);
			return true;
		}

		internal static bool RegisterDropTarget(IntPtr Window) {
			IntPtr	drop_target;
			uint	result;

			drop_target = ComIDropTarget.GetUnmanaged(Window);
			result = Win32RegisterDragDrop(Window, drop_target);

			if (result != S_OK) {
				return false;
			}
			return true;
		}

		// Thanks, Martin
		static MethodInfo CreateFuncPtrInterface(AssemblyBuilder assembly, string MethodName, Type ret_type, int param_count) {
			ModuleBuilder	mb;
			TypeBuilder	tb;
			Type[]		il_param_types;
			Type[]		param_types;

			mb = assembly.DefineDynamicModule("XplatUIWin32.FuncInterface" + MethodName);
			tb = mb.DefineType ("XplatUIWin32.FuncInterface" + MethodName, TypeAttributes.Public);

			param_types = new Type[param_count];
			il_param_types = new Type[param_count + 1];

			il_param_types[param_count] = typeof(IntPtr);
			for (int i = 0; i < param_count; i++) {
				param_types[i] = typeof(IntPtr);
				il_param_types[i] = typeof(IntPtr);
			}

			MethodBuilder method = tb.DefineMethod (
				MethodName, MethodAttributes.Static | MethodAttributes.Public,
				ret_type, il_param_types);

			ILGenerator ig = method.GetILGenerator ();
			if (param_count > 5) ig.Emit (OpCodes.Ldarg_S, 6);
			if (param_count > 4) ig.Emit (OpCodes.Ldarg_S, 5);
			if (param_count > 3) ig.Emit (OpCodes.Ldarg_S, 4);
			if (param_count > 2) ig.Emit (OpCodes.Ldarg_3);
			if (param_count > 1) ig.Emit (OpCodes.Ldarg_2);
			if (param_count > 0) ig.Emit (OpCodes.Ldarg_1);
			ig.Emit (OpCodes.Ldarg_0);
			ig.EmitCalli (OpCodes.Calli, CallingConvention.StdCall, ret_type, param_types);
			ig.Emit (OpCodes.Ret);

			Type t = tb.CreateType ();
			MethodInfo m = t.GetMethod (MethodName);

			return m;
		}

		[DllImport ("ole32.dll", EntryPoint="RegisterDragDrop", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32RegisterDragDrop(IntPtr Window, IntPtr pDropTarget);

		[DllImport ("ole32.dll", EntryPoint="RevokeDragDrop", CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32RevokeDragDrop(IntPtr Window);

		[DllImport ("ole32.dll", EntryPoint="DoDragDrop", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32DoDragDrop(IntPtr pDataObject, IntPtr pDropSource, IntPtr dwOKEffect, ref IntPtr pdwEffect);

		//[DllImport ("shell32.dll", EntryPoint="DragAcceptFiles", CallingConvention=CallingConvention.StdCall)]
		//private extern static int Win32DragAcceptFiles(IntPtr Window, bool fAccept);

		[DllImport ("ole32.dll", EntryPoint="OleInitialize", CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32OleInitialize(IntPtr pvReserved);

		[DllImport ("shell32.dll", EntryPoint="DragQueryFileW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32DragQueryFile(IntPtr hDrop, int iFile, IntPtr lpszFile, int cch);

		[DllImport ("shell32.dll", EntryPoint="DragQueryFileW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32DragQueryFile(IntPtr hDrop, int iFile, StringBuilder lpszFile, int cch);

		[DllImport ("shell32.dll", EntryPoint="SHCreateStdEnumFmtEtc", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32SHCreateStdEnumFmtEtc(uint cfmt, FORMATETC[] afmt, ref IntPtr ppenumFormatEtc);
	}
}
