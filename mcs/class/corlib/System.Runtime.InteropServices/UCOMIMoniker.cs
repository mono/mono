//
// System.Runtime.InteropServices.UCOMIMoniker.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIMoniker
	{
		void BindToObject (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, ref Guid riidResult, ref object ppvResult);
		void BindToStorage (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, ref Guid riid, ref object ppvObj);
		void CommonPrefixWith (UCOMIMoniker pmkOther, ref UCOMIMoniker ppmkPrefix);
		void ComposeWith (UCOMIMoniker pmkRight, bool fOnlyIfNotGeneric, ref UCOMIMoniker ppmkComposite);
		void Enum (bool fForward, ref UCOMIEnumMoniker ppenumMoniker);
		void GetClassID (ref Guid pClassID);
		void GetDisplayName (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, ref string ppszDisplayName);
		void GetSizeMax (ref long pcbSize);
		void GetTimeOfLastChange (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, ref FILETIME pFileTime);
		void Hash (ref int pdwHash);
		void Inverse (ref UCOMIMoniker ppmk);
		int IsDirty ();
		void IsEqual (UCOMIMoniker pmkOtherMoniker);
		void IsRunning (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, UCOMIMoniker pmkNewlyRunning);
		void IsSystemMoniker (ref int pdwMksys);
		void Load (UCOMIStream pStm);
		void ParseDisplayName (UCOMIBindCtx pbc, UCOMIMoniker pmkToLeft, string pszDisplayName, ref int pchEaten, ref UCOMIMoniker ppmkOut);
		void Reduce (UCOMIBindCtx pbc, int dwReduceHowFar, ref UCOMIMoniker ppmkToLeft, ref UCOMIMoniker ppmkReduced);
		void RelativePathTo (UCOMIMoniker pmkOther, ref UCOMIMoniker ppmkRelPath);
		void Save (UCOMIStream pStm, bool fClearDirty);
	}
}
