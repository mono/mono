'Author: Ritvik Mayank <mritvik@novell.com>
'Copyright (C) 2005 Novell Inc. (http://www.novell.com)
' use of Simple instead of SimpleAttribute for Interface 

Imports System

<AttributeUsage(AttributeTargets.Class Or AttributeTargets.Interface)> _
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
Class Class1
End Class

<Simple("hello")> _
public Interface Interface1
End Interface

Module Test
		Sub Main()
		End Sub
End Module
