Imports System

Class A
	public shared x as integer = 10

	Shared Sub New()
		Console.WriteLine("Shared ctor")
	End Sub

	public Sub New()
		Console.WriteLine("ctor")
	End Sub

End Class

Class B
	inherits A

	public shared y as integer = 20
	public z as integer = 30

	Shared Sub New()
		Console.WriteLine("Shared ctor in derived class")
	End Sub

	public Sub New()
		Console.WriteLine("ctor in derived class")
	End Sub

	Shared Sub f()
		Console.WriteLine("f")
	end sub


end class

Module M
	Sub Main()
		B.y = 25
		Console.WriteLine(B.y)
		Console.WriteLine(A.x)
		dim c as new B()
		Console.WriteLine(c.z)
	End Sub
End Module
