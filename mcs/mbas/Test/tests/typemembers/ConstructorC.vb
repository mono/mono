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
		if B.y<>25
			throw new System.Exception("#A1 Constructor not working")
		end if
		if A.x<>10
			throw new System.Exception("#A2 Constructor not working")
		end if
		dim c as new B()
		if C.z<>30
			throw new System.Exception("#A3 Constructor not working")
		end if
	End Sub
End Module
