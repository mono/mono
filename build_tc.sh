if [ "${UNITY_THISISABUILDMACHINE}" = "1" ]; then
	svn --username unitybuild --password uni10666 co svn://svn.hq.unity3d.com/home/svn/external-tools/native_client_sdk ../native_client_sdk
else
	yes p | svn co --username je_script --password raidUvWarHi https://intra.unity3d.com:3680/home/svn/external-tools/native_client_sdk/ ../native_client_sdk
fi

perl ../native_client_sdk/prepare.pl
export NACL_NATIVE_CLIENT="$PWD/../native_client_sdk/builds/"

./autogen.sh
make distclean
cd nacl
./nacl-runtime-mono.sh