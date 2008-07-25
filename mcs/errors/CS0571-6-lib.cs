using System.Runtime.CompilerServices;

public interface IFoo
{
	[IndexerName ("Jaj")]
	object this [int i] { get; set; }
}
