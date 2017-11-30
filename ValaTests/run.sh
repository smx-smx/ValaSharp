#!/bin/bash
../ValaCompiler/bin/Debug/ValaCompiler.exe \
	--path=/mingw64/bin \
	--vapidir=/mingw64/share/vala-0.38/vapi \
	--pkg gio-2.0 \
	"$@"
