
//
// AssemblyVersionCompatibility.cs
//
//    Implementation of the 
//    System.Configuration.Assemblies.AssemblyVersionCompatibility
//    enumeration for the Mono Class Library
//
// Written by Tomas Restrepo (tomasr@mvps.org)
//

namespace System.Configuration.Assemblies {
   
   public enum AssemblyVersionCompatibility
   {
      SameMachine = 0x00000001,
      SameProcess = 0x00000002,
      SameDomain  = 0x00000003, 
      
   } // enum AssemblyHashAlgorithm

} // namespace System.Configuration.Assemblies

