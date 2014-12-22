FROM mono:latest

RUN apt-get update && apt-get install -y git autoconf libtool automake build-essential mono-devel gettext

ENV PATH /usr/local/bin:$PATH

WORKDIR /data
COPY . /data
RUN ./autogen.sh --prefix=/usr/local && make && make install && rm -rf /data
