// Mono.Util.CorCompare.ToDoNameSpace
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
	class ToDoNameSpace : MissingNameSpace
	{
		// e.g. <namespace name="System" missing="267" todo="453" complete="21">

		CompletionInfo ci;
		Type [] rgTypesMono;
		string strNamespace;
		ArrayList rgMissing = new ArrayList ();
		ArrayList rgTodo = new ArrayList ();
		protected static Hashtable htGhostTypes;
		static string[] rgstrGhostTypes = {"System.Object", "System.ValueType", "System.Delegate", "System.Enum"};

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

		static ToDoNameSpace ()
		{
			htGhostTypes = new Hashtable ();

			foreach (string strGhostType in rgstrGhostTypes)
			{
				htGhostTypes.Add (strGhostType, null);
			}
		}

		public ToDoNameSpace(string nameSpace, Type[] referenceTypes, 
			Type[] types) : base (nameSpace, referenceTypes)
		{
			rgTypesMono = types;
			strNamespace = nameSpace;
		}

		public override CompletionTypes Completion
		{
			get { return CompletionTypes.Todo; }
		}

		/// <summary>
		/// first we go through all the microsoft types adding any mono types that match, or missing types otherwise
		/// then we go through the unmatched mono types adding those
		/// uses a hashtable to speed up lookups
		/// </summary>
		/// <returns></returns>
		public CompletionInfo Analyze ()
		{
			Hashtable htMono = new Hashtable ();
			foreach (Type t in rgTypesMono)
			{
				if (t != null && (t.Namespace == null || strNamespace == t.Namespace))
					htMono.Add (t.FullName, t);
			}
			foreach (Type t in rgTypesMS)
			{
				if (t != null && strNamespace == t.Namespace)
				{
					Type tMono = (Type) htMono [t.FullName];
					CompletionInfo ciType;
					if (tMono == null)
					{
						if (t.IsPublic && !htGhostTypes.Contains (t.FullName))
						{
							MissingType mt = new MissingType (t);
							rgMissing.Add (mt);
							ciType = mt.Analyze ();
							ci.cMissing ++;
						}
					}
					else
					{
						htMono.Remove (t.FullName);
						ToDoType tdt = new ToDoType (t, tMono);
						ciType = tdt.Analyze ();
						if (ciType.cTodo != 0 || ciType.cMissing != 0)
						{
							rgTodo.Add (tdt);
							ci.cTodo ++;
						}
						else
						{
//							rgTodo.Add (tdt);
							ci.cComplete ++;
						}
					}
				}
			}
			// do any mono types that aren't in microsoft's namespace
			foreach (Type tMono in htMono.Values)
			{
				ToDoType tdt = new ToDoType (tMono, tMono);
				CompletionInfo ciType = tdt.Analyze ();
				if (ciType.cTodo != 0 || ciType.cMissing != 0)
				{
					rgTodo.Add (tdt);
					ci.cTodo ++;
				}
				else
				{
//					rgTodo.Add (tdt);
					ci.cComplete ++;
				}
			}
			return ci;
		}

		public override XmlElement CreateXML (XmlDocument doc)
		{
			XmlElement eltNameSpace = base.CreateXML (doc);
			eltNameSpace.SetAttribute ("status", "todo");

			if (ci.cMissing > 0 || ci.cTodo > 0)
			{
				XmlElement eltClasses = doc.CreateElement("classes");
				eltNameSpace.AppendChild (eltClasses);

				foreach (MissingType type in rgMissing) 
				{
					XmlElement eltClass = type.CreateXML (doc);
					if (eltClass != null)
						eltClasses.AppendChild (eltClass);
				}
				foreach (ToDoType type in rgTodo)
				{
					XmlElement eltClass = type.CreateXML (doc);
					if (eltClass != null)
						eltClasses.AppendChild (eltClass);
				}
				ci.SetAttributes (eltNameSpace);
			}
			return eltNameSpace;
		}
	}
}
