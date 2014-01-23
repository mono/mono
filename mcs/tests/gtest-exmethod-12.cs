

using System;
using System.Collections.Specialized;

class Program
{
	public static void Main(string[] args)
	{
		var chat = new ChatClient();
		var lines = new StringCollection() { "a", "b", "c" };
		chat.Say("test", lines);
	}
}

class ChatClient
{
	public void Say(string to, string message)
	{
		Console.WriteLine("{0}: {1}", to, message);
	}
}


static class ChatExtensions
{
	public static void Say(this ChatClient chat, string to, StringCollection lines)
	{
		foreach (string line in lines)
		{
			chat.Say(to, line);
		}
	}
}