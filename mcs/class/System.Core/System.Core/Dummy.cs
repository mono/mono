namespace System.Core {
	/* Internal class so that the System.Core namespace exists. The dlr has a lot of "using System.Core;" in it when compiling for moonlight, which otherwise would cause compiler errors */
	class Dummy {}
}
