Package('sqlite-autoconf', '3260000', sources=[
    'https://www.sqlite.org/2018/%{name}-%{version}.tar.gz'
],configure_flags=['--disable-editline'])
