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
// Gerry Shaw (gerry_shaw@yahoo.com)
// Ian MacLean (ian_maclean@another.com)

namespace SourceForge.NAnt {

    using System;
    using System.Collections.Specialized;
    using System.Xml;

    public class Target {

        string _name;
        Project _project;
        bool _hasExecuted = false;
        TaskCollection _tasks = new TaskCollection();
        StringCollection _dependencies = new StringCollection();

        public Target(Project project) {
            Project = project;
        }

        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public Project Project {
            get { return _project; }
            set { _project = value; }
        }

        public bool HasExecuted {
            get { return _hasExecuted; }
        }

        public TaskCollection Tasks {
            get { return _tasks; }
        }

        public StringCollection Dependencies {
            get { return _dependencies; }
        }

        public void Initialize(XmlNode targetNode) {
            // get target name
            XmlNode nameNode = targetNode.SelectSingleNode("@name");
            if (nameNode == null) {
                // TODO: add Location to exception
                throw new BuildException("target must have a name attribute");
            }
            Name = nameNode.Value;

            // add dependicies
            XmlNode dependsNode = targetNode.SelectSingleNode("@depends");
            if (dependsNode != null) {
                string depends = dependsNode.Value;
                foreach (string str in depends.Split(new char[]{','})) {
                    string dependency = str.Trim();
                    if (dependency.Length > 0) {
                        Dependencies.Add(dependency);
                    }
                }
            }

            // select all the non-target nodes (these are global tasks for the project)
            XmlNodeList taskList = targetNode.SelectNodes("*");
            foreach (XmlNode taskNode in taskList) {
                Task task = Project.CreateTask(taskNode, this);
                if (task != null) {
                    Tasks.Add(task);
                }
            }
        }

        public void Execute() {
            if (!HasExecuted) {
                try {
                    foreach (string targetName in Dependencies) {
                        Target target = Project.Targets.Find(targetName);
                        if (target == null) {
                            // TODO: add Location to exception
                            throw new BuildException(String.Format("unknown dependent target '{0}' of target '{1}'", targetName, Name));
                        }
                        target.Execute();
                    }

                    Log.WriteLine();
                    Log.WriteLine("{0}:", Name);
                    foreach (Task task in Tasks) {
                        task.Execute();
                    }
                } finally {
                    _hasExecuted = true;
                }
            }
        }
    }
}
