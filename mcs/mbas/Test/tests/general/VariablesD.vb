Imports System
Class A
	public shared y as integer = 20
	public z as integer = 30

	Shared Sub New()
	End Sub

	public Sub New()
	End Sub

	Shared function f() as integer
		return 50
	end function
end class

Module M
	Sub Main()
		if (A.y <> 20) then
			Throw new Exception ("#A1, Unexpected result")
		end if

		dim c as new A()
		dim d as new A()

		if(c.y <> 20) then
			Throw new Exception ("#A2, Unexpected result")
		end if

		if(d.y <> 20) then
			Throw new Exception ("#A3, Unexpected result")
		end if

		A.y = 25

		if(c.y <> 25) then
			Throw new Exception ("#A4, Unexpected result")
		end if

		c.y = 35

		if(A.y <> 35) then
			Throw new Exception ("#A5, Unexpected result")
		end if

		if(d.y <> 35) then
			Throw new Exception ("#A6, Unexpected result")
		end if


		if(c.z <> 30) then
			Throw new Exception ("#A7, Unexpected result")
		end if

		if(A.f() <> 50) then
			Throw new Exception ("#A8, Unexpected result")
		end if
	End Sub
End Module
