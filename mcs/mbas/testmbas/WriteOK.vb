Option Explicit
Option Strict Off
Option Compare Text

Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System, IO = System.Console
Imports Microsoft.VisualBasic.Information

Imports Mono.GetOptions

<Assembly: AssemblyVersion("1.2.*")> 
<Assembly: AssemblyTitle("WriteOK - a test program for the MonoBASIC compiler")>
<Assembly: AssemblyCopyright("(c)2004 Rafael Teixeira")>
<Assembly: AssemblyDescription("Some logic and console outputting")>
<Assembly: Mono.About("Just a test program")>
<Assembly: Mono.GetOptions.Author("Rafael Teixeira")>

Module WriteOK

    Sub Main(args as string())
		Dim nodim as integer ' comment out to test explicit
		
		Dim octalLit as integer = &O177	
		Dim hexLit as long = &h1F34A6BFL
		
		dim singleLit as single = -1.1F
		dim doubleLit as double = 8e-3
		dim decimalLit as decimal = 1.1234567890123456789012D
		
		dim charLit as char = "?"c
		
		dim dateLit as date = #10/23/2003#

		dim optionprocessor as new another.driver
		
        REM Testing old-fashioned comments
        Console.WriteLine("OK!") ' Simple comments
		WriteOK2.[Sub]()
		IO.WriteLine("OK! ""via"" aliased name") ' from alias
		Console.WriteLine(Right("123",1))
		nodim = 1 ' test for explicit
        Console.WriteLine("nodim {0}" + _
			Constants.vbCRLF + "octalLit {1}" + _
			Constants.vbCRLF + "hexLit {2}" + _
			Constants.vbCRLF + "singleLit {3}" + _ 
			Constants.vbCRLF + "doubleLit {4}" + _ 
			Constants.vbCRLF + "decimalLit {5}" +  _ 
			Constants.vbCRLF + "charLit {6}" +  _ 
			Constants.vbCRLF + "dateLit {7}", _ 
			nodim, octalLit, hexLit, singleLit, doubleLit, decimalLit, charLit, dateLit)
		Console.WriteLine(123.ToString("g"))
		WriteOK5.ModuleSub("Qualified") 
		ModuleSub("Unqualified") 
		
		Another.WriteOK6.ModuleSub("Qualified") 
		Another.ModuleSub("SemiQualified")
		Another.Indirector.WriteIt()

		Console.WriteLine(Strings.ChrW(64))

		Console.Write("Positive cases for IsNumeric: ")
        if (IsNumeric(octalLit) And IsNumeric(hexLit) And IsNumeric(singleLit) And IsNumeric(doubleLit) And IsNumeric(decimalLit) And IsNumeric("123")) then
			Console.WriteLine("OK")
		else
			Console.WriteLine("FAILED")
		end if

		Console.Write("Negative cases for IsNumeric: ")
		if not (IsNumeric(nothing) Or IsNumeric(charLit) or IsNumeric(dateLit) or IsNumeric("123 ABC")) then
			Console.WriteLine("OK")
		else
			Console.WriteLine("FAILED")
		end if

		optionprocessor.ProcessArgs(args)
		if OptionProcessor.SayHo Then
			Console.WriteLine("Ho!")
		else
			Console.WriteLine("un-Ho!")	
		End If
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
		SyncLock Text
	        Console.WriteLine("Sub:OK! - """ & Text & """ " & someText & " " & someOtherText)
		End SyncLock
#If False 
'#If CAUSEERRORS
	Yield 1
	Yield Stop
#End If
    End Sub

End Class

Public Module WriteOK5
    Public Sub ModuleSub(Parm As String)
        Console.WriteLine("ModuleSub:OK! (" & Parm & ")")
    End Sub
End Module

Namespace Another

Public Module WriteOK6
    Public Sub ModuleSub(Parm As String)
        Console.WriteLine("Another.ModuleSub:OK! (" & Parm & ")")
    End Sub
End Module

Public Class Indirector
	Public Shared Sub WriteIt()
		ModuleSub("Through WriteIt") ' Must resolve to Another.WriteOK6.ModuleSub
	End Sub
End Class

Module Test ' modified Jambunathan test for intermixed directives with labels
     Sub MainLabeledSolo()
         100:
     End Sub
     Sub MainLabeled()
         100:
         #If False
               Console.WriteLine("Hello World")
        #End If
     End Sub
End Module

Public class Driver 
inherits Options

		<[Option]("Say 'Ho!'")> _
		public sayho as boolean = false

		<[Option]("About this test program", "about")> _
		overrides public function DoAbout() as WhatToDoNext 
			mybase.DoHelp()
			return WhatToDoNext.GoAhead
		end function

		<[Option](-1, "Say {it}", "say")> _
		public function Say(it as string) as WhatToDoNext
			console.writeline(it)
			return WhatToDoNext.GoAhead
		end function
end class
 
End Namespace
