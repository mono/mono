// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.IO;
    using System.Xaml;
    using System.Xml;

    internal static class TextExpressionCompilerHelper
    {
        public static void GetNamespacesLineInfo(string sourceXamlFileName, Dictionary<string, int> lineNumbersForNSes, Dictionary<string, int> lineNumbersForNSesForImpl)
        {
            // read until StartMember: TextExpression.NamespacesForImplementation OR TextExpression.Namespaces
            // create a subtree reader,
            // in the subtree, 
            // look for StartObject nodes of String type.  their values are added to either LineNumbersForNSes or LineNumbersForNSesForImpl dictionaries.
            if (!File.Exists(sourceXamlFileName))
            {
                return;
            }

            using (XmlReader xmlReader = XmlReader.Create(sourceXamlFileName))
            {
                using (XamlXmlReader xreader = new XamlXmlReader(xmlReader, new XamlXmlReaderSettings() { ProvideLineInfo = true }))
                {
                    bool hasHitFirstStartObj = false;
                    while (!hasHitFirstStartObj && xreader.Read())
                    {
                        if (xreader.NodeType == XamlNodeType.StartObject)
                        {
                            hasHitFirstStartObj = true;
                        }
                    }

                    if (hasHitFirstStartObj)
                    {
                        xreader.Read();
                        do
                        {
                            if (IsStartMemberTextExprNS(xreader))
                            {
                                XamlReader subTreeReader = xreader.ReadSubtree();
                                WalkSubTree(subTreeReader, lineNumbersForNSes);
                            }
                            else if (IsStartMemberTextExprNSForImpl(xreader))
                            {
                                XamlReader subTreeReader = xreader.ReadSubtree();
                                WalkSubTree(subTreeReader, lineNumbersForNSesForImpl);
                            }
                            else
                            {
                                xreader.Skip();
                            }
                        }
                        while (!xreader.IsEof);
                    }
                }
            }
        }

        private static bool IsStartMemberTextExprNS(XamlXmlReader xreader)
        {
            return xreader.NodeType == XamlNodeType.StartMember && xreader.Member.DeclaringType != null &&
                xreader.Member.DeclaringType.UnderlyingType == typeof(TextExpression) &&
                xreader.Member.Name == "Namespaces";
        }

        private static bool IsStartMemberTextExprNSForImpl(XamlXmlReader xreader)
        {
            return xreader.NodeType == XamlNodeType.StartMember && xreader.Member.DeclaringType != null &&
                xreader.Member.DeclaringType.UnderlyingType == typeof(TextExpression) &&
                xreader.Member.Name == "NamespacesForImplementation";
        }

        private static bool IsNamespaceString(XamlReader subTreeReader)
        {
            return subTreeReader.NodeType == XamlNodeType.StartObject && subTreeReader.Type.UnderlyingType == typeof(string);
        }

        private static void WalkSubTree(XamlReader subTreeReader, Dictionary<string, int> lineNumbersDictionary)
        {
            while (subTreeReader.Read())
            {
                if (IsNamespaceString(subTreeReader))
                {
                    while (subTreeReader.NodeType != XamlNodeType.Value)
                    {
                        subTreeReader.Read();
                    }

                    IXamlLineInfo ixamlLineInfo = (IXamlLineInfo)subTreeReader;
                    string namespaceName = subTreeReader.Value as string;
                    if (!string.IsNullOrEmpty(namespaceName))
                    {
                        lineNumbersDictionary[namespaceName] = ixamlLineInfo.LineNumber;
                    }
                }
            }
        }        
    }
}
