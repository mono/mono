// Mono.Util.CorCompare.MissingAttribute
//
// Author(s):
//   Piers Haken (piersh@friskit.com)
//
// (C) 2001-2002 Piers Haken
using System;
using System.Xml;
using System.Reflection;
using System.Collections;

namespace Mono.Util.CorCompare
{

	/// <summary>
	/// 	Represents an Attribute that is completely missing
	/// </summary>
	/// <remarks>
	/// 	created by - Piersh
	/// 	created on - 3/2/2002 9:47:00 pm
	/// </remarks>
	class MissingAttribute : MissingBase
	{
		// e.g. <attribute name="Equals" status="missing"/>
		Object attribute;

		public MissingAttribute (Object _attribute) 
		{
			attribute = _attribute;
		}

		public override string Name 
		{
			get { return attribute.ToString (); }
		}

		public override string Type
		{
			get { return "attribute"; }
		}

		/// <summary>
		/// creates a map from a list of attributes
		/// the hashtable maps from name to attribute
		/// </summary>
		/// <param name="rgAttributes">the list of attributes</param>
		/// <returns>a map</returns>
		public static Hashtable GetAttributeMap (Object [] rgAttributes)
		{
			Hashtable map = new Hashtable ();
			foreach (Object attribute in rgAttributes)
			{
				if (attribute != null)
				{
					string strName = attribute.ToString ();
					if (!map.Contains (strName))
						map.Add (strName, attribute);
				}
			}
			return map;
		}

		/// <summary>
		/// analyzes two sets of reflected attributes, generates a list
		/// of MissingAttributes according to the completion of the first set wrt the second.
		/// </summary>
		/// <param name="rgAttributesMono">mono attributes</param>
		/// <param name="rgAttributesMS">microsoft attributes</param>
		/// <param name="rgAttributes">where the results are put</param>
		/// <returns>completion info for the whole set</returns>
		public static CompletionInfo AnalyzeAttributes (Object [] rgAttributesMono, Object [] rgAttributesMS, ArrayList rgAttributes)
		{
			CompletionInfo ci = new CompletionInfo ();

			Hashtable mapAttributesMono = (rgAttributesMono == null) ? new Hashtable () : MissingAttribute.GetAttributeMap (rgAttributesMono);
			Hashtable mapAttributesMS   = (rgAttributesMS   == null) ? new Hashtable () : MissingAttribute.GetAttributeMap (rgAttributesMS);

			foreach (Object attribute in mapAttributesMS.Values)
			{
				string strAttribute = attribute.ToString ();
				Object attributeMono = mapAttributesMono [strAttribute];
				if (attributeMono == null)
				{
					rgAttributes.Add (new MissingAttribute (attribute));
					ci.cMissing ++;
				}
				else
				{
					rgAttributes.Add (new CompleteAttribute (attributeMono));
					mapAttributesMono.Remove (strAttribute);
					ci.cComplete ++;
				}
			}
			foreach (Object attribute in mapAttributesMono.Values)
			{
				if (attribute.ToString () == "System.MonoTODOAttribute")
				{
					ci.cTodo ++;
				}
				else
				{
					rgAttributes.Add (new CompleteAttribute (attribute));
					ci.cComplete ++;
				}
			}
			return ci;
		}
	}

	class CompleteAttribute : MissingAttribute
	{
		public CompleteAttribute (Object _attribute) : base (_attribute) {}

		public override CompletionTypes Completion
		{
			get { return CompletionTypes.Complete; }
		}
	}
}
