using System;
using Reign.Core;

namespace Demo
{
	static class Program
	{
		#if WINDOWS
		[STAThread]
		#elif METRO
		[MTAThread]
		#endif
		static void Main()
		{
			OS.Run(new MainApp(), 0);
		}
	}
}
