using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Globalization;

//using System.Workflow.Activities;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.Runtime.Hosting;
using Hosting = System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{
    /// <summary>
    /// Used for xml serializing a TrackProfile.
    /// </summary>    
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class TrackingProfileSerializer
    {
        public TrackingProfileSerializer()
        {
            StringReader reader = new StringReader(_xsd);
            _schema = XmlSchema.Read(new XmlTextReader(reader) { DtdProcessing = DtdProcessing.Prohibit }, null);
            _schema.Namespaces.Add("", _ns);
        }

        public XmlSchema Schema
        {
            get
            {
                return _schema;
            }
        }

        #region Deserialization

        List<ValidationEventArgs> _vArgs = new List<ValidationEventArgs>();
        bool _vex = false;
        /// <summary>
        /// Deserialize TrackingProfile in xml form to a TrackingProfile object.
        /// </summary>
        /// <param name="reader">TextReader containing TrackingProfile in xml form</param>
        /// <param name="profile">TrackingProfile</param>
        /// <exception cref="">XmlSchemaException</exception>
        /// <exception cref="">XmlException</exception>
        /// <exception cref="">ArgumentNullException</exception>
        /// <exception cref="">ArgumentException</exception>
        /// <exception cref="">ArgumentOutOfRangeException</exception>
        /// <exception cref="">FormatException</exception>
        /// <exception cref="">OverflowException</exception>
        /// <exception cref="">InvalidOperationException</exception>
        /// <exception cref="">TrackingProfileDeserializationException</exception>
        public TrackingProfile Deserialize(TextReader reader)
        {
            TrackingProfile profile = null;
            _vArgs = new List<ValidationEventArgs>();
            _vex = false;

            if (null == reader)
                throw new ArgumentNullException("reader");

            //
            // Specify that if no namespace is declare the default should be interpreted as ours
            NameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            nsmgr.AddNamespace(String.Empty, _ns);
            XmlParserContext ctx = new XmlParserContext(nt, nsmgr, null, XmlSpace.None);
            XmlReader pReader = XmlReader.Create(reader, GetSchemaReaderSettings(), ctx);

            try
            {
                profile = new TrackingProfile();
                //
                // Schema validation will catch if element is missing
                if (!pReader.ReadToDescendant("TrackingProfile"))
                {
                    //
                    // Will throw exception with validation errors
                    CheckSchemaErrors();
                    return null;
                }

                string version = pReader.GetAttribute("version");
                if ((null == version) || (0 == version.Trim().Length))
                    throw new TrackingProfileDeserializationException(ExecutionStringManager.InvalidProfileVersion);

                profile.Version = new Version(version);

                if (!pReader.ReadToDescendant("TrackPoints"))
                {
                    //
                    // Will throw exception with validation errors
                    CheckSchemaErrors();
                    return null;
                }

                CreateTrackPoints(pReader, profile);

                CheckSchemaErrors();
            }
            catch (Exception)
            {
                profile = null;
                throw;
            }
            finally
            {
                _vArgs = new List<ValidationEventArgs>();
                _vex = false;
                pReader.Close();
            }

            return profile;
        }

        private void CheckSchemaErrors()
        {
            //
            // If the parsing hit an error->throw
            // Clients can check ValidationEventArgs to get 
            // all errors & warnings that were caught.
            if (_vex)
            {
                TrackingProfileDeserializationException tpde = new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationSchemaError);
                if (null != _vArgs)
                {
                    foreach (ValidationEventArgs arg in _vArgs)
                        tpde.ValidationEventArgs.Add(arg);
                }
                throw tpde;
            }
        }

        private void CreateTrackPoints(XmlReader reader, TrackingProfile profile)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == profile)
                throw new ArgumentNullException("profile");

            if (0 != string.Compare(reader.Name, "TrackPoints", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "TrackPoints.");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "ActivityTrackPoint", StringComparison.Ordinal))
                            CreateActivityTrackPoint(reader, profile);
                        else if (0 == string.Compare(reader.Name, "UserTrackPoint", StringComparison.Ordinal))
                            CreateUserTrackPoint(reader, profile);
                        else if (0 == string.Compare(reader.Name, "WorkflowTrackPoint", StringComparison.Ordinal))
                            CreateWorkflowTrackPoint(reader, profile);
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "TrackPoints", StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "TrackPoints.");
        }

        private void CreateActivityTrackPoint(XmlReader reader, TrackingProfile profile)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == profile)
                throw new ArgumentNullException("profile");

            if (0 != string.Compare(reader.Name, "ActivityTrackPoint", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "ActivityTrackPoint.");

            if (reader.IsEmptyElement)
                return;

            ActivityTrackPoint point = new ActivityTrackPoint();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Annotations", StringComparison.Ordinal))
                            CreateAnnotations(reader, point.Annotations);
                        else if (0 == string.Compare(reader.Name, "MatchingLocations", StringComparison.Ordinal))
                            CreateActivityTrackingLocations(reader, point.MatchingLocations);
                        else if (0 == string.Compare(reader.Name, "ExcludedLocations", StringComparison.Ordinal))
                            CreateActivityTrackingLocations(reader, point.ExcludedLocations);
                        else if (0 == string.Compare(reader.Name, "Extracts", StringComparison.Ordinal))
                            CreateExtracts(reader, point.Extracts);
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "ActivityTrackPoint", StringComparison.Ordinal))
                        {
                            profile.ActivityTrackPoints.Add(point);
                            return;
                        }
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "ActivityTrackPoint.");
        }

        private void CreateActivityTrackingLocation(XmlReader reader, ActivityTrackingLocation location)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == location)
                throw new ArgumentNullException("location");

            if (0 != string.Compare(reader.Name, "ActivityTrackingLocation", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "ActivityTrackingLocation.");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "TypeName", StringComparison.Ordinal))
                        {
                            if (null != location.ActivityType)
                                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidType);

                            location.ActivityTypeName = reader.ReadString();
                        }
                        else if (0 == string.Compare(reader.Name, "Type", StringComparison.Ordinal))
                        {
                            if (null != location.ActivityTypeName)
                                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidType);

                            if (!reader.IsEmptyElement)
                            {
                                //
                                // Schema validation will catch empty elements, just make sure
                                // we don't pass GetType a null or empty string and continue.
                                string type = reader.ReadString();
                                if ((null != type) && (type.Trim().Length > 0))
                                    location.ActivityType = Type.GetType(type, true);
                            }
                        }
                        else if (0 == string.Compare(reader.Name, "MatchDerivedTypes", StringComparison.Ordinal))
                            location.MatchDerivedTypes = reader.ReadElementContentAsBoolean();
                        else if (0 == string.Compare(reader.Name, "ExecutionStatusEvents", StringComparison.Ordinal))
                            CreateStatusEvents(reader, location.ExecutionStatusEvents);
                        else if (0 == string.Compare(reader.Name, "Conditions", StringComparison.Ordinal))
                            CreateConditions(reader, location.Conditions);
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "ActivityTrackingLocation", StringComparison.Ordinal))
                        {
                            //
                            // If we don't have a type or name create the Activity type to track all activities
                            if ((null == location.ActivityType) && (null == location.ActivityTypeName))
                            {
                                location.ActivityType = typeof(Activity);
                                location.MatchDerivedTypes = true;
                            }

                            return;
                        }
                        break;
                }
            }
            //
            // Something bad happened
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "ActivityTrackingLocation.");
        }

        private void CreateUserTrackPoint(XmlReader reader, TrackingProfile profile)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == profile)
                throw new ArgumentNullException("profile");

            if (0 != string.Compare(reader.Name, "UserTrackPoint", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "UserTrackPoint.");

            if (reader.IsEmptyElement)
                return;

            UserTrackPoint point = new UserTrackPoint();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Annotations", StringComparison.Ordinal))
                            CreateAnnotations(reader, point.Annotations);
                        else if (0 == string.Compare(reader.Name, "MatchingLocations", StringComparison.Ordinal))
                            CreateUserTrackingLocations(reader, point.MatchingLocations);
                        else if (0 == string.Compare(reader.Name, "ExcludedLocations", StringComparison.Ordinal))
                            CreateUserTrackingLocations(reader, point.ExcludedLocations);
                        else if (0 == string.Compare(reader.Name, "Extracts", StringComparison.Ordinal))
                            CreateExtracts(reader, point.Extracts);
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "UserTrackPoint", StringComparison.Ordinal))
                        {
                            profile.UserTrackPoints.Add(point);
                            return;
                        }
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "UserTrackPoint.");
        }

        private void CreateUserTrackingLocation(XmlReader reader, UserTrackingLocation location)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == location)
                throw new ArgumentNullException("location");

            if (0 != string.Compare(reader.Name, "UserTrackingLocation", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "UserTrackingLocation.");

            if (reader.IsEmptyElement)
                return;

            string name = null, type = null;
            bool derived = false, seenAct = false, seenArg = false;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Activity", StringComparison.Ordinal))
                            seenAct = true;
                        else if (0 == string.Compare(reader.Name, "KeyName", StringComparison.Ordinal))
                            location.KeyName = reader.ReadString();
                        else if (0 == string.Compare(reader.Name, "Argument", StringComparison.Ordinal))
                            seenArg = true;
                        else if (0 == string.Compare(reader.Name, "TypeName", StringComparison.Ordinal))
                            name = reader.ReadString();
                        else if (0 == string.Compare(reader.Name, "Type", StringComparison.Ordinal))
                            type = reader.ReadString();
                        else if (0 == string.Compare(reader.Name, "MatchDerivedTypes", StringComparison.Ordinal))
                            derived = reader.ReadElementContentAsBoolean();
                        else if (0 == string.Compare(reader.Name, "Conditions", StringComparison.Ordinal))
                            CreateConditions(reader, location.Conditions);
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "UserTrackingLocation", StringComparison.Ordinal))
                        {
                            if (!seenAct)
                            {
                                location.ActivityType = typeof(Activity);
                                location.MatchDerivedActivityTypes = true;
                            }

                            if (!seenArg)
                            {
                                location.ArgumentType = typeof(object);
                                location.MatchDerivedArgumentTypes = true;
                            }

                            if ((null == location.ActivityType) && ((null == location.ActivityTypeName) || (0 == location.ActivityTypeName.Trim().Length)) && (null == location.ArgumentType) && ((null == location.ArgumentTypeName) || (0 == location.ArgumentTypeName.Trim().Length)))
                                throw new TrackingProfileDeserializationException(ExecutionStringManager.MissingActivityType);

                            return;
                        }
                        else if (0 == string.Compare(reader.Name, "Activity", StringComparison.Ordinal))
                        {
                            if (!seenAct)
                            {
                                location.ActivityType = typeof(Activity);
                                location.MatchDerivedActivityTypes = true;
                            }
                            else
                            {
                                if ((null != type) && (type.Trim().Length > 0))
                                    location.ActivityType = Type.GetType(type, true);
                                else
                                    location.ActivityTypeName = name;

                                location.MatchDerivedActivityTypes = derived;
                            }

                            name = null;
                            type = null;
                            derived = false;
                        }
                        else if (0 == string.Compare(reader.Name, "Argument", StringComparison.Ordinal))
                        {
                            if (!seenArg)
                            {
                                location.ArgumentType = typeof(object);
                                location.MatchDerivedArgumentTypes = true;
                            }
                            else
                            {
                                if ((null != type) && (type.Trim().Length > 0))
                                    location.ArgumentType = Type.GetType(type, true);
                                else
                                    location.ArgumentTypeName = name;

                                location.MatchDerivedArgumentTypes = derived;
                            }

                            name = null;
                            type = null;
                            derived = false;
                        }

                        break;
                }
            }
            //
            // Something bad happened
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "UserTrackingLocation.");
        }

        private void CreateWorkflowTrackPoint(XmlReader reader, TrackingProfile profile)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == profile)
                throw new ArgumentNullException("profile");

            if (0 != string.Compare(reader.Name, "WorkflowTrackPoint", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "WorkflowTrackPoint.");

            if (reader.IsEmptyElement)
                return;

            WorkflowTrackPoint point = new WorkflowTrackPoint();
            point.MatchingLocation = new WorkflowTrackingLocation();

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Annotations", StringComparison.Ordinal))
                            CreateAnnotations(reader, point.Annotations);
                        else if (0 == string.Compare(reader.Name, "TrackingWorkflowEvent", StringComparison.Ordinal))
                            point.MatchingLocation.Events.Add((TrackingWorkflowEvent)Enum.Parse(typeof(TrackingWorkflowEvent), reader.ReadString()));
                        //
                        // Xsd validation will catch unknown elements
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "WorkflowTrackPoint", StringComparison.Ordinal))
                        {
                            profile.WorkflowTrackPoints.Add(point);
                            return;
                        }
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "WorkflowTrackPoint.");
        }

        private void CreateStatusEvents(XmlReader reader, IList<ActivityExecutionStatus> events)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == events)
                throw new ArgumentNullException("events");

            if (0 != string.Compare("ExecutionStatusEvents", reader.Name, StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "ExecutionStatusEvents.");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "ExecutionStatus", StringComparison.Ordinal))
                        {
                            string status = reader.ReadString();
                            if ((null != status) && (status.Trim().Length > 0))
                            {
                                string[] names = Enum.GetNames(typeof(ActivityExecutionStatus));
                                foreach (string s in names)
                                {
                                    if (0 == string.Compare(s, status, StringComparison.Ordinal))
                                        events.Add((ActivityExecutionStatus)Enum.Parse(typeof(ActivityExecutionStatus), status));
                                }
                            }
                        }
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "ExecutionStatusEvents", StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Something is funky
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "ExecutionStatusEvents.");
        }

        private void CreateConditions(XmlReader reader, TrackingConditionCollection conditions)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == conditions)
                throw new ArgumentNullException("conditions");

            if (0 != string.Compare("Conditions", reader.Name, StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "Conditions.");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "ActivityTrackingCondition", StringComparison.Ordinal))
                        {
                            ActivityTrackingCondition condition = new ActivityTrackingCondition();
                            CreateCondition(reader, condition);
                            conditions.Add(condition);
                        }
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "Conditions", StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "Conditions.");
        }

        private void CreateCondition(XmlReader reader, TrackingCondition condition)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == condition)
                throw new ArgumentNullException("condition");

            if (0 != string.Compare(condition.GetType().Name, reader.Name, StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + condition.GetType().Name);

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Member", StringComparison.Ordinal))
                        {
                            condition.Member = reader.ReadString();
                        }
                        else if (0 == string.Compare(reader.Name, "Operator", StringComparison.Ordinal))
                        {
                            string op = reader.ReadString();
                            if ((null != op) && (op.Trim().Length > 0))
                            {
                                string[] names = Enum.GetNames(typeof(ComparisonOperator));
                                foreach (string s in names)
                                {
                                    if (0 == string.Compare(s, op, StringComparison.Ordinal))
                                        condition.Operator = (ComparisonOperator)Enum.Parse(typeof(ComparisonOperator), op);
                                }
                            }
                        }
                        else if (0 == string.Compare(reader.Name, "Value", StringComparison.Ordinal))
                        {
                            if (!reader.IsEmptyElement)
                                condition.Value = reader.ReadString();
                        }
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, condition.GetType().Name, StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + condition.GetType().Name);
        }

        private void CreateExtracts(XmlReader reader, ExtractCollection extracts)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == extracts)
                throw new ArgumentNullException("extracts");

            if (0 != string.Compare("Extracts", reader.Name, StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "Extracts");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "ActivityDataTrackingExtract", StringComparison.Ordinal))
                        {
                            ActivityDataTrackingExtract e = new ActivityDataTrackingExtract();
                            CreateExtract(reader, e);
                            extracts.Add(e);
                        }
                        else if (0 == string.Compare(reader.Name, "WorkflowDataTrackingExtract", StringComparison.Ordinal))
                        {
                            WorkflowDataTrackingExtract e = new WorkflowDataTrackingExtract();
                            CreateExtract(reader, e);
                            extracts.Add(e);
                        }
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "Extracts", StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "Extracts.");
        }

        private void CreateExtract(XmlReader reader, TrackingExtract extract)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == extract)
                throw new ArgumentNullException("extract");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Member", StringComparison.Ordinal))
                            extract.Member = reader.ReadString();
                        else if (0 == string.Compare(reader.Name, "Annotations", StringComparison.Ordinal))
                            CreateAnnotations(reader, extract.Annotations);
                        //
                        // Xsd validation will catch unknown elements

                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, extract.GetType().Name, StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + extract.GetType().Name);
        }

        private void CreateAnnotations(XmlReader reader, TrackingAnnotationCollection annotations)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == annotations)
                throw new ArgumentNullException("annotations");

            if (0 != string.Compare(reader.Name, "Annotations", StringComparison.Ordinal))
                throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationInvalidPosition + "Annotations.");

            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "Annotation", StringComparison.Ordinal))
                        {
                            //
                            // Preserve null and empty as distinct values
                            // null == <Annotation /> empty string = <Annotation></Annotation>
                            if (!reader.IsEmptyElement)
                                annotations.Add(reader.ReadString());
                            else
                                annotations.Add(null);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(reader.Name, "Annotations", StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + "Annotations.");
        }

        private void CreateActivityTrackingLocations(XmlReader reader, ActivityTrackingLocationCollection activities)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == activities)
                throw new ArgumentNullException("activities");

            if (reader.IsEmptyElement)
                return;

            string startName = reader.Name;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "ActivityTrackingLocation", StringComparison.Ordinal))
                        {
                            ActivityTrackingLocation location = new ActivityTrackingLocation();
                            CreateActivityTrackingLocation(reader, location);
                            activities.Add(location);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(startName, reader.Name, StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + startName + ".");
        }

        private void CreateUserTrackingLocations(XmlReader reader, UserTrackingLocationCollection user)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == user)
                throw new ArgumentNullException("user");

            if (reader.IsEmptyElement)
                return;

            string startName = reader.Name;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (0 == string.Compare(reader.Name, "UserTrackingLocation", StringComparison.Ordinal))
                        {
                            UserTrackingLocation location = new UserTrackingLocation();
                            CreateUserTrackingLocation(reader, location);
                            user.Add(location);
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (0 == string.Compare(startName, reader.Name, StringComparison.Ordinal))
                            return;
                        break;
                }
            }
            //
            // Only valid exit is on an EndElement that matches the element that is passed in.
            throw new TrackingProfileDeserializationException(ExecutionStringManager.TrackingDeserializationCloseElementNotFound + startName + ".");
        }

        private XmlReaderSettings GetSchemaReaderSettings()
        {
            XmlReaderSettings profileSettings = new XmlReaderSettings();

            profileSettings.Schemas.Add(_schema);
            profileSettings.ValidationType = ValidationType.Schema;
            profileSettings.ConformanceLevel = ConformanceLevel.Document;
            profileSettings.CloseInput = false;
            profileSettings.IgnoreComments = true;
            profileSettings.IgnoreProcessingInstructions = true;
            profileSettings.DtdProcessing = DtdProcessing.Prohibit;
            profileSettings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            return profileSettings;
        }

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            _vArgs.Add(e);

            if (e.Severity == XmlSeverityType.Error)
                _vex = true;
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Xml serialize a TrackingProfile.
        /// </summary>
        /// <param name="profile">TrackingProfile to serialize.</param>
        /// <param name="writer">TextWriter to store xml text.</param>
        public void Serialize(TextWriter writer, TrackingProfile profile)
        {
            if (null == profile)
                throw new ArgumentNullException("profile");

            if (null == writer)
                throw new ArgumentNullException("writer");

            XmlTextWriter xmlWriter = new XmlTextWriter(writer);

            InitWriter(xmlWriter);

            Write(profile, xmlWriter);

            xmlWriter.Flush();
            xmlWriter.Close();
        }

        private void Write(TrackingProfile profile, XmlTextWriter writer)
        {
            writer.WriteStartDocument(true);

            writer.WriteStartElement("TrackingProfile");
            // Write the namespace declaration.    
            writer.WriteAttributeString("xmlns", _ns);

            if (null == profile.Version)
                throw new ArgumentException(ExecutionStringManager.InvalidProfileVersion);

            string version = null;
            if (profile.Version.Revision >= 0)
                version = string.Format(NumberFormatInfo.InvariantInfo, "{0}.{1}.{2}.{3}", profile.Version.Major, profile.Version.Minor, profile.Version.Build, profile.Version.Revision);
            else if (profile.Version.Build >= 0)
                version = string.Format(NumberFormatInfo.InvariantInfo, "{0}.{1}.{2}", profile.Version.Major, profile.Version.Minor, profile.Version.Build);
            else if (profile.Version.Minor >= 0)
                version = string.Format(NumberFormatInfo.InvariantInfo, "{0}.{1}", profile.Version.Major, profile.Version.Minor);

            writer.WriteAttributeString("version", version);
            WriteTrackPoints(profile, writer);

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        #region TrackPoints

        private void WriteTrackPoints(TrackingProfile profile, XmlTextWriter writer)
        {
            //
            // We must have at least 1 trackpoint or the profile won't be valid
            if (((null == profile.WorkflowTrackPoints) || (0 == profile.WorkflowTrackPoints.Count))
                && ((null == profile.ActivityTrackPoints) || (0 == profile.ActivityTrackPoints.Count))
                && ((null == profile.UserTrackPoints) || (0 == profile.UserTrackPoints.Count)))
                throw new ArgumentException(ExecutionStringManager.TrackingSerializationNoTrackPoints);

            int count = 0;

            writer.WriteStartElement("TrackPoints");

            foreach (WorkflowTrackPoint point in profile.WorkflowTrackPoints)
            {
                if (null != point)
                {
                    WriteWorkflowTrackPoint(point, writer);
                    count++;
                }
            }

            foreach (ActivityTrackPoint point in profile.ActivityTrackPoints)
            {
                if (null != point)
                {
                    WriteActivityTrackPoint(point, writer);
                    count++;
                }
            }

            foreach (UserTrackPoint point in profile.UserTrackPoints)
            {
                if (null != point)
                {
                    WriteUserTrackPoint(point, writer);
                    count++;
                }
            }

            //
            // We must have at least 1 trackpoint or the profile isn't valid
            if (0 == count)
                throw new ArgumentException(ExecutionStringManager.TrackingSerializationNoTrackPoints);

            writer.WriteEndElement();
        }

        private void WriteActivityTrackPoint(ActivityTrackPoint point, XmlTextWriter writer)
        {
            if (null == point)
                throw new ArgumentNullException("point");
            //
            // Validate this element's required fields
            if ((null == point.MatchingLocations) || (0 == point.MatchingLocations.Count))
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);

            writer.WriteStartElement("ActivityTrackPoint");

            writer.WriteStartElement("MatchingLocations");
            //
            // Write the locations that should be matched
            // At least one non null location is required.
            int count = 0;
            foreach (ActivityTrackingLocation location in point.MatchingLocations)
            {
                if (null != location)
                {
                    WriteActivityTrackingLocation(location, writer);
                    count++;
                }
            }

            if (0 == count)
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);

            writer.WriteEndElement();
            //
            // Write the locations that should not be matched (these override the locations to match)
            // Excludes are not required.
            if ((null != point.ExcludedLocations) && (point.ExcludedLocations.Count > 0))
            {
                writer.WriteStartElement("ExcludedLocations");

                foreach (ActivityTrackingLocation location in point.ExcludedLocations)
                {
                    if (null != location)
                        WriteActivityTrackingLocation(location, writer);
                }

                writer.WriteEndElement();
            }
            //
            // Write annotations, not a required field
            WriteAnnotations(point.Annotations, writer);
            //
            // Write extracts, not a required field
            WriteExtracts(point.Extracts, writer);

            writer.WriteEndElement();
        }

        private void WriteWorkflowTrackPoint(WorkflowTrackPoint point, XmlTextWriter writer)
        {
            //
            // Validate this element's required fields
            if (null == point.MatchingLocation)
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocation);

            writer.WriteStartElement("WorkflowTrackPoint");

            WriteWorkflowTrackingLocation(point.MatchingLocation, writer);
            //
            // Write annotations, not a required field
            WriteAnnotations(point.Annotations, writer);

            writer.WriteEndElement();
        }

        private void WriteUserTrackPoint(UserTrackPoint point, XmlTextWriter writer)
        {
            //
            // Validate this element's required fields
            if ((null == point.MatchingLocations) || (0 == point.MatchingLocations.Count))
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);

            writer.WriteStartElement("UserTrackPoint");

            writer.WriteStartElement("MatchingLocations");

            //
            // Write the locations that should be matched
            // At least one non null location is required.
            int count = 0;
            foreach (UserTrackingLocation location in point.MatchingLocations)
            {
                if (null != location)
                {
                    WriteUserTrackingLocation(location, writer);
                    count++;
                }
            }

            if (0 == count)
                throw new ArgumentException(ExecutionStringManager.NoMatchingLocations);

            writer.WriteEndElement();

            //
            // Write the locations that should not be matched (these override the locations to match)
            // Excludes are not required.
            if ((null != point.ExcludedLocations) && (point.ExcludedLocations.Count > 0))
            {
                writer.WriteStartElement("ExcludedLocations");

                foreach (UserTrackingLocation location in point.ExcludedLocations)
                {
                    if (null != location)
                        WriteUserTrackingLocation(location, writer);
                }

                writer.WriteEndElement();
            }

            //
            // Write annotations, not a required field
            WriteAnnotations(point.Annotations, writer);
            //
            // Write extracts, not a required field
            WriteExtracts(point.Extracts, writer);

            writer.WriteEndElement();
        }

        #endregion

        #region Location

        private void WriteActivityTrackingLocation(ActivityTrackingLocation loc, XmlTextWriter writer)
        {
            //
            // Validate this element's required fields
            if ((null == loc.ActivityType) && ((null == loc.ActivityTypeName) || (0 == loc.ActivityTypeName.Trim().Length)))
                throw new ArgumentException(ExecutionStringManager.MissingActivityType);

            writer.WriteStartElement("ActivityTrackingLocation");

            writer.WriteStartElement("Activity");

            if (null != loc.ActivityType)
                writer.WriteElementString("Type", loc.ActivityType.AssemblyQualifiedName);
            else
                writer.WriteElementString("TypeName", loc.ActivityTypeName);

            writer.WriteElementString("MatchDerivedTypes", loc.MatchDerivedTypes.ToString().ToLower(CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            WriteEvents(loc.ExecutionStatusEvents, writer);

            if ((null != loc.Conditions) && (loc.Conditions.Count > 0))
                WriteConditions(loc.Conditions, writer);


            writer.WriteEndElement();
        }

        private void WriteUserTrackingLocation(UserTrackingLocation loc, XmlTextWriter writer)
        {
            //
            // Validate this element's required fields
            if ((null == loc.ActivityType) && ((null == loc.ActivityTypeName) || (0 == loc.ActivityTypeName.Trim().Length)))
                throw new ArgumentException(ExecutionStringManager.MissingActivityType);

            if ((null == loc.ArgumentType) && ((null == loc.ArgumentTypeName) || (0 == loc.ArgumentTypeName.Trim().Length)))
                throw new ArgumentException(ExecutionStringManager.MissingArgumentType);

            writer.WriteStartElement("UserTrackingLocation");
            //
            // Write the Acctivity node
            writer.WriteStartElement("Activity");

            if (null != loc.ActivityType)
                writer.WriteElementString("Type", loc.ActivityType.AssemblyQualifiedName);
            else
                writer.WriteElementString("TypeName", loc.ActivityTypeName);

            writer.WriteElementString("MatchDerivedTypes", loc.MatchDerivedActivityTypes.ToString().ToLower(CultureInfo.InvariantCulture));
            writer.WriteEndElement();
            //
            // Write the key name node if it is non null
            if (null != loc.KeyName)
                writer.WriteElementString("KeyName", loc.KeyName);
            //
            // Write the Argument node
            writer.WriteStartElement("Argument");

            if (null != loc.ArgumentType)
                writer.WriteElementString("Type", loc.ArgumentType.AssemblyQualifiedName);
            else
                writer.WriteElementString("TypeName", loc.ArgumentTypeName);

            writer.WriteElementString("MatchDerivedTypes", loc.MatchDerivedArgumentTypes.ToString().ToLower(CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            if ((null != loc.Conditions) && (loc.Conditions.Count > 0))
                WriteConditions(loc.Conditions, writer);

            writer.WriteEndElement();
        }

        private void WriteWorkflowTrackingLocation(WorkflowTrackingLocation loc, XmlTextWriter writer)
        {
            if ((null == loc.Events) || (0 == loc.Events.Count))
                throw new ArgumentException(ExecutionStringManager.MissingWorkflowEvents);

            writer.WriteStartElement("MatchingLocation");

            writer.WriteStartElement("WorkflowTrackingLocation");

            WriteWorkflowEvents(loc.Events, writer);

            writer.WriteEndElement();

            writer.WriteEndElement();
        }

        #endregion

        #region TrackingExtract

        private void WriteExtracts(ExtractCollection extracts, XmlTextWriter writer)
        {
            if ((null == extracts) || (0 == extracts.Count))
                return;

            writer.WriteStartElement("Extracts");

            foreach (TrackingExtract e in extracts)
            {
                if (null != e)
                    WriteExtract(e, writer);
            }

            writer.WriteEndElement();
        }

        private void WriteExtract(TrackingExtract extract, XmlTextWriter writer)
        {
            Type t = extract.GetType();

            if ((extract is ActivityDataTrackingExtract) || (extract is WorkflowDataTrackingExtract))
            {
                writer.WriteStartElement(extract.GetType().Name);
                writer.WriteElementString("Member", (null == extract.Member ? string.Empty : extract.Member));

                WriteAnnotations(extract.Annotations, writer);

                writer.WriteEndElement();
            }
            else
                throw new ArgumentException(ExecutionStringManager.TrackingSerializationInvalidExtract);
        }

        #endregion

        #region Shared

        private void WriteConditions(TrackingConditionCollection conditions, XmlTextWriter writer)
        {
            if ((null == conditions) || (0 == conditions.Count))
                return;

            writer.WriteStartElement("Conditions");

            foreach (TrackingCondition c in conditions)
            {
                if (null != c)
                    WriteCondition(c, writer);
            }

            writer.WriteEndElement();
        }

        private void WriteCondition(TrackingCondition condition, XmlTextWriter writer)
        {
            if (null == condition)
                return;

            writer.WriteStartElement(condition.GetType().Name);

            writer.WriteElementString("Operator", condition.Operator.ToString());

            if ((null == condition.Member) || (0 == condition.Member.Trim().Length))
                throw new ArgumentException(ExecutionStringManager.MissingMemberName);

            writer.WriteElementString("Member", condition.Member);

            if (null != condition.Value)
            {
                if (string.Empty == condition.Value)
                {
                    writer.WriteStartElement("Value");
                    writer.WriteRaw(string.Empty);
                    writer.WriteEndElement();
                }
                else
                    writer.WriteElementString("Value", condition.Value);
            }

            writer.WriteEndElement();
        }

        private void WriteAnnotations(TrackingAnnotationCollection annotations, XmlTextWriter writer)
        {
            if ((null == annotations) || (0 == annotations.Count))
                return;

            writer.WriteStartElement("Annotations");
            foreach (string s in annotations)
            {
                //
                // Preserve null and empty as distinct values
                // null == <Annotation /> empty string = <Annotation></Annotation>
                writer.WriteStartElement("Annotation");
                if ((null == s) || (s.Length > 0))
                {
                    writer.WriteValue(null == s ? String.Empty : s);
                    writer.WriteEndElement();
                }
                else
                    writer.WriteFullEndElement();
            }
            writer.WriteEndElement();
        }

        private void WriteEvents(IList<ActivityExecutionStatus> events, XmlTextWriter writer)
        {
            if ((null == events) || (0 == events.Count))
                throw new ArgumentException(ExecutionStringManager.MissingActivityEvents);

            writer.WriteStartElement("ExecutionStatusEvents");

            foreach (ActivityExecutionStatus s in events)
            {
                if (!IsStatus((int)s))
                    throw new ArgumentException(ExecutionStringManager.InvalidStatus);

                writer.WriteStartElement("ExecutionStatus");
                writer.WriteValue(s.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private bool IsStatus(int val)
        {
            foreach (ActivityExecutionStatus s in Enum.GetValues(typeof(ActivityExecutionStatus)))
            {
                if ((int)s == val)
                    return true;
            }
            return false;
        }

        private void WriteWorkflowEvents(IList<TrackingWorkflowEvent> events, XmlTextWriter writer)
        {
            if ((null == events) || (0 == events.Count))
                return;

            writer.WriteStartElement("TrackingWorkflowEvents");

            foreach (TrackingWorkflowEvent s in events)
            {
                if (!IsWorkflowEvent((int)s))
                    throw new ArgumentException(ExecutionStringManager.InvalidWorkflowEvent);

                writer.WriteStartElement("TrackingWorkflowEvent");
                writer.WriteValue(s.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private bool IsWorkflowEvent(int val)
        {
            foreach (TrackingWorkflowEvent s in Enum.GetValues(typeof(TrackingWorkflowEvent)))
            {
                if ((int)s == val)
                    return true;
            }
            return false;
        }

        #endregion

        private void InitWriter(XmlTextWriter writer)
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 4;
        }

        #endregion

        #region Schema

        private const string _ns = "http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile";
        private XmlSchema _schema = null;

        internal const string _xsd = @"<?xml version=""1.0"" encoding=""utf-8""?>
<xs:schema id=""WFTrackingProfile"" targetNamespace=""http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile"" elementFormDefault=""qualified"" xmlns=""http://schemas.microsoft.com/winfx/2006/workflow/trackingprofile"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""TrackingProfile"" type=""TrackingProfileType"" />

    <xs:complexType name=""TrackingProfileType"">
        <xs:sequence>
            <xs:element name=""TrackPoints"" type=""TrackPointListType"" minOccurs=""1"" maxOccurs=""1"" />
        </xs:sequence>
        <xs:attribute name=""version"" type=""VersionType"" />
    </xs:complexType>

    <xs:complexType name=""TrackPointListType"">
        <xs:sequence>
            <xs:choice minOccurs=""1"" maxOccurs=""unbounded"">
                <xs:element name=""ActivityTrackPoint"" type=""ActivityTrackPointType"" />
                <xs:element name=""UserTrackPoint"" type=""UserTrackPointType"" />
                <xs:element name=""WorkflowTrackPoint"" type=""WorkflowTrackPointType""  />
            </xs:choice>
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""ActivityTrackPointType"">
        <xs:sequence>
            <xs:element name=""MatchingLocations"" minOccurs=""1"" maxOccurs=""1"" type=""IncludeActivityTrackingLocationListType"" />
            <xs:element name=""ExcludedLocations"" minOccurs=""0"" maxOccurs=""1"" type=""ExcludeActivityTrackingLocationListType"" />
            <xs:element name=""Annotations"" type=""AnnotationListType"" minOccurs=""0"" maxOccurs=""1"" />
            <xs:element name=""Extracts"" type=""ExtractListType"" minOccurs=""0"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""IncludeActivityTrackingLocationListType"">
        <xs:sequence>
            <xs:element name=""ActivityTrackingLocation"" type=""ActivityTrackingLocationType"" minOccurs=""1"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""ExcludeActivityTrackingLocationListType"">
        <xs:sequence>
            <xs:element name=""ActivityTrackingLocation"" type=""ActivityTrackingLocationType"" minOccurs=""0"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""ActivityTrackingLocationType"">
        <xs:sequence>
            <xs:element name=""Activity"" type=""Type"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""ExecutionStatusEvents"" type=""ExecutionStatusEventListType"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Conditions"" type=""ConditionListType"" minOccurs=""0"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""UserTrackPointType"">
        <xs:sequence>
            <xs:element name=""MatchingLocations"" type=""IncludeUserTrackingLocationListType"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""ExcludedLocations"" type=""ExcludeUserTrackingLocationListType"" minOccurs=""0"" maxOccurs=""1"" />
            <xs:element name=""Annotations"" type=""AnnotationListType"" minOccurs=""0"" maxOccurs=""1"" />
            <xs:element name=""Extracts"" type=""ExtractListType"" minOccurs=""0"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""IncludeUserTrackingLocationListType"">
        <xs:sequence>
            <xs:element name=""UserTrackingLocation"" type=""UserTrackingLocationType"" minOccurs=""1"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""ExcludeUserTrackingLocationListType"">
        <xs:sequence>
            <xs:element name=""UserTrackingLocation"" type=""UserTrackingLocationType"" minOccurs=""0"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""UserTrackingLocationType"">
        <xs:sequence>
            <xs:element name=""Activity"" type=""Type"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""KeyName"" type=""NonNullString"" minOccurs=""0"" maxOccurs=""1"" />
            <xs:element name=""Argument"" type=""Type"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Conditions"" type=""ConditionListType"" minOccurs=""0"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""WorkflowTrackPointType"">
        <xs:sequence>
            <xs:element name=""MatchingLocation"" type=""WorkflowTrackingLocationMatchType"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Annotations"" type=""AnnotationListType"" minOccurs=""0"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""WorkflowTrackingLocationMatchType"">
        <xs:sequence>
            <xs:element name=""WorkflowTrackingLocation"" type=""WorkflowTrackingLocationType"" minOccurs=""1"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""WorkflowTrackingLocationType"">
        <xs:sequence>
            <xs:element name=""TrackingWorkflowEvents"" type=""TrackingWorkflowEventListType"" minOccurs=""1"" maxOccurs=""1"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""Type"">
        <xs:sequence>
            <xs:choice>
                <xs:element name=""TypeName"" type=""NonNullString"" />
                <xs:element name=""Type"" type=""NonNullString"" />
            </xs:choice>
            <xs:element name=""MatchDerivedTypes"" type=""xs:boolean"" minOccurs=""1"" maxOccurs=""1"" default=""false"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""AnnotationListType"">
        <xs:sequence>
            <xs:element name=""Annotation"" type=""xs:string"" minOccurs=""0"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""ConditionListType"">
        <xs:sequence>
            <xs:element name=""ActivityTrackingCondition"" type=""ActivityTrackingConditionType"" minOccurs=""0"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""ActivityTrackingConditionType"">
        <xs:sequence minOccurs=""1"" maxOccurs=""1"">
            <xs:element name=""Operator"" type=""OperatorType"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Member"" type=""NonNullString"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Value"" type=""xs:string"" minOccurs=""0"" maxOccurs=""1"" nillable=""true"" />
        </xs:sequence>
    </xs:complexType>

    <xs:simpleType name=""OperatorType"">
        <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Equals"" />
            <xs:enumeration value=""NotEquals"" />
        </xs:restriction>
    </xs:simpleType>

    <xs:complexType name=""ExecutionStatusEventListType"">
        <xs:sequence>
            <xs:element name=""ExecutionStatus"" type=""ExecutionStatusType"" minOccurs=""1"" maxOccurs=""6"" />
        </xs:sequence>
    </xs:complexType>

    <xs:simpleType name=""ExecutionStatusType"">
        <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Initialized"" />
            <xs:enumeration value=""Executing"" />
            <xs:enumeration value=""Compensating"" />
            <xs:enumeration value=""Canceling"" />
            <xs:enumeration value=""Closed"" />
            <xs:enumeration value=""Faulting"" />
        </xs:restriction>
    </xs:simpleType>

    <xs:complexType name=""TrackingWorkflowEventListType"">
        <xs:sequence>
            <xs:element name=""TrackingWorkflowEvent"" type=""TrackingWorkflowEventType"" minOccurs=""1"" maxOccurs=""13"" />
        </xs:sequence>
    </xs:complexType>
        
    <xs:simpleType name=""TrackingWorkflowEventType"">
        <xs:restriction base=""xs:string"">
            <xs:enumeration value=""Created"" />
            <xs:enumeration value=""Completed"" />
            <xs:enumeration value=""Idle"" />
            <xs:enumeration value=""Suspended"" />
            <xs:enumeration value=""Resumed"" />
            <xs:enumeration value=""Persisted"" />
            <xs:enumeration value=""Unloaded"" />
            <xs:enumeration value=""Loaded"" />
            <xs:enumeration value=""Exception"" />
            <xs:enumeration value=""Terminated"" />
            <xs:enumeration value=""Aborted"" />
            <xs:enumeration value=""Changed"" />
            <xs:enumeration value=""Started"" />
        </xs:restriction>
    </xs:simpleType>

    <xs:complexType name=""ExtractListType"">
        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
            <xs:element name=""ActivityDataTrackingExtract"" type=""ActivityDataTrackingExtractType"" />
            <xs:element name=""WorkflowDataTrackingExtract"" type=""WorkflowDataTrackingExtractType"" />
        </xs:choice>
    </xs:complexType>

    <xs:complexType name=""ActivityDataTrackingExtractType"">
        <xs:sequence minOccurs=""1"" maxOccurs=""1"">
            <xs:element name=""Member"" type=""xs:string"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Annotations"" type=""AnnotationListType"" minOccurs=""0"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name=""WorkflowDataTrackingExtractType"">
        <xs:sequence minOccurs=""1"" maxOccurs=""1"">
            <xs:element name=""Member"" type=""xs:string"" minOccurs=""1"" maxOccurs=""1"" />
            <xs:element name=""Annotations"" type=""AnnotationListType"" minOccurs=""0"" maxOccurs=""unbounded"" />
        </xs:sequence>
    </xs:complexType>

    <xs:simpleType name=""VersionType"">
        <xs:restriction base=""xs:string"">
            <xs:pattern value=""(^(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\.(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\.(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\.(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)|(^(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\.(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\.(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)|(^(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))\.(0*([0-9]\d{0,8}|[1-2][0-1][0-4][0-7][0-4][0-8][0-3][0-6][0-4][0-7]))$)"" />
        </xs:restriction>
    </xs:simpleType>

    <xs:simpleType name=""NonNullString"">
        <xs:restriction base=""xs:string"">
            <xs:minLength value=""1"" />
        </xs:restriction>
    </xs:simpleType>
</xs:schema>";
        #endregion Schema
    }
}
