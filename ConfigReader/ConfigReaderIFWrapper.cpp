#include "stdafx.h"

#ifdef CSHARP_WRAPPER

#include "msclr\marshal.h"
#include "ConfigReaderIFWrapper.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace ConfigReader;

CConfigReaderIFWrapper::CConfigReaderIFWrapper()
{
	m_pIF = new CConfigReaderIF();
}

CConfigReaderIFWrapper::~CConfigReaderIFWrapper()
{
	this->!CConfigReaderIFWrapper();
}

CConfigReaderIFWrapper::!CConfigReaderIFWrapper()
{
	delete m_pIF;
}

int CConfigReaderIFWrapper::ReadConfigFile(String^ configFilePath)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(configFilePath);
	return m_pIF->ReadConfigFile(lpctstr);
}

int CConfigReaderIFWrapper::WriteConfigFile(String^ configFilePath)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(configFilePath);
	return m_pIF->WriteConfigFile(lpctstr);
}

int CConfigReaderIFWrapper::GetBlockCount()
{
	return m_pIF->GetBlockCount();;
}

CConfigBlockWrapper^ CConfigReaderIFWrapper::GetBlock(int idx)
{
	CConfigBlock* pBlock = m_pIF->GetBlock(idx);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigReaderIFWrapper::GetBlock(String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	CConfigBlock* pBlock = m_pIF->GetBlock(lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigReaderIFWrapper::AddBlock(String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	CConfigBlock* pBlock = m_pIF->AddBlock(lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

int CConfigReaderIFWrapper::SetValue(String^ szKey, String^ szValue)
{
	marshal_context context;
	LPCTSTR lpctstrKey = context.marshal_as<const TCHAR*>(szKey);
	LPCTSTR lpctstrValue = context.marshal_as<const TCHAR*>(szValue);
	return m_pIF->SetValue(lpctstrKey, lpctstrValue);
}

int CConfigReaderIFWrapper::RemoveBlock(int idx)
{
	return m_pIF->RemoveBlock(idx);
}

CConfigBlockWrapper::CConfigBlockWrapper(CConfigBlock* pBlock)
{
	m_pBlock = pBlock;
}

CConfigBlockWrapper::~CConfigBlockWrapper()
{
	this->!CConfigBlockWrapper();
}

CConfigBlockWrapper::!CConfigBlockWrapper()
{
}

String^ CConfigBlockWrapper::GetName()
{
	LPCSTR lpcstr = m_pBlock->GetName();
	return marshal_as<System::String^>(lpcstr);
}

String^ CConfigBlockWrapper::GetComment()
{
	LPCSTR lpcstr = m_pBlock->GetComment();
	return marshal_as<System::String^>(lpcstr);
}

int CConfigBlockWrapper::SetName(String^ szKey)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(szKey);
	return m_pBlock->SetName(lpctstr);
}

int CConfigBlockWrapper::SetComment(String^ szComment)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(szComment);
	return m_pBlock->SetComment(lpctstr);
}

int CConfigBlockWrapper::GetItemCount(BlockItemType type, String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	return m_pBlock->GetItemCount((ENM_BLOCKITEM_TYPE)type, lpctstr);
}

int CConfigBlockWrapper::GetItemIndexes([Out]int% pIndexes, int maxSize, BlockItemType type, String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	int iIndexes;
	UINT ret = m_pBlock->GetItemIndexes(&iIndexes, maxSize, (ENM_BLOCKITEM_TYPE)type, lpctstr);
	pIndexes = iIndexes;
	return ret;
}

CConfigBlockWrapper^ CConfigBlockWrapper::GetItem(int idx)
{
	CConfigBlock* pBlock = m_pBlock->GetItem(idx);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigBlockWrapper::GetItem(int idxByKey, String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	CConfigBlock* pBlock = m_pBlock->GetItem(idxByKey, lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigBlockWrapper::GetItem(String^ szkey)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(szkey);
	CConfigBlock* pBlock = m_pBlock->GetItem(lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

int CConfigBlockWrapper::GetBlockCount(String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	return m_pBlock->GetBlockCount(lpctstr);
}

int CConfigBlockWrapper::GetBlockIndexes(String^ keyName, [Out]int% indexes, int size)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	int iIndexes;
	UINT ret = m_pBlock->GetBlockIndexes(lpctstr, &iIndexes, size);
	indexes = iIndexes;
	return ret;
}

CConfigBlockWrapper^ CConfigBlockWrapper::GetBlock(int idx)
{
	CConfigBlock* pBlock = m_pBlock->GetBlock(idx);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigBlockWrapper::GetBlock(int idxByKey, String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	CConfigBlock* pBlock = m_pBlock->GetBlock(idxByKey, lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigBlockWrapper::GetBlock(String^ keyName)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(keyName);
	CConfigBlock* pBlock = m_pBlock->GetBlock(lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

CConfigBlockWrapper^ CConfigBlockWrapper::AddBlock(String^ szKey)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(szKey);
	CConfigBlock* pBlock = m_pBlock->AddBlock(lpctstr);
	if(!pBlock)
	{
		return nullptr;
	}
	return gcnew CConfigBlockWrapper(pBlock);
}

int CConfigBlockWrapper::GetValueCount()
{
	return m_pBlock->GetValueCount();
}

String^ CConfigBlockWrapper::GeValueKey(int idx)
{
	LPCSTR lpcstr = m_pBlock->GeValueKey(idx);
	return marshal_as<System::String^>(lpcstr);
}

String^ CConfigBlockWrapper::GetValue(int idx)
{
	LPCSTR lpcstr = m_pBlock->GetValue(idx);
	return marshal_as<System::String^>(lpcstr);
}

String^ CConfigBlockWrapper::GetValue(int idx, String^ szDefault)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(szDefault);
	LPCSTR lpcstr = m_pBlock->GetValue(idx, lpctstr);
	return marshal_as<System::String^>(lpcstr);
}

String^ CConfigBlockWrapper::GetValue(String^ szkey)
{
	marshal_context context;
	LPCTSTR lpctstr = context.marshal_as<const TCHAR*>(szkey);
	LPCSTR lpcstr = m_pBlock->GetValue(lpctstr);
	return marshal_as<System::String^>(lpcstr);
}

String^ CConfigBlockWrapper::GetValue(String^ szkey, String^ szDefault)
{
	marshal_context context;
	LPCTSTR lpctstrKey = context.marshal_as<const TCHAR*>(szkey);
	LPCTSTR lpctstrDefault = context.marshal_as<const TCHAR*>(szDefault);
	LPCSTR lpcstr = m_pBlock->GetValue(lpctstrKey, lpctstrDefault);
	return marshal_as<System::String^>(lpcstr);
}

String^ CConfigBlockWrapper::GetValue()
{
	LPCSTR lpcstr = m_pBlock->GetValue();
	return marshal_as<System::String^>(lpcstr);
}

int CConfigBlockWrapper::SetValue(String^ szkey, String^ szValue)
{
	marshal_context context;
	LPCTSTR lpctstrKey = context.marshal_as<const TCHAR*>(szkey);
	LPCTSTR lpctstrValue = context.marshal_as<const TCHAR*>(szValue);
	return m_pBlock->SetValue( lpctstrKey, lpctstrValue);
}

#endif
