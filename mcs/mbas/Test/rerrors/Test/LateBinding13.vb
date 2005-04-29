Imports System
Imports Nunit.Framework

Class TestConstant
	public const c as integer = 10
End Class

<TestFixture>_
Public Class Constant1
	_<Test, ExpectedException (GetType (System.FieldAccessException))>
        Public Sub TestForException()	
		Dim o as Object = new TestConstant()	
		o.c = 20
        End Sub
End Class 
