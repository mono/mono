//
// TabInfo.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//

using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Microsoft.VisualBasic {
	[EditorBrowsable(EditorBrowsableState.Never)] 
		public struct TabInfo {
		// Declarations
		public short Column;

		// Constructors

		public TabInfo(short value) { Column = value ; }
		// Properties
		// Methods
		public void __Ctor__(short value)
    		{
        		Column = value;
    		}
		
		// clones current instance
		public TabInfo __Clone__()
    		{
        		return new TabInfo(Column);
    		}

		// Initializes current instance to zero
		public void __ZeroInit__()
    		{
		        Column = 0;
	        }
		
		// Copies the given value to current
		public void __Copy__(TabInfo other)
		{
        		Column = other.Column;
    		}

		public bool equals(System.Object obj)
    		{
        		if (obj == null)
            			return false;

		        if (obj.Equals(this))
            			return true;

		        if (obj is TabInfo)
        		{
		            TabInfo Int16 = (TabInfo) obj;
		            return Column == Int16.Column;
        		}

		        return false;
  		}

		public int hashCode()
    		{
		        return Column | (Column << 16);
	        }
	
	};
}
