'BC30579: '#ExternalSource' directive must end with matching '#End ExternalSource' directive

Imports System
Module ExternalSourceDirective
	Sub Main()
		#ExternalSource("/home/test.aspx",30)
		Console.WriteLine("In test.aspx")
	End Sub
End Module
