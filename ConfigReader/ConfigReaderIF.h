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

	//定義ファイル読み込み
	virtual int ReadConfigFile( LPCSTR configFilePath );
	//定義ファイル書き出し
	virtual int WriteConfigFile( LPCSTR configFilePath );

	//ルートブロック数取得
	virtual UINT GetBlockCount();

	//ルートブロックオブジェクト取得
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

	//ブロック名取得・設定
	virtual LPCSTR GetName(){return NULL;}
	virtual LPCSTR GetComment(){return NULL;}
	virtual LPCSTR GetSpaceToComment() { return NULL; }
	virtual LPCSTR GetBlockValue() { return NULL; }
	virtual int SetName(LPCSTR szKey) { return 0; };
	virtual int SetComment(LPCSTR szComment) { return 0; }

	//ブロック内のアイテム項目数取得
//	/*削除*/virtual UINT GetBlockItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return 0;}
	virtual UINT GetItemCount(ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return 0;}
	//ブロック内のアイテムの登録先Index列情報取得
	virtual UINT GetItemIndexes(int* pIndexes, int maxSize, ENM_BLOCKITEM_TYPE type=BLOCKITEM_TYPE_NONE, LPCSTR keyName=NULL){return 0;}

	//ブロック内のアイテムオブジェクト取得
//	/*削除*/virtual CConfigBlock* GetBlockItem(UINT idx){return NULL;}
//	/*削除*/virtual CConfigBlock* GetBlockItem(LPCSTR szkey){return NULL;}
	virtual CConfigBlock* GetItem(UINT idx){return NULL;}
	virtual CConfigBlock* GetItem(UINT idxByKey, LPCSTR keyName){return NULL;}
	virtual CConfigBlock* GetItem(LPCSTR szkey){return NULL;}

	//ブロック内のBLOCK型アイテム項目数取得
	virtual UINT GetBlockCount(LPCSTR keyName=NULL){return 0;}
	virtual UINT GetBlockIndexes(LPCSTR keyName, int* indexes, int size){return 0;}
	//ブロック内のBLOCK型アイテムオブジェクト取得
	virtual CConfigBlock* GetBlock(UINT idx){return NULL;}
	virtual CConfigBlock* GetBlock(UINT idxByKey, LPCSTR keyName){return NULL;}
	virtual CConfigBlock* GetBlock(LPCSTR keyName){return NULL;}
	//ブロックの追加
	virtual CConfigBlock* AddBlock( LPCSTR szKey){return NULL;}


	//ブロック内のVALUE型項目数取得
	virtual UINT GetValueCount(){return 0;}
	//ブロック内のVALUE型のKey文字列取得
	virtual LPCSTR GeValueKey(UINT idx){return NULL;}
	//ブロック内のVALUE形データの値取得
	virtual LPCSTR GetValue(UINT idx, LPCSTR szDefault=NULL){return NULL;}
	virtual LPCSTR GetValue(LPCSTR szkey, LPCSTR szDefault=NULL){return NULL;}
	//ブロック内のVALUE形データの値設定
	virtual int SetValue(LPCSTR szkey, LPCSTR szValue){return NULL;}

	
	//Reserve Function
	//以下はCDataBlockValueオブジェクト用です。
	virtual LPCSTR GetValue(){return NULL;}
	virtual int WriteFile( FILE *fp, UINT& layer ){return 0;}

	//ブロックアイテム名
	std::string m_sKey;
	std::string m_sValue;
	std::string m_sSpaceToComment;
	std::string m_sComment;

	ENM_BLOCKITEM_TYPE m_blockItemtype;

};



#ifndef CONFIGREADER_EXPORTS

	//---------------------------------------------------------------------
	// DLLの読込みとIFオブジェクト生成
	//---------------------------------------------------------------------
	//引数
	//	(I/ )dllName	... 対象DLL名称
	//戻り値
	//	!=NULL...IFオブジェクト
	//　 =NULL...異常
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
	// IFオブジェクトの解放とDLLのUnload
	//---------------------------------------------------------------------
	//引数
	//	(I/ )CConfigReaderIF	... IFオブジェクト
	//戻り値
	//	なし
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
