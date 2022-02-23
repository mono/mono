set -euxo pipefail

sudo apt-get install -qy zip unzip

# Use our forked 7za with zstd support.
osarch=`uname -s`-`uname -m`
case $osarch in
	Linux-x86_64)
		sevenzip_artifact=7za-linux-x64/9e098bea868c_201bbbd99b245a6f887497113ec305d1a7d158a1595d753e77017237ac91b722.zip
		;;
	*)
		echo "Error: this script does not support $osarch"
		exit 1
	;;
esac

unpack_dir=external/buildscripts/artifacts/7za-zstd
mono external/buildscripts/bee.exe steve internal-unpack public $sevenzip_artifact $unpack_dir
chmod +x $unpack_dir/7za
export PATH=`pwd`/$unpack_dir:$PATH

perl external/buildscripts/collect_allbuilds.pl
pwd
ls -al
