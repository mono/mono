namespace System.Runtime.Remoting
{
	public interface IRemotingTypeInfo
	{
		string TypeName { get; set; }
		bool CanCastTo (Type fromType, object o);
	}
}
