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
// Mike Krueger (mike@icsharpcode.net)
// Ian MacLean (ian_maclean@another.com)

namespace SourceForge.NAnt {

    using System;
    using System.Reflection;
    using System.Xml;

    public abstract class Task {

        /// <summary>Gets and sets how much spacing log prefix names will be padded.</summary>
        /// <remarks>
        /// Includes characters for a space after the name and the [ ] brackets. Default is 12.
        /// </remarks>
        public static int LogPrefixPadding = Log.IndentSize;

        Location _location = Location.UnknownLocation;
        Target _target = null;
        Project _project = null;

        /// <summary>
        /// Location in build file where task is defined.
        /// </summary>
        protected Location Location {
            get { return _location; }
            set { _location = value; }
        }

        public string Name {
            get {
                string name = null;
                TaskNameAttribute taskName = (TaskNameAttribute) Attribute.GetCustomAttribute(GetType(), typeof(TaskNameAttribute));
                if (taskName != null) {
                    name = taskName.Name;
                }
                return name;
            }
        }

        public string LogPrefix {
            get {
                string prefix = "[" + Name + "] ";
                return prefix.PadLeft(LogPrefixPadding);
            }
        }

        public Target Target {
            get { return _target; }
            set { _target = value; }
        }

        public Project Project {
            get { return _project; }
            set { _project = value; }
        }

        protected void AutoInitializeAttributes(XmlNode taskNode) {

            // TODO: BooleanValidatorAttribute and Int32ValidatorAttribute implementation in Task

            // Go down the inheritance tree to find the private fields in the object.
            // We are looking for task attributes to initialize.
            Type currentType = GetType();
            while (currentType != typeof(object)) {
                FieldInfo[] fieldInfoArray = currentType.GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
                foreach (FieldInfo fieldInfo in fieldInfoArray) {

                    // process TaskAttribute attributes
                    TaskAttributeAttribute taskAttribute = (TaskAttributeAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(TaskAttributeAttribute));
                    if (taskAttribute != null) {

                        // get value from xml file
                        XmlNode node = taskNode.SelectSingleNode("@" + taskAttribute.Name);

                        // check if its required
                        if (node == null && taskAttribute.Required) {
                            // TODO: add Location to exception
                            throw new BuildException(String.Format("{0} is a required attribute.", taskAttribute.Name), Location);
                        }

                        if (node != null) {
                            fieldInfo.SetValue(this, Convert.ChangeType(node.Value, fieldInfo.FieldType));
                        }
                    }

                    // process TaskFileSet attributes
                    TaskFileSetAttribute fileSetAttribute = (TaskFileSetAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(TaskFileSetAttribute));
                    if (fileSetAttribute != null) {
                        // have file set initialize itself
                        FileSet fileSet = (FileSet) fieldInfo.GetValue(this);

                        // set task fileset belongs to
                        fileSet.Task = this;

                        // load values from build file
                        XmlNode fileSetNode = taskNode.SelectSingleNode(fileSetAttribute.Name);
                        if (fileSetNode != null) {

                            XmlNode baseDirectoryNode = fileSetNode.SelectSingleNode("@basedir");
                            if (baseDirectoryNode != null) {
                                fileSet.BaseDirectory = baseDirectoryNode.Value;
                            }

                            foreach (XmlNode node in fileSetNode.SelectNodes("includes")) {
                                string pathname = node.SelectSingleNode("@name").Value;
                                fileSet.Includes.Add(pathname);
                            }

                            foreach (XmlNode node in fileSetNode.SelectNodes("excludes")) {
                                fileSet.Excludes.Add(node.SelectSingleNode("@name").Value);
                            }
                        }
                    }
                }
                currentType = currentType.BaseType;
            }
        }

        protected void AutoExpandAttributes() {

            // Go down the inheritance tree to find the private fields in the object.
            // We are looking for task attributes to initialize.
            Type currentType = GetType();
            while (currentType != typeof(object)) {
                FieldInfo[] fieldInfoArray = currentType.GetFields(BindingFlags.NonPublic|BindingFlags.Instance);
                foreach (FieldInfo fieldInfo in fieldInfoArray) {

                    // proces string parameters
                    TaskAttributeAttribute taskAttribute = (TaskAttributeAttribute) Attribute.GetCustomAttribute(fieldInfo, typeof(TaskAttributeAttribute));
                    if (taskAttribute != null) {
                        if (taskAttribute.ExpandText) {
                            string value = (string) fieldInfo.GetValue(this);
                            value = Project.ExpandText(value);
                            fieldInfo.SetValue(this, value);
                        }

                        // if a field also has a validator attribute then ensure that value is correct
                        ValidatorAttribute[] validators = (ValidatorAttribute[]) Attribute.GetCustomAttributes(fieldInfo, typeof(ValidatorAttribute));
                        foreach (ValidatorAttribute validator in validators) {
                            string errorMessage = validator.Validate(fieldInfo.GetValue(this));
                            if (errorMessage != null) {
                                throw new BuildException(String.Format("Error processing '{0}' attribute in <{1}> task: {2}", taskAttribute.Name, Name, errorMessage), Location);
                            }
                        }
                    }
                }
                currentType = currentType.BaseType;
            }
        }

        public void Initialize(XmlNode taskNode) {
            Initialize(taskNode, null);
        }

        public void Initialize(XmlNode taskNode, Location location) {
            if (location != null) {
                _location = location;
            }
            AutoInitializeAttributes(taskNode);
            InitializeTask(taskNode);
        }

        public void Execute() {
            AutoExpandAttributes();
            ExecuteTask();
        }

        protected virtual void InitializeTask(XmlNode taskNode) {
        }

        protected abstract void ExecuteTask();
    }
}
