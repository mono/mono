using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class Tuple <a,b> {
  public a field1;
  public b field2;

  public Tuple (a x, b y) {
    field1 = x;
    field2 = y;
  }
}


public class Test {
   public static void Main()  {

      //Creates a new TestSimpleObject object.
      TestSimpleObject obj = new TestSimpleObject();

      Console.WriteLine("Before serialization the object contains: ");
      obj.Print();

      //Opens a file and serializes the object into it in binary format.
      Stream stream = File.Open("data.xml", FileMode.Create);
      BinaryFormatter formatter = new BinaryFormatter();

      //BinaryFormatter formatter = new BinaryFormatter();

      formatter.Serialize(stream, obj);
      stream.Close();
   
      //Empties obj.
      obj = null;
   
      //Opens file "data.xml" and deserializes the object from it.
      stream = File.Open("data.xml", FileMode.Open);
      formatter = new BinaryFormatter();

      //formatter = new BinaryFormatter();

      obj = (TestSimpleObject)formatter.Deserialize(stream);
      stream.Close();

      Console.WriteLine("");
      Console.WriteLine("After deserialization the object contains: ");
      obj.Print();
   }
}


// A test object that needs to be serialized.
[Serializable()]        
public class TestSimpleObject  {

    public Tuple <string,int>  member6;
    
    public TestSimpleObject() {
        member6 = new Tuple <string, int> ("aa", 22);
    }


    public void Print() {
        Console.WriteLine("member6 = '{0}'", member6);
    }
}
