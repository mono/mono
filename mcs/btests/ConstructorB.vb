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
	Inherits A

	' call base class ctor explicitly
	public Sub New()
		MyBase.New()
	End Sub
End Class

Class C
	Inherits A

	' call base class ctor with parameter
	public Sub New()
		MyBase.NEw("abc")
	End Sub
End Class

Class D

	' call another ctor in the same class
	' either of the methods mentioned below should give same result
	public Sub New()
		MyClass.NEw("aaa")
		'Me.NEw("aaa")
	End Sub

	public Sub New(name as String)
		if name <> "aaa" then
			throw new exception ("#A2, Unexpected result")
		end if
	End Sub
End Class

Module M
	Sub Main()
		dim x as B = new B()
		dim y as C = new C()
		dim z as D = new D()
	End Sub
End Module
