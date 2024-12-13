#include "stdafx.h"
#include "ConfigReaderIF.h"
#include "ConfigManager.h"


CConfigReaderIF::CConfigReaderIF(void)
{
	//m_pConfigMng = NULL;
	m_pConfigMng = new CConfigManager();
}


CConfigReaderIF::~CConfigReaderIF(void)
{
	if(m_pConfigMng){
		delete (CConfigManager*)m_pConfigMng;
		m_pConfigMng=NULL;
	}
}

int CConfigReaderIF::ReadConfigFile( LPCSTR configFilePath )
{
	if( !m_pConfigMng){	return -1;}

	return ((CConfigManager*)m_pConfigMng)->ReadConfig( configFilePath );


}
int CConfigReaderIF::WriteConfigFile( LPCSTR configFilePath )
{
	if( !m_pConfigMng){	return -1;}

	return ((CConfigManager*)m_pConfigMng)->WriteConfig( configFilePath );


}

UINT CConfigReaderIF::GetBlockCount( )
{	
	if( !m_pConfigMng){	return NULL;}

	CConfigManager* pMng = (CConfigManager*)m_pConfigMng;
	return pMng->GetBlockCount();	
}
CConfigBlock* CConfigReaderIF::GetBlock(UINT idx)
{	
	if( !m_pConfigMng){	return NULL;}

	CConfigManager* pMng = (CConfigManager*)m_pConfigMng;
	return (CConfigBlock*)(pMng->GetBlock( idx ));	
}

CConfigBlock* CConfigReaderIF::GetBlock(LPCSTR keyName)
{	
	if( !m_pConfigMng){	return NULL;}

	CConfigManager* pMng = (CConfigManager*)m_pConfigMng;
	return (CConfigBlock*)(pMng->GetBlock( keyName ));	
}

CConfigBlock* CConfigReaderIF::AddBlock( LPCSTR keyName )
{
	if( !m_pConfigMng){	return NULL;}

	CConfigManager* pMng = (CConfigManager*)m_pConfigMng;
	return (CConfigBlock*)(pMng->AddBlock( keyName ));	

}
int CConfigReaderIF::RemoveBlock( UINT idx )
{
	if( !m_pConfigMng){	return NULL;}

	CConfigManager* pMng = (CConfigManager*)m_pConfigMng;
	return pMng->RemoveBlock( idx );	

}

int CConfigReaderIF::SetValue(LPCSTR szkey, LPCSTR szValue)
{
	if( !m_pConfigMng){	return NULL;}

	CConfigManager* pMng = (CConfigManager*)m_pConfigMng;
	return pMng->SetValue( szkey, szValue );	

}


////////////////////////////////////////////////////////



extern "C" CONFIGREADERDLL_API CConfigReaderIF*  CreateClass()
{
	CConfigReaderIF* obj = new CConfigReaderIF();
	//obj->m_pConfigMng = new CConfigManager();
	return obj;
}

extern "C" CONFIGREADERDLL_API void  DestroyClass(CConfigReaderIF *pObj )
{
	if(pObj) delete pObj;
}
