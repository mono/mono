// docstub.cs
//
// Adam Treat (manyoso@yahoo.com)
//
// (C) 2002 Adam Treat
//
// Licensed under the terms of the GNU GPL

using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml;

namespace Mono.Util
{

	class DocStub
	{
		Type currentType;
		MemberInfo[] members;
		XmlTextWriter writer;
		//XmlDocument xmldoc;
		string output_file, assembly_file, directory;

		void Usage()
		{
			Console.Write (
				"docstub (-d <directory> || -o <file>) -a <assembly>\n\n" +
				"   -d || --dir <directory>			The directory to write the xml files to.\n" +
				"   -o || --output <file>			Specifies that docstub should write to one large output file.\n" +
				"   -a || --assembly <assembly>		Specifies the target assembly to load and parse.\n\n");
		}

		public static void Main(string[] args)
		{
			DocStub stub = new DocStub(args);
		}

		public DocStub(string [] args)
		{
			output_file = null;
			assembly_file = null;
			directory = null;
			int argc = args.Length;

			for(int i = 0; i < argc; i++)
			{
				string arg = args[i];

				// The "/" switch is there for wine users, like me ;-)
				if(arg.StartsWith("-") || arg.StartsWith("/"))
				{
					switch(arg)
					{

					case "-d": case "/-d": case "--directory":
						if((i + 1) >= argc)
						{
							Usage();
							return;
						}
						directory = args[++i];
						continue;

					case "-o": case "/-o": case "--output":
						if((i + 1) >= argc)
						{
							Usage();
							return;
						}
						output_file = args[++i];
						continue;

					case "-a": case "/-a": case "--assembly":
						if((i + 1) >= argc)
						{
							Usage();
							return;
						}
						assembly_file = args[++i];
						continue;

					default:
						Usage();
						return;
					}
				}
			}

			if(assembly_file == null)
			{
				Usage();
				return;
			} else if(directory == null && output_file == null)
			{
				Usage();
				return;
			}

			if(directory != null)
			{
				// This specifies that writing to a directory is the default behavior
				// If someone attempts to write to both a directory and an output file
				// Only the directory will be written to...
				output_file = null;
			}

			if (!Directory.Exists(directory) && directory != null)
			{
                Directory.CreateDirectory(directory);
            }

			// Call the main driver to get some things done
			MainDriver();
		}

		//
		// This method loads the assembly and generates a types list
		// It also takes care of the end product, ie writing the xml
		// to the given filename, or filenames...
		//
		void MainDriver()
		{
			Type[] assemblyTypes;
			Assembly assembly = null;
			ArrayList TypesList = new ArrayList();

			assembly = LoadAssembly(Path.GetFullPath(assembly_file));

			// GetTypes() doesn't seem to like loading some dll's, but then
			// the exception holds all the types in the dll anyway, some
			// are in Types and some are in the LoaderExceptions array.
			try
			{
				assemblyTypes = assembly.GetTypes();
			}
			catch(ReflectionTypeLoadException e)
			{
				assemblyTypes = e.Types;
				foreach(TypeLoadException loadException in e.LoaderExceptions)
				{
					TypesList.Add(loadException.TypeName);
				}
			}

			// Create an xml document to check the output [debugging purposes]
   			//xmldoc = new XmlDocument();
			//xmldoc.PreserveWhitespace = true;


			// GenerateXML will take care of converting all the Types info
			// into XML and return a string for writing out to file, files

			if(output_file != null)
			{
				writer = new XmlTextWriter (output_file, null);
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument();
				writer.WriteDocType("MonoDocStub", null, null, null);
				writer.WriteStartElement("assembly");

				foreach(Type name in assemblyTypes)
				{
					if (name != null)
					{
						GenerateXML(name);
					}
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
				writer.Flush();
				writer.Close();
    	 		// Load the file and check it is wellformed [debugging purposes]
     			//xmldoc.Load(output_file);

			}
			else if (directory != null)
			{
				foreach(Type name in assemblyTypes)
				{
					if (name != null)
					{
						string filename = directory+"/"+name+".xml";
    					writer = new XmlTextWriter (filename, null);
						writer.Formatting = Formatting.Indented;
						writer.WriteStartDocument();
						writer.WriteDocType("MonoDocStub", null, null, null);
						writer.WriteStartElement("assembly");
						GenerateXML(name);
						writer.WriteEndElement();
						writer.WriteEndDocument();
						writer.Flush();
						writer.Close();
						// Load the file and check it is wellformed [debugging purposes]
     					//xmldoc.Load(filename);
					}
				}
			}
			return;
		}

		// This method loads an assembly into memory. If you
		// use Assembly.Load or Assembly.LoadFrom the assembly file locks.
		// This method doesn't lock the assembly file.
		Assembly LoadAssembly(string filename)
		{
			try
			{
				FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read);
				byte[] buffer = new byte[fs.Length];
				fs.Read(buffer, 0, (int)fs.Length);
				fs.Close();

				return Assembly.Load(buffer);
			}
			catch(FileNotFoundException)
			{
				Console.WriteLine("Could not find assembly file: {0}", assembly_file);
				return null;
			}
		}

		//
		// The main xml generation method!
		//
		void GenerateXML(Type name)
		{
			try
			{
				currentType = name;
				//This is what the try block is for
				members = currentType.GetMembers();
				// This is where we generate xml for the class/type
				writer.WriteStartElement("class");
				writer.WriteStartElement("name");
				writer.WriteString(currentType.FullName);
				writer.WriteEndElement();
				getTypeInfoXml();

				// Generate xml for members (constructors, fields, methods etc)
				if(members.Length > 0)
				{
					foreach(MemberInfo m in members)
					{
						if (m is ConstructorInfo)
						{
							writer.WriteStartElement("member");
							writer.WriteStartElement("name");
    						writer.WriteString(currentType.Name);
							writer.WriteEndElement();
							writer.WriteStartElement("type");
   							writer.WriteString("constructor");
							writer.WriteEndElement();
							getConstructorXml((ConstructorInfo)m);
							writer.WriteEndElement();
						}
						else if (m is EventInfo)
						{
							writer.WriteStartElement("member");
							writer.WriteStartElement("name");
    						writer.WriteString(m.Name);
							writer.WriteEndElement();
							writer.WriteStartElement("type");
    						writer.WriteString("event");
							writer.WriteEndElement();
							getEventXml((EventInfo)m);
							writer.WriteEndElement();
						}
						else if (m is FieldInfo)
						{
							writer.WriteStartElement("member");
							writer.WriteStartElement("name");
    						writer.WriteString(m.Name);
							writer.WriteEndElement();
							writer.WriteStartElement("type");
   							writer.WriteString("field");
							writer.WriteEndElement();
							getFieldXml((FieldInfo)m);
							writer.WriteEndElement();
						}
						else if (m is PropertyInfo)
						{
							writer.WriteStartElement("member");
							writer.WriteStartElement("name");
    						writer.WriteString(m.Name);
							writer.WriteEndElement();
							writer.WriteStartElement("type");
     						writer.WriteString("property");
							writer.WriteEndElement();
							getPropertyXml((PropertyInfo)m);
							writer.WriteEndElement();
						}
						else if (m is MethodInfo)
						{
							writer.WriteStartElement("member");
							writer.WriteStartElement("name");
    						writer.WriteString(m.Name);
							writer.WriteEndElement();
							writer.WriteStartElement("type");
    						writer.WriteString("method");
							writer.WriteEndElement();
							getMethodXml((MethodInfo)m);
							writer.WriteEndElement();
						}
						else{}
					}
				}
				// Don't forget to close the xml ;-)
				writer.WriteEndElement();
			}
			catch(TypeLoadException e)
			{
				// Todo Mono's corlib keeps failing here because System.Object
				// Doesn't have any parents
				Console.WriteLine("Class: "+e.TypeName+" has caused a TypeLoad Exception."
								 +" No XML will be generated for this type.");
			}
		}

		//
		// This just calls the methods for elements within a constructor
		//
		void getConstructorXml(ConstructorInfo construct)
		{
			getMethodBaseInfoXml(construct);
			getParameterInfoXml(construct);
			getInheritInfoXml(construct);
		}

		//
		// Calls the methods for elements within an event member
		//
		void getEventXml(EventInfo eve)
		{
			getEventInfoXml(eve);
			getInheritInfoXml(eve);
		}

		//
		// Calls the methods for xml elements within a field
		//
		void getFieldXml(FieldInfo field)
		{
			writer.WriteStartElement("field_type");
  			writer.WriteString(field.FieldType.Name);
			writer.WriteEndElement();
			getFieldInfoXml(field);
			getInheritInfoXml(field);
		}

		//
		// Calls the methods for xml elements within a property
		//
		void getPropertyXml(PropertyInfo property)
		{
			writer.WriteStartElement("property_type");
  			writer.WriteString(property.PropertyType.Name);
			writer.WriteEndElement();
			getPropertyInfoXml(property);
			getInheritInfoXml(property);
		}

		//
		// Calls the methods for xml elements within a method
		//
		void getMethodXml(MethodInfo method)
		{
			getMethodBaseInfoXml(method);
			getParameterInfoXml(method);
			getReturnInfoXml(method);
			getInheritInfoXml(method);
		}

		//
		// Probably should just go in the getMethodXml, but here for asthetic reasons ;-)
		//
		void getReturnInfoXml(MethodInfo method)
		{
			try
			{
				writer.WriteStartElement("return");
  				writer.WriteString(method.ReturnType.Name);
				writer.WriteEndElement();
			}
			catch(Exception)
			{
				// Mysteriously this is also causing some corlib types
				// to spit out some Object doesn't have a parent errors
				//Console.WriteLine(e.Message);
				writer.WriteEndElement();
			}
		}

		//
		// Checks to see if a member is inherited and output xml
		//
		void getInheritInfoXml(MemberInfo member)
		{
			if(member.DeclaringType != currentType)
			{
				writer.WriteStartElement("inherit");
  				writer.WriteString(member.DeclaringType.Name);
				writer.WriteEndElement();
				writer.WriteStartElement("inheritfull");
  				writer.WriteString(member.DeclaringType.FullName);
				writer.WriteEndElement();
			}
		}

		//
		// Checks for get or set in properties
		//
		void getPropertyInfoXml(PropertyInfo property)
		{
			if(property.CanRead)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("get");
				writer.WriteEndElement();
			}
			if(property.CanWrite)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("set");
				writer.WriteEndElement();
			}
		}

		//
		// Self explanatory
		//
		void getParameterInfoXml(MethodBase method)
		{
			try
			{
				ParameterInfo[] parameters = method.GetParameters();
				if(parameters.Length != 0)
				{
					foreach(ParameterInfo p in parameters)
					{
						writer.WriteStartElement("param");
						writer.WriteStartElement("name");
  						writer.WriteString(p.Name);
						writer.WriteEndElement();
						writer.WriteStartElement("type");
  						writer.WriteString(p.ParameterType.Name);
						writer.WriteEndElement();
						if(p.DefaultValue != DBNull.Value)
						{
							writer.WriteStartElement("default");
  							writer.WriteString(p.DefaultValue.ToString());
							writer.WriteEndElement();
						}
						writer.WriteStartElement("position");
  						writer.WriteString(p.Position.ToString());
						writer.WriteEndElement();
						writer.WriteEndElement();
					}
				}
			}
			catch(Exception)
			{
				// Mysteriously this is also causing some corlib types
				// to spit out some Object doesn't have a parent errors
				//Console.WriteLine(e.Message);
			}
		}

		//
		// Find Attributes for events
		//
		void getEventInfoXml(EventInfo _obj)
		{
			if(_obj.IsMulticast)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("multicast");
				writer.WriteEndElement();
			}
			writer.WriteStartElement("eventhandler");
  			writer.WriteString(_obj.EventHandlerType.Name);
			writer.WriteEndElement();
		}

		//
		// Find Attributes for fields
		//
		void getFieldInfoXml(FieldInfo _obj)
		{
			if(_obj.IsPrivate)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("private");
				writer.WriteEndElement();
			}

			if(_obj.IsPublic)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("public");
				writer.WriteEndElement();
			}

			if(_obj.IsStatic)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("static");
				writer.WriteEndElement();
			}
		}

		//
		// Find Attributes for constructors and methods
		//
		void getMethodBaseInfoXml(MethodBase _obj)
		{

			if(_obj.IsAbstract)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("abstract");
				writer.WriteEndElement();
			}

			if(_obj.IsFinal)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("final");
				writer.WriteEndElement();
			}

			if(_obj.IsPrivate)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("private");
				writer.WriteEndElement();
			}

			if(_obj.IsPublic)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("public");
				writer.WriteEndElement();
			}

			if(_obj.IsStatic)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("static");
				writer.WriteEndElement();
			}

			if(_obj.IsVirtual)
			{
				writer.WriteStartElement("attribute");
  				writer.WriteString("virtual");
				writer.WriteEndElement();
			}
		}

		//
		// Put this at the end because it's ugly, but gets the job done
		//
		void getTypeInfoXml()
		{
			try
			{
				writer.WriteStartElement("inherit");
				writer.WriteString(currentType.BaseType.Name);
				writer.WriteEndElement();
				writer.WriteStartElement("inheritfull");
				writer.WriteString(currentType.BaseType.FullName);
				writer.WriteEndElement();

				if(currentType.IsAbstract)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("abstract");
					writer.WriteEndElement();
				}

				if(currentType.IsEnum)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("enum");
					writer.WriteEndElement();
				}

				if(currentType.IsInterface)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("interface");
					writer.WriteEndElement();
				}

				if(currentType.IsPrimitive)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("primitive");
					writer.WriteEndElement();
				}

				if(currentType.IsPublic)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("public");
					writer.WriteEndElement();
				}

				if(currentType.IsSealed)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("sealed");
					writer.WriteEndElement();
				}

				if(currentType.IsSerializable)
				{
					writer.WriteStartElement("attribute");
  					writer.WriteString("serializable");
					writer.WriteEndElement();
				}

			}
			catch(Exception) {
				Console.WriteLine("A Class has caused an Exception."
								 +" This occurred whilst trying to retrieve said types inheritable information.");
				writer.WriteEndElement();
			}
		}
	}
}
