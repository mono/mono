Option Explicit
Option Strict Off
Option Compare Text

Imports System, IO = System.Console
Imports Microsoft.VisualBasic

Module WriteOK

    Sub Main()
		Dim nodim as integer ' comment out to test explicit
		
		Dim octalLit as integer = &O177	
		Dim hexLit as long = &h1F34A6BFL
		
		dim singleLit as single = -1.1F
		dim doubleLit as double = 8e-3
		dim decimalLit as decimal = 1.1234567890123456789012D
		
		dim charLit as char = "?"c
		
        REM Testing old-fashioned comments
        Console.WriteLine("OK!") ' Simple comments
		WriteOK2.[Sub]()
		IO.WriteLine("OK! ""via"" aliased name") ' from alias
		nodim = 1 ' test for explicit
        Console.WriteLine("nodim {0}" + _
			Constants.vbCRLF + "octalLit {1}" + _
			Constants.vbCRLF + "hexLit {2}" + _
			Constants.vbCRLF + "singleLit {3}" + _ 
			Constants.vbCRLF + "doubleLit {4}" + _ 
			Constants.vbCRLF + "decimalLit {5}" +  _ 
			Constants.vbCRLF + "charLit {6}", _ 
			nodim, octalLit, hexLit, singleLit, doubleLit, decimalLit, charLit)
		Console.WriteLine(123.ToString("g"))
		WriteOK5.ModuleSub() ' 122
		' ModuleSub() ' 103
		
		Console.WriteLine(Strings.ChrW(64))

    End Sub

End Module

Public Class WriteOK2

    Friend Shared Sub [Sub]() ' Escaped identifier
		Dim Text as string ' here 'Text' isn't a keyword
		Dim sometext = "Yeah! Some Text" 'TODO: still case sensitive on identifiers
		Dim someOtherText as string = "Blah! Some Other Text"
'		Const sometext = "Yeah! Some Text" ' FIXME: raises InvalidCastException in yyParse
'		Const someOtherText as string = "Blah! Some Other Text" ' FIXME: raises InvalidCastException in yyParse
		Text = "This is a test!"
        Console.WriteLine("Sub:OK! - """ & Text & """ " & someText & " " & someOtherText)
#If CAUSEERRORS
	Yield 1
	Yield Stop
#End If
    End Sub

End Class

Public Module WriteOK5
    Public Sub ModuleSub()
        Console.WriteLine("ModuleSub:OK!")
    End Sub
End Module
