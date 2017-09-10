using System;
using System.Runtime.InteropServices;

namespace RunTime.Windows
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct WNDCLASSEX
	{
		public int cbSize;
		public int style;
		public IntPtr lpfnWndProc; // not WndProc
		public int cbClsExtra;
		public int cbWndExtra;
		public IntPtr hInstance;
		public IntPtr hIcon;
		public IntPtr hCursor;
		public IntPtr hbrBackground;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszMenuName;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string lpszClassName;
		public IntPtr hIconSm;
	}

	public struct POINT
	{
		public int x;
		public int y;
	}

	public struct MSG
	{
		public IntPtr hwnd;
		public int message;
		public int wParam;
		public int lParam;
		public int time;
		public POINT pt;
	}
}