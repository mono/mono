// Mono.Util.CorCompare.CompletionInfo
//
// Author(s):
//   Piers Haken (piersh@friskit.com)
//
// (C) 2001-2002 Piers Haken

using System;
using System.Reflection;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// Represents the 3 different stages of completion that an element can be in.
	/// </summary>
	public enum CompletionTypes
	{
		Complete,
		Todo,
		Missing
	}

	/// <summary>
	/// 	Represents the amount of work done on a node
	/// </summary>
	/// <remarks>
	/// 	created by - Piersh
	/// 	created on - 3/2/2002 1:12:00 AM
	/// </remarks>

	public struct CompletionInfo
	{
		public int cComplete;
		public int cTodo;
		public int cMissing;

		/// <summary>
		/// converts a CompletionTypes into a CompletionInfo
		/// sets the corresponding field to '1'
		/// </summary>
		/// <param name="ct">the CompletionTypes to convert</param>
		public CompletionInfo (CompletionTypes ct)
		{
			cComplete = cTodo = cMissing = 0;
			switch (ct)
			{
				case CompletionTypes.Complete:
					cComplete = 1;
					break;
				case CompletionTypes.Todo:
					cTodo = 1;
					break;
				case CompletionTypes.Missing:
					cMissing = 1;
					break;
				default:
					throw new Exception ("Invalid CompletionType: "+ct.ToString ());
			}
		}

		/// <summary>
		/// counts the total number of elements represented by this info
		/// </summary>
		public int cTotal
		{
			get
			{
				return cComplete + cTodo + cMissing;
			}
		}

		/// <summary>
		/// adds two CompletionInfos together
		/// </summary>
		/// <param name="ci"></param>
		public void Add (CompletionInfo ci)
		{
			cComplete += ci.cComplete;
			cTodo += ci.cTodo;
			cMissing += ci.cMissing;
		}

		/// <summary>
		/// increments the corresponding field
		/// </summary>
		/// <param name="ct"></param>
		public void Add (CompletionTypes ct)
		{
			switch (ct)
			{
				case CompletionTypes.Complete:
					cComplete ++;
					break;
				case CompletionTypes.Todo:
					cTodo ++;
					break;
				case CompletionTypes.Missing:
					cMissing ++;
					break;
				default:
					throw new Exception ("Invalid CompletionType: "+ct.ToString ());
			}
		}
		/// <summary>
		/// decrements the corresponding field
		/// </summary>
		/// <param name="ct"></param>
		public void Sub (CompletionTypes ct)
		{
			switch (ct)
			{
				case CompletionTypes.Complete:
					cComplete --;
					break;
				case CompletionTypes.Todo:
					cTodo --;
					break;
				case CompletionTypes.Missing:
					cMissing --;
					break;
				default:
					throw new Exception ("Invalid CompletionType: "+ct.ToString ());
			}
			if (cComplete < 0 || cTodo < 0 || cMissing < 0)
				throw new Exception ("Completion underflow on subtract");
		}

		/// <summary>
		/// adds appropriate 'missing', 'todo' & 'complete' attributes to an XmlElement
		/// </summary>
		/// <param name="elt"></param>
		public void SetAttributes (XmlElement elt)
		{
			elt.SetAttribute ("missing", cMissing.ToString ());
			elt.SetAttribute ("todo", cTodo.ToString ());

			int percentComplete = (cTotal == 0) ? 100 : (100 - 100 * (cMissing + cTodo) / cTotal);
			elt.SetAttribute ("complete", percentComplete.ToString ());
		}
	}
}
