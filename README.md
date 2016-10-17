# BOOTP Server

A simple BOOTP server to support and simplify UEFI application development.

Allows to run UEFI apps on remote PC over network directly from Visual Studio.

### Usage

From command line:

```
bootp <path\filename.efi>
```

In Visual Studio:

![A simple BOOTP server that allows to run UEFI apps on remote PC over network directly from Visual Studio](https://raw.githubusercontent.com/vurdalakov/bootp/master/img/debugging.png)

### Sample application

Included a simple UEFI `helloworld` C++ application to demonstrate usage of `BOOTP Server` from Visual Studio.

It can be compiled on Windows using Visual Studio 2015 for x86_32, x86_64 or ARM targets.
