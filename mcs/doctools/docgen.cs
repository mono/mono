// docgen.cs
//
// Adam Treat (manyoso@yahoo.com)
//
// (C) 2002 Adam Treat
//
// Licensed under the terms of the GNU GPL

using System;
using System.IO;
using System.Collections;
using System.Xml;

namespace Mono.Util
{
	public class DocGen
	{
		public static void Main(string[] args)
		{
			string stubfile = null;
			string externfile = null;
			string monofile = null;

            if(args.Length < 2)
			{
				Console.Write("Usage: docgen <stubfile> <externfile> <outputfile>\n");
				return;
            }

            if(!File.Exists(args[0]))
			{
                Console.WriteLine("\n   No stub.xml file at {0}\n", args[0]);
                return;
            }

            if(!File.Exists(args[1]))
			{
                Console.WriteLine("\n   No extern.xml file at {0}\n", args[1]);
                return;
            }

			if(args.Length > 2)
			{
				stubfile = args[0];
				externfile = args[1];
				monofile = args[2];
			}

			try
			{
				XmlDocument stubxml = new XmlDocument();
				stubxml.PreserveWhitespace = true;
				stubxml.Load(stubfile);

				XmlDocument externxml = new XmlDocument();
				externxml.PreserveWhitespace = true;
				externxml.Load(externfile);

				XmlDocument monoxml = new XmlDocument();
				monoxml.PreserveWhitespace = true;

				XmlNode stubassembly = stubxml.DocumentElement;
				XmlNode stubclass = stubassembly["class"];
				IEnumerator stubenum = stubclass.GetEnumerator();
				XmlNode stubtarget;

				XmlNode externassembly = externxml.DocumentElement;
				XmlNode externclass = externassembly["class"];
				IEnumerator externenum = externclass.GetEnumerator();
				XmlNode externtarget;

				// This is an ugly ugly UGLY hack.  I know.  It is just here to prove
				// a point and will be replaced shortly.
				while(stubenum.MoveNext())
				{
					stubtarget = (XmlNode)stubenum.Current;
					if(stubtarget.Name == "member")
					{
						XmlNode stubmember = (XmlNode)stubenum.Current;
						XmlNode stubmembername = stubmember["name"];
						//Console.WriteLine(stubmembername.InnerText);
						while(externenum.MoveNext())
						{
							externtarget = (XmlNode)externenum.Current;
							if(externtarget.Name == "member")
							{
								XmlNode externmember = (XmlNode)externenum.Current;
								XmlAttributeCollection attrColl = externmember.Attributes;
								if(attrColl["name"].InnerText == stubmembername.InnerText)
								{
									if (externmember.HasChildNodes)
									{
										for (int i=0; i<externmember.ChildNodes.Count; i++)
										{
											string w ="#whitespace";
											if(externmember.ChildNodes[i].Name != w)
											{
												XmlNode child = stubxml.ImportNode(
														externmember.ChildNodes[i], true);
												stubmember.AppendChild(child);
											}
										}
									}
								}
							}
    					}
						// This resets the enumerator for the next go round
						externenum.Reset();
					}
    			}
				XmlTextWriter writer = new XmlTextWriter(monofile, null);
				writer.Formatting = Formatting.Indented;
				stubxml.WriteContentTo(writer);
				writer.Flush();
				writer.Close();
			}
			catch(XmlException e)
			{
				throw(e);
			}
			catch(Exception e)
			{
				throw(e);
			}
		}
	}
}
