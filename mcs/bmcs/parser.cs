
namespace Compiler {

	using System;

	abstract public class Parser {
		public string name;
		public System.IO.Stream input;
		
		public Parser (Mono.CSharp.Tree tree, string name, System.IO.Stream stream) 
		{
			this.tree = tree;
			this.name = name;
			this.input = stream;
		}

		public string getName (){
			return name;
		}

		abstract public int parse ();
	}
}
