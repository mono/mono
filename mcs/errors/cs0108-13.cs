// CS0108: `IMutableSequence.this[int]' hides inherited member `ISequence.this[int]'. Use the new keyword if hiding was intended
// Line: 15
// Compiler options: -warnaserror -warn:2

public interface ISequence
{
	object this [int index] 
	{
		get;
	}
}

public interface IMutableSequence : ISequence
{
	object this [int index] 
	{
		get;
		set;
	}
}