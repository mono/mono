'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell, Inc (http://www.novell.com)
'AddressOf

Imports System

	Class NewClass
		Public i As Integer
		Public Overridable Sub MyMethod(i)
		End Sub
		Public Sub AMethod(ByVal s As integer)
			if s <> 10
				Throw New Exception ("Unexpected Behavior S should be 10 but got s=" &s  )
			End If
		End Sub
	End Class

Module Test 
	Delegate Sub ADelegate(ByVal s As Integer)
	Sub Main() 
		Dim TestObj As NewClass = New NewClass()
		Dim delg As ADelegate
		delg=New ADelegate(AddressOf TestObj.AMethod)	      
		delg.Invoke(10)
	End Sub
End Module
