using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xaml;

namespace DrtXaml
{
    class MalformedXamlNodeStream : Exception
    {
        public MalformedXamlNodeStream(string message)
            : base(message) { }
    }

    class NodeListValidator
    {
        // More validation will need to be added.
        public static void Validate(XamlNodeList nodeList)
        {
            ValidateStartEndCounts(nodeList.GetReader());
            ValidateNamespaceOnlyBeforeStartObject(nodeList.GetReader());
        }

        private static void ValidateStartEndCounts(XamlReader reader)
        {
            int objCount = 0;
            int propCount = 0;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                case XamlNodeType.StartObject:
                    ++objCount;
                    break;

                case XamlNodeType.EndObject:
                    --objCount;
                    break;

                case XamlNodeType.StartMember:
                    ++propCount;
                    break;

                case XamlNodeType.EndMember:
                    --propCount;
                    break;
                }
            }
            if (objCount != 0 && propCount != 0)
            {
                throw new MalformedXamlNodeStream("Mismatched number of Start/End Objects or Properties");
            }
        }

        private static void ValidateNamespaceOnlyBeforeStartObject(XamlReader reader)
        {
            bool atNamespace = false;

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                case XamlNodeType.NamespaceDeclaration:
                    atNamespace = true;
                    break;

                case XamlNodeType.StartObject:
                    atNamespace = false;
                    break;

                case XamlNodeType.EndObject:
                    if (atNamespace) throw new MalformedXamlNodeStream("Prefix Definintion Before an End Object");
                    break;

                case XamlNodeType.StartMember:
                    // We may want to enable this as a feature someday, but it is an error now.
                    if (atNamespace) throw new MalformedXamlNodeStream("Prefix Definintion Before an Start Member");
                    break;

                case XamlNodeType.EndMember:
                    if (atNamespace) throw new MalformedXamlNodeStream("Prefix Definintion Before an End Member");
                    break;

                case XamlNodeType.Value:
                    if (atNamespace) throw new MalformedXamlNodeStream("Prefix Definition Before a Text Node");
                    break;
                }
            }
        }

    }
}
