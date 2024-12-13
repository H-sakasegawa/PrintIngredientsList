#pragma once
#include <string>
#include <vector>

#include "DataBlock.h"

class CBlockWriter
{
public:
	CBlockWriter(void);
	~CBlockWriter(void);

	static int WriteFile(LPCSTR fname ,CDataBlock& blockData );

};

