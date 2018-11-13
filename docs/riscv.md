# Mono RISC-V Port

These are some useful links and notes pertaining to the RISC-V port of Mono.

## Useful RISC-V documents

* [RISC-V User-Level ISA Specification](https://riscv.org/specifications/)
* [RISC-V Privileged ISA Specification](https://riscv.org/specifications/privileged-isa/)
* [RISC-V Debug Specification Standard](https://github.com/riscv/riscv-debug-spec/blob/master/riscv-debug-spec.pdf)
* [RISC-V Assembly Programmer's Manual](https://github.com/riscv/riscv-asm-manual/blob/master/riscv-asm.md)
* [RISC-V ELF psABI Specification](https://github.com/riscv/riscv-elf-psabi-doc/blob/master/riscv-elf.md)

## Useful RISC-V repositories

* [RISC-V Organization](https://github.com/riscv)
  * [RISC-V Linux](https://github.com/riscv/riscv-linux)
  * [RISC-V LLD](https://github.com/riscv/riscv-lld)
  * [RISC-V Tools](https://github.com/riscv/riscv-tools)
    * [RISC-V GNU Toolchain](https://github.com/riscv/riscv-gnu-toolchain)
      * [RISC-V Binutils](https://github.com/riscv/riscv-binutils-gdb)
      * [RISC-V GCC](https://github.com/riscv/riscv-gcc)
      * [RISC-V Glibc](https://github.com/riscv/riscv-glibc)
      * [RISC-V Newlib](https://github.com/riscv/riscv-newlib)
      * [RISC-V QEMU](https://github.com/riscv/riscv-qemu)
    * [RISC-V Opcodes](https://github.com/riscv/riscv-opcodes)
    * [RISC-V Tests](https://github.com/riscv/riscv-tests)
* [lowRISC Organization](https://github.com/lowrisc)
  * [RISC-V LLVM](https://github.com/lowrisc/riscv-llvm)

## Setting up a cross environment

Setting up a cross environment with a Linux toolchain and QEMU is quite easy.

First, add these to your `.bashrc` (or some other script that you run):

```bash
export RISCV=$HOME/riscv
export PATH=$RISCV/bin:$PATH
export QEMU_LD_PREFIX=$RISCV/sysroot
```

Install some dependencies needed to build the toolchain:

```console
# apt install autoconf automake autotools-dev curl libmpc-dev libmpfr-dev libgmp-dev libusb-1.0-0-dev gawk build-essential bison flex texinfo gperf libtool patchutils bc zlib1g-dev device-tree-compiler pkg-config libexpat-dev libglib2.0-dev libpixman-1-dev
```

Now you can build the toolchain:

```console
$ git clone --recursive git@github.com:riscv/riscv-tools.git
$ cd riscv-tools
$ ./build.sh
$ cd riscv-gnu-toolchain
$ ./configure --prefix=$RISCV --enable-multilib
$ make linux
$ cd ../..
$ git clone --recursive git@github.com:riscv/riscv-qemu.git
$ cd riscv-qemu
$ mkdir build
$ cd build
$ ../configure --prefix=$RISCV --disable-werror
$ make
$ make install
```

## Building Mono with a cross toolchain

Building Mono is quite straightforward:

```console
$ ./autogen.sh --prefix=$RISCV/sysroot --host=riscv64-unknown-linux-gnu
$ make
$ make install
```

You can set `CFLAGS` as appropriate to change the RISC-V options, such as which
standard extensions and ABI to use. For example, to use the 64-bit soft float
ABI:

```console
$ CFLAGS="-mabi=lp64" ./autogen.sh --prefix=$RISCV/sysroot --host=riscv64-unknown-linux-gnu
```

Note that, since this is a cross build, the `mcs` directory won't be built. You
will have to build the managed libraries and tools through a native build of
Mono and copy them into `$RISCV/sysroot`.

You can run Mono with QEMU like this:

```console
$ qemu-riscv64 $RISCV/sysroot/bin/mono hello.exe
```

## Debugging

```console
$ qemu-riscv64 -g 12345 ./mono/mini/mono-sgen --interp basic.exe &
$ riscv64-unknown-elf-gdb -ex 'target remote localhost:12345' -ex 'b main' -ex 'c' ./mono/mini/mono-sgen
```

## Things to be done

In no particular order:

* Complete the soft float port.
* Complete the 32-bit port.
* Add unwind info to trampolines.
* Implement AOT support.
* Implement interpreter support.
* Implement LLVM support.
* Implement SDB support.
* Implement `dyn_call` support.
* Ensure all runtime tests pass.
* Ensure all corlib tests pass.
* Set up CI on Jenkins.
