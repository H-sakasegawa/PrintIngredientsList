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

	//�u���b�N�A�C�e��
	std::vector<CConfigBlock*>& GetDataBlockItems() {return m_vDataItems;}

	//�u���b�N���̃A�C�e�����ڐ��擾
//	/*�폜*/UINT GetBlockItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return GetItemCount(type);}
	UINT GetItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL);
	UINT GetItemIndexes(int* pIndexes, int maxSize, ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL);

	
	//�u���b�N���̃A�C�e���I�u�W�F�N�g�擾
//	/*�폜*/CConfigBlock* GetBlockItem(UINT idx){ return GetItem(idx); }
//	/*�폜*/CConfigBlock* GetBlockItem(LPCSTR szKey){ return GetItem(szKey); }
	CConfigBlock* GetItem(UINT idx){ return m_vDataItems[idx]; }
	CConfigBlock* GetItem(UINT idxByKey, LPCSTR keyName);
	CConfigBlock* GetItem(LPCSTR szKey);


	//�u���b�N���̃u���b�N�^�A�C�e�����ڐ��擾
	UINT GetBlockCount(LPCSTR keyName=NULL){return GetItemCount(BLOCKITEM_TYPE_BLOCK, keyName);}
	UINT GetBlockIndexes(LPCSTR keyName, int* indexes, int size){return GetItemIndexes(indexes, size, BLOCKITEM_TYPE_BLOCK, keyName);}

	//�u���b�N����BLOCK�^�A�C�e���I�u�W�F�N�g�擾
	CConfigBlock* GetBlock(UINT idx);
	CConfigBlock* GetBlock(UINT idxByKey, LPCSTR keyName);
	CConfigBlock* GetBlock(LPCSTR keyName);
	//�u���b�N�̒ǉ�
	CConfigBlock* AddBlock( LPCSTR szKey);
	int RemoveBlock(UINT idx);

	//�u���b�N����VALUE�^���ڐ��擾
	UINT GetValueCount(){return GetItemCount(BLOCKITEM_TYPE_VALUE);}
	//�u���b�N����VALUE�^��Key������擾
	LPCSTR GetValueKey(UINT idx);
	//�u���b�N����VALUE�`�f�[�^�̒l�擾
	LPCSTR GetValue(UINT idx, LPCSTR szDefault=NULL);
	LPCSTR GetValue(LPCSTR szKey, LPCSTR szDefault=NULL);
	//�u���b�N����VALUE�`�f�[�^�̒l�ݒ�
	int SetValue(UINT idx, LPCSTR szValue);
	int SetValue(LPCSTR szkey, LPCSTR szValue);


protected:

	//�u���b�N��(BlockValue)
	std::string m_sBlockValue;
	//�u���b�N��1�s�ݒ荀�ڂ� { } �ň͂܂ꂽ�T�u�u���b�N����
	std::vector<CConfigBlock*> m_vDataItems;

	//CDataBlockValue* GetValueItem(UINT idx){ return &m_vValues[idx];}


};


class CDataBlockValue : public CConfigBlock
{
public:
	CDataBlockValue(void);
	~CDataBlockValue(void);

	int WriteFile( FILE *fp, int layer );

	//���u���b�N���̐ݒ�l�֘A
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