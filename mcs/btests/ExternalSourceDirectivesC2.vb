'Line 13, BC30579: '#ExternalSource' directive must end with matching '#End ExternalSource' directive

Imports System
Module ExternalSourceDirective
	Sub Main()
		#ExternalSource("/home/a.aspx",30)
		Console.WriteLine("In a.aspx")
		#End ExternalSource

		#ExternalSource("/home/index.aspx", 1024)
		Console.WriteLine("In index.aspx")
	End Sub
End Module
