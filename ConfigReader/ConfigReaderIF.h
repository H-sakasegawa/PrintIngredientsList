#pragma once


#ifdef CONFIGREADER_EXPORTS
#define CONFIGREADERDLL_API __declspec(dllexport)
#else
#define CONFIGREADERDLL_API __declspec(dllimport)
#endif

#include <string>

enum ENM_BLOCKITEM_TYPE
{
	BLOCKITEM_TYPE_NONE=-1,
	BLOCKITEM_TYPE_COMMENT=0,
	BLOCKITEM_TYPE_VALUE,
	BLOCKITEM_TYPE_BLOCK
};

class CConfigBlock;

class CConfigReaderIF
{
public:
	CConfigReaderIF(void);
	virtual ~CConfigReaderIF(void);

	//��`�t�@�C���ǂݍ���
	virtual int ReadConfigFile( LPCSTR configFilePath );
	//��`�t�@�C�������o��
	virtual int WriteConfigFile( LPCSTR configFilePath );

	//���[�g�u���b�N���擾
	virtual UINT GetBlockCount();

	//���[�g�u���b�N�I�u�W�F�N�g�擾
	virtual CConfigBlock* GetBlock(UINT idx);
	virtual CConfigBlock* GetBlock(LPCSTR keyName);

	virtual CConfigBlock* AddBlock( LPCSTR keyName );
	virtual int SetValue( LPCSTR szKey, LPCSTR szValue );
	virtual int RemoveBlock( UINT idx );


public:
	HMODULE m_hModule;
	void *m_pConfigMng;
};



class CConfigBlock
{
public:
	virtual ~CConfigBlock(){}

	//�u���b�N���擾�E�ݒ�
	virtual LPCSTR GetName(){return NULL;}
	virtual LPCSTR GetComment(){return NULL;}
	virtual LPCSTR GetSpaceToComment() { return NULL; }
	virtual LPCSTR GetBlockValue() { return NULL; }
	virtual int SetName(LPCSTR szKey) { return 0; };
	virtual int SetComment(LPCSTR szComment) { return 0; }

	//�u���b�N���̃A�C�e�����ڐ��擾
//	/*�폜*/virtual UINT GetBlockItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return 0;}
	virtual UINT GetItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return 0;}
	//�u���b�N���̃A�C�e���̓o�^��Index����擾
	virtual UINT GetItemIndexes(int* pIndexes, int maxSize, ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return 0;}

	//�u���b�N���̃A�C�e���I�u�W�F�N�g�擾
//	/*�폜*/virtual CConfigBlock* GetBlockItem(UINT idx){return NULL;}
//	/*�폜*/virtual CConfigBlock* GetBlockItem(LPCSTR szkey){return NULL;}
	virtual CConfigBlock* GetItem(UINT idx){return NULL;}
	virtual CConfigBlock* GetItem(UINT idxByKey, LPCSTR keyName){return NULL;}
	virtual CConfigBlock* GetItem(LPCSTR szkey){return NULL;}

	//�u���b�N����BLOCK�^�A�C�e�����ڐ��擾
	virtual UINT GetBlockCount(LPCSTR keyName=NULL){return 0;}
	virtual UINT GetBlockIndexes(LPCSTR keyName, int* indexes, int size){return 0;}
	//�u���b�N����BLOCK�^�A�C�e���I�u�W�F�N�g�擾
	virtual CConfigBlock* GetBlock(UINT idx){return NULL;}
	virtual CConfigBlock* GetBlock(UINT idxByKey, LPCSTR keyName){return NULL;}
	virtual CConfigBlock* GetBlock(LPCSTR keyName){return NULL;}
	//�u���b�N�̒ǉ�
	virtual CConfigBlock* AddBlock( LPCSTR szKey){return NULL;}


	//�u���b�N����VALUE�^���ڐ��擾
	virtual UINT GetValueCount(){return 0;}
	//�u���b�N����VALUE�^��Key������擾
	virtual LPCSTR GeValueKey(UINT idx){return NULL;}
	//�u���b�N����VALUE�`�f�[�^�̒l�擾
	virtual LPCSTR GetValue(UINT idx, LPCSTR szDefault=NULL){return NULL;}
	virtual LPCSTR GetValue(LPCSTR szkey, LPCSTR szDefault=NULL){return NULL;}
	//�u���b�N����VALUE�`�f�[�^�̒l�ݒ�
	virtual int SetValue(LPCSTR szkey, LPCSTR szValue){return NULL;}

	
	//Reserve Function
	//�ȉ���CDataBlockValue�I�u�W�F�N�g�p�ł��B
	virtual LPCSTR GetValue(){return NULL;}
	virtual int WriteFile( FILE *fp, UINT& layer ){return 0;}

	//�u���b�N�A�C�e����
	std::string m_sKey;
	std::string m_sValue;
	std::string m_sSpaceToComment;
	std::string m_sComment;

	ENM_BLOCKITEM_TYPE m_blockItemtype;

};



#ifndef CONFIGREADER_EXPORTS

	//---------------------------------------------------------------------
	// DLL�̓Ǎ��݂�IF�I�u�W�F�N�g����
	//---------------------------------------------------------------------
	//����
	//	(I/ )dllName	... �Ώ�DLL����
	//�߂�l
	//	!=NULL...IF�I�u�W�F�N�g
	//�@ =NULL...�ُ�
	//---------------------------------------------------------------------
	static CConfigReaderIF* CConfigReaderAttachDll( LPCSTR dllName)
	{
	
		HMODULE hDll = LoadLibrary(dllName);
		if( hDll==NULL ){
			return NULL;
		}

		typedef CConfigReaderIF* ( *class_factory)();
		class_factory factory_func = (class_factory)::GetProcAddress(hDll, "CreateClass");
		if (!factory_func) {
			::FreeLibrary(hDll);
			return NULL;
		}


		CConfigReaderIF* pIF = factory_func();
		pIF->m_hModule = hDll;

		return pIF;

	}

	//---------------------------------------------------------------------
	// IF�I�u�W�F�N�g�̉����DLL��Unload
	//---------------------------------------------------------------------
	//����
	//	(I/ )CConfigReaderIF	... IF�I�u�W�F�N�g
	//�߂�l
	//	�Ȃ�
	//---------------------------------------------------------------------
	static void ConfigReaderDetachDll( CConfigReaderIF* pIF )
	{
		if( pIF==NULL){
			return;
		}
		typedef void ( *class_factory)(CConfigReaderIF*);
		class_factory destroy_func = (class_factory)::GetProcAddress((pIF)->m_hModule, "DestroyClass");
		HMODULE hModule = (pIF)->m_hModule;
		if (destroy_func) {
       
			destroy_func( pIF );
			
		}

		FreeLibrary( hModule );

	}

#endif
