#include "stdafx.h"
#include "BlockReader.h"
#include "CommonFunc.h"
#include "Const.h"
#include <locale>
#include <codecvt>



CBlockReader::CBlockReader(void)
{
}


CBlockReader::~CBlockReader(void)
{
}

//---------------------------------------------------
// Static functions
//---------------------------------------------------
#define READ_BUF_SIZE	(1024)
int CBlockReader::ReadFile(LPCSTR fname ,std::vector<std::wstring>& vLines )
{
	char buf[READ_BUF_SIZE];
	int wBufSize = READ_BUF_SIZE*2;

	wchar_t wChar[ READ_BUF_SIZE ];

	vLines.clear();

	FILE* fp;
	errno_t err = fopen_s(&fp, fname, "r");
	if( err!=0){
		return -1;
	}

	std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t> cv;

	while( fgets( buf, sizeof(buf), fp) ){
		std::string wk(buf);

		MultiByteToWideChar( CP_ACP, 0, buf, -1, wChar, wBufSize);
		std::wstring wStr(wChar);
		trim(wStr);
		vLines.push_back( wStr );
	}

	if( fp) {fclose(fp);}

	return 0;
}

int CBlockReader::ReadBlockItem(std::vector<std::wstring>& vLines,
							//std::vector<CConfigBlock*>& vBlockItems
							CDataBlock& dataBlock
							)
{
	char startBlock	= TAG_BLOCK_START;
	char endBlock	= TAG_BLOCK_END;
	std::string keyName="";
	std::string keyBlockSpaceToComment="";
	std::string keyBlockComment="";

	int nBlockCnt=0;
	std::vector<CConfigBlock*>& vBlockItems = dataBlock.GetDataBlockItems();
	vBlockItems.clear();
	size_t index=0;
	std::wstring_convert<std::codecvt_utf8<wchar_t>, wchar_t> cv;
	std::vector<std::wstring> vBlockLines;

	for( index=0; index< vLines.size(); index++){

		std::wstring wStrOrg( vLines[index] );
		std::wstring wStr( vLines[index] );

		std::wstring sComment;
		std::wstring sSpaceToComment;
		RemoveComment( wStr, sComment, sSpaceToComment);

		trim(wStr);
		trim(sComment);

		if( wStr.size()==0 && sComment.size()==0){
			continue;
		}

		if( nBlockCnt==0)
		{
			if( wStr.find(startBlock)!=std::string::npos){
				nBlockCnt++;

			}else{

				if(wStr.size()==0){
					if(sComment.size()>0){
						CConfigBlock* pBlockValue = new CDataBlockValue();
						((CDataBlockValue*)pBlockValue)->SetComment( sComment.c_str(), sSpaceToComment.c_str() );
						vBlockItems.push_back( pBlockValue );
					}
				}else{
					if( wStr.find(L"=")==std::string::npos){
						//"{"の直前の文字をブロックのキー文字とする

						keyName = wide_to_multi(wStr);
						keyBlockComment = wide_to_multi(sComment);
						keyBlockSpaceToComment = wide_to_multi(sSpaceToComment);
					}else{

						
						auto idx = wStr.find(L"=");
						if (idx == std::wstring::npos)
						{	//書式エラー
							return -1;
						}
						std::wstring key = wStr.substr(0, idx);
						std::wstring value = wStr.substr(idx+1);

						CConfigBlock* pBlockValue = new CDataBlockValue();
						((CDataBlockValue*)pBlockValue)->SetValue( 
																key.c_str(),
																value.c_str(),
																sSpaceToComment.c_str(),
																sComment.c_str()
																);

						vBlockItems.push_back( pBlockValue );
					}
				}

			}

		}else{
			//ブロック処理中

			if( wStr.find(startBlock)!=std::string::npos){
				nBlockCnt++;

			}else if( wStr.find(endBlock)!=std::string::npos)
			{
				nBlockCnt--;
				if( nBlockCnt==0){
					//ブロック
					CConfigBlock* pBlockValue = new CDataBlock();
					//pBlockValue->m_blockItemtype = BLOCKITEM_TYPE_BLOCK;
					((CDataBlock*)pBlockValue)->Build( keyName, keyBlockComment, keyBlockSpaceToComment, vBlockLines );


					vBlockItems.push_back( pBlockValue );
					vBlockLines.clear();

					continue;
				}
			}
			vBlockLines.push_back( wStrOrg );

		}

	}
	return 0;
}

