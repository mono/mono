'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'MyClass behaves like an object variable referring to the current instance of a class as originally implemented.

Imports System

	Class BaseClass
		Public i As Integer
		Public Overridable Sub MyMethod(i)
			if i <> 100 then 
				Throw New Exception ("Unexpected Behavior Expected 100 but got i = "&i )
			End If		
		End Sub
		Public Sub UseMyClass(i)
	      	MyClass.MyMethod(i)   
		End Sub
	End Class
	
	Class DerivedClass : Inherits BaseClass
		Public Overrides Sub MyMethod(i)
		i = 50
		End Sub
	End Class

Module Test 
	Sub Main() 
		Dim TestObj As DerivedClass = New DerivedClass()
	      TestObj.UseMyClass(100)
	End Sub
End Module

