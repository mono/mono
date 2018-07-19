//------------------------------------------------------------------------------
// <copyright file="ParseRecorder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {

    using System.CodeDom;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Abstract base class for an object that wants to be notified of parse
    /// events during page parsing.
    /// </summary>
    public abstract class ParseRecorder {
        internal static readonly ParseRecorder Null = new NullParseRecorder();

        private static List<Func<ParseRecorder>> _factories;

        /// <summary>
        /// Functions added to this list are called to instantiate ParseRecorders
        /// for each TemplateParser.
        /// </summary>
        public static IList<Func<ParseRecorder>> RecorderFactories {
            get {
                if (_factories == null) {
                    _factories = new List<Func<ParseRecorder>>();
                }
                return _factories;
            }
        }

        internal static ParseRecorder CreateRecorders(TemplateParser parser) {
            if (_factories == null) {
                return ParseRecorder.Null;
            }

            List<ParseRecorder> recorders = new List<ParseRecorder>();
            foreach (Func<ParseRecorder> factory in _factories) {
                ParseRecorder recorder = factory();

                if (recorder != null) {
                    recorders.Add(recorder);
                }
            }

            ParseRecorderList list = new ParseRecorderList(recorders);

            list.Initialize(parser);

            return list;
        }

        /// <summary>
        /// Called to initialize the listener.
        /// </summary>
        public virtual void Initialize(TemplateParser parser) {
        }

        /// <summary>
        /// Called when the TemplateParser encounters a web control start tag
        /// </summary>
        public virtual void RecordBeginTag(ControlBuilder builder, Match tag) {
        }

        /// <summary>
        /// Called when the TemplateParser encounters a web control end tag
        /// </summary>
        public virtual void RecordEndTag(ControlBuilder builder, Match tag) {
        }

        /// <summary>
        /// Called when the TemplateParser encounters a web control empty tag
        /// </summary>
        public virtual void RecordEmptyTag(ControlBuilder builder, Match tag) {
        }

        /// <summary>
        /// Called when the TemplateParser encounters a code block
        /// </summary>
        public virtual void RecordCodeBlock(ControlBuilder builder, Match codeBlock) {
        }

        /// <summary>
        /// Called when the TemplateParser is finished parsing the file
        /// </summary>
        public virtual void ParseComplete(ControlBuilder root) {
        }

        /// <summary>
        /// Enables the ParseRecorder to access the generated CodeDom and insert
        /// and modify code
        /// </summary>
        public virtual void ProcessGeneratedCode(
            ControlBuilder builder, 
            CodeCompileUnit codeCompileUnit, 
            CodeTypeDeclaration baseType, 
            CodeTypeDeclaration derivedType, 
            CodeMemberMethod buildMethod, 
            CodeMemberMethod dataBindingMethod) {
        }

        private sealed class NullParseRecorder : ParseRecorder {
        }

        private sealed class ParseRecorderList : ParseRecorder {
            private readonly IEnumerable<ParseRecorder> _recorders;

            internal ParseRecorderList(IEnumerable<ParseRecorder> recorders) {
                _recorders = recorders;
            }

            public override void Initialize(TemplateParser parser) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.Initialize(parser);
                }
            }

            public override void RecordBeginTag(ControlBuilder builder, Match tag) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.RecordBeginTag(builder, tag);
                }
            }

            public override void RecordEndTag(ControlBuilder builder, Match tag) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.RecordEndTag(builder, tag);
                }
            }

            public override void RecordEmptyTag(ControlBuilder builder, Match tag) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.RecordEmptyTag(builder, tag);
                }
            }

            public override void RecordCodeBlock(ControlBuilder builder, Match codeBlock) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.RecordCodeBlock(builder, codeBlock);
                }
            }

            public override void ParseComplete(ControlBuilder root) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.ParseComplete(root);
                }
            }

            public override void ProcessGeneratedCode(ControlBuilder builder, CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod) {
                foreach (ParseRecorder recorder in _recorders) {
                    recorder.ProcessGeneratedCode(builder, codeCompileUnit, baseType, derivedType, buildMethod, dataBindingMethod);
                }
            }
        }
    }
}
