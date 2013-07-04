using System;
using Reign.Core;

namespace MainDemo_Win32
{
	static class Program
	{
		#if WIN32
		[STAThread]
		#elif WINRT
		[MTAThread]
		#endif
		static void Main()
		{
			OS.Run(new MainApp(), 0);
		}
	}
}
