Imports System

Class A
	public Sub New()
	End Sub

	public Sub New(name as String)
		if name <> "abc" then
			throw new exception ("#A1, Unexpected result")
		end if
	End Sub
End Class

Class B
	' implicitly call base class default ctor
	public Sub New()
	End Sub
End Class

Module M
	Sub Main()
		dim x as A = new A()
		dim y as A = new A("abc")
		dim z as B = new B()
	End Sub
End Module
