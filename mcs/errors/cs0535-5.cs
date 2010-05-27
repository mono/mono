// CS0535: `ServerProperty' does not implement interface member `IServerProperty.SetValue(string, uint)'
// Line: 10

public interface IServerProperty
{
	int[] GetChildren (uint timeout);
	void SetValue (string value, uint timeout);
}

public class ServerProperty : IServerProperty
{
	public int[] GetChildren (uint timeout)
	{
		return null;
	}
}
