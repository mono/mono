'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'MyBase will call through the chain of inherited classes until it finds a callable implementation.

Imports System
Class A
	Public i As Integer
	Public Overridable Sub X(i) 
	End Sub 
End Class 

Class B 
	Inherits A 
	Public Overrides Sub X(i) 
		If i <> 20
			Throw New Exception ("Unexpected Value. R.Y / R.Z should be eual to 20 but got i =" &i)
		End If
	End Sub 

	Public Sub Y(i) 
		MyClass.X(i)  	
	End Sub 

End Class 

Class C
	Inherits B
	Public Overrides Sub X(i) 
		If i <> 10
			Throw New Exception ("Unexpected Value R.X should be eual to 10 but got i = " &i)
		End If	
	End Sub 

	Public Sub Z(i) 
		MyBase.X(i) 
	End Sub 
End Class 

Module Test 
	Sub Main() 
		Dim R As C = new C() 
		R.X(10)
		R.Y(20)
		R.Z(20)
	End Sub 
End Module
