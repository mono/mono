/**
 * Namespace: System.Web.UI.WebControls
 * Class:     FontInfo
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  80%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Drawing;

namespace System.Web.UI.WebControls
{
	public sealed class FontInfo
	{
		private bool bold;
		private bool italic;
		private bool overline;
		private bool strikeout;
		private bool underline;
		private string name;		//TODO: This will have the value of names[0] by default
		private string[] names;		//TODO: How do get the list of fonts available?
		private FontUnit size = FontUnit.Empty;
		
		internal FontInfo()
		{
			bold      = false;
			italic    = false;
			overline  = false;
			strikeout = false;
			underline = false;
			name      = string.Empty;
		}
		
		public bool Bold
		{
			get
			{
				return bold;
			}
			set
			{
				bold = value;
			}
		}
		
		public bool Italic
		{
			get
			{
				return italic;
			}
			set
			{
				italic = value;
			}
		}
		
		public bool Overline
		{
			get
			{
				return overline;
			}
			set
			{
				overline = value;
			}
		}
		
		public bool Strikeout
		{
			get
			{
				return strikeout;
			}
			set
			{
				strikeout = value;
			}
		}
		
		public bool Underline
		{
			get
			{
				return underline;
			}
			set
			{
				underline = value;
			}
		}
		
		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}
		
		public string[] Names
		{
			get
			{
				return names;
			}
			set
			{
				names = value;
				name = names[0];
			}
		}

		//TODO: To throw exception if the index is negative
		public FontUnit Size
		{
			get
			{
				return size;
			}
			set
			{
				size = value;
			}
		}
		
		public void CopyFrom(FontInfo from)
		{
			//TODO: What a rubbish way to accomplish the task
			/*this.bold = from.Bold;
			this.italic = from.Italic;
			this.name = from.Name;
			this.names = from.Names;
			this.overline = from.Overline;
			this.size = from.Size;*/
			//TODO: Let me try Relflection
			Type t = from.GetType();
			MethodInfo[] fi = t.GetMethods();
			foreach(MethodInfo f in fi)
			{
				//System.Console.WriteLine("Field: {0}", f.Name);
				if(f.Name.StartsWith("get_"))
				{
					System.Console.WriteLine("\tStarts with get_");
				}
			}
		}
		
		private void ListFields(FontInfo from)
		{
			Type t = from.GetType();
			MethodInfo[] fi = t.GetMethods();
			foreach(MethodInfo f in fi)
			{
				System.Console.WriteLine("Field: {0}", f.Name);
				if(f.Name.StartsWith("get_"))
				{
					System.Console.WriteLine("\tStarts with get_");
				}
			}
		}

		//TODO: after CopyFrom is implemented
		public void MergeWith(FontInfo with)
		{
		}

		public override string ToString()
		{
			string retVal = this.name;
			if(this.size != FontUnit.Empty)
			{
				this.name += ("," + this.size);
			}
			return retVal;
		}

		/*
		protected object MemberwiseClone()
		{
		}
//*/

	}
}
