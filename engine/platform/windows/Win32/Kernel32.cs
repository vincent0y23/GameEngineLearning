using System;
using System.Runtime.InteropServices;

namespace RunTime.Windows.Win32
{
	public class Kernel32
	{
		[DllImport("kernel32")]
		public static extern IntPtr LoadLibrary(string fileName);
		[DllImport("kernel32")]
		public static extern IntPtr GetProcAddress(IntPtr module, string procName);
		[DllImport("kernel32")]
		public static extern int FreeLibrary(IntPtr module);
		[DllImport("kernel32")]
		public static extern uint GetLastError();
	}
}