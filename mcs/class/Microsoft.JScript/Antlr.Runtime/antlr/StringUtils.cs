using System;

namespace antlr
{
	/* ANTLR Translator Generator
	 * Project led by Terence Parr at http://www.jGuru.com
	 * Software rights: http://www.antlr.org/RIGHTS.html
	 *
	 * $Id: StringUtils.cs,v 1.1 2003/04/22 04:56:13 cesar Exp $
	 */

	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//
	
	public class StringUtils
	{
		/*General-purpose utility function for removing
		* characters from back of string
		* @param s The string to process
		* @param c The character to remove
		* @return The resulting string
		*/
		static public string stripBack(string s, char c)
		{
			while (s.Length > 0 && s[s.Length - 1] == c)
			{
				s = s.Substring(0, (s.Length - 1) - (0));
			}
			return s;
		}
		
		/*General-purpose utility function for removing
		* characters from back of string
		* @param s The string to process
		* @param remove A string containing the set of characters to remove
		* @return The resulting string
		*/
		static public string stripBack(string s, string remove)
		{
			bool changed;
			do 
			{
				changed = false;
				 for (int i = 0; i < remove.Length; i++)
				{
					char c = remove[i];
					while (s.Length > 0 && s[s.Length - 1] == c)
					{
						changed = true;
						s = s.Substring(0, (s.Length - 1) - (0));
					}
				}
			}
			while (changed);
			return s;
		}
		
		/*General-purpose utility function for removing
		* characters from front of string
		* @param s The string to process
		* @param c The character to remove
		* @return The resulting string
		*/
		static public string stripFront(string s, char c)
		{
			while (s.Length > 0 && s[0] == c)
			{
				s = s.Substring(1);
			}
			return s;
		}
		
		/*General-purpose utility function for removing
		* characters from front of string
		* @param s The string to process
		* @param remove A string containing the set of characters to remove
		* @return The resulting string
		*/
		static public string stripFront(string s, string remove)
		{
			bool changed;
			do 
			{
				changed = false;
				 for (int i = 0; i < remove.Length; i++)
				{
					char c = remove[i];
					while (s.Length > 0 && s[0] == c)
					{
						changed = true;
						s = s.Substring(1);
					}
				}
			}
			while (changed);
			return s;
		}
		
		/*General-purpose utility function for removing
		* characters from the front and back of string
		* @param s The string to process
		* @param head exact string to strip from head
		* @param tail exact string to strip from tail
		* @return The resulting string
		*/
		public static string stripFrontBack(string src, string head, string tail)
		{
			int h = src.IndexOf(head);
			int t = src.LastIndexOf(tail);
			if (h == - 1 || t == - 1)
				return src;
			return src.Substring(h + 1, (t) - (h + 1));
		}
	}
}