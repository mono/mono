'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

REM LineNo: 24 
REM ExpectedError: BC30367
REM ErrorMessage: Class 'base' cannot be indexed because it has Default property.

Imports System

Class base
     ReadOnly Property Item(i as Integer)As Integer
		Get			
			Return 10
		End Get
     End Property
End Class

Module DefaultA
	Sub Main()	
	Dim i as Integer	
	Dim a as base=new base()
	i=a(10)
	
	End Sub
End Module
