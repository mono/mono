
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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
   
	[Serializable]
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

