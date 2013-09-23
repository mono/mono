using System;
using System.Threading.Tasks;

public class Program
{
	public static void Main (string[] args)
	{
		var p = new Program ();
		p.LoadPlayers ().Wait ();
	}

	class Model
	{
		public Player SelectedPlayer { get; set; }
	}

	class Player
	{
	}

	Model model = new Model ();

	private async Task LoadPlayers ()
	{
		Action<Player> selectPlayer = player => { };
		Func<Action<Player>, Action<Player>> selector = functor => player => {
			Console.WriteLine (model);
		};

		selector (selectPlayer);
	}

}