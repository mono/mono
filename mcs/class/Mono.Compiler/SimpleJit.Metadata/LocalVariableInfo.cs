
namespace SimpleJit.Metadata
{
	public class LocalVariableInfo
	{
		public ClrType LocalType;
		public int LocalIndex;

		public LocalVariableInfo (ClrType t, int idx)
		{
			LocalType = t;
			LocalIndex = idx;
		}
		
	}

}
