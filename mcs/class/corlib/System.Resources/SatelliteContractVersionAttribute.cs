//
// System.Resources.SatelliteContractVersionAttribute.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.		http://www.ximian.com
//

namespace System.Resources
{
	   [AttributeUsage (AttributeTargets.Assembly)]
	   public sealed class SatelliteContractVersionAttribute : Attribute
	   {
			 private Version ver; 

			 // Constructor
			 public SatelliteContractVersionAttribute (string version)
			 {
				    ver = new Version(version);
			 }
			 public string Version
			 {
				    get { return ver.ToString(); }
			 }
	   }
}
