#include "stdafx.h"
#include "ConfigManager.h"
#include "DataBlock.h"
#include "BlockReader.h"
#include "BlockWriter.h"
#include "CommonFunc.h"
#include <locale>
#include <codecvt>

CConfigManager::CConfigManager(void)
{
}


CConfigManager::~CConfigManager(void)
{
	Init();
}

void CConfigManager::Init()
{
	std::vector<CConfigBlock*>& vBlockItems = m_rootBlock.GetDataBlockItems();
	for( size_t i=0; i< vBlockItems.size(); i++){
		if( vBlockItems[i]) delete vBlockItems[i];
	}

	vBlockItems.clear();

}



int CConfigManager::ReadConfig( LPCSTR fname )
{
	std::vector<std::wstring> vLines;

	Init();

	CBlockReader::ReadFile(fname , vLines );

	std::vector<CConfigBlock*>& vBlockItems = m_rootBlock.GetDataBlockItems();
	vBlockItems.clear();

	CBlockReader::ReadBlockItem( vLines, m_rootBlock);
	return 0;
}

int CConfigManager::WriteConfig( LPCSTR fname )
{

	CBlockWriter::WriteFile(fname, m_rootBlock );

	return 0;
}

UINT CConfigManager::GetItemCount()
{ 
	return GetCount(BLOCKITEM_TYPE_VALUE);
}
UINT CConfigManager::GetBlockCount()
{ 	
	return GetCount(BLOCKITEM_TYPE_BLOCK);

}

UINT CConfigManager::GetCount(ENM_BLOCKITEM_TYPE type)
{
	std::vector<CConfigBlock*>& vBlockItems = m_rootBlock.GetDataBlockItems();
	UINT cnt=0;
	for(size_t i=0; i< vBlockItems.size(); i++){
		if( vBlockItems[i]->m_blockItemtype == type ){
			cnt++;
		}
	}
	return cnt;
}


CConfigBlock* CConfigManager::GetBlock(UINT iItem)
{
	return (CDataBlock*)GetItem(BLOCKITEM_TYPE_BLOCK, iItem);
}

CDataBlockValue* CConfigManager::GetValueItem(UINT iItem)
{
	return (CDataBlockValue*)GetItem(BLOCKITEM_TYPE_VALUE, iItem);
}
CConfigBlock* CConfigManager::GetItem(ENM_BLOCKITEM_TYPE type, UINT iItem)
{
	std::vector<CConfigBlock*>& vBlockItems = m_rootBlock.GetDataBlockItems();
	UINT idx=0;
	for(size_t i=0; i< vBlockItems.size(); i++){
		if( vBlockItems[i]->m_blockItemtype == type ){
			
			if( idx== iItem){
				return vBlockItems[i];
			}
			idx++;
		}
	}
	return NULL;
}

CConfigBlock* CConfigManager::GetBlock(LPCSTR sKey, char sep/*='\'*/)
{
#if 0
	std::vector<std::string> vKeys = StringSplit( sKey, sep);
	CConfigBlock* pBlock=NULL;
	for( int i=0; i<vKeys.size(); i++){

		int blockNum=0;
		if( pBlock==NULL){
			blockNum = GetBlockCount();
		}else{
			blockNum = pBlock->GetBlockCount();
		}

		BOOL bFind=FALSE;
		for( int iItem=0; iItem<blockNum; iItem++){


			if( pBlock==NULL){
				pBlock = GetBlock(iItem);
			}else{
				pBlock = pBlock->GetItem(iItem);
			}

			if( pBlock->GetName() == vKeys[i] ){
				//pBlock = (CDataBlock*)pBlockItem;
				bFind=TRUE;
				break;
			}
		}
		if( !bFind ){
			return NULL;
		}
	}
#else
	std::vector<std::string> vKeys = StringSplit( sKey, sep);
	
	CConfigBlock* pTargetBlock=NULL;
	CConfigBlock* pBlock=NULL;
	for(size_t i=0; i< vKeys.size(); i++){


		int blockNum=0;
		if( pTargetBlock==NULL){
			blockNum = GetBlockCount();
		}else{
			blockNum = pTargetBlock->GetItemCount();
		}

		BOOL bFind=FALSE;
		for( int iItem=0; iItem<blockNum; iItem++){


			if( pTargetBlock==NULL){
				pBlock = GetBlock(iItem);
			}else{
				pBlock = pTargetBlock->GetItem(iItem);
			}
			if( !pBlock){
				continue;
			}
			if( pBlock->GetName() == vKeys[i] ){
				//pBlock = (CDataBlock*)pBlockItem;
				bFind=TRUE;
				pTargetBlock = pBlock;
				break;
			}
		}
		if( !bFind ){
			return NULL;
		}
	}

#endif
	return pBlock;
}

CConfigBlock* CConfigManager::AddBlock( LPCSTR szKey, char sep/*='\'*/ )
{

	std::vector<std::string> vKeys = StringSplit( szKey, sep);
	CConfigBlock* pBlock=NULL;
	for(size_t i=0; i< vKeys.size(); i++){

		
		int blockNum=0;
		if( pBlock==NULL){
			blockNum = GetBlockCount();
		}else{
			blockNum = pBlock->GetBlockCount();
		}

		BOOL bFind=FALSE;
#if 0
		if( !bAddSameNameBlock){
			for( int iItem=0; iItem<blockNum; iItem++){


				if( pBlock==NULL){
					pBlock = GetBlock(iItem);
				}else{
					pBlock = pBlock->GetItem(iItem);
				}

				//登録済みブロックチェック
				if( pBlock->GetName() == vKeys[i]){
					pBlock = pBlock;
					bFind=TRUE;
					break;
				}
			}
		}
#endif
		if( !bFind ){
			//未登録ブロックを追加
			std::vector<CConfigBlock*>& vBlockItems = m_rootBlock.GetDataBlockItems();
			pBlock = new CDataBlock();
			pBlock->SetName(vKeys[i].c_str());
			vBlockItems.push_back(pBlock);
		}
	}


	return pBlock;
}

int CConfigManager::RemoveBlock( UINT iItem )
{
	std::vector<CConfigBlock*>& vBlockItems = m_rootBlock.GetDataBlockItems();
	UINT idx=0;
	for(size_t i=0; i<vBlockItems.size(); i++){
		if( vBlockItems[i]->m_blockItemtype == BLOCKITEM_TYPE_BLOCK ){
			
			if( idx== iItem){
				delete vBlockItems[i];

				vBlockItems.erase(vBlockItems.begin()+i);
				break;
			}
			idx++;
		}
	}
	return 0;
}


int CConfigManager::SetValue( LPCSTR szKey, LPCSTR szValue, char sep/*='\'*/ )
{
	std::vector<std::string> vKeys = StringSplit( szKey, sep);
	CConfigBlock* pBlock=NULL;

	if( vKeys.size()>1){
		std::string blockPath;
		//定義ブロックを追加
		for(size_t i=0; i<vKeys.size()-1; i++){
			if( blockPath.size()!=0){ 
				blockPath += sep;
			}
			blockPath += vKeys[i];
		}
		pBlock = AddBlock( blockPath.c_str(), sep );
	}
	return pBlock->SetValue( vKeys[ vKeys.size()-1 ].c_str(), szValue);
}


