
namespace SimpleJit.Metadata
{
	public class ParameterInfo
	{
		public ClrType ParameterType;
		public int Position;


		public ParameterInfo (ClrType parameterType, int idx)
		{
			ParameterType = parameterType;
			Position = idx;
		}
	}

}
