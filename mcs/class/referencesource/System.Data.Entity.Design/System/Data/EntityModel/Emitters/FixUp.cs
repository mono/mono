//---------------------------------------------------------------------
// <copyright file="FixUp.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Entity.Design;

namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class FixUp
    {
        #region Internal Property
        internal delegate string FixMethod(string line);
        #endregion

        #region Instance Fields
        FixUpType m_type;
        private string _class = null;
        private string _method = null;
        private string _property = null;
        #endregion

        #region static
        private static readonly FixMethod[] _CSFixMethods = new FixMethod[] 
        {
            null,
            new FixMethod(CSMarkOverrideMethodAsSealed),
            new FixMethod(CSMarkPropertySetAsInternal),
            new FixMethod(CSMarkClassAsStatic),
            new FixMethod(CSMarkPropertyGetAsPrivate),
            new FixMethod(CSMarkPropertyGetAsInternal),
            new FixMethod(CSMarkPropertyGetAsPublic),
            new FixMethod(CSMarkPropertySetAsPrivate),
            new FixMethod(CSMarkPropertySetAsPublic),
            new FixMethod(CSMarkMethodAsPartial),
            new FixMethod(CSMarkPropertyGetAsProtected),
            new FixMethod(CSMarkPropertySetAsProtected)
        };

        private static readonly FixMethod[] _VBFixMethods = new FixMethod[] 
        {
            null,
            new FixMethod(VBMarkOverrideMethodAsSealed),
            new FixMethod(VBMarkPropertySetAsInternal),
            null, // VB doesn't support static classes (during CodeGen we added a private ctor to the class)
            new FixMethod(VBMarkPropertyGetAsPrivate),
            new FixMethod(VBMarkPropertyGetAsInternal),
            new FixMethod(VBMarkPropertyGetAsPublic),
            new FixMethod(VBMarkPropertySetAsPrivate),
            new FixMethod(VBMarkPropertySetAsPublic),
            new FixMethod(VBMarkMethodAsPartial),
            new FixMethod(VBMarkPropertyGetAsProtected),
            new FixMethod(VBMarkPropertySetAsProtected)
        };
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fqName"></param>
        /// <param name="type"></param>
        public FixUp(string fqName,FixUpType type)
        {
            Type = type;
            string[] nameParts = Utils.SplitName(fqName);
            if ( type == FixUpType.MarkClassAsStatic )
            {
                Class = nameParts[nameParts.Length-1];
            }
            else
            {
                Class = nameParts[nameParts.Length-2];
                string name = nameParts[nameParts.Length-1];
                switch ( type )
                {
                    case FixUpType.MarkAbstractMethodAsPartial:
                    case FixUpType.MarkOverrideMethodAsSealed:
                        Method = name;
                        break;
                    case FixUpType.MarkPropertyGetAsPrivate:
                    case FixUpType.MarkPropertyGetAsInternal:
                    case FixUpType.MarkPropertyGetAsPublic:
                    case FixUpType.MarkPropertyGetAsProtected:
                    case FixUpType.MarkPropertySetAsPrivate:
                    case FixUpType.MarkPropertySetAsInternal:
                    case FixUpType.MarkPropertySetAsPublic:
                    case FixUpType.MarkPropertySetAsProtected:
                        Property = name;
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="language"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public string Fix(LanguageOption language, string line)
        {
            FixMethod method = null;
            if ( language == LanguageOption.GenerateCSharpCode )
            {
                method = _CSFixMethods[(int)Type];
            }
            else if ( language == LanguageOption.GenerateVBCode )
            {
                method = _VBFixMethods[(int)Type];
            }

            if ( method != null )
            {
                line = method( line );
            }

            return line;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Class
        {
            get
            {
                return _class;
            }
            private set
            {
                _class = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Property
        {
            get
            {
                return _property;
            }
            private set
            {
                _property = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Method
        {
            get
            {
                return _method;
            }
            private set
            {
                _method = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public FixUpType Type
        {
            get
            {
                return m_type;
            }
            private set
            {
                m_type = value;
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string CSMarkMethodAsPartial(string line)
        {
            line = ReplaceFirst(line, "public abstract", "partial");
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string VBMarkMethodAsPartial(string line)
        {
            line = ReplaceFirst(line, "Public MustOverride", "Partial Private");
            line += Environment.NewLine + "        End Sub";
            return line;
        }

        private static string ReplaceFirst(string line, string str1, string str2)
        {
            int idx = line.IndexOf(str1, StringComparison.Ordinal);
            if (idx >= 0)
            {
                line = line.Remove(idx, str1.Length);
                line = line.Insert(idx, str2);
            }
            return line;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string CSMarkOverrideMethodAsSealed(string line)
        {
            return InsertBefore(line,"override","sealed");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string VBMarkOverrideMethodAsSealed(string line)
        {
            return InsertBefore(line, "Overrides", "NotOverridable");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string CSMarkPropertySetAsInternal(string line)
        {
            return InsertBefore(line,"set","internal");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string VBMarkPropertySetAsInternal(string line)
        {
            return InsertBefore(line,"Set","Friend");
        }


        private static string CSMarkPropertyGetAsPrivate(string line)
        {
            return InsertBefore(line, "get", "private");
        }

        private static string VBMarkPropertyGetAsPrivate(string line)
        {
            return InsertBefore(line, "Get", "Private");
        }


        private static string CSMarkPropertyGetAsInternal(string line)
        {
            return InsertBefore(line, "get", "internal");
        }

        private static string VBMarkPropertyGetAsInternal(string line)
        {
            return InsertBefore(line, "Get", "Friend");
        }

        private static string CSMarkPropertySetAsProtected(string line)
        {
            return InsertBefore(line, "set", "protected");
        }

        private static string VBMarkPropertySetAsProtected(string line)
        {
            return InsertBefore(line, "Set", "Protected");
        }

        private static string CSMarkPropertyGetAsProtected(string line)
        {
            return InsertBefore(line, "get", "protected");
        }

        private static string VBMarkPropertyGetAsProtected(string line)
        {
            return InsertBefore(line, "Get", "Protected");
        }

        private static string CSMarkPropertyGetAsPublic(string line)
        {
            return InsertBefore(line, "get", "public");
        }

        private static string VBMarkPropertyGetAsPublic(string line)
        {
            return InsertBefore(line, "Get", "Public");
        }


        private static string CSMarkPropertySetAsPrivate(string line)
        {
            return InsertBefore(line, "set", "private");
        }

        private static string VBMarkPropertySetAsPrivate(string line)
        {
            return InsertBefore(line, "Set", "Private");
        }


        private static string CSMarkPropertySetAsPublic(string line)
        {
            return InsertBefore(line, "set", "public");
        }

        private static string VBMarkPropertySetAsPublic(string line)
        {
            return InsertBefore(line, "Set", "Public");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private static string CSMarkClassAsStatic(string line)
        {
            if ( IndexOfKeyword(line,"static") >= 0 )
                return line;

            int insertPoint = IndexOfKeyword(line,"class");
            if ( insertPoint < 0 )
                return line;

            // nothing can be between partial and class
            int partialIndex = IndexOfKeyword(line,"partial");
            if ( partialIndex >= 0 )
                insertPoint = partialIndex;

            return line.Insert(insertPoint,"static ");
        }

        /// <summary>
        /// Inserts one keyword before another one.
        /// Does nothing if the keyword to be inserted already exists in the line OR if the keyword to insert before doesn't
        /// </summary>
        /// <param name="line">line of text to examine</param>
        /// <param name="searchText">keyword to search for </param>
        /// <param name="insertText">keyword to be inserted</param>
        /// <returns>the possibly modified line line</returns>
        private static string InsertBefore(string line,string searchText,string insertText)
        {
            if ( IndexOfKeyword(line,insertText) >= 0 )
                return line;

            int index = IndexOfKeyword(line,searchText);
            if ( index < 0 )
                return line;

            return line.Insert(index,insertText+" ");
        }

        /// <summary>
        /// Finds location of a keyword in a line.
        /// keyword must be at the beginning of the line or preceeded by whitespace AND at the end of the line or followed by whitespace
        /// </summary>
        /// <param name="line">line to seach</param>
        /// <param name="keyword">keyword to search for</param>
        /// <returns>location of first character of keyword</returns>
        private static int IndexOfKeyword(string line,string keyword)
        {
            int index = line.IndexOf(keyword,StringComparison.Ordinal);
            if ( index < 0 )
                return index;

            int indexAfter = index+keyword.Length;
            if ( (index == 0 || char.IsWhiteSpace(line,index-1)) && (indexAfter == line.Length || char.IsWhiteSpace(line,indexAfter)) )
                return index;

            return -1;
        }
        #endregion
    }
}
