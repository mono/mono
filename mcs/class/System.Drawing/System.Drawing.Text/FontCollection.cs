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
		
		//internal IFontCollection implementation;
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
			Dispose (true);
			System.GC.SuppressFinalize (this);
		}

		[MonoTODO]
		protected virtual void Dispose (bool disposing)
		{
			//Nothing for now
		}

		// properties
		public FontFamily[] Families
		{
			get { 
				int found;
				int returned;
				Status status;
				
				Console.WriteLine("came to Families method of FontCollection");
				
				status = GDIPlus.GdipGetFontCollectionFamilyCount( nativeFontCollection, out found);
				if (status != Status.Ok){
					throw new Exception ("Error calling GDIPlus.GdipGetFontCollectionFamilyCount: " +status);
				}
				
				Console.WriteLine("FamilyFont count returned in Families method of FontCollection " + found);
				
				int nSize =  Marshal.SizeOf(IntPtr.Zero);
				IntPtr dest = Marshal.AllocHGlobal(nSize* found);			
				
				status = GDIPlus.GdipGetFontCollectionFamilyList( nativeFontCollection, found, dest, out returned);
				if (status != Status.Ok){
					Console.WriteLine("Error calling GDIPlus.GdipGetFontCollectionFamilyList: " +status);
					throw new Exception ("Error calling GDIPlus.GdipGetFontCollectionFamilyList: " +status);					
				}
				
				IntPtr[] ptrAr = new IntPtr[returned];
	        	
				int pos = dest.ToInt32();
				for (int i=0; i<returned; i++, pos+=nSize)
					ptrAr[i] = (IntPtr) Marshal.PtrToStructure((IntPtr)pos, typeof(IntPtr));
			
				Marshal.FreeHGlobal(dest);			
				
				FontFamily [] familyList = new FontFamily[returned];
				Console.WriteLine("No of FontFamilies returned in Families method of FontCollection " + returned);
				for( int i = 0 ; i < returned ; i++ )
				{
					Console.WriteLine("Handle returned " + ptrAr[i]);
					familyList [i] = new FontFamily(ptrAr[i]);
				}
				
				return familyList; 
			}
		}

		~FontCollection()
		{
			Dispose (false);
		}

	}

}
