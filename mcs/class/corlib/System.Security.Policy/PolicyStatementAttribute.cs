// System.Security.Policy.PolicyStatementAttribute
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

namespace System.Security.Policy
{
	[Flags]
	[Serializable]
	public enum PolicyStatementAttribute	{
		Nothing = 0,
		Exclusive = 1,
		LevelFinal = 2,
		All = 3
	}
}
