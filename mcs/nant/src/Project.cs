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
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Collections;
    using System.Collections.Specialized;

    /// <summary>
    /// Central representation of an NAnt project.
    /// </summary>
    public class Project {

        public static readonly string BuildFilePattern = "*.build";

        /// <summary>
        /// Finds the file name for the build file in the specified directory.
        /// </summary>
        /// <param name="directory">The directory to look for a build file.  When in doubt use Environment.CurrentDirectory for directory.</param>
        /// <returns>The path to the build file or <c>null</c> if no build file could be found.</returns>
        public static string FindBuildFileName(string directory) {
            string buildFileName = null;

            // find first file ending in .build
            DirectoryInfo directoryInfo = new DirectoryInfo(directory);
            FileInfo[] files = directoryInfo.GetFiles(BuildFilePattern);
            if (files.Length > 0) {
                buildFileName = Path.Combine(directory, files[0].Name);
            }
            return buildFileName;
        }

        string _name;
        string _defaultTargetName;
        string _baseDirectory;
        string _buildFileName;
        bool _verbose = false;

        StringCollection _buildTargets = new StringCollection();
        TaskCollection _tasks = new TaskCollection();
        TargetCollection _targets = new TargetCollection();
        XPathTextPositionMap _positionMap; // created when Xml document is loaded
        TaskFactory _taskFactory; // created in constructor
        PropertyDictionary _properties = new PropertyDictionary();

        public Project() {
            _taskFactory = new TaskFactory(this);
        }

        /// <summary>
        /// The name of the project.
        /// </summary>
        public string Name {
            get { return _name; }
            set { _name = value; }
        }

        public string BaseDirectory {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        public string BuildFileName {
            get { return _buildFileName; }
            set { _buildFileName = value; }
        }

        /// <summary>
        /// When true tasks should output more output.
        /// </summary>
        public bool Verbose {
            get { return _verbose; }
            set { _verbose = value; }
        }

        /// <summary>
        /// The list of targets to built.
        /// </summary>
        /// <remarks>
        /// Targets are built in the order they appear in the collection.  If
        /// the collection is empty the default target will be built.
        /// </remarks>
        public StringCollection BuildTargets {
            get { return _buildTargets; }
        }

        /// <summary>
        /// The list of tasks to perform before any targets executed.
        /// </summary>
        /// <remarks>
        /// Tasks are executed in the order they appear in the collection.
        /// </remarks>
        public TaskCollection Tasks {
            get { return _tasks; }
        }

        public PropertyDictionary Properties {
            get { return _properties; }
        }

        public TargetCollection Targets {
            get { return _targets; }
        }

        public bool Run() {
            bool buildResult = false;
            try {
                DateTime startTime = DateTime.Now;

                if (BaseDirectory == null) {
                    BaseDirectory = Environment.CurrentDirectory;
                }
                BaseDirectory = Path.GetFullPath(BaseDirectory);

                if (BuildFileName == null || BuildFileName == String.Empty) {
                    BuildFileName = FindBuildFileName(BaseDirectory);
                    if (BuildFileName == null) {
                        throw new BuildException(String.Format("Could not find a '{0}' file in '{1}'", BuildFilePattern, BaseDirectory));
                    }
                }

                Log.WriteLine("Buildfile: {0}", BuildFileName);
                if (Verbose) {
                    Log.WriteLine("Base Directory: {0}", BaseDirectory);
                }

                XmlDocument doc = new XmlDocument();
                try {
                    doc.Load(BuildFileName);
                    // TODO: validate against xsd schema
                } catch (XmlException e) {
                    throw new BuildException(String.Format("Could not load '{0}'", BuildFileName), e);
                }

                Initialize(doc);
                Properties.Add("nant.buildfile", BuildFileName);

                Execute();

                Log.WriteLine();
                Log.WriteLine("BUILD SUCCEEDED");

                TimeSpan buildTime = DateTime.Now - startTime;
                Log.WriteLine();
                Log.WriteLine("Total time: {0} seconds", (int) buildTime.TotalSeconds);

                buildResult = true;
            } catch (BuildException e) {
                Log.WriteLine();
                Log.WriteLine("BUILD FAILED");
                Log.WriteLine(e.Message);
                if (e.InnerException != null) {
                    Log.WriteLine(e.InnerException.Message);
                }
            } catch (Exception e) {
                // all other exceptions should have been caught
                Log.WriteLine();
                Log.WriteLine("INTERNAL ERROR");
                Log.WriteLine(e.ToString());
            }
            return buildResult;
        }

        public int AddTasks(string assemblyPath) {

            Assembly assembly;
            if (assemblyPath == null) {
                assembly = Assembly.GetExecutingAssembly();
            } else {
                assembly = Assembly.LoadFrom(assemblyPath);
            }

            int taskCount = 0;
            foreach(Type type in assembly.GetTypes()) {
                if (type.IsSubclassOf(typeof(Task)) && !type.IsAbstract) {
                    if (_taskFactory.Builders.Add(new TaskBuilder(type.FullName, assemblyPath))) {
                        taskCount++;
                    }
                }
            }
            return taskCount;
        }

        public void Initialize(XmlDocument doc) {

            Name = doc.SelectSingleNode("project/@name").Value;

            // make it possible for user to override this value
            if (BaseDirectory == null) {
                BaseDirectory = doc.SelectSingleNode("project/@basedir").Value;
            }

            // used only if BuildTargets collection is empty
            _defaultTargetName = doc.SelectSingleNode("project/@default").Value;

            // initialize builtin tasks
            AddTasks(null);

            // init static built in properties
            Properties.Add("nant.project.name", Name);
            Properties.Add("nant.base.dir",     BaseDirectory);
            Properties.Add("nant.default.name", _defaultTargetName);

            // add all environment variables
            IDictionary variables = Environment.GetEnvironmentVariables();
            foreach (string name in variables.Keys) {
                string value = (string) variables[name];
                Properties.Add("nant.env." + name, value);
            }

            // Load line Xpath to linenumber array
            _positionMap = new XPathTextPositionMap(doc.BaseURI);

            // process all the non-target nodes (these are global tasks for the project)
            XmlNodeList taskList = doc.SelectNodes("project/*[name() != 'target']");
            foreach (XmlNode taskNode in taskList) {

                // TODO: do somethiing like Project.CreateTask(taskNode) and have the project set the location
                TextPosition textPosition = _positionMap.GetTextPosition(taskNode);

                Task task = CreateTask(taskNode);
                if (task != null) {
                    Tasks.Add(task);
                }
            }

            // execute global tasks now - before anything else
            // this lets us include tasks that do things like add more tasks
            foreach (Task task in Tasks) {
                task.Execute();
            }

            // process all the targets
            XmlNodeList targetList = doc.SelectNodes("project/target");
            foreach (XmlNode targetNode in targetList) {
                Target target = new Target(this);
                target.Initialize(targetNode);
                Targets.Add(target);
            }
        }

        public void Execute() {
            if (BuildTargets.Count == 0) {
                BuildTargets.Add(_defaultTargetName);
            }

            foreach(string targetName in BuildTargets) {
                Execute(targetName);
            }
        }

        public void Execute(string targetName) {
            Target target = Targets.Find(targetName);
            if (target == null) {
                throw new BuildException(String.Format("unknown target '{0}'", targetName));
            }
            target.Execute();
        }

        public Task CreateTask(XmlNode taskNode) {
            return CreateTask(taskNode, null);
        }

        public Task CreateTask(XmlNode taskNode, Target target) {
            Task task = _taskFactory.CreateTask(taskNode, target);
            if (task != null) {
                // save task location in case of error
                TextPosition pos = _positionMap.GetTextPosition(taskNode);

                // initialize the task
                task.Initialize(taskNode, new Location(taskNode.BaseURI, pos.Line, pos.Column));
            }
            return task;
        }

        public string ExpandText(string input) {
            string output = input;
            if (input != null) {
                const string pattern = @"\$\{([^\}]*)\}";
                foreach (Match m in Regex.Matches(input, pattern)) {
                    if (m.Length > 0) {

                        string token         = m.ToString();
                        string propertyName  = m.Groups[1].Captures[0].Value;
                        string propertyValue = Properties[propertyName];

                        if (propertyValue != null) {
                            output = output.Replace(token, propertyValue);
                        }
                    }
                }
            }
            return output;
        }

        public string GetFullPath(string path) {
            string baseDir = ExpandText(BaseDirectory);

            if (path != null) {
                if (!Path.IsPathRooted(path)) {
                    path = Path.Combine(baseDir, path);
                }
            } else {
                path = baseDir;
            }
            return Path.GetFullPath(path);
        }
    }
}
