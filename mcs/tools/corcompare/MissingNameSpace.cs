// Mono.Util.CorCompare.MissingNameSpace
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001-2002 Nick Drochak

using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace Mono.Util.CorCompare {

	/// <summary>
	/// 	Represents a namespace that has missing and/or MonoTODO classes.
	/// </summary>
	/// <remarks>
	/// 	created by - Nick
	/// 	created on - 2/20/2002 10:43:57 PM
	/// </remarks>
	class MissingNameSpace : MissingBase
	{
		// e.g. <namespace name="System" missing="267" todo="453" complete="21">
		protected ArrayList rgTypesMono, rgTypesMS;
		string strNamespace;
		ArrayList rgTypes = new ArrayList ();
		protected static Hashtable htGhostTypes;
		static string[] rgstrGhostTypes = {"System.Object", "System.ValueType", "System.Delegate", "System.Enum"};


		static MissingNameSpace ()
		{
			htGhostTypes = new Hashtable ();

			foreach (string strGhostType in rgstrGhostTypes)
			{
				htGhostTypes.Add (strGhostType, null);
			}
		}

		public MissingNameSpace(string nameSpace, ArrayList _rgTypesMono, ArrayList _rgTypesMS)
		{
			strNamespace = nameSpace;
			rgTypesMono = _rgTypesMono;
			rgTypesMS = _rgTypesMS;
			m_nodeStatus = new NodeStatus (_rgTypesMono, _rgTypesMS);
		}

		public virtual string [] MissingTypeNames (bool f)
		{
			return null;
		}

		public virtual ArrayList ToDoTypeNames
		{
			get { return null; }
		}

		public override string Name 
		{
			get { return strNamespace; }
		}
		public override string Type
		{
			get { return "namespace"; }
		}


		/// <summary>
		/// first we go through all the microsoft types adding any mono types that match, or missing types otherwise
		/// then we go through the unmatched mono types adding those
		/// uses a hashtable to speed up lookups
		/// </summary>
		/// <returns></returns>
		public override NodeStatus Analyze ()
		{
			Hashtable htMono = new Hashtable ();
			if (rgTypesMono != null)
			{
				foreach (Type t in rgTypesMono)
				{
					htMono.Add (t.FullName, t);
				}
			}
			if (rgTypesMS != null)
			{
				foreach (Type t in rgTypesMS)
				{
					Type tMono = (Type) htMono [t.FullName];
					MissingType mt = null;
					if (tMono == null)
					{
						if (t.IsPublic && !htGhostTypes.Contains (t.FullName))
							mt = new MissingType (null, t);
					}
					else
					{
						if (t.IsPublic)
						{
							htMono.Remove (t.FullName);
							mt = new MissingType (tMono, t);
						}
					}
					if (mt != null)
					{
						NodeStatus nsType = mt.Analyze ();
						m_nodeStatus.AddChildren (nsType);
						rgTypes.Add (mt);
					}
				}
			}
			// do any mono types that aren't in microsoft's namespace
			foreach (Type tMono in htMono.Values)
			{
				if (tMono.IsPublic)
				{
					MissingType tdt = new MissingType (tMono, null);
					NodeStatus nsType = tdt.Analyze ();
					m_nodeStatus.AddChildren (nsType);
					rgTypes.Add (tdt);
				}
			}
			return m_nodeStatus;
		}

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltNameSpace = base.CreateXML (doc);

			// TODO: include complete namespaces?
//			if (m_nodeStatus.statusCountsTotal.cMissing > 0 || m_nodeStatus.statusCountsTotal.cTodo > 0)
			{
				XmlElement eltClasses = doc.CreateElement("classes");
				eltNameSpace.AppendChild (eltClasses);

				foreach (MissingType type in rgTypes) 
				{
					XmlElement eltClass = type.CreateXML (doc);
					if (eltClass != null)
						eltClasses.AppendChild (eltClass);
				}
			}
			return eltNameSpace;
		}


		public static ArrayList GetNamespaces(Type[] types) 
		{
			ArrayList nsList = new ArrayList();
			foreach (Type t in types) 
			{
				if (!nsList.Contains(t.Namespace)) 
				{
					nsList.Add(t.Namespace);
				}
			}
			return nsList;
		}
	}
}

