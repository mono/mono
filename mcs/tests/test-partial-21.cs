namespace Mono.Sms
{
	partial class Main
	{
	}
}

namespace Mono.Sms
{
	using Mono.Sms.Core;

	public partial class Main
	{
		public void Test ()
		{
			Contacts frm = new Contacts ();
			frm.ContactsEventHandler += delegate () {
				Agenda.AddContact ();
			};
		}
	}

	public partial class Contacts
	{
		public void Test ()
		{
			ContactsEventHandler ();
		}

		public delegate void ContactsHandler ();
		public event ContactsHandler ContactsEventHandler;
	}
}

namespace Mono.Sms.Core
{
	public class Agenda
	{
		public static void AddContact ()
		{
		}
		
		public static void Main ()
		{
		}		
	}
}
