using System;

class MainClass
{
	public struct DC
	{
		public readonly Guid m_Id;

		public DC (Guid Id)
		{
			m_Id = Id;
		}

		public Guid Id
		{
			get { return m_Id; }
		}


	}

	public static int Main ()
	{
		Guid Id = Guid.NewGuid ();
		DC dc = new DC (Id);
		Console.WriteLine ("id: {0} default: {1}", Id, default (Guid));
		if (dc.Id.Equals (default (Guid)))
			return 1;

		if (dc.m_Id.Equals (default (Guid)))
			return 2;

Console.WriteLine ("ok");
		return 0;
	}
}
 
