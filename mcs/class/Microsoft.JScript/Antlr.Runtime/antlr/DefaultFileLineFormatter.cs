using System;
using StringBuilder			= System.Text.StringBuilder;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: DefaultFileLineFormatter.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	*/
	
	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	public class DefaultFileLineFormatter : FileLineFormatter
	{
		public override string getFormatString(string fileName, int line, int column)
		{
			StringBuilder buf = new StringBuilder();
			
			if (fileName != null)
				buf.Append(fileName + ":");
			
			if (line != - 1)
			{
				if (fileName == null)
					buf.Append("line ");
				
				buf.Append(line);
				
				if (column != - 1)
					buf.Append(":" + column);
				
				buf.Append(":");
			}
			
			buf.Append(" ");
			
			return buf.ToString();
		}
	}
}