// ILReader.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;
using System.IO;
using System.Text;
using System.Collections;

namespace Mono.ILASM {


	/// <summary>
	/// </summary>
	public class ILReader {
		
		private StreamReader reader;
		private Stack putback_stack;
		private Location location;
		private Location markedLocation;
		
		public ILReader (StreamReader reader)
		{
			this.reader = reader;
			putback_stack = new Stack ();
			
			location = new Location ();
			markedLocation = Location.Unknown;
		}



		/// <summary>
		/// </summary>
		public Location Location {
			get {
				return location;
			}
		}


		/// <summary>
		/// Provides access to underlying StreamReader.
		/// </summary>
		public StreamReader BaseReader {
			get {
				return reader;
			}
		}

		private int DoRead ()
		{
			if (putback_stack.Count > 0) 
				return (char) putback_stack.Pop ();
			
			return reader.Read ();
		}

		private int DoPeek ()
		{
			if (putback_stack.Count > 0)
				return (char) putback_stack.Peek ();
			
			return reader.Peek ();
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public int Read ()
		{
			int read = DoRead ();
			if (read == '\n')
				location.NewLine ();
			else
				location.NextColumn ();
			return read;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public int Peek ()
		{
			return DoPeek ();
		}


		/// <summary>
		/// </summary>
		public void Unread (char c)
		{
			putback_stack.Push (c);

			if ('\n' == c)
				location.PreviousLine ();

			location.PreviousColumn ();
		}


		/// <summary>
		/// </summary>
		/// <param name="chars"></param>
		public void Unread (char [] chars)
		{
			for (int i=chars.Length-1; i>=0; i--)
				Unread (chars[i]);					
		}

		/// <summary>
		/// </summary>
		/// <param name="c"></param>
		public void Unread (int c)
		{
			Unread ((char)c);
		}


		/// <summary>
		/// </summary>
		public void SkipWhitespace ()
		{
			int ch = Read ();
			for (; ch != -1 && Char.IsWhiteSpace((char) ch); ch = Read ());
			if (ch != -1) Unread (ch);
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public string ReadToWhitespace ()
		{
			StringBuilder sb = new StringBuilder ();
			int ch = Read ();
			for (; ch != -1 && !Char.IsWhiteSpace((char) ch); sb.Append ((char) ch), ch = Read ());
			if (ch != -1) Unread (ch);
			return sb.ToString ();
		}


		/// <summary>
		/// </summary>
		public void MarkLocation ()
		{
			if (markedLocation == Location.Unknown) {
				markedLocation = new Location (location);
			} else {
				markedLocation.CopyFrom (location);
			}
		}


		/// <summary>
		/// </summary>
		public void RestoreLocation ()
		{
			if (markedLocation != Location.Unknown) {
				location.CopyFrom (markedLocation);
			}
		}

	}

}

