//
// SpcInfo.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//

using System.ComponentModel;

namespace Microsoft.VisualBasic {
	[EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	public struct SpcInfo {
		// Declarations
		public short Count;
	
		// Constructors
		public SpcInfo(short value) { Count = value ; }
		
		// methods
		public void __Ctor__(short value)
    		{
        		Count = value;
    		}
     		
		// clones the current instance.
		public SpcInfo __Clone__()
    		{
        		return new SpcInfo(Count);
    		}
     		
		// Initializes current instance to zero
		public void __ZeroInit__()
    		{
        		Count = 0;
    		}
     
    		// Copies the given value to current
		public void __Copy__(SpcInfo other)
    		{
        		Count = other.Count;
    		}

		public bool equals(System.Object obj)
    		{
        		if (obj == null)
            			return false;

        		if (obj.Equals(this))
            			return true;

        		if (obj is SpcInfo)
        		{
            			SpcInfo int16 = (SpcInfo) obj;
            			return Count == int16.Count;
        		}

        		return false;
    		}

    		public int hashCode()
    		{
        		return Count | (Count << 16);
    		}
	
	};
}
