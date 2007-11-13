//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Mono.MonoConfig
{
	class HandlerDescription
	{
		Type handlerType;
		Type handlerStorageType;
		string section;

		object handler;
		object storage;
		
		public object Handler {
			get {
				if (handler != null)
					return handler;
				handler = Activator.CreateInstance (handlerType);
				return handler;
			}
		}

		public object Storage {
			get {
				if (storage != null)
					return storage;
				
				storage = Activator.CreateInstance (handlerStorageType);
				return storage;
			}
		}

		public string Section {
			get { return section; }
		}

		public bool Implements (string interfaceName)
		{
			return handlerType.GetInterface (interfaceName) != null;
		}
		
		public HandlerDescription (string handlerTypeName, string handlerStorageTypeName, string section)
		{
			handlerType = Type.GetType (handlerTypeName, true);
			if (handlerType.GetInterface ("Mono.MonoConfig.IDocumentNodeHandler") == null)
				throw new ApplicationException (
					String.Format ("Handler for section '{0}' must implement the '{1}' interface",
						       section, typeof (Mono.MonoConfig.IDocumentNodeHandler)));

			handlerStorageType = Type.GetType (handlerStorageTypeName, true);
			this.section = section;
		}
	}
	
	public class Configuration
	{
		string[] configs;
		Dictionary <string, HandlerDescription> section_handlers;
		List <HandlerDescription> section_handlers_ordered;
		List <XPathDocument> config_documents;
		bool loaded;

		public Configuration () : this (null)
		{}
		
		public Configuration (string[] configs)
		{
			this.configs = configs;
			section_handlers = new Dictionary <string, HandlerDescription> ();
			section_handlers_ordered = new List <HandlerDescription> ();
			config_documents = new List <XPathDocument> (configs != null ? configs.Length : 1);
		}

		public void WriteDefaultConfigFile (string name, string path, FeatureTarget target)
		{
			AssertLoaded ();
			
			if (String.IsNullOrEmpty (name))
				throw new ArgumentException ("name", "Must not be null or empty");

			IDefaultConfigFileContainer[] containers = GetHandlersForInterface <IDefaultConfigFileContainer> ();
			if (containers == null || containers.Length == 0)
				throw new ApplicationException ("Cannot find any handler for writing default config files");
			
			IDefaultContainer[] defaults = GetHandlersForInterface <IDefaultContainer> ();

			bool written = false;
			foreach (IDefaultConfigFileContainer container in containers) {
				if (container.HasDefaultConfigFile (name, target)) {
					container.WriteDefaultConfigFile (name, target, path, defaults);
					written = true;
					break;
				}
			}

			if (!written)
				throw new ApplicationException (
					String.Format ("Definition of default config file '{0}' for target '{1}' not found.",
						       name, target));
		}

		public string[] DefaultConfigFiles {
			get {
				AssertLoaded ();
				
				IDefaultConfigFileContainer[] containers = GetHandlersForInterface <IDefaultConfigFileContainer> ();
				if (containers == null || containers.Length == 0)
					return null;

				List <string> defaults = new List <string> ();
				foreach (IDefaultConfigFileContainer container in containers)
					defaults.AddRange (container.DefaultConfigFiles);
				
				defaults.Sort ();
				return defaults.ToArray ();
			}
		}
		
		public void AddFeature (string configFilePath, FeatureTarget target, string featureName)
		{
			AssertLoaded ();
			
			if (String.IsNullOrEmpty (configFilePath))
				throw new ArgumentException ("configFilePath", "Must not be null or empty");
			if (String.IsNullOrEmpty (featureName))
				throw new ArgumentException ("featureName", "Must not be null or empty");

			IFeatureGenerator[] generators = GetHandlersForInterface <IFeatureGenerator> ();
			if (generators == null || generators.Length == 0)
				throw new ApplicationException ("Cannot find any feature generator");

			IDefaultContainer[] defaults = GetHandlersForInterface <IDefaultContainer> ();
			IConfigBlockContainer[] configBlocks = GetHandlersForInterface <IConfigBlockContainer> ();

			bool added = false;
			foreach (IFeatureGenerator generator in generators) {
				if (generator.HasFeature (featureName)) {
					generator.AddFeature (configFilePath, featureName, target, defaults, configBlocks);
					added = true;
					break;
				}
			}

			if (!added)
				throw new ApplicationException (
					String.Format ("Definition of feature '{0}' for target '{1}' not found.",
						       featureName, target));
		}

		public string[] Features {
			get {
				AssertLoaded ();
				
				IFeatureGenerator[] generators = GetHandlersForInterface <IFeatureGenerator> ();
				if (generators == null || generators.Length == 0)
					return null;
				
				List <string> features = new List <string> ();
				foreach (IFeatureGenerator generator in generators)
					features.AddRange (generator.Features);

				features.Sort ();
				return features.ToArray ();
			}
		}
		
		public void Load (string[] configs)
		{
			this.configs = configs;
			Load ();
		}
		
		public void Load ()
		{
			if (configs == null || configs.Length == 0)
				return;

			if (loaded) {
				section_handlers.Clear ();
				section_handlers_ordered.Clear ();
				config_documents.Clear ();
				loaded = false;
			}
			
			XPathDocument doc;
			foreach (string config in configs) {
				if (String.IsNullOrEmpty (config))
					continue;
				
				try {
					doc = new XPathDocument (config);
					config_documents.Add (doc);
				} catch (XmlException ex) {
					throw new ApplicationException (
						String.Format ("Failed to parse config file '{0}'.", config),
						ex);
				} catch (Exception) {
					continue;
				}
			}

			XPathNavigator nav;
			XPathNodeIterator iter;
			
			// First configure section handlers
			List <HandlerDescription> handlers_from_file = new List <HandlerDescription> ();
			
			foreach (XPathDocument xpdoc in config_documents) {
				handlers_from_file.Clear ();
				
				nav = xpdoc.CreateNavigator ();
				iter = nav.Select ("//mconfig/configuration/handlers/handler[string-length (@section) > 0]");

				while (iter.MoveNext ())
					AddSectionHandler (iter.Current, handlers_from_file);
				section_handlers_ordered.InsertRange (0, handlers_from_file);
			}

			// Process all configs looking for all sections with known handlers
			foreach (XPathDocument xpdoc in config_documents) {
				nav = xpdoc.CreateNavigator ();
				iter = nav.Select ("//mconfig/*");

				while (iter.MoveNext ())
					HandleTopLevelNode (iter.Current);				
			}

			loaded = true;
		}

		public T[] GetHandlersForInterface <T> ()
		{
			AssertLoaded ();
			
			string typeName = typeof (T).ToString ();
			object handler;
			
			List <T> handlers = null;
			foreach (HandlerDescription hd in section_handlers_ordered) {
				if (hd.Implements (typeName)) {
					if (handlers == null)
						handlers = new List <T> (1);
					handler = hd.Handler;
					if (handler is IStorageConsumer)
						((IStorageConsumer) handler).SetStorage (hd.Storage);
					
					handlers.Add ((T)handler);
				}
			}

			if (handlers == null)
				return null;
			
			return handlers.ToArray ();
		}
		
		void HandleTopLevelNode (XPathNavigator nav)
		{
			string section = nav.LocalName;

			if (!section_handlers.ContainsKey (section))
				return;
			
			HandlerDescription hd = section_handlers [section];
			if (hd == null)
				return;

			IDocumentNodeHandler handler = hd.Handler as IDocumentNodeHandler;
			object storage = hd.Storage;

			if (handler == null || storage == null)
				return;

			if (handler is IStorageConsumer)
				((IStorageConsumer) handler).SetStorage (storage);
			
			handler.ReadConfiguration (nav);
			handler.StoreConfiguration ();
		}
		
		void AddSectionHandler (XPathNavigator nav, List <HandlerDescription> handlers)
		{
			string section = Helpers.GetRequiredNonEmptyAttribute (nav, "section");
			HandlerDescription hd = new HandlerDescription (Helpers.GetRequiredNonEmptyAttribute (nav, "type"),
									Helpers.GetRequiredNonEmptyAttribute (nav, "storageType"),
									section);
			
			if (section_handlers.ContainsKey (section)) {
				HandlerDescription old = section_handlers [section];
				section_handlers [section] = hd;

				handlers.Remove (old);
				handlers.Add (hd);
			} else {
				section_handlers.Add (section, hd);
				handlers.Add (hd);
			}
		}

		void AssertLoaded ()
		{
			if (!loaded)
				throw new ApplicationException ("Configuration not loaded yet");
		}
	}
}

