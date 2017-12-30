using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Unix {
	internal class NativeImports {
		public const int STDIN_FILENO = 0;
		public const int STDOUT_FILENO = 1;
		public const int STDERR_FILENO = 2;

		public const int O_RDONLY = 0x0000;
		public const int O_WRONLY = 0x0001;
		public const int O_RDWR = 0x0002;
		public const int O_CREAT = 0x0100;

		public const int RTLD_LOCAL = 0;
		public const int RTLD_LAZY = 0x00001;
		public const int RTLD_NOW = 0x00002;
		public const int RTLD_NOLOAD = 0x00004;
		public const int RTLD_DEEPBIND = 0x00008;
		public const int RTLD_GLOBAL = 0x00100;
		public const int RTLD_NODELETE = 0x01000;

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr fopen(string filename, string mode);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int fclose(IntPtr handle);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int open([MarshalAs(UnmanagedType.LPStr)] string filename, int oflag, int pmode = 0);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int close(int fd);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int dup2(int oldfd, int newfd);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int dup(int oldfd);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr fdopen(int fd, string mode);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int fileno(IntPtr stream);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int fputs(string str, IntPtr stream);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int putc(int ch, IntPtr stream);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int vfprintf(IntPtr stream, string format, IntPtr args);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int vsprintf(
			StringBuilder buffer,
			string format,
			IntPtr ptr);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		public static extern int vsnprintf(StringBuilder sb, uint n, string format, IntPtr args);

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr mktemp(IntPtr template);

		public static string mktemp(string template) {
			IntPtr templatePtr = Marshal.StringToHGlobalAnsi(template);
			IntPtr resultPtr = mktemp(templatePtr);

			string result = null;
			if (resultPtr != IntPtr.Zero)
				result = Marshal.PtrToStringAnsi(resultPtr);

			Marshal.FreeHGlobal(templatePtr);
			return result;
		}

		[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlopen(string filename, int flags);

		[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr dlsym(IntPtr handle, string symbol);

		[DllImport("libdl.so", CallingConvention = CallingConvention.Cdecl)]
		public static extern int dlclose(IntPtr handle);
	}
}
