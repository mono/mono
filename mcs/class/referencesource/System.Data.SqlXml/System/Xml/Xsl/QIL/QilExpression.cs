//------------------------------------------------------------------------------
// <copyright file="QilExpression.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Xml.Xsl.Runtime;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// The CQR implementation of QilExpression.
    /// </summary>
    /// <remarks>
    ///    <p>QilExpression is the XML Query Intermediate Language invented by Michael Brundage and Chris Suver.
    ///    QilExpression is an intermediate representation (IR) for all XML query and view languages.  QilExpression is
    ///    designed for optimization, composition with virtual XML views, translation into other forms,
    ///    and direct execution.  See also <a href="http://dynamo/qil/qil.xml">the QIL specification</a>.</p>
    /// </remarks>
    internal class QilExpression : QilNode {
        private QilFactory  factory;
        private QilNode     isDebug;
        private QilNode     defWSet;
        private QilNode     wsRules;
        private QilNode     gloVars;
        private QilNode     gloParams;
        private QilNode     earlBnd;
        private QilNode     funList;
        private QilNode     rootNod;


        //-----------------------------------------------
        // Constructors
        //-----------------------------------------------

        /// <summary>
        /// Construct QIL from a rooted graph of QilNodes with a new factory.
        /// </summary>
        public QilExpression(QilNodeType nodeType, QilNode root) : this(nodeType, root, new QilFactory()) {
        }

        /// <summary>
        /// Construct QIL from a rooted graph of QilNodes with a specific factory.
        /// </summary>
        public QilExpression(QilNodeType nodeType, QilNode root, QilFactory factory) : base(nodeType) {
            this.factory = factory;
            this.isDebug = factory.False();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Auto;
            this.defWSet = factory.LiteralObject(settings);

            this.wsRules = factory.LiteralObject(new List<WhitespaceRule>());
            this.gloVars = factory.GlobalVariableList();
            this.gloParams = factory.GlobalParameterList();
            this.earlBnd = factory.LiteralObject(new List<EarlyBoundInfo>());
            this.funList = factory.FunctionList();
            this.rootNod = root;
        }


        //-----------------------------------------------
        // IList<QilNode> methods -- override
        //-----------------------------------------------

        public override int Count {
            get { return 8; }
        }

        public override QilNode this[int index] {
            get {
                switch (index) {
                    case 0: return this.isDebug;
                    case 1: return this.defWSet;
                    case 2: return this.wsRules;
                    case 3: return this.gloParams;
                    case 4: return this.gloVars;
                    case 5: return this.earlBnd;
                    case 6: return this.funList;
                    case 7: return this.rootNod;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set {
                switch (index) {
                    case 0: this.isDebug = value; break;
                    case 1: this.defWSet = value; break;
                    case 2: this.wsRules = value; break;
                    case 3: this.gloParams = value; break;
                    case 4: this.gloVars = value; break;
                    case 5: this.earlBnd = value; break;
                    case 6: this.funList = value; break;
                    case 7: this.rootNod = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }


        //-----------------------------------------------
        // QilExpression methods
        //-----------------------------------------------

        /// <summary>
        /// QilFactory to be used in constructing nodes in this graph.
        /// </summary>
        public QilFactory Factory {
            get { return this.factory; }
            set { this.factory = value; }
        }

        /// <summary>
        /// True if this expression contains debugging information.
        /// </summary>
        public bool IsDebug {
            get { return this.isDebug.NodeType == QilNodeType.True; }
            set { this.isDebug = value ? this.factory.True() : this.factory.False(); }
        }

        /// <summary>
        /// Default serialization options that will be used if the user does not supply a writer at execution time.
        /// </summary>
        public XmlWriterSettings DefaultWriterSettings {
            get { return (XmlWriterSettings) ((QilLiteral) this.defWSet).Value; }
            set {
                value.ReadOnly = true;
                ((QilLiteral) this.defWSet).Value = value;
            }
        }

        /// <summary>
        /// Xslt whitespace strip/preserve rules.
        /// </summary>
        public IList<WhitespaceRule> WhitespaceRules {
            get { return (IList<WhitespaceRule>) ((QilLiteral) this.wsRules).Value; }
            set { ((QilLiteral) this.wsRules).Value = value; }
        }

        /// <summary>
        /// External parameters.
        /// </summary>
        public QilList GlobalParameterList {
            get { return (QilList) this.gloParams; }
            set { this.gloParams = value; }
        }

        /// <summary>
        /// Global variables.
        /// </summary>
        public QilList GlobalVariableList {
            get { return (QilList) this.gloVars; }
            set { this.gloVars = value; }
        }

        /// <summary>
        /// Early bound function objects.
        /// </summary>
        public IList<EarlyBoundInfo> EarlyBoundTypes {
            get { return (IList<EarlyBoundInfo>) ((QilLiteral) this.earlBnd).Value; }
            set { ((QilLiteral) this.earlBnd).Value = value; }
        }

        /// <summary>
        /// Function definitions.
        /// </summary>
        public QilList FunctionList {
            get { return (QilList) this.funList; }
            set { this.funList = value; }
        }

        /// <summary>
        /// The root node of the QilExpression graph
        /// </summary>
        public QilNode Root {
            get { return this.rootNod; }
            set { this.rootNod = value; }
        }
    }
}
