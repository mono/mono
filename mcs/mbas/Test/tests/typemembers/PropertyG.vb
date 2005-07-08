'Author:
'   Satya Sudha K (ksathyasudha@novell.com)
'
' (C) 2005 Novell, Inc.
'
'
' Property taking 'ParamArray' parameter, overloading of properties

Imports System
Module M
	Dim a As Integer() = {1,2,3}
	Property Prop (ParamArray i As Integer()) As Integer
		Get
			return i.Length
		End Get
		Set
			a (0) = Value
		End Set
	End Property
	Property Prop (ByVal i As Integer) As Integer
		Get 
			return a (i)
		End Get
		Set
			a (i) = Value
		End Set
	End Property

	Sub Main ()
		if Prop (2, 3, 4, 5) <> 4 Then
		End If
		Prop (2,3,4,5) = 6
		if (Prop (0) <> 6) Then
			throw new exception ("A#1 - Overloaded properties not working properly")
		End If
		Prop (1) = 9
		if (Prop (1) <> 9) Then
			throw new exception ("A#2 - Overloaded properties not working properly")
		End If
	End Sub
End Module
