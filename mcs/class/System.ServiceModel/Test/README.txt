How to add a new test WCF Client/Server test:

1. In directory FeatureBased/Features.Contracts add a new contract and implementation class. It is best to copy an existing one and modify it.
2. In directory FeatureBased/Features.Serialization add a new test class. Again copy from a different test class, but remove all members. 
   Maintain the inheritance from TestFixtureBase<clientProxy, ServerImplementation, ServerContract>.  However, since you don't have a client
   proxy yet, use 'object' instead.
3. Run the WCFServers executable. This will start the server so it is now running.
4. In command prompt, in directory  Test\FeatureBased\Features.Client run "svcutil.exe <endpointbase>/<serverclassname>_wsdl /n:*,Proxy.MonoTests.Features.Client" e.g.
   svcutil http://localhost:9999/ExitProcessHelperServer_wsdl /n:*,Proxy.MonoTests.Features.Client
5. Include the file in the project
6. In the test class created in step #2, modify the name of the client proxy class to the name of the generated class.