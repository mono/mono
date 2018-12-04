namespace Test.Elements
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xaml;
    using System.Xml;
    using System.Windows.Markup;
    using System.ComponentModel;
    using System.Collections.Generic;
    using DrtXaml.XamlTestFramework;
    using Test.Elements;
    using DRT;
    using DrtXaml;

    [TestClass]
    class ValueSerializerSerializationTests : XamlTestSuite
    {
        public ValueSerializerSerializationTests()
            : base("ValueSerializerSerializationTests")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        public void SimpleTest()
        {
            string generated = SaveToString(new VSContainer { Vehicle = new Bicycle { Brand = "RoadRanger", Year = 2002 } });
            string expected =
@"<VSContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer.Vehicle>
    <Bicycle>a RoadRanger bicycle made in 2002</Bicycle>
  </VSContainer.Vehicle>
</VSContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DerivedClassTest1()
        {
            Car car = new Sedan { Brand = "Toyota", Model = "Corolla", Trim = "LX", Year = 2008 };
            string generated = SaveToString(new VSContainer { Vehicle = car });
            string expected =
@"<VSContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer.Vehicle>
    <Sedan>a Toyota 2008 Corolla sedan LX</Sedan>
  </VSContainer.Vehicle>
</VSContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DerivedClassTest2()
        {
            Car car = new Hatchback { Brand = "Toyota", Model = "Corolla", Year = 2008, CanCarryBikes = true };
            string generated = SaveToString(new VSContainer { Vehicle = car });
            string expected = @"<VSContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer.Vehicle>
    <Hatchback>a Toyota 2008 Corolla hatchback which can carry bikes</Hatchback>
  </VSContainer.Vehicle>
</VSContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void DerivedClassTest3()
        {
            Car car = new Coupe { Brand = "Toyota", Model = "Corolla", Year = 2008, Description = "Aggressive feel." };
            string generated = SaveToString(new VSContainer { Vehicle = car });
            string expected =
@"<VSContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer.Vehicle>
    <Coupe Brand=""Toyota"" Description=""Aggressive feel."" Model=""Corolla"" Year=""2008"" />
  </VSContainer.Vehicle>
</VSContainer>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void VSandTCTest()
        {
            //ValueSerializer should be preferred over TypeConverter
            Motorcycle motorcycle = new Motorcycle { MPG = 70 };
            string generated = SaveToString(new VSContainer { Vehicle = motorcycle });
            string expected =
@"<VSContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer.Vehicle>
    <Motorcycle>a motorcycle with great fuel economy at 70MPG</Motorcycle>
  </VSContainer.Vehicle>
</VSContainer>";

            Assert.AreEqual(expected, generated);
        }

//        [TestMethod]
//        public void ArrayTest()
//        {
//            Vehicle[] vehicles = new Vehicle[3];
//            vehicles[0] = new Bicycle { Brand = "Road", Year = 1998 };
//            vehicles[1] = new Sedan { Brand = "Acura", Model = "TL", Trim = "L", Year = 2006 };
//            vehicles[2] = new Coupe { Brand = "Mazda", Model = "3", Year = 2000, Description = "small turn-angle" };
//            string generated = SaveToString(new VSContainer3 { Vehicles = vehicles });
//            string expected = 
//@"<tx:VSContainer3 xmlns:tx=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
//  <tx:VSContainer3.Vehicles>
//    <x:Array Type=""tx:Vehicle"">
//      <tx:Bicycle>a Road bicycle made in 1998</tx:Bicycle>
//      <tx:Sedan>a Acura 2006 TL sedan L</tx:Sedan>
//      <tx:Coupe Brand=""Mazda"" Description=""small turn-angle"" Model=""3"" Year=""2000"" />
//    </x:Array>
//  </tx:VSContainer3.Vehicles>
//</tx:VSContainer3>";
//            Assert.AreEqual(expected, generated);
//        }

//        [TestMethod]
//        public void DictionaryTest1()
//        {
//            IDictionary<Vehicle, int> dictionary = new Dictionary<Vehicle, int>();
//            dictionary.Add(new Bicycle { Brand = "Road", Year = 1999 }, 1);
//            dictionary.Add(new Bicycle { Brand = "Turf", Year = 2000 }, 2);
//            dictionary.Add(new Bicycle { Brand = "Lega", Year = 2001 }, 3);

//            string generated = SaveToString(dictionary);
//            string expected = 
//@"<Dictionary x:TypeArguments=""tx:Vehicle, p:Int32"" xmlns=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"" xmlns:tx=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
//  <p:Int32>
//    <x:Key>
//      <tx:Bicycle>a Road bicycle made in 1999</tx:Bicycle>
//    </x:Key>1</p:Int32>
//  <p:Int32>
//    <x:Key>
//      <tx:Bicycle>a Turf bicycle made in 2000</tx:Bicycle>
//    </x:Key>2</p:Int32>
//  <p:Int32>
//    <x:Key>
//      <tx:Bicycle>a Lega bicycle made in 2001</tx:Bicycle>
//    </x:Key>3</p:Int32>
//</Dictionary>";
//            Assert.AreEqual(expected, generated);
//        }

//        [TestMethod]
//        public void DictionaryTest2()
//        {
//            IDictionary<int, Vehicle> dictionary = new Dictionary<int, Vehicle>();
//            dictionary.Add(1, new Bicycle { Brand = "Road", Year = 1999 });
//            dictionary.Add(2, new Bicycle { Brand = "Turf", Year = 2000 });
//            dictionary.Add(3, new Bicycle { Brand = "Lega", Year = 2001 });
//            string generated = SaveToString(dictionary);
//            string expected = 
//@"<Dictionary x:TypeArguments=""p:Int32, tx:Vehicle"" xmlns=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:p=""http://schemas.microsoft.com/netfx/2008/xaml/schema"" xmlns:tx=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
//  <tx:Bicycle x:Key=""1"">a Road bicycle made in 1999</tx:Bicycle>
//  <tx:Bicycle x:Key=""2"">a Turf bicycle made in 2000</tx:Bicycle>
//  <tx:Bicycle x:Key=""3"">a Lega bicycle made in 2001</tx:Bicycle>
//</Dictionary>";
//            Assert.AreEqual(expected, generated);
//        }

        [TestMethod]
        public void PropertyTest1()
        {
            //A valueSerializer exists on the property and on the type; the ValueSerializer on the property is preferred.
            Car car = new Sedan { Brand = "Toyota", Model = "Corolla", Trim = "LX", Year = 2008 };
            string generated = SaveToString(new VSContainer2 { Vehicle = car });
            string expected = @"<VSContainer2 Vehicle=""Car: Toyota"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void PropertyTest2()
        {
            Car car = new Coupe { Brand = "Toyota", Model = "Corolla", Year = 2008, Description = "Aggressive feel." };
            string generated = SaveToString(new VSContainer2 { Vehicle = car });
            string expected = @"<VSContainer2 Vehicle=""Car: Toyota"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);

        }

        [TestMethod]
        public void PropertyTest3()
        {
            //A valueSerializer exists on the property and on the type; the ValueSerializer on the property is preferred.
            //In the event that the ValueSerializer does not convert to string, the ValueSerializer on the type will be called.
            Bicycle bike = new Bicycle { Brand = "RoadRanger", Year = 2002 };
            string generated = SaveToString(new VSContainer2 { Vehicle = bike });
            string expected =
@"<VSContainer2 xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer2.Vehicle>
    <Bicycle>a RoadRanger bicycle made in 2002</Bicycle>
  </VSContainer2.Vehicle>
</VSContainer2>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void PropertyTest4()
        {
            //we don't serialize a property that has value serializer but does not have a type converter that converts from string
            Motorcycle3 bike = new Motorcycle3 { MPG = 99 };
            string generated = SaveToString(new VSContainer4 { Vehicle = bike });
            string expected =
@"<VSContainer4 xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"">
  <VSContainer4.Vehicle>
    <Motorcycle3 MPG=""99"" />
  </VSContainer4.Vehicle>
</VSContainer4>";
            Assert.AreEqual(expected, generated);
        }

        [TestMethod]
        public void PropertyTest5()
        {
            //we serialize a property that has value serializer and only has a type converter on the type (not on the property)
            //that converts from string
            Motorcycle4 bike = new Motorcycle4 { MPG = 99 };
            string generated = SaveToString(new VSContainer5 { Vehicle = bike });
            string expected =
@"<VSContainer5 Vehicle=""a motorcycle with great fuel economy at 99MPG"" xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses"" />";
            Assert.AreEqual(expected, generated);
        }


        [TestMethod]
        public void VSOnStringPropertyTest()
        {
            SaveToString(new VSOnStringPropertyContainer { Prop = "Hello" });
        }

        string SaveToString(object o)
        {
            var sw = new StringWriter();
            using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                XamlServices.Save(xw, o);
            }
            return sw.ToString();
        }
    }

    [TestClass]
    class ValueSerializerDeserializationAndRoundtripTest : XamlTestSuite
    {
        public ValueSerializerDeserializationAndRoundtripTest()
            : base("ValueSerializerDeserializationAndRoundtripTest")
        {
        }

        public override DrtTest[] PrepareTests()
        {
            DrtTest[] tests = DrtTestFinder.FindTests(this);
            return tests;
        }

        [TestMethod]
        public void SimpleTest()
        {
            string xaml = @"<VSContainer xmlns=""clr-namespace:Test.Elements;assembly=XamlTestClasses""><VSContainer.Vehicle><Motorcycle>a motorcycle with great fuel economy at 70MPG</Motorcycle></VSContainer.Vehicle></VSContainer>";
            VSContainer c = (VSContainer)XamlServices.Parse(xaml);
            Motorcycle motorcycle = c.Vehicle as Motorcycle;
            Assert.IsNotNull(motorcycle);
            Assert.AreEqual(motorcycle.MPG, 70.0);
        }

        [TestMethod]
        [TestExpectedException(typeof(XamlObjectWriterException))]
        public void NoTCTest()
        {
            string xaml = @"<VSContainer xmlns=""clr-namespace:MyNamespace;assembly=ValueSerializerTest""><VSContainer.Vehicle><Motorcycle2>a motorcycle with great fuel economy at 70MPG</Motorcycle2></VSContainer.Vehicle></VSContainer>";
            VSContainer c = (VSContainer)XamlServices.Parse(xaml);
        }

        [TestMethod]
        public void RoundtripTest1()
        {
            StringBuilder sb = new StringBuilder();
            Motorboat boat = new SpeedBoat { MaxSpeed = 100 };
            XamlServices.Save(XmlWriter.Create(sb), new VSContainer { Vehicle = boat });
            VSContainer c = (VSContainer)XamlServices.Parse(sb.ToString());
            SpeedBoat speedBoat = c.Vehicle as SpeedBoat;
            Assert.IsNotNull(speedBoat);
            Assert.AreEqual(speedBoat.MaxSpeed, 100);
        }

        [TestMethod]
        public void RoundtripTest2()
        {
            StringBuilder sb = new StringBuilder();
            Motorboat boat = new Cruiser { Description = "Sleek look" };
            XamlServices.Save(XmlWriter.Create(sb), new VSContainer { Vehicle = boat });
            VSContainer c = (VSContainer)XamlServices.Parse(sb.ToString());
        }

        [TestMethod]
        public void ClassWithNoMemberTest()
        {
            string s = XamlServices.Save(new VSContainer { Vehicle = new EmptyBoat() });
            var c = (VSContainer)XamlServices.Parse(s);
            Assert.IsTrue(c.Vehicle is EmptyBoat);
        }
    }
}