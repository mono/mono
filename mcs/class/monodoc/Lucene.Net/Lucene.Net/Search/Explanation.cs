/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Expert: Describes the score computation for document and query. </summary>
	[Serializable]
	public class Explanation
	{
		private float value_Renamed; // the value of this node
		private System.String description; // what it represents
		private System.Collections.ArrayList details; // sub-explanations
		
		public Explanation()
		{
		}
		
		public Explanation(float value_Renamed, System.String description)
		{
			this.value_Renamed = value_Renamed;
			this.description = description;
		}
		
		/// <summary>The value assigned to this explanation node. </summary>
		public virtual float GetValue()
		{
			return value_Renamed;
		}
		/// <summary>Sets the value assigned to this explanation node. </summary>
		public virtual void  SetValue(float value_Renamed)
		{
			this.value_Renamed = value_Renamed;
		}
		
		/// <summary>A description of this explanation node. </summary>
		public virtual System.String GetDescription()
		{
			return description;
		}
		/// <summary>Sets the description of this explanation node. </summary>
		public virtual void  SetDescription(System.String description)
		{
			this.description = description;
		}
		
		/// <summary>The sub-nodes of this explanation node. </summary>
		public virtual Explanation[] GetDetails()
		{
			if (details == null)
				return null;
            return (Explanation[]) details.ToArray(typeof(Explanation));
		}
		
		/// <summary>Adds a sub-node to this explanation node. </summary>
		public virtual void  AddDetail(Explanation detail)
		{
			if (details == null)
				details = new System.Collections.ArrayList();
			details.Add(detail);
		}
		
		/// <summary>Render an explanation as text. </summary>
		public override System.String ToString()
		{
			return ToString(0);
		}
		private System.String ToString(int depth)
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			for (int i = 0; i < depth; i++)
			{
				buffer.Append("  ");
			}
			buffer.Append(GetValue());
			buffer.Append(" = ");
			buffer.Append(GetDescription());
			buffer.Append("\n");
			
			Explanation[] details = GetDetails();
			if (details != null)
			{
				for (int i = 0; i < details.Length; i++)
				{
					buffer.Append(details[i].ToString(depth + 1));
				}
			}
			
			return buffer.ToString();
		}
		
		
		/// <summary>Render an explanation as HTML. </summary>
		public virtual System.String ToHtml()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			buffer.Append("<ul>\n");
			
			buffer.Append("<li>");
			buffer.Append(GetValue());
			buffer.Append(" = ");
			buffer.Append(GetDescription());
			buffer.Append("</li>\n");
			
			Explanation[] details = GetDetails();
			if (details != null)
			{
				for (int i = 0; i < details.Length; i++)
				{
					buffer.Append(details[i].ToHtml());
				}
			}
			
			buffer.Append("</ul>\n");
			
			return buffer.ToString();
		}
	}
}