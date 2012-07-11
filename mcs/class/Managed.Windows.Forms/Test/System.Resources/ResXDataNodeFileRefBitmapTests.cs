 /*
#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections;

using NUnit.Framework;
using System.ComponentModel.Design;

namespace MonoTests.System.Resources
{
    [TestFixture]
    public class ResXDataNodeFileRefBitmapTests : MonoTests.System.Windows.Forms.TestHelper
    {
        string _tempDirectory;
        string _otherTempDirectory;

        [Test, ExpectedException (typeof (NotImplementedException))]
        public void GetValueParamIsTouchedWhenFileRefReturnedFromResXResourceReader ()
        {
            // after running the enumerator of ResXResourceReader with UseResXDataNodes set 
            // to true, check params supplied to GetValue method
            // of ResXDataNode are used to deserialise

            // for now just throwing exception in param object to ensure its accessed

            ResXDataNode originalNode, returnedNode;

            originalNode = GetNodeFileRefToBitmap ();

            string fileName = GetResXFileWithNode (originalNode);

            using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
                reader.UseResXDataNodes = true;

                IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
                returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

                Assert.IsNotNull (returnedNode, "#A1");

                Bitmap ico = (Bitmap)returnedNode.GetValue (new ExceptionalTypeResolutionService ());

            }
        }

        [Test] // FIXME: i would like an valid alternative TypeResolutionService that could be used to test
        public void GetValueParamIsNotUsedWhenFileRefReturnedFromResXResourceReader ()
        {
            // after running the enumerator of ResXResourceReader with UseResXDataNodes set 
            // to true, check params supplied to GetValue method
            // of ResXDataNode are used to deserialise

            ResXDataNode originalNode, returnedNode;

            originalNode = GetNodeFileRefToBitmap ();

            string fileName = GetResXFileWithNode (originalNode);

            using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
                reader.UseResXDataNodes = true;

                IDictionaryEnumerator enumerator = reader.GetEnumerator ();
                enumerator.MoveNext ();
                returnedNode = (ResXDataNode)((DictionaryEntry)enumerator.Current).Value;

                Assert.IsNotNull (returnedNode, "#A1");

                object val = returnedNode.GetValue (new AlwaysReturnIntTypeResolutionService ());

                Assert.IsInstanceOfType (typeof (Bitmap),val, "#A2");
            }
        }

        [Test]
        public void GetValueTypeParamIsUsedWhenFileRefReturnedFromResXResourceReader ()
        {
            // after running the enumerator of ResXResourceReader with UseResXDataNodes set 
            // to true, check supplying params GetValueType of the 
            // ResXDataNode changes the output of the method

            ResXDataNode originalNode, returnedNode;

            originalNode = GetNodeFileRefToBitmap ();

            string fileName = GetResXFileWithNode (originalNode);

            using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
                reader.UseResXDataNodes = true;

                IDictionaryEnumerator enumerator = reader.GetEnumerator ();
                enumerator.MoveNext ();
                returnedNode = (ResXDataNode)((DictionaryEntry)enumerator.Current).Value;

                Assert.IsNotNull (returnedNode, "#A1");

                string returnedType = returnedNode.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());

                Assert.AreEqual ((typeof (Int32)).AssemblyQualifiedName, returnedType, "#A2");
            }
        }

        [Test]
        public void GetValueTypeParamIsUsedWhenFileRefCreatedNew ()
        {
            // check supplying params GetValueType of the 
            // UseResXDataNode does not change the output of the method for an instance
            // initialised by me

            ResXDataNode node;

            node = GetNodeFileRefToBitmap ();

            string returnedType = node.GetValueTypeName (new AlwaysReturnIntTypeResolutionService ());

            Assert.AreEqual ((typeof (Int32)).AssemblyQualifiedName, returnedType, "#A1");
        }

        [Test, ExpectedException (typeof (NotImplementedException))]
        public void GetValueParamIsTouchedWhenFileRefCreatedNew ()
        {
            // check supplying params GetValue of the 
            // UseResXDataNode is ignored for an instance
            // initialised by me

            ResXDataNode node;

            node = GetNodeFileRefToBitmap ();

            //raise exception if param used
            Bitmap ico = (Bitmap)node.GetValue (new ExceptionalTypeResolutionService ());
            
        }

        [Test] // FIXME: i would like an valid alternative TypeResolutionService that could be used to test
        public void GetValueParamIsNotUsedWhenFileRefCreatedNew ()
        {
            // after running the enumerator of ResXResourceReader with UseResXDataNodes set 
            // to true, check params supplied to GetValue method
            // of ResXDataNode are used to deserialise

            ResXDataNode node;

            node = GetNodeFileRefToBitmap ();
            
            object val = node.GetValue (new AlwaysReturnIntTypeResolutionService ());

            Assert.IsInstanceOfType (typeof (Bitmap), val, "#A2");
            
        }

        [TearDown]
        protected override void TearDown ()
        {
            //teardown
            if (Directory.Exists (_tempDirectory))
                Directory.Delete (_tempDirectory, true);

            base.TearDown ();
        }

        string GetResXFileWithNode (ResXDataNode node)
        {
            string fileName;
            
            _tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
            _otherTempDirectory = Path.Combine (_tempDirectory, "in");
            if (!Directory.Exists (_otherTempDirectory)) {
                Directory.CreateDirectory (_otherTempDirectory);
            }

            fileName = Path.Combine (_tempDirectory, "myresx.resx");

            using (ResXResourceWriter writer = new ResXResourceWriter (fileName)) {
                writer.AddResource (node);
            }

            return fileName;
        }

        ResXDataNode GetNodeFileRefToBitmap ()
        {
            _tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
            _otherTempDirectory = Path.Combine (_tempDirectory, "in");
            if (!Directory.Exists (_otherTempDirectory)) {
                Directory.CreateDirectory (_otherTempDirectory);
            }

            string refFile = Path.Combine (_tempDirectory, "a.cur");
            WriteEmbeddedResource ("a.cur", refFile);

            ResXFileRef fileRef = new ResXFileRef (refFile, typeof (Bitmap).AssemblyQualifiedName);
            ResXDataNode node = new ResXDataNode ("test", fileRef);

            return node;
        }
        
        private static void WriteEmbeddedResource (string name, string filename)
        {
            const int size = 512;
            byte[] buffer = new byte[size];
            int count = 0;

            Stream input = typeof (ResXDataNodeTest).Assembly.
                GetManifestResourceStream (name);
            Stream output = File.Open (filename, FileMode.Create);

            try
            {
                while ((count = input.Read (buffer, 0, size)) > 0)
                {
                    output.Write (buffer, 0, count);
                }
            }
            finally
            {
                output.Close ();
            }
        }

    }

}
#endif

*/