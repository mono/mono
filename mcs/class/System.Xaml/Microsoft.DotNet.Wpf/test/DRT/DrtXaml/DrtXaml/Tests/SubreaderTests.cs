using System;
using System.Collections.Generic;
using System.Text;
using DRT;
using DrtXaml.XamlTestFramework;
using System.Xaml;
using System.IO;
using System.Xml;

namespace DrtXaml.Tests
{
    [TestClass]
    sealed class SubreaderTests : XamlTestSuite
    {
        public SubreaderTests()
            : base("SubreaderTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        // =============================================

        XamlSchemaContext _xamlSchemaContext;

        // Remove the Property "Element2"
        XamlMember _removeProperty;

        // Remove all instances of "HoldsOneElement"
        XamlType _removeType;

        XamlReader _mainReader;
        XamlWriter _mainWriter;

        public enum SkipMode { Skip, ReadSubtree_discard, ReadSubtree_build }


        [TestXaml]
        const string SubtreeXAML = @"
<Element10   xmlns='clr-namespace:Test.Elements;assembly=XamlTestClasses'>

    <Element10.Element1>
       <ElementListHolder>
         <ElementListHolder.Elements>
            <ElementWithTitle Title='my title'/>
            <HoldsOneElement>
                <HoldsOneElement.Element>
                    <ColorElement ColorName='pink'/>
                </HoldsOneElement.Element>  
            </HoldsOneElement>
          </ElementListHolder.Elements>
       </ElementListHolder>
    </Element10.Element1>

    <Element10.Element2>
       <ElementListHolder>
         <ElementListHolder.Elements>
            <ElementWithTitle Title='my title'/>
         </ElementListHolder.Elements>
       </ElementListHolder>
    </Element10.Element2>

    <Element10.Element3>
        <HoldsOneElement>
            <HoldsOneElement.Element>
                <ColorElement ColorName='pink'/>
            </HoldsOneElement.Element>  
        </HoldsOneElement>
    </Element10.Element3>

</Element10>";

        [TestSetup]
        private void TestSetup()
        {
            _mainReader = null;
            _mainWriter = null;
            _removeProperty = null;
            _removeType = null;
            _xamlSchemaContext = null;

            _xamlSchemaContext = new XamlSchemaContext();
            string elementXns = "clr-namespace:Test.Elements;assembly=XamlTestClasses";
            XamlType element3 = _xamlSchemaContext.GetXamlType(elementXns, "Element10");

            // Remove the Property "Element2"
            _removeProperty = element3.GetMember("Element2");

            // Remove all instances of "HoldsOneElement"
            _removeType = _xamlSchemaContext.GetXamlType(elementXns, "HoldsOneElement");

            StringReader stringReader = new StringReader(SubtreeXAML);
            XmlReader xmlReader = XmlReader.Create(stringReader);

            _mainReader = new XamlXmlReader(xmlReader, _xamlSchemaContext);
            _mainWriter = new XamlObjectWriter(_xamlSchemaContext);
        }

        [TestMethod]
        void RemoveStuffWithSkip()
        {
            object tree = DoStuffWithSkipper(SkipMode.Skip);
            ConfirmStuffIsRemoved(tree);
        }

        [TestMethod]
        void RemoveStuffWithSubreader()
        {
            object tree = DoStuffWithSkipper(SkipMode.ReadSubtree_discard);
            ConfirmStuffIsRemoved(tree);
        }

        [TestMethod]
        void BuildStuffWithSubreader()
        {
            object tree = DoStuffWithSkipper(SkipMode.ReadSubtree_build);
            ConfirmStuffIsNotRemoved(tree);
        }

        private object DoStuffWithSkipper(SkipMode skipMode)
        {
            if (_removeType == null || _removeProperty == null)
            {
                throw new Exception("remove something is null");
            }

            bool more = _mainReader.Read();
            while (more)
            {
                bool skipped = false;
                switch (_mainReader.NodeType)
                {
                case XamlNodeType.StartObject:
                    if (_mainReader.Type == _removeType)
                    {
                        Process(skipMode);
                        skipped = true;
                    }
                    break;
                case XamlNodeType.StartMember:
                    if (_mainReader.Member == _removeProperty)
                    {
                        Process(skipMode);
                        skipped = true;
                    }
                    break;
                }
                if (!skipped)
                {
                    _mainWriter.WriteNode(_mainReader);
                    more = _mainReader.Read();
                }
            }

            _mainWriter.Close();
            XamlObjectWriter objectWriter = _mainWriter as XamlObjectWriter;
            return (objectWriter == null) ? null : objectWriter.Result;
        }

        public void Process(SkipMode mode)
        {
            switch (mode)
            {
            case SkipMode.Skip:
                _mainReader.Skip();
                break;

            case SkipMode.ReadSubtree_discard:
                DiscardSubtree(_mainReader.ReadSubtree());
                break;

            case SkipMode.ReadSubtree_build:
                BuildSubtree(_mainReader.ReadSubtree(), _mainWriter);
                break;
            }
        }

        private void DiscardSubtree(XamlReader subReader)
        {
            while (subReader.Read())
            {
                ;
            }
        }

        private void BuildSubtree(XamlReader subReader, XamlWriter writer)
        {
            while (subReader.Read())
            {
                writer.WriteNode(subReader);
            }
        }

        public void ConfirmStuffIsRemoved(object tree)
        {
            var e3 = (Test.Elements.Element10)tree;
            if (e3.Element2 != null)
            {
                throw new InvalidOperationException("Property Element2 should be nulll");
            }
            if (e3.Element3 != null)
            {
                throw new InvalidOperationException("Property Element3 should be nulll");
            }

            if (e3.Element1 == null)
            {
                throw new InvalidOperationException("Property Element1 should NOT be nulll");
            }

            var elh = (Test.Elements.ElementListHolder)e3.Element1;
            foreach (Test.Elements.Element element in elh.Elements)
            {
                if (element is Test.Elements.HoldsOneElement)
                {
                    throw new InvalidOperationException("All HoldsOneElement should have been removed");
                }
            }
        }

        public void ConfirmStuffIsNotRemoved(object tree)
        {
            var e3 = (Test.Elements.Element10)tree;
            if (e3.Element2 == null)
            {
                throw new InvalidOperationException("Property Element2 should NOT be nulll");
            }
            if (e3.Element3 == null)
            {
                throw new InvalidOperationException("Property Element3 should NOT be nulll");
            }

            if (e3.Element1 == null)
            {
                throw new InvalidOperationException("Property Element1 should NOT be nulll");
            }

            var elh = (Test.Elements.ElementListHolder)e3.Element1;
            int HOEcount = 0;
            foreach (Test.Elements.Element element in elh.Elements)
            {
                if (element is Test.Elements.HoldsOneElement)
                {
                    HOEcount += 1;
                }
            }
            if (HOEcount != 1)
            {
                throw new InvalidOperationException("Too many or too few HoldsOneElements elements found");
            }
        }
    }
}
