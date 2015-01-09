#include "pch.h"

int wmain(int argc, TCHAR* argv[])
{
	std::wstring styleFile;
	std::wstring outDir;

	if (argc >= 2)
	{
		styleFile = argv[1];

		if (argc >= 3)
		{
			outDir = argv[2];
		}
	}
	else
	{
	}

	return 0;
}
