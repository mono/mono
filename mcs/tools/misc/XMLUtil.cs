// IFaceDisco.cs
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

using System;
using System.Text;
using System.Collections;

namespace Mono.Util
{
	class XMLUtil{
		public static string ToXML(
			ArrayList list, 
			string itemWrap,
			string listWrap)
		{
			if (null == itemWrap){
				throw new ArgumentNullException("itemWrap");
			}
			if (null == listWrap){
				throw new ArgumentNullException("listWrap");
			}
			StringBuilder output = new StringBuilder();
			output.Append("<"+listWrap+">");
			foreach(object o in list){
				output.Append("\n<"+itemWrap+">");
				output.Append(o.ToString());
				output.Append("</"+itemWrap+">");
			}
			output.Append("\n</"+listWrap+">");
			return output.ToString();
		}
	}
}