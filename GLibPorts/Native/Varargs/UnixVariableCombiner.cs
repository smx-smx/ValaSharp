using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GLibPorts.Native.Varargs {
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct UnixVaList {
		public uint gp_offset;
		public uint fp_offset;
		public IntPtr overflow_arg_area;
		public IntPtr reg_save_area;
	}

	public class UnixVariableCombiner : Win32VariableCombiner{
		private IntPtr _va_list_ptr;

		public UnixVariableCombiner(VariableArgument[] args) : base(args) {
		}

		public override void Build() {
			_disposables.Clear();

			UnixVaList va_list = new UnixVaList();
			if(IntPtr.Size == 8) {
				va_list.gp_offset = 48;	//6 registers * 8 bytes
				va_list.fp_offset = va_list.gp_offset + 256; //16 fp registers * 16 bytes
				va_list.reg_save_area = IntPtr.Zero;
				va_list.overflow_arg_area = base.GetPtr();
			}

			_va_list_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(va_list));
			Marshal.StructureToPtr(va_list, _va_list_ptr, false);
		}

		public override IntPtr GetPtr() {
			if (_va_list_ptr == IntPtr.Zero)
				Build();

			if (_disposed)
				throw new InvalidOperationException("Disposed already.");

			return _va_list_ptr;
		}

		public override void Dispose() {
			base.Dispose();

			if (_va_list_ptr != IntPtr.Zero)
				Marshal.FreeHGlobal(_va_list_ptr);
		}
	}
}
