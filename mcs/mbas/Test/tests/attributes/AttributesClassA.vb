'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell Inc. (http://www.novell.com)
'use of Simple instead of SimpleAttribute for Class

Imports System

<AttributeUsage(AttributeTargets.Class)> _
Public Class SimpleAttribute
	Inherits System.Attribute

	Public Sub New(ByVal A As String)
		Me.A = A
	End Sub

	Public B As String
	Private A As String

	Public ReadOnly Property A1() As String
		Get
			Return A
		End Get
	End Property
End Class

<Simple("hello")> _
public Class Class1
	shared Sub Main()
		Dim type As Type = GetType(Class1)
		Dim arr() As Object = _
		type.GetCustomAttributes(GetType(SimpleAttribute), True)
		If arr.Length <> 1 Then
            		Throw New Exception ("Class1 should get SimpleAttribute. Lenght of the array should be 1 but got " & arr.Length)	
		End If
	End Sub

End Class
