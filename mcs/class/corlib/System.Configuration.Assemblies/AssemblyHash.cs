
//
// AssemblyHash.cs
//
//    Implementation of the 
//    System.Configuration.Assemblies.AssemblyHash
//    class for the Mono Class Library
//
// Author:
//    Tomas Restrepo (tomasr@mvps.org)
//

namespace System.Configuration.Assemblies {
   
   public struct AssemblyHash : System.ICloneable
   {
      private AssemblyHashAlgorithm _algorithm;
      private byte[] _value;

      public static readonly AssemblyHash Empty = 
         new AssemblyHash(AssemblyHashAlgorithm.None,null);


      //
      // properties
      //
      public AssemblyHashAlgorithm Algorithm {
         get { return _algorithm; }
         set { _algorithm = value; }
      }


      //
      // construction
      //
      public AssemblyHash ( AssemblyHashAlgorithm algorithm, byte[] value )
      {
         _algorithm = algorithm;
         _value = null;
         if ( value != null )
         {
            int size = value.Length;
            _value = new byte[size];
            System.Array.Copy ( value, _value, size );
         }
      }

      public AssemblyHash ( byte[] value )
         : this(AssemblyHashAlgorithm.SHA1, value)
      {
      }

      public object Clone()
      {
         return new AssemblyHash(_algorithm,_value);
      }

      public byte[] GetValue()
      {
         return _value;
      }
      public void SetValue ( byte[] value )
      {
         _value = value;
      }

   } // class AssemblyHash

} // namespace System.Configuration.Assemblies

