//
// System.Runtime.InteropServices.UCOMIMoniker.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("0000000f-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIMoniker
	{
		void BindToObject (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, [In] ref Guid riidResult, out object ppvResult);
		void BindToStorage (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, [In] ref Guid riid, out object ppvObj);
		void CommonPrefixWith (UCOMIMoniker pmkOther, out UCOMIMoniker ppmkPrefix);
		void ComposeWith (UCOMIMoniker pmkRight, bool fOnlyIfNotGeneric, out UCOMIMoniker ppmkComposite);
		void Enum (bool fForward, out UCOMIEnumMoniker ppenumMoniker);
		void GetClassID (out Guid pClassID);
		void GetDisplayName (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, out string ppszDisplayName);
		void GetSizeMax (out long pcbSize);
		void GetTimeOfLastChange (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, out FILETIME pFileTime);
		void Hash (out int pdwHash);
		void Inverse (out UCOMIMoniker ppmk);
		int IsDirty ();
		void IsEqual (UCOMIMoniker pmkOtherMoniker);
		void IsRunning (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, UCOMIMoniker pmkNewlyRunning);
		void IsSystemMoniker (out int pdwMksys);
		void Load (UCOMIStream pStm);
		void ParseDisplayName (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, string pszDisplayName, out int pchEaten, out UCOMIMoniker ppmkOut);
		void Reduce (UCOMIBindCtx pbc, int dwReduceHowFar, ref UCOMIMoniker ppmkToLeft, out UCOMIMoniker ppmkReduced);
		void RelativePathTo (UCOMIMoniker pmkOther, out UCOMIMoniker ppmkRelPath);
		void Save (UCOMIStream pStm, bool fClearDirty);
	}
}
