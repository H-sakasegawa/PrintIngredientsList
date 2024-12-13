#include "stdafx.h"
#include "BlockWriter.h"

#include <ImageHlp.h>

CBlockWriter::CBlockWriter(void)
{
}


CBlockWriter::~CBlockWriter(void)
{
}
//---------------------------------------------------
// Static functions
//---------------------------------------------------
#define READ_BUF_SIZE	(1024)
int CBlockWriter::WriteFile(LPCSTR fname ,CDataBlock& blockData )
{
	char buf[READ_BUF_SIZE];
	int wBufSize = READ_BUF_SIZE*2;


	//出力先フォルダの作成
	std::string sFolderPath = fname;
	sFolderPath = sFolderPath.substr(0, sFolderPath.find_last_of("\\")+1);

	if(!MakeSureDirectoryPathExists(sFolderPath.c_str())){
		return -1;
	}

	FILE* fp;
	errno_t err = fopen_s(&fp, fname, "wt");
	if( err!=0){
		return -1;
	}


	UINT layer=0;
	blockData.WriteFile(fp, layer);

	if( fp) {fclose(fp);}

	return 0;
}