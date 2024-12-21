#pragma once
#include <string>
#include <vector>
#include <map>

#include "DataBlock.h"

class CBlockReader
{
public:
	CBlockReader(void);
	~CBlockReader(void);

	FILE* fp;
	static int ReadFile(LPCSTR fname ,std::vector<std::wstring>& vLines );


	static int ReadBlockItem(std::vector<std::wstring>& vLines,
							//std::vector<CConfigBlock*>& vBlockItems							
							CDataBlock& dataBlock);
};

