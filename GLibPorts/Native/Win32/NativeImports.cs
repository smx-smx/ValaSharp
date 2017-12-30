using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Win32 {
	internal class NativeImports {
		public const int STD_INPUT_HANDLE = -10;
		public const int STD_OUTPUT_HANDLE = -11;
		public const int STD_ERROR_HANDLE = -12;

		public const int _O_RDONLY = 0x0000;
		public const int _O_WRONLY = 0x0001;
		public const int _O_RDWR = 0x0002;
		public const int _O_APPEND = 0x0008;
		public const int _O_CREAT = 0x0100;
		public const int _O_TRUNC = 0x0200;
		public const int _O_EXCL = 0x0400;
		public const int _O_TEXT = 0x4000;
		public const int _O_BINARY = 0x8000;
		public const int _O_WTEXT = 0x10000;
		public const int _O_U16TEXT = 0x20000;
		public const int _O_U8TEXT = 0x40000;

		public const int _IOFBF = 0x0000;
		public const int _IOLBF = 0x0040;
		public const int _IONBF = 0x0004;

		public const int DUPLICATE_SAME_ACCESS = 0x00000002;


		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr fopen(string filename, string mode);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int fclose(IntPtr handle);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int _open_osfhandle(IntPtr osfhandle, int flags);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr _fdopen(int fd, string mode);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int vscanf_s(string format, IntPtr va_args);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int _close(int fd);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle,
			IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle,
			uint dwDesiredAccess, bool bInheritHandle, uint dwOptions);


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hObject);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int putc(int ch, IntPtr stream);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int setvbuf(IntPtr stream, string buffer, int mode, ulong size);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int _fileno(IntPtr stream);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int fputs(string str, IntPtr stream);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FreeLibrary(IntPtr hModule);


		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int vfprintf_s(IntPtr stream, string format, IntPtr args);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int vsprintf(
			StringBuilder buffer,
			string format,
			IntPtr ptr);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int _vscprintf(
			string format,
			IntPtr ptr);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		public static extern int _open([MarshalAs(UnmanagedType.LPStr)] string filename, int oflag, int pmode = 0);

		[DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _mktemp(IntPtr template);

		public static string mktemp(string template) {
			IntPtr templatePtr = Marshal.StringToHGlobalAnsi(template);
			IntPtr resultPtr = _mktemp(templatePtr);

			string result = null;
			if (resultPtr != IntPtr.Zero)
				result = Marshal.PtrToStringAnsi(resultPtr);

			Marshal.FreeHGlobal(templatePtr);
			return result;
		}
	}
}
