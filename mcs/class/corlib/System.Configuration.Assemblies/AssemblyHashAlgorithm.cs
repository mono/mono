
//
// AssemblyHashAlgorithm.cs
//
//    Implementation of the 
//    System.Configuration.Assemblies.AssemblyHashAlgorithm
//    enumeration for the Mono Class Library
//
// Written by Tomas Restrepo (tomasr@mvps.org)
//

namespace System.Configuration.Assemblies {
   
   public enum AssemblyHashAlgorithm 
   {
      None  = 0x00000000, 
      MD5   = 0x00008003,
      SHA1  = 0x00008004,      
   } // enum AssemblyHashAlgorithm

} // namespace System.Configuration.Assemblies

