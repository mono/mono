// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Ian MacLean (ian_maclean@another.com)


namespace SourceForge.NAnt {
	
    using System;
    using System.Xml;
    using System.Collections;
    using System.Collections.Specialized;
    
    /// <summary>
	/// Summary description for IncludeTask.
	/// </summary>
	
    [TaskName("include")] // TODO make into ant:include
    public class IncludeTask : Task {		
        
        /// <summary>hours to to add to the sleep time</summary>
        [TaskAttribute("href", Required=true)]
        string _href = null;
                
        // Attribute properties
        public string Href                  { get { return _href; } }

        XPathTextPositionMap _positionMap; // created when Xml document is loaded        
        TaskCollection _tasks = new TaskCollection();
          
        // static members
        static System.Collections.Stack _includesStack = new Stack();        
        static bool IsIncluded( string href ) {
            bool result = false;
            IEnumerator stackenum = _includesStack.GetEnumerator();
            while ( stackenum.MoveNext()) {
                if ( href == (string)stackenum.Current ) {
                    result = true; break;
                }
            }
            return result;
        }
                              
        protected void InitializeIncludedDocument(XmlDocument doc) {
            
            // Load line Xpath to linenumber array
            _positionMap = new XPathTextPositionMap(doc.BaseURI);                        
            
            // process all the non-target nodes (these are global tasks for the project)
            XmlNodeList taskList = doc.SelectNodes("project/*[name() != 'target']");
            foreach (XmlNode taskNode in taskList) {

                // TODO: do somethiing like Project.CreateTask(taskNode) and have the project set the location
                TextPosition textPosition = _positionMap.GetTextPosition(taskNode);

                Task task = Project.CreateTask(taskNode);
                if (task != null) {
                    // Store a local copy also so we can execute only those
                    _tasks.Add(task);                
                }
            }    
            
            // execute global tasks now - before anything else
            // this lets us include tasks that do things like add more tasks
            // Here is where we should check for recursive dependencies
            //
            foreach (Task task in _tasks ) {
                task.Execute();              
            }

            // process all the targets
            XmlNodeList targetList = doc.SelectNodes("project/target");
            foreach (XmlNode targetNode in targetList) {
                Target target = new Target(Project);
                target.Initialize(targetNode);
                Project.Targets.Add(target);
            }
        }
                                
        /// <summary>
        ///  verify parameters
        ///</summary>
        ///<param name="taskNode"> taskNode used to define this task instance </param>
        protected override void InitializeTask(XmlNode taskNode) {
          
            //TODO check where we are in document - if not at top level then bail out on error ...          
            // basic recursion check
            if (IsIncluded( Project.GetFullPath(Href) )) {
                throw new BuildException("Recursive includes are not allowed", Location);            
            }           
        }
        
        protected override void ExecuteTask() {
            
            string fullpath = Project.GetFullPath(Href);
            // push ourselves onto the stack
            _includesStack.Push(fullpath);
            try {
      
                XmlDocument doc = new XmlDocument();
               
                // Handle local file case              
                doc.Load(fullpath);
                               
                InitializeIncludedDocument(doc);
            }
            // Handling the case where a nested include causes an exception during initialization
            catch ( BuildException ) {
                throw;
            }   
            catch ( Exception e) {
                throw new BuildException(e.Message, Location, e);
            }
            finally {
              // Pop off the stack
              _includesStack.Pop();  
            }
        }    
    }
}
