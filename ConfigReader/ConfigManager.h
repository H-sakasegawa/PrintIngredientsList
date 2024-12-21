#pragma once

#include <vector>
#include "DataBlock.h"

class CConfigManager
{
public:
	CConfigManager(void);
	virtual ~CConfigManager(void);

	void Init();

	int ReadConfig( LPCSTR fname );
	int WriteConfig( LPCSTR fname );

	UINT GetItemCount();
	UINT GetBlockCount();
	CConfigBlock* GetBlock(UINT iItem);
	CDataBlockValue* GetValueItem(UINT iItem);
	CConfigBlock* GetBlock(LPCSTR szKey, char sep='\\');

	CConfigBlock* AddBlock( LPCSTR szKey, char sep='\\');
	int SetValue( LPCSTR szKey, LPCSTR szValue, char sep='\\');
	int RemoveBlock( UINT iItem );

protected:
	//std::vector<CConfigBlock*> m_vDataItems;

	CDataBlock m_rootBlock;

	UINT GetCount(ENM_BLOCKITEM_TYPE type);
	CConfigBlock* GetItem(ENM_BLOCKITEM_TYPE type, UINT iItem);

};

