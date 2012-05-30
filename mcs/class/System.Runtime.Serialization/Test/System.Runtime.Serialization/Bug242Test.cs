
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Xml;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Serialization
{
    public class DataItemTest242
    {
        public string Name { get; set; }
    }

    public class DataTest242
    {
        public DataItemTest242[] DataArray { get; set; }
        public IList<DataItemTest242> DataIList { get; set; }
        public List<DataItemTest242> DataList { get; set; }
        public ICollection<DataItemTest242> DataICollection { get; set; }
        public IEnumerable<DataItemTest242> DataIEnumerable { get; set; }
    }
    
    [TestFixture]
    public class Bug242Test
    {
        [Test]
        public void TestMixListArraySerialize()
        {
            var dataItems  = new[] { new DataItemTest242 () { Name = "aaaaa" }, 
                                     new DataItemTest242 () { Name = "bbbbb" } };

            var data = new DataTest242 ()
            {
                DataArray       = dataItems,
                DataIList       = dataItems.ToList (),
                DataList        = dataItems.ToList (),
                DataICollection = dataItems.ToList (),
                DataIEnumerable = dataItems.ToList ()
            };

            // Serialize
            string xml;

            using (var stream = new MemoryStream ())
            {
                var serializer = new DataContractSerializer (typeof (DataTest242));
                serializer.WriteObject (stream, data);
                xml = Encoding.UTF8.GetString (stream.ToArray ());
            }

            // Deserialize
            DataTest242 clonedData;
            using (var reader = XmlDictionaryReader.CreateTextReader (Encoding.UTF8.GetBytes (xml), new XmlDictionaryReaderQuotas ()))
            {
                var serializer = new DataContractSerializer (typeof (DataTest242));
                clonedData     = (DataTest242)serializer.ReadObject (reader);
            }

            // ensure resulting object is populated
            Assert.AreEqual (clonedData.DataArray.Length , data.DataArray.Length,"#1 clonedData.DataArray.Length" );
            Assert.AreEqual (clonedData.DataList.Count, data.DataList.Count,"#2 clonedData.DataList.Count" );
            Assert.AreEqual (clonedData.DataIList.Count, data.DataIList.Count,"#3 clonedData.DataIList.Count" );
            Assert.AreEqual (clonedData.DataICollection.Count, data.DataICollection.Count,"#4 clonedData.DataICollection.Count" );
            Assert.AreEqual (clonedData.DataIEnumerable.Count (), data.DataIEnumerable.Count (),"#5 clonedData.DataIEnumerable.Count()" );
        }
    }
}
