# Mono RISC-V Port

These are some useful links and notes pertaining to the RISC-V port of Mono.

## Useful RISC-V documentation

* [RISC-V User-Level ISA Specification](https://riscv.org/specifications)
* [RISC-V Privileged ISA Specification](https://riscv.org/specifications/privileged-isa)
* [RISC-V Debug Specification](https://riscv.org/specifications/debug-specification)
* [RISC-V ELF psABI Specification](https://github.com/riscv/riscv-elf-psabi-doc)
* [RISC-V C API Specification](https://github.com/riscv/riscv-c-api-doc)
* [RISC-V Toolchain Conventions](https://github.com/riscv/riscv-toolchain-conventions)
* [RISC-V Assembly Programmer's Manual](https://github.com/riscv/riscv-asm-manual)

## Useful RISC-V repositories

* [RISC-V Organization](https://github.com/riscv)
  * [RISC-V GNU Toolchain](https://github.com/riscv/riscv-gnu-toolchain)
    * [RISC-V Binutils](https://github.com/riscv/riscv-binutils-gdb)
    * [RISC-V GCC](https://github.com/riscv/riscv-gcc)
    * [RISC-V Glibc](https://github.com/riscv/riscv-glibc)
    * [RISC-V Newlib](https://github.com/riscv/riscv-newlib)
  * [RISC-V Tools](https://github.com/riscv/riscv-tools)
    * [RISC-V Opcodes](https://github.com/riscv/riscv-opcodes)
    * [RISC-V Tests](https://github.com/riscv/riscv-tests)

## Setting up a cross environment

### Debian/Ubuntu packages

This is the most painless way of getting a functional toolchain installed. Note
that these packages may not be available on older distributions; this is tested
on Ubuntu 19.04.

First, add this to your `$HOME/.bashrc` (or some other script that you run):

```bash
export RISCV=$HOME/riscv
export PATH=$RISCV/bin:$PATH
export QEMU_LD_PREFIX=/usr/riscv64-linux-gnu
```

Next, install the toolchain packages like so:

```bash
# apt install autoconf automake binutils-riscv64-linux-gnu build-essential gcc-riscv64-linux-gnu gdb-multiarch g++-riscv64-linux-gnu libtool qemu qemu-system-misc qemu-user qemu-user-static
```

You will now have all the toolchain binaries in `/usr/bin`.

### Building manually

This approach may be somewhat unstable since you will be using the latest
versions of the various parts of the toolchain.

First, add this to your `$HOME/.bashrc` (or some other script that you run):

```bash
export RISCV=$HOME/riscv
export PATH=$RISCV/bin:$PATH
export QEMU_LD_PREFIX=$RISCV/sysroot
```

Next, install some dependencies needed to build the toolchain:

```console
# apt install autoconf automake autotools-dev curl libmpc-dev libmpfr-dev libgmp-dev gawk build-essential bison flex texinfo gperf libtool patchutils bc zlib1g-dev libexpat-dev
```

Finally, build the toolchain:

```console
$ git clone --recursive git@github.com:riscv/riscv-gnu-toolchain.git
$ cd riscv-gnu-toolchain
$ ./configure --prefix=$RISCV --enable-linux --enable-multilib --with-arch=rv64imafdc --with-abi=lp64d
$ make linux
```

This will install the built toolchain binaries in `$RISCV/bin`.

## Building Mono with a cross toolchain

Building Mono is quite straightforward:

```console
$ ./autogen.sh --prefix=$RISCV/sysroot --host=riscv64-linux-gnu
$ make
$ make install
```

(Note: You may need to use `--host=riscv64-unknown-linux-gnu` instead if you
built the toolchain manually.)

You can set `CFLAGS` as appropriate to change the RISC-V options, such as which
standard extensions and ABI to use. For example, to use the 64-bit soft float
ABI:

```console
$ CFLAGS="-mabi=lp64" ./autogen.sh --prefix=$RISCV/sysroot --host=riscv64-linux-gnu
```

Note that, since this is a cross build, the `mcs` directory won't be built. You
will have to build the managed libraries and tools through a native build of
Mono and copy them into `$RISCV/sysroot`.

You can run Mono with QEMU like this:

```console
$ qemu-riscv64 ./mono/mini/mono hello.exe
```

## Debugging

Debugging with GDB currently requires a manually built toolchain.

It can be done like so:

```console
$ qemu-riscv64 -g 12345 ./mono/mini/mono --interp basic.exe &
$ riscv64-unknown-elf-gdb -ex 'target remote localhost:12345' -ex 'b main' -ex 'c' ./mono/mini/mono
```

## Things to be done

Things that need to be done beyond the basic 64-bit port, in no particular
order:

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
