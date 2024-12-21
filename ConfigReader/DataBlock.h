#pragma once
#include <vector>
#include <string>
#include <map>

#include "ConfigReaderIF.h"


class CDataBlock : public CConfigBlock
{
public:
	CDataBlock(void);
	~CDataBlock(void);

	int Build( std::string sKey, std::string sComment, std::string sSpaceToComment, std::vector<std::wstring>& vLines);
	int WriteFile( FILE *fp, UINT& layer );

	LPCSTR GetName() { return m_sKey.c_str(); }
	LPCSTR GetValue() { return m_sBlockValue.c_str(); }
	LPCSTR GetComment(){return m_sComment.c_str();}
	LPCSTR GetSpaceToComment(){return m_sSpaceToComment.c_str();}
	LPCSTR GetBlockValue() { return m_sBlockValue.c_str(); }
	int SetName(LPCWSTR szKey);
	int SetName(LPCSTR szKey);
	int SetComment(LPCWSTR szComment,LPCWSTR szSpaceToComment=L"");
	int SetComment( LPCSTR szComment,LPCSTR szSpaceToComment="");

	//ブロックアイテム
	std::vector<CConfigBlock*>& GetDataBlockItems() {return m_vDataItems;}

	//ブロック内のアイテム項目数取得
//	/*削除*/UINT GetBlockItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return GetItemCount(type);}
	UINT GetItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL);
	UINT GetItemIndexes(int* pIndexes, int maxSize, ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL);

	
	//ブロック内のアイテムオブジェクト取得
//	/*削除*/CConfigBlock* GetBlockItem(UINT idx){ return GetItem(idx); }
//	/*削除*/CConfigBlock* GetBlockItem(LPCSTR szKey){ return GetItem(szKey); }
	CConfigBlock* GetItem(UINT idx){ return m_vDataItems[idx]; }
	CConfigBlock* GetItem(UINT idxByKey, LPCSTR keyName);
	CConfigBlock* GetItem(LPCSTR szKey);


	//ブロック内のブロック型アイテム項目数取得
	UINT GetBlockCount(LPCSTR keyName=NULL){return GetItemCount(BLOCKITEM_TYPE_BLOCK, keyName);}
	UINT GetBlockIndexes(LPCSTR keyName, int* indexes, int size){return GetItemIndexes(indexes, size, BLOCKITEM_TYPE_BLOCK, keyName);}

	//ブロック内のBLOCK型アイテムオブジェクト取得
	CConfigBlock* GetBlock(UINT idx);
	CConfigBlock* GetBlock(UINT idxByKey, LPCSTR keyName);
	CConfigBlock* GetBlock(LPCSTR keyName);
	//ブロックの追加
	CConfigBlock* AddBlock( LPCSTR szKey);
	int RemoveBlock(UINT idx);

	//ブロック内のVALUE型項目数取得
	UINT GetValueCount(){return GetItemCount(BLOCKITEM_TYPE_VALUE);}
	//ブロック内のVALUE型のKey文字列取得
	LPCSTR GetValueKey(UINT idx);
	//ブロック内のVALUE形データの値取得
	LPCSTR GetValue(UINT idx, LPCSTR szDefault=NULL);
	LPCSTR GetValue(LPCSTR szKey, LPCSTR szDefault=NULL);
	//ブロック内のVALUE形データの値設定
	int SetValue(UINT idx, LPCSTR szValue);
	int SetValue(LPCSTR szkey, LPCSTR szValue);


protected:

	//ブロック名(BlockValue)
	std::string m_sBlockValue;
	//ブロック内1行設定項目と { } で囲まれたサブブロック情報列
	std::vector<CConfigBlock*> m_vDataItems;

	//CDataBlockValue* GetValueItem(UINT idx){ return &m_vValues[idx];}


};


class CDataBlockValue : public CConfigBlock
{
public:
	CDataBlockValue(void);
	~CDataBlockValue(void);

	int WriteFile( FILE *fp, int layer );

	//現ブロック内の設定値関連
	LPCSTR GetKey(){
		return m_sKey.c_str();}
	LPCSTR GetValue(){
		return m_sValue.c_str();}
	LPCSTR GetName(){return m_sKey.c_str();}

	void SetValue(LPCWSTR szkey, LPCWSTR szValue, LPCWSTR szSpaceToComment, LPCWSTR szComment);
	void SetValue(LPCSTR szKey, LPCSTR szValue, LPCSTR szSpaceToComment, LPCSTR szComment);
	void UpdateValue(LPCSTR szValue);


	void SetComment(LPCWSTR sComment, LPCWSTR sSpaceToComment=L"");
	void SetComment(LPCSTR sComment, LPCSTR sSpaceToComment="");
	LPCSTR GetComment(){return m_sComment.c_str();}
	LPCSTR GetSpaceToComment(){return m_sSpaceToComment.c_str();}


protected:

	//std::string m_sValue;
	//std::string m_sSpaceToComment;
	//std::string m_sComment;
	
};