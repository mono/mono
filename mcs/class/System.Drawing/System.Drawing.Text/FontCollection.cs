//
// System.Drawing.Text.FontCollection.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Everaldo Canuto everaldo.canuto@bol.com.br
//			Sanjay Gupta (gsanjay@novell.com)
//
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Text {

	public abstract class FontCollection : IDisposable {
		
		internal IntPtr nativeFontCollection = IntPtr.Zero;
				
		internal FontCollection ()
		{
		}
        
		internal FontCollection (IntPtr ptr)
		{
			nativeFontCollection = ptr;
		}

		// methods
		public void Dispose()
		{
		}

		protected virtual void Dispose (bool disposing)
		{		
		}

		// properties
		public FontFamily[] Families
		{
			get { 
				int found;
				int returned;
				Status status;
				FontFamily[] families;
				
				status = GDIPlus.GdipGetFontCollectionFamilyCount (nativeFontCollection, out found);
				
				int nSize =  Marshal.SizeOf (IntPtr.Zero);
				IntPtr dest = Marshal.AllocHGlobal (nSize * found);           
               
				status = GDIPlus.GdipGetFontCollectionFamilyList(nativeFontCollection, found, dest, out returned);
				   
				IntPtr[] ptrAr = new IntPtr [returned];
				int pos = dest.ToInt32 ();
				for ( int i = 0; i < returned ; i++, pos+=nSize)
					ptrAr[i] = (IntPtr)Marshal.PtrToStructure ((IntPtr)pos, typeof(IntPtr));
           
				Marshal.FreeHGlobal (dest);           
                   
				families = new FontFamily [returned];
				for ( int i = 0; i < returned; i++ )
					families[i] = new FontFamily (ptrAr[i]);                     
                           
				return families;               
			}
		}

		~FontCollection()
		{
			Dispose (false);
		}

	}

}
