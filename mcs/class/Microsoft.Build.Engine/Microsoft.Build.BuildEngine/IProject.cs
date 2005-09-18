//
// IProject.cs:
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
// 
// (C) 2005 Marek Sieradzki
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

using System;
using System.Collections;
using System.IO;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	public interface IProject {
		void AddNewImport (string importLocation,
				   string importCondition);

		BuildItem AddNewItem (string itemName, string itemInclude);

		BuildItemGroup AddNewItemGroup ();

		BuildPropertyGroup AddNewPropertyGroup (bool insertAtEndOfProject);

		bool Build (string[] targetNamesToBuild,
			    IDictionary targetOutputs);

		bool BuildTarget (string targetName,
				  IDictionary targetOutputs);

		bool BuildTargetWithFlags (string targetName,
					   IDictionary targetOutputs,
					   BuildSettings buildFlags);

		string[] GetConditionedPropertyValues (string propertyName);

		string[] GetDirectlyImportedProjects ();

		BuildItemGroup GetEvaluatedItemsByName (string itemName);

		BuildItemGroup GetEvaluatedItemsByNameIgnoringCondition (string itemName);

		string GetEvaluatedProperty (string propertyName);

		string[] GetNonImportedItemNames ();

		string[] GetNonImportedPropertyNames ();

		string[] GetNonImportedTargetNames ();

		string[] GetNonImportedUsingTasks ();

		string GetProjectExtensions (string id);

		void LoadFromFile (string projectFileName);

		void LoadFromXml (XmlDocument projectXml);

		void MarkProjectAsDirty ();

		void RemoveAllItemGroups ();

		void RemoveAllItemsGroupsByCondition (string condition);

		void RemoveAllPropertyGroups ();

		void RemoveAllPropertyGroupsByCondition (string condition);

		void RemoveItem (BuildItem itemToRemove);

		void RemoveItemGroup (BuildItemGroup itemGroupToRemove);

		void RemoveItemsByName (string itemName);

		void RemovePropertyGroup (BuildPropertyGroup propertyGroupToRemove);

		void ResetBuildStatus ();

		void SaveToFile (string projectFileName);

		void SaveToFile (string projectFileName,
				ProjectFileEncoding encoding);

		void SaveToTextWriter (TextWriter outTextWriter);

		void SetImportedProperty (string propertyName,
					  string propertyValue,
					  string condition,
					  Project importedProject);

		void SetImportedPropertyAt (string propertyName,
					    string propertyValue,
					    string condition,
					    Project importedProject,
					    PropertyPosition position);

		void SetProjectExtensions (string id, string xmlText);

		void SetProperty (string propertyName, string propertyValue,
				  string condition);

		void SetPropertyAt (string propertyName, string propertyValue,
				    string condition,
				    PropertyPosition postition);

		void Unload ();

		bool BuildEnabled {
			get;
			set;
		}

		ProjectFileEncoding CurrentProjectFileEncoding {
			get;
		}

		string DefaultTargets {
			get;
			set;
		}

		BuildItemGroup EvaluatedItems {
			get;
		}

		BuildItemGroup EvaluatedItemsIgnoringCondition {
			get;
		}

		BuildPropertyGroup EvaluatedProperties {
			get;
		}

		string FullFileName {
			get;
		}

		BuildPropertyGroup GlobalProperties {
			get;
			set;
		}

		bool IsDirty {
			get;
		}

		BuildItemGroupCollection ItemGroups {
			get;
		}

		Engine ParentEngine {
			get;
		}

		BuildPropertyGroupCollection PropertyGroups {
			get;
		}

		DateTime TimeOfLastDirty {
			get;
		}
	}
}
