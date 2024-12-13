#pragma once

#include "ConfigReaderIF.h"
using namespace System;


namespace Wrapper
{
	public ref class ConfigReaderIFWrapper
	{

	internal:
		CConfigReaderIF* pNative;

	public:
		ConfigReaderIFWrapper(){ pNative = new CConfigReaderIF();}
		~ConfigReaderIFWrapper(){ this->!ConfigReaderIFWrapper();}
		!ConfigReaderIFWrapper(){ delete pNative;}

		int ReadConfigFile( string configFilePath ){ return pNative->ReadConfigFile( configFilePath );}

		uint GetBlockCount(){ return pNative->GetBlockCount( );}
		CConfigBlock* GetBlock(uint idx){ return pNative->GetBlock( idx );}
		CConfigBlock* GetBlock(string keyName){ return pNative->GetBlock( keyName );}

	};

}