'Author:
'   V. Sudharsan (vsudharsan@novell.com)
'
' (C) 2003 Ximian, Inc.

Imports System
Class A
	Dim i as Integer
	Dim c as Char
	Dim by as Byte
	Dim l as Long
	Dim b as Boolean
	Dim s as Single
	Dim d as Double
	Dim de as Decimal
	Dim da as Date
	Dim st as String
	Sub fun()		
		if i<>0 Then
			Throw new Exception("Integer Default is not Zero")
		End If
		if c<>Nothing Then
			Throw new Exception("Char Default is not Nothing")
		End If
		if by<>0 Then
			Throw new Exception("Byte Default is not Zero")
		End If
		if l<>0 Then
			Throw new Exception("Long Default is not Zero")
		End If
		if b<>False Then
			Throw new Exception("Boolean Default is not Zero")
		End If
		if s<>0.0 Then
			Throw new Exception("Single Default is not Zero")
		End If
		if d<>0.0 Then
			Throw new Exception("Double Default is not Zero")
		End If
		if de<>0 Then
			Throw new Exception("Decimal Default is not Zero")
		End If
		if da<>"1/1/0001 12:00:00 AM" Then
			Throw new Exception("Date Default is not 1/1/0001 12:00:00 AM")
		End If
		if st<>0 Then
			Throw new Exception("String Default is not Null")
		End If
	End Sub	
End Class

Module Default1	
	Sub Main()
		Dim a As A=New A()
		a.fun()
	End Sub
End Module
