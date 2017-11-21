import itertools
import os
import re
import shutil
import string
import sys
import tempfile
import traceback

from glob import glob

from MacSDK import profile
from bockbuild.util.util import *


class MonoXamarinPackageProfile(MonoReleaseProfile):
    description = 'The Mono Framework for macOS (official release)'

    def setup (self):
        MonoReleaseProfile.setup (self)
        bockbuild.packages_to_build.extend(['mono-extensions'])

    def setup_release(self):
        MonoReleaseProfile.setup_release(self)
        self.release_packages['mono'].configure_flags.extend(
            ['--enable-extension-module=xamarin --enable-native-types --enable-pecrypt'])
        info('Xamarin extensions enabled')

    def run_pkgbuild(self, working_dir, package_type):
        output = MonoReleaseProfile.run_pkgbuild(
            self, working_dir, package_type)

MonoXamarinPackageProfile()