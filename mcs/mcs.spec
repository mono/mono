Summary: The mono c# compiler
Name: mcs-%{LABEL}
Version: 0.14
Release: 0
License: GPL
Group: Development/Tools
Source0: http://www.go-mono.com/archive/mcs-%{LABEL}.tar.gz
Source1: mcs.sh
URL: http://www.go-mono.com/
BuildRoot: %{_tmppath}/%{name}-root
Packager: Miguel de Icaza (miguel@ximian.com)
BuildArch: noarch
Requires: mono, mono-classes
BuildRequires: mono, mono-classes, mcs

%description
The Mono C# Compiler.

%package -n mono-classes-%{LABEL}
Summary: The .NET compatible mono class libraries
Group: System Environment/Base
License: X11

%description -n mono-classes-%{LABEL}
Mono runtime class libraries.

%prep
%setup -n mcs-%{LABEL}

%build
make -f makefile.gnu

%install
rm -rf %{buildroot}
make -f makefile.gnu install prefix=$RPM_BUILD_ROOT%{_prefix}
install %{SOURCE1} $RPM_BUILD_ROOT/%{_bindir}/mcs

%clean
rm -rf %{buildroot}

%files
%defattr(-, root, root)
%doc AUTHORS ChangeLog INSTALL README
%{_bindir}/mcs
%{_bindir}/*.exe
%{_libdir}/NUnitCore_mono.dll

%files -n mono-classes-%{LABEL}
%defattr(-, root, root)
%{_libdir}/corlib.dll
%{_libdir}/System*.dll

%changelog
* Tue Aug 20 2002 Miguel de Icaza <miguel@ximian.com>
Small fixes

* Mon Aug 19 2002 Daniel Resare <noa@resare.com>
- Initial RPM release.
