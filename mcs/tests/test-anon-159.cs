using System;

namespace TestGenericsSubtypeMatching
{
	public class Sender<T> : IDisposable
	{
		public void DoSend<TMessage> (Action<T> action)
		{
			using (Sender<T> sub = new Sender<T> ())
			{
				Send (t =>
				{
					action(t);
					sub.ActionOnObject (t);
				});
			}
		}
		
		private static void Send (Action<T> action)
		{
		}
		
		void ActionOnObject (object o)
		{
			o.ToString ();
		}
	
		#region IDisposable implementation
		public void Dispose ()
		{
			Console.WriteLine ("Dispose!");
		}
		
		#endregion
	}
	
	public class C
	{
		public static void Main ()
		{
			new Sender<string> ().DoSend<bool>(l => Console.WriteLine (l));
		}
	}
}
