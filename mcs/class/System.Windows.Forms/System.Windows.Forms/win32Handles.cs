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

//namespace UtilityLibrary.Win32
namespace System.Windows.Forms{
	internal class ShellHandle : IDisposable
	{
		#region Class Variables
		IntPtr handle = IntPtr.Zero;
		#endregion
        
		#region Constructors 
		// Can only be used as base classes
		internal ShellHandle(IntPtr handle)
		{
			this.handle = handle;
		}
        
		~ShellHandle()
		{
			Dispose(false);
		}
		#endregion

		#region Properties
		internal IntPtr Handle
		{
			get { return handle; }
		}
	
		#endregion

		#region Virtuals
		internal virtual void Dispose(bool disposing)
		{
			// This class encapsulate a PIDL handle that
			// it is allocated my the Shell Memory Manager
			// it needs to be deallocated by the Shell Memory Manager 
			// interface too
			
			// To avoid threads simultaneously releasing this resource
			lock (this)
			{
				
				if ( handle != IntPtr.Zero )
				{
					// If we have a valid handle
					// Release pointer that was allocated by the COM memory allocator
					Win32.SHFreeMalloc(handle);
					handle = IntPtr.Zero;
				}
			}
		}
		#endregion

		#region Methods
		// Implements the IDisposable Interface
		public void Dispose()
		{
			// Let the Garbage Collector know that it does
			// not need to call finalize for this class
			GC.SuppressFinalize(this);

			// Do the disposing
			Dispose(true);
		}
		#endregion
	
	}

	internal class COMInterface : IDisposable
	{
		#region Class Variables
		internal IUnknown iUnknown = null;
		#endregion
        
		#region Constructors 
		// Can only be used as base classes
		internal COMInterface(IUnknown iUnknown)
		{
			this.iUnknown = iUnknown;
		}
        
		~COMInterface()
		{
			Dispose(false);
		}
		#endregion

		#region Properties
		#endregion

		#region Virtuals
		protected virtual void Dispose(bool disposing)
		{
			// Release the reference to this interface
			lock(this)
			{
				if ( iUnknown != null )
				{
					iUnknown.Release();
					iUnknown = null;
				}
			}
		}
		#endregion

		#region Methods
		// Implements the IDisposable Interface
		public void Dispose()
		{
			// Let the Garbage Collector know that it does
			// not need to call finalize for this class
			GC.SuppressFinalize(this);

			// Do the disposing
			Dispose(true);
		}
		#endregion
		
	}

	internal class GdiHandle : IDisposable
	{
		#region Class Variables
		IntPtr handle = IntPtr.Zero;
		#endregion
        
		#region Constructors 
		// Can only be used as base classes
		protected GdiHandle(IntPtr handle)
		{
			this.handle = handle;
		}
        
		~GdiHandle()
		{
			Dispose(false);
		}
		#endregion

		#region Properties
		public IntPtr Handle
		{
			get { return handle; }
		}
	
		#endregion

		#region Virtuals
		protected virtual void Dispose(bool disposing)
		{
			// To avoid threads simultaneously releasing this resource
			lock (this)
			{
				
				if ( handle != IntPtr.Zero )
				{
					// If we have a valid handle
					// Destroy the handle
					Win32.DeleteObject(handle);
					handle = IntPtr.Zero;
				}
			}
		}
		#endregion

		#region Methods
		// Implements the IDisposable Interface
		public void Dispose()
		{
			// Let the Garbage Collector know that it does
			// not need to call finalize for this class
			GC.SuppressFinalize(this);

			// Do the disposing
			Dispose(true);
		}
		#endregion
	
	}



}
