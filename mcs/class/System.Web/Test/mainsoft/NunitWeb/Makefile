run-test: Test1.dll
	mono nunit-console2 Test1.dll -out:res.log

run-test-ondotnet: Test1.dll
	nunit-console2 Test1.dll -out:res.log

Test1.dll: NunitWeb.dll Test1/Class1.cs Makefile
	gmcs -d:NET_2_0 -r:../../../../lib/net_2_0/nunit.framework -r:System.Web -r:NunitWeb -target:library -r:System.Drawing -out:Test1.dll Test1/Class1.cs


NunitWeb.dll: NunitWeb/Helper.cs NunitWeb/MyHandler.cs NunitWeb/MyHost.cs NunitWeb/Resources/My.master NunitWeb/Resources/MyPage.aspx NunitWeb/Resources/MyPage.aspx.cs NunitWeb/Resources/MyPageWithMaster.aspx NunitWeb/Resources/MyPageWithMaster.aspx.cs NunitWeb/Resources/Web.config NunitWeb/Resources/MyPageWithMasterInvalidPlaceHolder.aspx Makefile
	gmcs -d:NET_2_0 -r:System.Configuration -r:System.Web NunitWeb/CustomSection.cs NunitWeb/Helper.cs NunitWeb/MyHandler.cs NunitWeb/MyHost.cs -resource:NunitWeb/Resources/My.master -resource:NunitWeb/Resources/MyPage.aspx -resource:NunitWeb/Resources/MyPage.aspx.cs -resource:NunitWeb/Resources/MyPageWithMaster.aspx -resource:NunitWeb/Resources/MyPageWithMaster.aspx.cs -resource:NunitWeb/Resources/MyPageWithMasterInvalidPlaceHolder.aspx -resource:NunitWeb/Resources/Web.config -target:library -out:NunitWeb.dll
