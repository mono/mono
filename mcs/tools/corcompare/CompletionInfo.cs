// Mono.Util.CorCompare.CompletionInfo
//
// Author(s):
//   Piers Haken (piersh@friskit.com)
//
// (C) 2001-2002 Piers Haken

using System;
using System.Xml;
using System.Collections;

namespace Mono.Util.CorCompare
{
	#region
	public struct CompletionType
	{
		private enum CompletionTypes
		{
			Present,
			Missing,
			Extra
		}
		private const int MASK_TYPE = 0x0f;
		private const int MASK_TODO = 0x10;
		//private const int MASK_ERROR = 0x20;
		private int m_type;

		private CompletionType (CompletionTypes type, bool fTodo)
		{
			m_type = (int) type;

			if (fTodo)
				m_type |= MASK_TODO;
		}

		public bool IsPresent
		{
			get { return Type == CompletionTypes.Present; }
		}
		public bool IsMissing
		{
			get { return Type == CompletionTypes.Missing; }
		}
		public bool IsExtra
		{
			get { return Type == CompletionTypes.Extra; }
		}
		public bool IsTodo
		{
			get { return (m_type & MASK_TODO) != 0; }
		}
		private CompletionTypes Type
		{
			get { return (CompletionTypes) (m_type & MASK_TYPE); }
		}

		public override string ToString ()
		{
			switch (Type)
			{
				case CompletionTypes.Missing:
					return "missing";
				case CompletionTypes.Extra:
					return "extra";
				case CompletionTypes.Present:
					return "present";
				default:
					throw new Exception ("Invalid CompletionType: "+Type);
			}
		}

		public static CompletionType Present
		{
			get { return new CompletionType (CompletionTypes.Present, false); }
		}
		public static CompletionType Missing
		{
			get { return new CompletionType (CompletionTypes.Missing, false); }
		}
		public static CompletionType Extra
		{
			get { return new CompletionType (CompletionTypes.Extra, false); }
		}
		public static CompletionType Compare (Object oMono, Object oMS)
		{
			if (oMono == null)
				return Missing;
			else if (oMS == null)
				return Extra;
			else
				return Present;
		}
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
		public int cPresent;
		public int cExtra;
		public int cMissing;
		public int cTodo;

		/// <summary>
		/// converts a CompletionTypes into a CompletionInfo
		/// sets the corresponding field to '1'
		/// </summary>
		/// <param name="ct">the CompletionTypes to convert</param>
		public CompletionInfo (CompletionType ct)
		{
			cPresent = cTodo = cMissing = cExtra = 0;
			if (ct.IsPresent)
				cPresent = 1;
			else if (ct.IsMissing)
				cMissing = 1;
			else if (ct.IsExtra)
				cExtra = 1;

			if (ct.IsTodo)
				cTodo = 1;
		}

		/// <summary>
		/// counts the total number of elements represented by this info
		/// </summary>
		public int cTotal
		{
			get
			{
				return cPresent + cTodo + cMissing;
			}
		}

		/// <summary>
		/// adds two CompletionInfos together
		/// </summary>
		/// <param name="m_nodeStatus"></param>
		public void Add (CompletionInfo m_nodeStatus)
		{
			cPresent += m_nodeStatus.cPresent;
			cTodo += m_nodeStatus.cTodo;
			cMissing += m_nodeStatus.cMissing;
			cExtra += m_nodeStatus.cExtra;
		}

		/// <summary>
		/// subtracts two CompletionInfos
		/// </summary>
		/// <param name="m_nodeStatus"></param>
		public void Sub (CompletionInfo m_nodeStatus)
		{
			cPresent -= m_nodeStatus.cPresent;
			cTodo -= m_nodeStatus.cTodo;
			cMissing -= m_nodeStatus.cMissing;
			cExtra -= m_nodeStatus.cExtra;
			if (cPresent < 0 || cTodo < 0 || cMissing < 0 || cExtra < 0)
				throw new Exception ("Completion underflow on subtract");
		}

		/// <summary>
		/// increments the corresponding field
		/// </summary>
		/// <param name="ct"></param>
		public void Add (CompletionType ct)
		{
			Add (new CompletionInfo (ct));
		}
		/// <summary>
		/// decrements the corresponding field
		/// </summary>
		/// <param name="ct"></param>
		public void Sub (CompletionType ct)
		{
			Sub (new CompletionInfo (ct));
		}

		/// <summary>
		/// adds appropriate 'missing', 'todo' & 'complete' attributes to an XmlElement
		/// </summary>
		/// <param name="elt"></param>
		public void SetAttributes (XmlElement elt)
		{
			elt.SetAttribute ("present", cPresent.ToString ());
			elt.SetAttribute ("missing", cMissing.ToString ());
			elt.SetAttribute ("extra", cExtra.ToString ());
			elt.SetAttribute ("todo", cTodo.ToString ());

			//int percentComplete = (cTotal == 0) ? 100 : (100 - 100 * (cMissing + cExtra) / cTotal);
			//elt.SetAttribute ("complete", percentComplete.ToString ());
		}
	}

	#endregion

	public enum PresenceTypes
	{
//		UNINITIALIZED = 0,
		Missing = 0,
		Present,
		Extra
	}

	public enum ErrorTypes
	{
		// TODO: order is important here... (see Status.SetError ())
//		UNINITIALIZED = 0,
		OK = 0,
		Todo,
		Warning,
		Error
	}

	public struct PresenceCounts
	{
		public int cMissing;
		public int cPresent;
		public int cExtra;

		public PresenceCounts (PresenceTypes type)
		{
			cMissing = cPresent = cExtra = 0;
			if (type == PresenceTypes.Missing)
				cMissing = 1;
			else if (type == PresenceTypes.Present)
				cPresent = 1;
			else if (type == PresenceTypes.Extra)
				cExtra = 1;
			else throw new Exception ("Invalid PresenceType");
		}
		public int Total
		{
			get { return cMissing + cPresent + cExtra; }
		}
		public void Add (PresenceCounts counts)
		{
			cMissing += counts.cMissing;
			cPresent += counts.cPresent;
			cExtra += counts.cExtra;
		}
		public void Sub (PresenceCounts counts)
		{
			cMissing -= counts.cMissing;
			cPresent -= counts.cPresent;
			cExtra -= counts.cExtra;

			if (cMissing < 0 || cPresent < 0 || cExtra < 0)
				throw new Exception ("Underflow");
		}
		public void Add (PresenceTypes type)
		{
			Add (new PresenceCounts (type));
		}
		public void Sub (PresenceTypes type)
		{
			Sub (new PresenceCounts (type));
		}
		public void SetAttributes (XmlElement elt, string strSuffix)
		{
			if (cMissing != 0)
				elt.SetAttribute ("missing"+strSuffix, cMissing.ToString ());
			if (cPresent != 0)
				elt.SetAttribute ("present"+strSuffix, cPresent.ToString ());
			if (cExtra != 0)
				elt.SetAttribute ("extra"+strSuffix, cExtra.ToString ());
		}
	}

	public struct ErrorCounts
	{
		public int cOK;
		public int cTodo;
		public int cWarning;
		public int cError;

		public ErrorCounts (ErrorTypes type)
		{
			cOK = cTodo = cWarning = cError = 0;
			if (type == ErrorTypes.OK)
				cOK = 1;
			else if (type == ErrorTypes.Todo)
				cTodo = 1;
			else if (type == ErrorTypes.Warning)
				cWarning = 1;
			else if (type == ErrorTypes.Error)
				cError = 1;
			else throw new Exception ("Invalid ErrorType");
		}
		public int Total
		{
			get { return cOK + cTodo + cWarning + cError; }
		}
		public void Add (ErrorCounts counts)
		{
			cOK += counts.cOK;
			cTodo += counts.cTodo;
			cWarning += counts.cWarning;
			cError += counts.cError;
		}
		public void Sub (ErrorCounts counts)
		{
			cOK -= counts.cOK;
			cTodo -= counts.cTodo;
			cWarning -= counts.cWarning;
			cError -= counts.cError;
			if (cOK < 0 || cTodo < 0 || cWarning < 0 || cError < 0)
				throw new Exception ("Underflow");
		}
		public void Add (ErrorTypes type)
		{
			Add (new ErrorCounts (type));
		}
		public void Sub (ErrorTypes type)
		{
			Sub (new ErrorCounts (type));
		}
		public void SetAttributes (XmlElement elt, string strSuffix)
		{
			if (cOK != 0)
				elt.SetAttribute ("ok"+strSuffix, cOK.ToString ());
			if (cTodo != 0)
				elt.SetAttribute ("todo"+strSuffix, cTodo.ToString ());
			if (cWarning != 0)
				elt.SetAttribute ("warning"+strSuffix, cWarning.ToString ());
			if (cError != 0)
				elt.SetAttribute ("error"+strSuffix, cError.ToString ());
		}
	}

	public struct Status
	{
		public PresenceTypes presence;
		public ErrorTypes error;

		public string PresenceName
		{
			get
			{
				if (presence == PresenceTypes.Missing)
					return "missing";
				else if (presence == PresenceTypes.Present)
					return "present";
				else if (presence == PresenceTypes.Extra)
					return "extra";
				else throw new Exception ("Invalid PresenceType");
			}
		}
		public string ErrorName
		{
			get
			{
				if (error == ErrorTypes.OK)
					return "OK";
				else if (error == ErrorTypes.Todo)
					return "todo";
				else if (error == ErrorTypes.Warning)
					return "warning";
				else if (error == ErrorTypes.Error)
					return "error";
				else throw new Exception ("Invalid ErrorType");
			}
		}
		public void SetAttributes (XmlElement elt)
		{
			if (presence != PresenceTypes.Present)
				elt.SetAttribute ("presence", PresenceName);
			if (error != ErrorTypes.OK)
				elt.SetAttribute ("error", ErrorName);
		}
	}

	public struct StatusCounts
	{
		public PresenceCounts presenceCounts;
		public ErrorCounts errorCounts;

		public void Add (StatusCounts statusCounts)
		{
			presenceCounts.Add (statusCounts.presenceCounts);
			errorCounts.Add (statusCounts.errorCounts);
			if (presenceCounts.Total != errorCounts.Total)
				throw new Exception ("invalid status counts");
		}
		public void Sub (StatusCounts statusCounts)
		{
			presenceCounts.Sub (statusCounts.presenceCounts);
			errorCounts.Sub (statusCounts.errorCounts);
			if (presenceCounts.Total != errorCounts.Total)
				throw new Exception ("invalid status counts");
		}
		public void Add (Status status)
		{
			presenceCounts.Add (status.presence);
			errorCounts.Add (status.error);
			if (presenceCounts.Total != errorCounts.Total)
				throw new Exception ("invalid status counts");
		}
		public void Sub (Status status)
		{
			presenceCounts.Sub (status.presence);
			errorCounts.Sub (status.error);
			if (presenceCounts.Total != errorCounts.Total)
				throw new Exception ("invalid status counts");
		}
		public void SetAttributes (XmlElement elt, string strSuffix)
		{
			presenceCounts.SetAttributes (elt, strSuffix);
			errorCounts.SetAttributes (elt, strSuffix);

			int cTotal = presenceCounts.cMissing + presenceCounts.cPresent;
			int cIncomplete =
				presenceCounts.cMissing +
				errorCounts.cTodo +
				errorCounts.cWarning +
				errorCounts.cError;

			if (presenceCounts.Total != errorCounts.Total)
				throw new Exception ("invalid status counts");

			if (cTotal != 0)
			{
				int percentComplete = 100 * (cTotal - cIncomplete) / cTotal;
				elt.SetAttribute ("complete" + strSuffix, percentComplete.ToString ());
			}
		}
	}

	public class NodeMessage
	{
		protected string msg;

		public NodeMessage (string _msg)
		{
			msg = _msg;
		}

		public string Message
		{
			get { return msg; }
		}
	}

	public class NodeStatus
	{
		public Status status;
		protected StatusCounts statusCountsChildren;
		protected StatusCounts statusCountsTotal;
		protected IList lstWarnings;

		public NodeStatus ()
		{
			status.error = ErrorTypes.OK;
		}

		/// <summary>
		/// Constructs a NodeStatus by comparing the presence of two objects
		/// it only sets the status.presence field
		/// </summary>
		/// <param name="objMono"></param>
		/// <param name="objMS"></param>
		public NodeStatus (Object objMono, Object objMS)
		{
			status.error = ErrorTypes.OK;
			statusCountsChildren = statusCountsTotal = new StatusCounts ();
			if (objMono == null)
				status.presence = PresenceTypes.Missing;
			else if (objMS == null)
				status.presence = PresenceTypes.Extra;
			else
				status.presence = PresenceTypes.Present;
		}
		public void Add (NodeStatus statusChild)
		{
			if ((int) statusChild.status.error > (int) status.error)
				status.error = statusChild.status.error;
			statusCountsTotal.Add (statusChild.statusCountsTotal);
			statusCountsChildren.Add (statusChild.statusCountsChildren);
		}
		public void AddChildren (NodeStatus statusChild)
		{
			statusCountsTotal.Add (statusChild.statusCountsTotal);
			statusCountsTotal.Add (statusChild.status);
			statusCountsChildren.Add (statusChild.status);
		}

		public void SubChildren (NodeStatus statusChild)
		{
			statusCountsTotal.Sub (statusChild.statusCountsTotal);
			statusCountsTotal.Sub (statusChild.status);
			statusCountsChildren.Sub (statusChild.status);
		}

		public void Add (StatusCounts statusCounts)
		{
			statusCountsChildren.Add (statusCounts);
			statusCountsTotal.Add (statusCounts);
		}

		public void Sub (StatusCounts statusCounts)
		{
			statusCountsChildren.Sub (statusCounts);
			statusCountsTotal.Sub (statusCounts);
		}

		public void Add (Status status)
		{
			statusCountsChildren.Add (status);
			statusCountsTotal.Add (status);
		}

		public void Sub (Status status)
		{
			statusCountsChildren.Sub (status);
			statusCountsTotal.Sub (status);
		}


		public bool IsMissing
		{
			get { return status.presence == PresenceTypes.Missing; }
		}
		public bool IsPresent
		{
			get { return status.presence == PresenceTypes.Present; }
		}
		public bool IsExtra
		{
			get { return status.presence == PresenceTypes.Extra; }
		}

		public void SetAttributes (XmlElement elt)
		{
			status.SetAttributes (elt);
			statusCountsChildren.SetAttributes (elt, "");
			statusCountsTotal.SetAttributes (elt, "_total");

			// add any warning messages
			if (lstWarnings != null)
			{
				XmlElement eltWarnings = elt.OwnerDocument.CreateElement ("warnings");
				elt.AppendChild (eltWarnings);
				foreach (NodeMessage msg in lstWarnings)
				{
					XmlElement eltWarning = elt.OwnerDocument.CreateElement ("warning");
					eltWarnings.AppendChild (eltWarning);
					eltWarning.SetAttribute ("text", msg.Message);
				}
			}

			//int percentComplete = (cTotal == 0) ? 100 : (100 - 100 * (cMissing + cExtra) / cTotal);
			//elt.SetAttribute ("complete", percentComplete.ToString ());
		}
		public void SetError (ErrorTypes errorNew)
		{
			// TODO: assumes order of error values
			if ((int) errorNew > (int) status.error)
				status.error = errorNew;
		}
		public void AddWarning (string strWarning)
		{
			if (lstWarnings == null)
				lstWarnings = new ArrayList ();
			lstWarnings.Add (new NodeMessage (strWarning));
			SetError (ErrorTypes.Warning);
		}
	}
}