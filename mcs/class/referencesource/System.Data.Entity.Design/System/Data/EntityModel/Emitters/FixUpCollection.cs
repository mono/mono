//---------------------------------------------------------------------
// <copyright file="FixUpCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Data;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Entity.Design;

namespace System.Data.EntityModel.Emitters
{
    internal sealed class FixUpCollection : List<FixUp>
    {
        #region Private Types
        private enum CSDeclType
        {
            Method,
            Property,
            Other,
        }
        public enum VBStatementType
        {
            BeginClass,
            EndClass,
            BeginProperty,
            EndProperty,
            BeginMethod,
            EndMethod,
            BeginPropertyGetter,
            EndPropertyGetter,
            BeginPropertySetter,
            EndPropertySetter,
            Other,
        }
        #endregion

        #region Instance Fields
        private Dictionary<string,List<FixUp>> _classFixUps = null;
        private LanguageOption _language;
        #endregion

        #region Static Fields
        static readonly char[] _CSEndOfClassDelimiters = new char[] { ' ',':' };
        const string _CSClassKeyWord = " class ";
        static readonly char[] _CSFieldMarkers = new char[] { '=',';' };
        static readonly char[] _VBEndOfClassDelimiters = new char[] { ' ', '(' };
        static readonly char[] _VBNonDeclMarkers = new char[] { '=', '"', '\'' };
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        public FixUpCollection()
        {
        }

        public static bool IsLanguageSupported(LanguageOption language)
        {
            switch ( language )
            {
                case LanguageOption.GenerateVBCode:
                case LanguageOption.GenerateCSharpCode:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        /// <param name="language"></param>
        public void Do(System.IO.TextReader reader, System.IO.TextWriter writer, LanguageOption language, bool hasNamespace)
        {
            Language = language;

            // set up the fix ups for each class.
            foreach ( FixUp fixUp in this )
            {
                List<FixUp> fixUps = null;
                if ( ClassFixUps.ContainsKey(fixUp.Class) )
                {
                    fixUps = ClassFixUps[fixUp.Class];
                }
                else
                {
                    fixUps = new List<FixUp>();
                    ClassFixUps.Add(fixUp.Class,fixUps);
                }
                fixUps.Add(fixUp);
            }

            switch ( Language )
            {
                case LanguageOption.GenerateVBCode:
                    DoFixUpsForVB(reader, writer);
                    break;
                case LanguageOption.GenerateCSharpCode:
                    DoFixUpsForCS(reader, writer, hasNamespace);
                    break;
                default:
                    Debug.Assert(false,"Unexpected language value: "+Language.ToString());
                    CopyFile(reader,writer);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        private static void CopyFile(System.IO.TextReader reader, System.IO.TextWriter writer)
        {
            string line;
            while ( (line=reader.ReadLine()) != null )
                writer.WriteLine(line);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        private void DoFixUpsForCS(System.IO.TextReader reader, System.IO.TextWriter writer, bool hasNamespace)
        {
            int braceCount = 0;
            string line;
            string trimmedLine;
            string currentOuterClass = null;
            string className;
            bool classWanted = false;
            FixUp getterFixUp = null;
            FixUp setterFixUp = null;
            int nameSpaceLevel = hasNamespace ? 1 : 0;
            while ( (line=reader.ReadLine()) != null )
            {
                trimmedLine = line.Trim();
                if ( trimmedLine == "{" )
                    ++braceCount;
                else if ( trimmedLine == "}" )
                {
                    --braceCount;
                    if (braceCount < nameSpaceLevel + 2)
                    {
                        setterFixUp = null;
                        if (braceCount < nameSpaceLevel + 1)
                        {
                            currentOuterClass = null;
                            classWanted = false;
                        }
                    }
                }
                else if ( string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//",StringComparison.Ordinal) )
                {
                    // comment, just emit as is....
                }
                else if ( IsCSClassDefinition(line,out className) )
                {
                    if (braceCount == nameSpaceLevel)
                    {
                        currentOuterClass = className;
                        className = null;
                        classWanted = IsClassWanted(currentOuterClass);
                        if ( classWanted )
                            line = FixUpClassDecl(currentOuterClass,line);
                    }
                }
                else if ( classWanted )
                {
                    //we only care about methods/properties in top level classes
                    if (braceCount == nameSpaceLevel + 1)
                    {
                        string name;
                        switch ( GetCSDeclType(trimmedLine,out name) )
                        {
                            case CSDeclType.Method:
                                line = FixUpMethodDecl(currentOuterClass,name,line);
                                break;
                            case CSDeclType.Property:
                                setterFixUp = FixUpSetter(currentOuterClass, name);
                                getterFixUp = FixUpGetter(currentOuterClass, name);
                                break;
                        }
                    }
                    else if (braceCount == nameSpaceLevel + 2)
                    {
                        if (trimmedLine == "set" && setterFixUp != null)
                        {
                            line = setterFixUp.Fix(LanguageOption.GenerateCSharpCode, line);
                            setterFixUp = null;
                        }
                        else if (trimmedLine == "get" && getterFixUp != null)
                        {
                            line = getterFixUp.Fix(LanguageOption.GenerateCSharpCode, line);
                            getterFixUp = null;
                        }
                    }
                }
                writer.WriteLine(line);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="writer"></param>
        public void DoFixUpsForVB(System.IO.TextReader reader, System.IO.TextWriter writer)
        {
            Language = LanguageOption.GenerateVBCode;

            string line;
            Stack<VBStatementType> context = new Stack<VBStatementType>();
            int classDepth = 0;
            string currentOuterClass = null;
            bool classWanted = false;
            FixUp getterFixUp = null;
            FixUp setterFixUp = null;
            while ( (line=reader.ReadLine()) != null )
            {
                if ( line == null || line.Length == 0 || line[0] == '\'' )
                {
                    // empty line or comment, ouput as is
                }
                else
                {
                    string name;
                    switch ( GetVBStatementType(context, line, out name) )
                    {
                        case VBStatementType.BeginClass:
                            ++classDepth;
                            setterFixUp = null;
                            if ( classDepth == 1 )
                            {
                                currentOuterClass = name;
                                classWanted = IsClassWanted(name);
                                if ( classWanted )
                                    line = FixUpClassDecl(currentOuterClass, line);
                            }
                            break;

                        case VBStatementType.EndClass:
                            --classDepth;
                            if (classDepth == 0)
                            {
                                currentOuterClass = null;
                            }
                            break;

                        case VBStatementType.BeginProperty:
                            if (classWanted)
                            {
                                getterFixUp = FixUpGetter(currentOuterClass, name);
                                setterFixUp = FixUpSetter(currentOuterClass, name);
                            }
                            else
                            {
                                getterFixUp = null;
                                setterFixUp = null;
                            }
                            break;

                        case VBStatementType.EndProperty:
                            getterFixUp = null;
                            setterFixUp = null;
                            break;

                        case VBStatementType.BeginMethod:
                            if (classWanted)
                            {
                                line = FixUpMethodDecl(currentOuterClass, name, line);
                            }
                            break;

                        case VBStatementType.BeginPropertySetter:
                            if (setterFixUp != null)
                            {
                                line = setterFixUp.Fix(Language, line);
                            }
                            setterFixUp = null;
                            break;

                        case VBStatementType.BeginPropertyGetter:
                            if (getterFixUp != null)
                            {
                                line = getterFixUp.Fix(Language, line);
                            }
                            getterFixUp = null;
                            break;
                    }
                }
                writer.WriteLine(line);
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private bool IsClassWanted(string className)
        {
            return ClassFixUps.ContainsKey(className);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private static bool IsCSClassDefinition(string line,out string className)
        {
            int index = line.IndexOf(_CSClassKeyWord,StringComparison.Ordinal);
            if ( index < 0 )
            {
                className = null;
                return false;
            }
            index += _CSClassKeyWord.Length;
            int end = line.IndexOfAny(_CSEndOfClassDelimiters, index);
            if ( end < 0 )
                className = line.Substring(index);
            else
                className = line.Substring(index,end-index);


            if (className.StartsWith("@", StringComparison.Ordinal))
            {
                // remove the escaping mechanisim for C# keywords
                className = className.Substring(1);
            }
            
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private string FixUpClassDecl(string className,string line)
        {
            IList<FixUp> fixUps = ClassFixUps[className];
            foreach ( FixUp fixUp in fixUps )
            {
                if ( fixUp.Type == FixUpType.MarkClassAsStatic )
                {
                    return fixUp.Fix(Language,line);
                }
            }
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static CSDeclType GetCSDeclType(string line, out string name)
        {
            // we know we're at the class member level.
            // things we could encounter are (limited to things we actually emit):
            //    nested classes (already identified)
            //    attributes
            //    fields
            //    methods
            //    properties

            name = null;

            //Attributes
            if (line[0] == '[')
                return CSDeclType.Other;

            // Methods have ( and ) without a =
            int parIdx1 = line.IndexOf('(');
            int parIdx2 = line.IndexOf(')');
            int equIdx = line.IndexOf('='); //return -1 for absent equal sign.

            if (equIdx == -1 && parIdx1 >= 0 && parIdx2 > parIdx1)
            {
                line = line.Substring(0, parIdx1).TrimEnd(null);
                name = line.Substring(line.LastIndexOf(' ') + 1);
                return CSDeclType.Method;
            }

            //we assume fields have = or ;
            if (line.IndexOfAny(_CSFieldMarkers, 0) >= 0)
                return CSDeclType.Other;

            //Properties
            CSDeclType declType = CSDeclType.Property;
            name = line.Substring(line.LastIndexOf(' ') + 1);
            return declType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="methodName"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        private string FixUpMethodDecl(string className,string methodName,string line)
        {
            IList<FixUp> fixUps = ClassFixUps[className];
            foreach ( FixUp fixUp in fixUps )
            {
                if ( fixUp.Method == methodName && 
                    (fixUp.Type == FixUpType.MarkOverrideMethodAsSealed || fixUp.Type == FixUpType.MarkAbstractMethodAsPartial) )
                {
                    return fixUp.Fix(Language,line);
                }
            }
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private FixUp FixUpSetter(string className,string propertyName)
        {
            IList<FixUp> fixUps = ClassFixUps[className];
            foreach ( FixUp fixUp in fixUps )
            {
                if (fixUp.Property == propertyName &&
                    (fixUp.Type == FixUpType.MarkPropertySetAsPrivate ||
                     fixUp.Type == FixUpType.MarkPropertySetAsInternal ||
                     fixUp.Type == FixUpType.MarkPropertySetAsPublic ||
                     fixUp.Type == FixUpType.MarkPropertySetAsProtected))
                {
                    return fixUp;
                }
            }
            return null;
        }

        private FixUp FixUpGetter(string className, string propertyName)
        {
            IList<FixUp> fixUps = ClassFixUps[className];
            foreach (FixUp fixUp in fixUps)
            {
                if (fixUp.Property == propertyName && 
                    (fixUp.Type == FixUpType.MarkPropertyGetAsPrivate ||
                     fixUp.Type == FixUpType.MarkPropertyGetAsInternal ||
                     fixUp.Type == FixUpType.MarkPropertyGetAsPublic ||
                     fixUp.Type == FixUpType.MarkPropertyGetAsProtected))
                {
                    return fixUp;
                }
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="line"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static VBStatementType GetVBStatementType(Stack<VBStatementType> context, string line, out string name)
        {
            name = null;
            VBStatementType current = VBStatementType.Other;

            // if the statement constains ", =, or... then it's not a statement type we care about
            if ( line.IndexOfAny(_VBNonDeclMarkers) >= 0 )
                return current;

            
            string normalizedLine = NormalizeForVB(line);

            if ( context.Count <= 0 )
            {
                // without context we only accept BeginClass
                if ( LineIsVBBeginClassMethodProperty(normalizedLine, "Class", ref name) )
                {
                    current = VBStatementType.BeginClass;
                    context.Push(current);
                }
            }
            else
            {
                // we only look for things based on context:
                switch ( context.Peek() )
                {
                    // at BeginClass we only accept 
                    //    BeginClass
                    //    EndClass
                    //    BeginProperty
                    //    BeginMethod
                    case VBStatementType.BeginClass:
                        if ( normalizedLine == "End Class" )
                        {
                            current = VBStatementType.EndClass;
                            context.Pop();
                        }
                        else
                        {
                            if ( LineIsVBBeginClassMethodProperty(normalizedLine, "Class", ref name) )
                            {
                                current = VBStatementType.BeginClass;
                                context.Push(current);
                            }
                            else if ( LineIsVBBeginClassMethodProperty(normalizedLine, "MustOverride Sub", ref name) )
                            {
                                // Abstract methods do not have an "End Sub", this don't push the context.
                                current = VBStatementType.BeginMethod;
                            }
                            else if ( LineIsVBBeginClassMethodProperty(normalizedLine, "Function", ref name) 
                                || LineIsVBBeginClassMethodProperty(normalizedLine, "Sub", ref name) )
                            {
                                current = VBStatementType.BeginMethod;
                                context.Push(current);
                            }
                            else if ( LineIsVBBeginClassMethodProperty(normalizedLine, "Property", ref name) )
                            {
                                current = VBStatementType.BeginProperty;
                                context.Push(current);
                            }
                        }
                        break;

                    // at BeginProperty we only accept
                    //    EndProperty
                    //    BeginPropertyGetter
                    //    BeginPropertySetter
                    case VBStatementType.BeginProperty:
                        if ( normalizedLine == "End Property" )
                        {
                            current = VBStatementType.EndProperty;
                            context.Pop();
                        }
                        else
                        {
                            if ( LineIsVBBeginSetterGetter(normalizedLine, "Get") )
                            {
                                current = VBStatementType.BeginPropertyGetter;
                                context.Push(current);
                            }
                            else if ( LineIsVBBeginSetterGetter(normalizedLine, "Set") )
                            {
                                current = VBStatementType.BeginPropertySetter;
                                context.Push(current);
                            }
                        }
                        break;

                    // at BeginMethod we only accept
                    //    EndMethod
                    case VBStatementType.BeginMethod:
                        if ( normalizedLine == "End Sub" || normalizedLine == "End Function" )
                        {
                            current = VBStatementType.EndMethod;
                            context.Pop();
                        }
                        break;

                    // at BeginPropertyGetter we only accept
                    //    EndPropertyGetter
                    case VBStatementType.BeginPropertyGetter:
                        if ( normalizedLine == "End Get" )
                        {
                            current = VBStatementType.EndPropertyGetter;
                            context.Pop();
                        }
                        break;

                    // at BeginPropertySetter we only accept
                    //    EndPropertySetter
                    case VBStatementType.BeginPropertySetter:
                        if ( normalizedLine == "End Set" )
                        {
                            current = VBStatementType.EndPropertySetter;
                            context.Pop();
                        }
                        break;
                }
            }

            return current;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string NormalizeForVB(string line)
        {
            // no leading or trailing spaces and tabs are replaced with spaces
            line = line.Replace('\t', ' ').Trim();

            // consecutuve spaces are replaced with single spaces...
            // (we don't care about hammering strings; we just use the normalized line for statment identification...
            while ( line.IndexOf("  ", 0,StringComparison.Ordinal) >= 0 )
                line = line.Replace("  ", " ");

            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private static bool LineIsVBBeginSetterGetter(string line, string keyword)
        {
            return IndexOfKeyword(line, keyword) >= 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="keyword"></param>
        /// <returns></returns>
        private static int IndexOfKeyword(string line, string keyword)
        {
            int index = line.IndexOf(keyword,StringComparison.Ordinal);
            if ( index < 0 )
                return index;

            char ch;
            int indexAfter = index+keyword.Length;
            if ( (index == 0 || char.IsWhiteSpace(line, index-1)) && (indexAfter == line.Length || (ch=line[indexAfter]) == '(' || char.IsWhiteSpace(ch)) )
                return index;

            return -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="keyword"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool LineIsVBBeginClassMethodProperty(string line, string keyword, ref string name)
        {
            // line must contain the keyword
            int index = IndexOfKeyword(line, keyword);
            if ( index < 0 )
                return false;

            // after the keyword we expact a space and the name
            index += keyword.Length;
            if ( index >= line.Length || !char.IsWhiteSpace(line, index) )
                return false;
            ++index;
            if ( index >= line.Length )
                return false;

            // after the name we expect a EOL or a delimiter...
            int end = line.IndexOfAny(_VBEndOfClassDelimiters, index);
            if ( end < 0 )
                end = line.Length;

            name = line.Substring(index, end-index).Trim();

            if (name.StartsWith("[", StringComparison.Ordinal) && name.EndsWith("]", StringComparison.Ordinal))
            {
                // remove the vb keyword escaping mechanisim
                name = name.Substring(1, name.Length - 2);
            }
            
            return true;
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// 
        /// </summary>
        private LanguageOption Language
        {
            get
            {
                return _language;
            }
            set
            {
                _language = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        private Dictionary<string,List<FixUp>> ClassFixUps
        {
            get
            {
                if ( _classFixUps == null )
                {
                    _classFixUps = new Dictionary<string,List<FixUp>>();
                }

                return _classFixUps;
            }
        }
        #endregion
    }
}
