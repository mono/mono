using System;
using System.IO;
using System.Collections;
using System.Xml.Serialization;

public class Test
{
	public static void Main()
	{
		Test t=new Test();
		t.Create("icollection.xml");
		t.Read("icollection.xml");
	}

	private void Create(string filename)
	{
		Employees emps=new Employees();

		/* Note that only the collection is serialized, not
		 * the CollectionName or any other public property of
		 * the class.
		 */
		emps.CollectionName="Employees";
		Employee john100=new Employee("John", "100xxx");
		emps.Add(john100);

		XmlSerializer ser=new XmlSerializer(typeof(Employees));
		TextWriter writer=new StreamWriter(filename);
		ser.Serialize(writer, emps);
		writer.Close();
	}

	private void Read(string filename)
	{
		XmlSerializer ser=new XmlSerializer(typeof(Employees));
		FileStream fs=new FileStream(filename, FileMode.Open);
		Employees emps;

		emps=(Employees)ser.Deserialize(fs);
		fs.Close();

		/* Not serialized! */
		Console.WriteLine("Collection name: "+emps.CollectionName);
		foreach(Employee emp in emps) 
		{
			Console.WriteLine("Employee name: "+emp.EmpName);
			Console.WriteLine("Employee ID: "+emp.EmpID);
		}
	}
}

public class Employees:ICollection
{
	public string CollectionName;
	private ArrayList empArray=new ArrayList();

	public Employee this[int index]
	{
		get {
			return((Employee)empArray[index]);
		}
	}

	public void CopyTo(Array a, int index)
	{
		empArray.CopyTo(a, index);
	}

	public int Count
	{
		get {
			return(empArray.Count);
		}
	}

	public object SyncRoot
	{
		get {
			return(this);
		}
	}

	public bool IsSynchronized
	{
		get {
			return(false);
		}
	}

	public IEnumerator GetEnumerator()
	{
		return(empArray.GetEnumerator());
	}

	public void Add(Employee newEmployee) 
	{
		empArray.Add(newEmployee);
	}
}

public class Employee
{
	public string EmpName;
	public string EmpID;

	public Employee()
	{}

	public Employee(string empName, string empID)
	{
		EmpName=empName;
		EmpID=empID;
	}
}
