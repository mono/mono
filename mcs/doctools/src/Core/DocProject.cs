// DocProject.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette

// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Xml;

namespace Mono.Doc.Core
{
	public class DocProject
	{
		public static string UntitledProjectName = "Untitled"; // TODO: i18n

		private ArrayList assemblyFiles;
		private ArrayList xmlDirectories;
		private Hashtable properties;
		private bool      isModified;
		private bool      isNewProject;
		private string    projectFileName;

		public event EventHandler Modified;

		public DocProject()
		{
			assemblyFiles   = new ArrayList();
			xmlDirectories  = new ArrayList();
			properties      = new Hashtable();
			isModified      = false;
			isNewProject    = true;
			projectFileName = DocProject.UntitledProjectName;
		}

		#region Public Instance Methods
		
		public void Load(string fileName)
		{
			Clear();

			XmlTextReader xml = null;

			try
			{
				StreamReader stream = new StreamReader(fileName);
				xml                 = new XmlTextReader(stream);

				xml.MoveToContent();
				xml.ReadStartElement("monodoc-project");

				while (xml.Read()) 
				{
					if (xml.NodeType == XmlNodeType.Element)
					{
						switch (xml.Name)
						{
							case "assemblies":
								LoadAssemblies(xml);
								break;
							case "xmlDocs":
								LoadXmlDirectories(xml);
								break;
							case "property":
								LoadProperties(xml);
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				// FIXME: i18n
				throw new MonodocException(
					"Problem reading project file '" + fileName + "'.\n" + e.Message, e
					);
			} 
			finally 
			{
				if (xml != null) xml.Close();
			}


			projectFileName = fileName;
			isModified      = false;
			isNewProject    = false;
		}

		public void Save()
		{
			Save(projectFileName);
		}

		public void Save(string fileName)
		{
			if (DocProject.UntitledProjectName == fileName)
			{
				throw new MonodocException("Cannot save untitled (default) project.");
			}
			
			XmlTextWriter xml = null;

			try
			{
				StreamWriter stream = new StreamWriter(fileName, false, new UTF8Encoding());
				xml                 = new XmlTextWriter(stream);
				xml.Formatting      = Formatting.Indented;
				xml.Indentation     = 4;

				// <monodoc-project>
				xml.WriteStartElement("monodoc-project");

				SaveAssemblies(xml);
				SaveXmlDirectories(xml);
				SaveProperties(xml);

				// </monodoc-project>
				xml.WriteEndElement();

			} 
			catch (Exception e)
			{
				// TODO: i18n
				throw new MonodocException(
					"Problem saving project file '" + fileName + "'.\n" + e.Message, e
					);
			}
			finally
			{
				if (xml != null) xml.Close();
			}

			projectFileName = fileName;
			isModified      = false;
			isNewProject    = false;
		}

		public void Clear()
		{
			assemblyFiles.Clear();
			xmlDirectories.Clear();
			properties.Clear();

			projectFileName = DocProject.UntitledProjectName;
			isModified      = false;
			isNewProject    = true;
		}
		
		#endregion // Public Instance Methods

		#region Private Instance Methods

		private void LoadAssemblies(XmlTextReader xml)
		{
			while (xml.Read() && !(xml.NodeType == XmlNodeType.EndElement && xml.Name == "assemblies"))
			{
				if (xml.NodeType == XmlNodeType.Element && xml.Name == "assembly")
				{
					assemblyFiles.Add(xml["location"]);
				}
			}
		}

		private void LoadXmlDirectories(XmlTextReader xml)
		{
			while (xml.Read() && !(xml.NodeType == XmlNodeType.EndElement && xml.Name == "xmlDocs"))
			{
				if (xml.NodeType == XmlNodeType.Element && xml.Name == "directory")
				{
					xmlDirectories.Add(xml["location"]);
				}
			}
		}

		// TODO: this is dropping the first property, and I'm too tired to know why.
		private void LoadProperties(XmlTextReader xml)
		{
			while (xml.Read() && !(xml.NodeType == XmlNodeType.EndElement && xml.Name == "properties"))
			{
				if (xml.NodeType == XmlNodeType.Element && xml.Name == "property")
				{
					Console.WriteLine("project property load: {0} = '{1}'",
						xml["name"], xml["value"]);

					properties[xml["name"]] = xml["value"];
				}
			}
		}

		private void SaveAssemblies(XmlTextWriter xml)
		{
			// <assemblies>
			xml.WriteStartElement("assemblies");

			foreach (string assemblyFile in assemblyFiles)
			{
				// <assembly location="...">
				xml.WriteStartElement("assembly");
				xml.WriteAttributeString("location", assemblyFile);

				// </assembly>
				xml.WriteEndElement();
			}

			// </assemblies>
			xml.WriteEndElement();
		}

		private void SaveXmlDirectories(XmlTextWriter xml)
		{
			// <xmlDocs>
			xml.WriteStartElement("xmlDocs");

			foreach (string xmlDir in xmlDirectories)
			{
				// <directory location="...">
				xml.WriteStartElement("directory");
				xml.WriteAttributeString("location", xmlDir);

				// </directory>
				xml.WriteEndElement();
			}

			// </xmlDocs>
			xml.WriteEndElement();
		}

		private void SaveProperties(XmlTextWriter xml)
		{
			// <properties>
			xml.WriteStartElement("properties");

			foreach (string name in properties.Keys)
			{
				// <property name="..." value="...">
				xml.WriteStartElement("property");
				xml.WriteAttributeString("name", name);
				xml.WriteAttributeString("value", properties[name].ToString());

				// </property>
				xml.WriteEndElement();
			}

			// </properties>
			xml.WriteEndElement();
		}

		#endregion // Private Instance Methods

		#region Public Instance Properties
		
		public string FilePath
		{
			get { return projectFileName;  }
			set { projectFileName = value; }
		}

		public bool IsModified
		{
			get { return isModified;  }
			set
			{
				if (!isModified && value && Modified != null)
				{
					Modified(this, new EventArgs());
				}
				else
				{
					isModified = value;
				}
			}
		}

		public bool IsNewProject
		{
			get { return isNewProject;  }
			set { isNewProject = value; }
		}

		public ArrayList AssemblyFiles
		{
			get { return assemblyFiles; }
		}

		public ArrayList XmlDirectories
		{
			get { return xmlDirectories; }
		}

		public Hashtable Properties
		{
			get { return properties; }
		}
		
		#endregion // Public Instance Properties
	}
}
