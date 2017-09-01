using System;
using System.Runtime.InteropServices;

namespace RunTime
{
	public static class User32
	{
		private static WindowsDll _instance = new WindowsDll("user32.dll");
	}
}