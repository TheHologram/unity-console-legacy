#pragma once

struct AssemblyRef
{
	char assembly[256];
	char classname[256];
	char method[256];
	int enable;
};