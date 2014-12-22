FROM mono:latest

ENV MONO_INSTALL /usr/local

RUN apt-get update && apt-get install -y git autoconf libtool automake build-essential mono-devel gettext

ENV PATH $MONO_INSTALL/bin:$PATH
WORKDIR /data

COPY . /data
RUN ./autogen.sh --prefix=$MONO_INSTALL
RUN make
RUN make install

RUN rm -rf /data
