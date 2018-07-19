//------------------------------------------------------------------------------
// <copyright file="ResourcePermissionBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Security.Permissions {
    using System;
    using System.Text;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Globalization;
    using System.Diagnostics;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [
    Serializable(),
    SecurityPermissionAttribute(SecurityAction.InheritanceDemand, ControlEvidence = true, ControlPolicy = true)
    ]
    public abstract class ResourcePermissionBase :  CodeAccessPermission, IUnrestrictedPermission {
        private static volatile string computerName;
        private string[] tagNames;
        private Type permissionAccessType;
        private bool isUnrestricted;
        private Hashtable rootTable = CreateHashtable();

        public const string Any = "*";
        public const string Local = ".";

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected ResourcePermissionBase() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected ResourcePermissionBase(PermissionState state) {
            if (state == PermissionState.Unrestricted)
                this.isUnrestricted = true;
            else if (state == PermissionState.None)
                this.isUnrestricted = false;
            else
                throw new ArgumentException(SR.GetString(SR.InvalidPermissionState), "state");
        }

        // Put this in one central place.  Some resource types may require a
        // different form of string comparison.  If we need to fix this, then
        // consider making this protected & virtual, and override it where 
        // necessary.  Or consider doing this all internally so we could 
        // reimplement this permission to use a generic collection, etc.
        private static Hashtable CreateHashtable()
        {
#pragma warning disable 618
            // Most subclasses should be using an OSCasing string comparer,
            // and this is our best current match.
            // We're using the obsolete classes so we can deserialize on v1.1. 
            return new Hashtable(StringComparer.OrdinalIgnoreCase);
#pragma warning restore 618
        }

        private string ComputerName {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                if (computerName == null) {
                    lock (typeof(ResourcePermissionBase)) {
                        if (computerName == null) {
                            StringBuilder sb = new StringBuilder(256);
                            int len = sb.Capacity;
                            UnsafeNativeMethods.GetComputerName(sb, ref len);
                            computerName = sb.ToString();
                        }
                    }
                }

                return computerName;
            }
        }

        private bool IsEmpty {
            get {
                return (!isUnrestricted && rootTable.Count == 0);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected Type PermissionAccessType {
            get {
                return this.permissionAccessType;
            }

            set {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (!value.IsEnum)
                    throw new ArgumentException(SR.GetString(SR.PermissionBadParameterEnum), "value");
                    
                this.permissionAccessType = value;                    
            }             
        }  
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected string[] TagNames {
            get {
                return this.tagNames;
            }

            set {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value.Length == 0)
                    throw new ArgumentException(SR.GetString(SR.PermissionInvalidLength, "0"),"value");
                                        
                this.tagNames = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void AddPermissionAccess(ResourcePermissionBaseEntry entry) {
            if (entry == null)
                throw new ArgumentNullException("entry");

            if (entry.PermissionAccessPath.Length != this.TagNames.Length)
                throw new InvalidOperationException(SR.GetString(SR.PermissionNumberOfElements));

            Hashtable currentTable = this.rootTable;
            string[] accessPath = entry.PermissionAccessPath;
            for (int index = 0; index < accessPath.Length - 1; ++ index) {
                if (currentTable.ContainsKey(accessPath[index]))
                    currentTable = (Hashtable)currentTable[accessPath[index]];
                else {
                    Hashtable newHashTable = CreateHashtable();
                    currentTable[accessPath[index]] = newHashTable;
                    currentTable = newHashTable;
                }
            }

            if (currentTable.ContainsKey(accessPath[accessPath.Length - 1]))
                throw new InvalidOperationException(SR.GetString(SR.PermissionItemExists));

            currentTable[accessPath[accessPath.Length - 1]] = entry.PermissionAccess;
        }

        protected void Clear() {
            this.rootTable.Clear();
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override IPermission Copy() {
            ResourcePermissionBase permission = CreateInstance();
            permission.tagNames = this.tagNames;
            permission.permissionAccessType = this.permissionAccessType;
            permission.isUnrestricted = this.isUnrestricted;
            permission.rootTable = CopyChildren(this.rootTable, 0);
            return permission;
        }

        private Hashtable CopyChildren(object currentContent, int tagIndex) {
            IDictionaryEnumerator contentEnumerator = ((Hashtable)currentContent).GetEnumerator();
            Hashtable newTable = CreateHashtable();
            while(contentEnumerator.MoveNext()) {
                if (tagIndex < (this.TagNames.Length -1))
                    newTable[contentEnumerator.Key] = CopyChildren(contentEnumerator.Value, tagIndex + 1);
                else
                    newTable[contentEnumerator.Key] = contentEnumerator.Value;
            }

            return newTable;
        }

        private ResourcePermissionBase CreateInstance() {
            // SECREVIEW: Here we are using reflection to create an instance of the current
            // type (which is a subclass of ResourcePermissionBase).
            new PermissionSet(PermissionState.Unrestricted).Assert();
            return (ResourcePermissionBase)Activator.CreateInstance(this.GetType(), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, null, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected ResourcePermissionBaseEntry[] GetPermissionEntries() {
            return GetChildrenAccess(this.rootTable, 0);
        }

        private ResourcePermissionBaseEntry[] GetChildrenAccess(object currentContent, int tagIndex) {
            IDictionaryEnumerator contentEnumerator = ((Hashtable)currentContent).GetEnumerator();
            ArrayList list = new ArrayList();
            while(contentEnumerator.MoveNext()) {
                if (tagIndex < (this.TagNames.Length -1)) {
                    ResourcePermissionBaseEntry[] currentEntries = GetChildrenAccess(contentEnumerator.Value, tagIndex + 1);
                    for (int index = 0; index < currentEntries.Length; ++index)
                        currentEntries[index].PermissionAccessPath[tagIndex] = (string)contentEnumerator.Key;

                     list.AddRange(currentEntries);
                }
                else {
                    ResourcePermissionBaseEntry entry = new ResourcePermissionBaseEntry((int)contentEnumerator.Value, new string[this.TagNames.Length]);
                    entry.PermissionAccessPath[tagIndex] = (string)contentEnumerator.Key;

                    list.Add(entry);
                }
            }

            return (ResourcePermissionBaseEntry[])list.ToArray(typeof(ResourcePermissionBaseEntry));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void FromXml(SecurityElement securityElement) {
            if (securityElement == null)
                throw new ArgumentNullException("securityElement");
            
            if (!securityElement.Tag.Equals ("Permission") && !securityElement.Tag.Equals ("IPermission"))
                throw new ArgumentException(SR.GetString(SR.Argument_NotAPermissionElement));

            String version = securityElement.Attribute( "version" );
            if (version != null && !version.Equals( "1" ))
                throw new ArgumentException(SR.GetString(SR.Argument_InvalidXMLBadVersion));

            string unrestrictedValue = securityElement.Attribute("Unrestricted");
            if (unrestrictedValue != null && (string.Compare(unrestrictedValue, "true", StringComparison.OrdinalIgnoreCase) == 0)) {
                this.isUnrestricted = true;
                return;
            }
            else
                isUnrestricted = false;
                

            this.rootTable = (Hashtable)ReadChildren(securityElement, 0);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override IPermission Intersect(IPermission target) {
            if (target == null)
                return null;

            if (target.GetType() != this.GetType())
                throw new ArgumentException(SR.GetString(SR.PermissionTypeMismatch), "target");
                       
            ResourcePermissionBase targetPermission = (ResourcePermissionBase)target;            
            if (this.IsUnrestricted())
                return targetPermission.Copy();

            if (targetPermission.IsUnrestricted())
                return this.Copy();

            ResourcePermissionBase newPermission = null;
            Hashtable newPermissionRootTable = (Hashtable)IntersectContents(this.rootTable, targetPermission.rootTable);
            if (newPermissionRootTable != null) {
                newPermission = CreateInstance();
                newPermission.rootTable = newPermissionRootTable;
            }
            return newPermission;
        }

        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private object IntersectContents(object currentContent, object targetContent) {
            if (currentContent is int) {
                int currentAccess = (int)currentContent;
                int targetAccess = (int)targetContent;
                return  (currentAccess & targetAccess);
            }
            else {
                Hashtable newContents = CreateHashtable();

                //Before executing the intersect operation, need to
                //resolve the "." entries
                object currentLocalContent = ((Hashtable)currentContent)[Local];
                object currentComputerNameContent = ((Hashtable)currentContent)[ComputerName];
                if (currentLocalContent != null || currentComputerNameContent != null) {
                    object targetLocalContent = ((Hashtable)targetContent)[Local];
                    object targetComputerNameContent = ((Hashtable)targetContent)[ComputerName];
                    if (targetLocalContent != null || targetComputerNameContent != null) {
                        object currentLocalMergedContent = currentLocalContent;
                        if (currentLocalContent != null && currentComputerNameContent != null)
                            currentLocalMergedContent = UnionOfContents(currentLocalContent, currentComputerNameContent);
                        else if (currentComputerNameContent != null)
                            currentLocalMergedContent = currentComputerNameContent;

                        object targetLocalMergedContent = targetLocalContent;
                        if (targetLocalContent != null && targetComputerNameContent != null)
                            targetLocalMergedContent = UnionOfContents(targetLocalContent, targetComputerNameContent);
                        else if (targetComputerNameContent != null)
                            targetLocalMergedContent = targetComputerNameContent;

                        object computerNameValue = IntersectContents(currentLocalMergedContent, targetLocalMergedContent);
                        if (HasContent(computerNameValue)) {
                            // There should be no computer name key added if the information
                            // was not specified in one of the targets
                            if (currentComputerNameContent != null || targetComputerNameContent != null) {
                                newContents[ComputerName] = computerNameValue;
                            }
                            else {
                                newContents[Local] = computerNameValue;
                            }
                        }
                    }
                }

                IDictionaryEnumerator contentEnumerator;
                Hashtable contentsTable;
                if (((Hashtable)currentContent).Count <  ((Hashtable)targetContent).Count) {
                    contentEnumerator = ((Hashtable)currentContent).GetEnumerator();
                    contentsTable = ((Hashtable)targetContent);
                }
                else{
                    contentEnumerator = ((Hashtable)targetContent).GetEnumerator();
                    contentsTable = ((Hashtable)currentContent);
                }

                //The wildcard entries intersection should be treated
                //as any other entry.
                while(contentEnumerator.MoveNext()) {
                    string currentKey = (string)contentEnumerator.Key;
                    if (contentsTable.ContainsKey(currentKey) &&
                          currentKey != Local &&
                          currentKey != ComputerName)  {

                        object currentValue = contentEnumerator.Value;
                        object targetValue = contentsTable[currentKey];
                        object newValue = IntersectContents(currentValue, targetValue);
                        if (HasContent(newValue))
                            newContents[currentKey] = newValue;
                    }
                }

                return (newContents.Count > 0) ? newContents : null;
            }
        }

        // This is used from IntersectContents.  IntersectContents can return either a hashtable or
        // an int.  If the hashtable is null or the int is 0, we don't want to save those values - 
        // ie the intersection was empty.  This checks for null and a zero int value. 
        private bool HasContent(object value) {
            if (value == null) 
                return false;

            if (value is int) {
                int intValue = (int)value;
                return (intValue != 0);
            }
            else {
                Hashtable table = (Hashtable)value;
                IDictionaryEnumerator tableEnumerator = table.GetEnumerator();
                while (tableEnumerator.MoveNext()) {
                    if (HasContent(tableEnumerator.Value))
                        return true;
                }
                return false;
            }
        }
        
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        private bool IsContentSubset(object currentContent, object targetContent) {
            //
            // The content of the permission is a two level hashtable.
            // The first level is indexed by the machine name and the value is another Hashtable.
            // The second level Hashtable is indexed by category name and the value are integers.
            // The integers represent the access right.
            //
            // 


            if (currentContent is int) {
                int currentAccess = (int)currentContent;
                int targetAccess = (int)targetContent;
                if ((currentAccess & targetAccess) != currentAccess)
                    return false;

                return true;
            }
            else {
                Hashtable currentContentTable = (Hashtable)currentContent;                
                Hashtable targetContentTable = (Hashtable)targetContent;                   
                    
                //If the target table contains a wild card, all the current entries need to be
                //a subset of the target.
                object targetAnyContent = targetContentTable[Any];
                if (targetAnyContent != null) {
                    foreach(DictionaryEntry currentEntry in currentContentTable) {
                        if (!IsContentSubset(currentEntry.Value, targetAnyContent)) {
                            return false;                        
                        }
                    }                    
                    return true;
                }

                //Check the entries for remote machines first
                foreach(DictionaryEntry currentEntry in currentContentTable) {
                    string currentContentKey = (string)currentEntry.Key;
                    // only look for a subset if there's actually some content
                    if (HasContent(currentEntry.Value)) {
                        if (currentContentKey != Local && currentContentKey != ComputerName) {
                            if (!targetContentTable.ContainsKey(currentContentKey)) {
                                return false;
                            }
                            else if (!IsContentSubset(currentEntry.Value, targetContentTable[currentContentKey])) {
                                return false;
                            }
                        }
                    }
                }
        
                // Entries for "." and local machine name apply to the same target.
                // Merge them before further processing.
                object currentLocalMergedContent = MergeContents(currentContentTable[Local], currentContentTable[ComputerName]);
                if (currentLocalMergedContent != null ) {
                    object targetLocalMergedContent  = MergeContents(targetContentTable[Local], targetContentTable[ComputerName]);
                    if (targetLocalMergedContent != null) {
                        return IsContentSubset(currentLocalMergedContent, targetLocalMergedContent); 
                    }
                    else if (!IsEmpty) {
                        return false;
                    }
                }
                return true;
            }
        }

        private object MergeContents( object content1, object content2) {
            if (content1 == null) {            
                if( content2 == null) {
                    return null;
                }
                else {
                    return content2;
                }
            }
            else {
                if( content2 == null) {
                    return content1;
                }
                else {
                    return UnionOfContents(content1, content2);
                }
            }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool IsSubsetOf(IPermission target) {
            if (target == null) {
                return (IsEmpty);
            }

            if (target.GetType() != this.GetType())
                return false;

            ResourcePermissionBase targetPermission = (ResourcePermissionBase)target;
            if (targetPermission.IsUnrestricted())
                return true;
            else if (this.IsUnrestricted())
                return false;

            return IsContentSubset(this.rootTable, targetPermission.rootTable);

        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsUnrestricted() {
            return this.isUnrestricted;
        }

        private object ReadChildren(SecurityElement securityElement, int tagIndex) {
            Hashtable newTable = CreateHashtable();
            if (securityElement.Children != null) {
                for (int index = 0; index < securityElement.Children.Count; ++ index) {
                    SecurityElement currentElement =  (SecurityElement)securityElement.Children[index];
                    if (currentElement.Tag == this.TagNames[tagIndex]) {
                        string contentName = currentElement.Attribute("name");

                        if (tagIndex < (this.TagNames.Length -1))
                            newTable[contentName] = ReadChildren(currentElement, tagIndex +1);
                        else {
                            string accessString = currentElement.Attribute("access");
                            int permissionAccess = 0;
                            if (accessString != null) {
                                permissionAccess = (int) Enum.Parse(PermissionAccessType, accessString);
                            }
                            newTable[contentName] = permissionAccess;
                        }
                    }
                }
            }
            return newTable;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void RemovePermissionAccess(ResourcePermissionBaseEntry entry) {
            if (entry == null)
                throw new ArgumentNullException("entry");

            if (entry.PermissionAccessPath.Length != this.TagNames.Length)
                throw new InvalidOperationException(SR.GetString(SR.PermissionNumberOfElements));

            Hashtable currentTable = this.rootTable;
            string[] accessPath = entry.PermissionAccessPath;
            for (int index = 0; index < accessPath.Length; ++ index) {
                if (currentTable == null || !currentTable.ContainsKey(accessPath[index]))
                    throw new InvalidOperationException(SR.GetString(SR.PermissionItemDoesntExist));
                else {
                    Hashtable oldTable = currentTable;
                    if (index < accessPath.Length - 1) {
                        currentTable = (Hashtable)currentTable[accessPath[index]];
                        if (currentTable.Count == 1)
                            oldTable.Remove(accessPath[index]);
                    }
                    else {
                        currentTable = null;
                        oldTable.Remove(accessPath[index]);
                    }
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override SecurityElement ToXml() {
            SecurityElement root = new SecurityElement("IPermission");
            Type type = this.GetType();
            root.AddAttribute("class", type.FullName + ", " + type.Module.Assembly.FullName.Replace('\"', '\''));
            root.AddAttribute("version", "1");

            if (this.isUnrestricted) {
                root.AddAttribute("Unrestricted", "true");
                return root;
            }

            WriteChildren(root, this.rootTable, 0);
            return root;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override IPermission Union(IPermission target) {
            if (target == null)
                return this.Copy();
        
            if (target.GetType() != this.GetType())                                                 
                throw new ArgumentException(SR.GetString(SR.PermissionTypeMismatch), "target");
                        
            ResourcePermissionBase targetPermission = (ResourcePermissionBase)target;                                        
            ResourcePermissionBase newPermission = null;
            if (this.IsUnrestricted() || targetPermission.IsUnrestricted()) {
                newPermission = CreateInstance();
                newPermission.isUnrestricted = true;
            }
            else {
                Hashtable newPermissionRootTable = (Hashtable)UnionOfContents(this.rootTable, targetPermission.rootTable);
                if (newPermissionRootTable != null) {
                    newPermission = CreateInstance();
                    newPermission.rootTable = newPermissionRootTable;
                }
            }
            return newPermission;
        }

        private object UnionOfContents(object currentContent, object targetContent) {
            if (currentContent is int) {
                int currentAccess = (int)currentContent;
                int targetAccess = (int)targetContent;
                return (currentAccess | targetAccess);
            }
            else {
                //The wildcard and "." entries can be merged as
                //any other entry.
                Hashtable newContents = CreateHashtable();
                IDictionaryEnumerator contentEnumerator = ((Hashtable)currentContent).GetEnumerator();
                IDictionaryEnumerator targetContentEnumerator = ((Hashtable)targetContent).GetEnumerator();
                while(contentEnumerator.MoveNext())
                    newContents[(string)contentEnumerator.Key] = contentEnumerator.Value;

                while(targetContentEnumerator.MoveNext()) {
                    if (!newContents.ContainsKey(targetContentEnumerator.Key))
                        newContents[targetContentEnumerator.Key] = targetContentEnumerator.Value;
                    else {
                        object currentValue = newContents[targetContentEnumerator.Key];
                        object targetValue =targetContentEnumerator.Value;
                        newContents[targetContentEnumerator.Key] = UnionOfContents(currentValue, targetValue);
                    }
                }

                return (newContents.Count > 0) ? newContents : null;
            }
        }

        private void WriteChildren(SecurityElement currentElement, object currentContent, int tagIndex) {
            IDictionaryEnumerator contentEnumerator = ((Hashtable)currentContent).GetEnumerator();
            while(contentEnumerator.MoveNext()) {
                SecurityElement contentElement = new SecurityElement(this.TagNames[tagIndex]);
                currentElement.AddChild(contentElement);
                contentElement.AddAttribute("name", (string)contentEnumerator.Key);

                if (tagIndex < (this.TagNames.Length -1))
                    WriteChildren(contentElement, contentEnumerator.Value, tagIndex + 1);
                else {
                    String accessString = null;
                    int currentAccess = (int)contentEnumerator.Value;
                    if (this.PermissionAccessType != null && currentAccess != 0) {
                        accessString = Enum.Format(PermissionAccessType, currentAccess, "g");
                        contentElement.AddAttribute("access", accessString);
                    }
                }
            }
        }

        [SuppressUnmanagedCodeSecurity()]
        private static class UnsafeNativeMethods {
            [DllImport(ExternDll.Kernel32, CharSet=CharSet.Auto, BestFitMapping=false)]
            [ResourceExposure(ResourceScope.Machine)]
            internal static extern bool GetComputerName(StringBuilder lpBuffer, ref int nSize);
        }
    }
}

