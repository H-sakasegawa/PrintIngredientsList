#include "stdafx.h"
#include "DataBlock.h"
#include "BlockReader.h"
#include "CommonFunc.h"

#include <locale>
#include <codecvt>

CDataBlock::CDataBlock(void)
{
	m_blockItemtype = BLOCKITEM_TYPE_BLOCK;
}


CDataBlock::~CDataBlock(void)
{
	for(size_t i=0; i<m_vDataItems.size(); i++){
		if(m_vDataItems[i]) delete  m_vDataItems[i];
	}
	m_vDataItems.clear();

}

int CDataBlock::Build( std::string sKey, std::string sComment, std::string sSpaceToComment,std::vector<std::wstring>& vLines)
{
	if (SetName(sKey.c_str()) != 0)
	{
		return -1;
	}
	SetComment(sComment.c_str(), sSpaceToComment.c_str());

	// xxxx = vvvvv,vvvvv を収集
	int rc = CBlockReader::ReadBlockItem(vLines, *this);
	if( rc<0){
		return -1;
	}

	return 0;
}


int CDataBlock::WriteFile( FILE *fp, UINT& layer )
{
	int rc=0;
	std::string sTab="";
	for(size_t i=0; i<layer; i++){
		sTab += '\t';
	}



	for(size_t i=0; i<m_vDataItems.size(); i++){

		CConfigBlock* pBlock = m_vDataItems[i];

		switch( m_vDataItems[i]->m_blockItemtype){
		case BLOCKITEM_TYPE_COMMENT:
		case BLOCKITEM_TYPE_VALUE:
			rc=((CDataBlockValue*)pBlock)->WriteFile(fp, layer);
			break;
		case BLOCKITEM_TYPE_BLOCK:
			++layer;
			std::string sComment = m_vDataItems[i]->GetComment();
			std::string sSpacrToComment = m_vDataItems[i]->GetSpaceToComment();
			std::string sBlockValue = m_vDataItems[i]->GetBlockValue();

			fprintf_s(fp, "%s%s", sTab.c_str(), m_vDataItems[i]->GetName());
			if (!sBlockValue.empty())
			{
				fprintf_s(fp, "(%s)", sBlockValue.c_str());
			}

			if( !sComment.empty()){
				fprintf_s(fp, "%s%s", sSpacrToComment.c_str(), sComment.c_str());
			}
			fprintf_s(fp, "\n");


			fprintf_s(fp, "%s{\n",sTab.c_str());
			rc=((CDataBlock*)pBlock)->WriteFile(fp, layer);

			fprintf_s(fp, "%s}\n",sTab.c_str());
			--layer;
			break;
		}
	}

	return rc;
}



int CDataBlock::SetName(LPCWSTR szKey)
{ 
	return SetName(wide_to_multi(szKey).c_str());
}
int CDataBlock::SetName(LPCSTR szKey)
{ 
	//AAAA(xxxxx) → m_sKey = AAAA  m_sBlockValue = xxxxxx

	LPSTR ptr = (LPSTR)strchr(szKey, '(');
	if (ptr!=NULL)
	{
		LPCSTR ptrValue = ptr+1; //xxxxxx) の先頭ポインタ
		(*ptr)  = '\0';
		m_sKey = szKey;
		
		ptr = (LPSTR)strchr(ptrValue, ')');
		if (ptr==NULL)
		{
			return -1;
		}
		*ptr = '\0';

		m_sBlockValue = ptrValue;
		trim(m_sKey);
		trim(m_sBlockValue);
	}
	else
	{
		m_sKey = szKey;
		trim(m_sKey);
	}

	return 0;
}
int CDataBlock::SetComment(LPCWSTR szComment, LPCWSTR szSpaceToComment/*=L""*/)
{
	return SetComment(wide_to_multi(szComment).c_str(), wide_to_multi(szSpaceToComment).c_str());
}
int CDataBlock::SetComment( LPCSTR szComment,LPCSTR szSpaceToComment/*=""*/)
{ 
	m_sComment = szComment;
	m_sSpaceToComment = szSpaceToComment;
	return 0;
}
		
//指定されたデータタイプの数を取得
UINT CDataBlock::GetItemCount(ENM_BLOCKITEM_TYPE type/*=BLOCKITEM_TYPE_NONE*/, LPCSTR keyName/*=NULL*/)
{ 
	if( type == BLOCKITEM_TYPE_NONE){
		return (UINT)m_vDataItems.size();
	}

	UINT cnt=0; 
	for( size_t i=0; i<m_vDataItems.size(); i++){

		if( m_vDataItems[i]->m_blockItemtype!=type){
			continue;
		}
		if( keyName ){
			if( _stricmp(m_vDataItems[i]->GetName(), keyName)!=0){
				continue;
			}
		}
		cnt++;
	}
	return cnt;
}

//指定されたタイプ、キー名のブロックのインデックス列情報を返す
//戻り値：取得インデックス数
UINT CDataBlock::GetItemIndexes(int* pIndexes, int maxSize, ENM_BLOCKITEM_TYPE type/*=BLOCKITEM_TYPE_NONE*/, LPCSTR keyName/*=NULL*/)
{ 

	int cnt=0; 
	for( size_t i=0; i<m_vDataItems.size(); i++){

		if( cnt>=maxSize){
			return cnt;
		}
		if( m_vDataItems[i]->m_blockItemtype!=type){
			continue;
		}
		if( keyName ){
			if( _stricmp(m_vDataItems[i]->GetName(), keyName)!=0){
				continue;
			}
		}
		pIndexes[cnt] = i;

		cnt++;
	}
	return cnt;
}

//ブロック内のアイテムオブジェクト取得
CConfigBlock* CDataBlock::GetItem(UINT idxByKey, LPCSTR keyName)
{
	int idxCnt=0;
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype != BLOCKITEM_TYPE_COMMENT ){

			if(pBlockItem->m_sKey == keyName){
				if(idxCnt==idxByKey ){
					return pBlockItem;
				}
				idxCnt++; //指定されたKeyNameに合致するブロックの場合にのみカウントアップ
			}
		}

	}
	return NULL;
}
//ブロック内のアイテムオブジェクト取得
CConfigBlock* CDataBlock::GetItem(LPCSTR szKey)
{
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype != BLOCKITEM_TYPE_COMMENT ){

			if(pBlockItem->m_sKey == szKey ){
				return pBlockItem;
			}
		}

	}
	return NULL;
}

//ブロック内のBLOCK型アイテムオブジェクト取得
CConfigBlock* CDataBlock::GetBlock(UINT idx)
{
	if( idx>=GetBlockCount()){
		return NULL;
	}
	int iBlock=0;
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_BLOCK ){

			if(idx == iBlock ){
				return (CConfigBlock*)pBlockItem;
			}
			iBlock++;
		}

	}
	return NULL;

}

CConfigBlock* CDataBlock::GetBlock(UINT idxByKey, LPCSTR keyName)
{
	int idxCnt=0;
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_COMMENT ){
			continue;
		}
		if( pBlockItem->m_blockItemtype != BLOCKITEM_TYPE_BLOCK ){
			continue;
		}

		if(pBlockItem->m_sKey == keyName){
			if(idxCnt==idxByKey ){
				return pBlockItem;
			}
			idxCnt++; //指定されたKeyNameに合致するブロックの場合にのみカウントアップ
		}
	}

	return NULL;

}
//ブロック内のBLOCK型アイテムオブジェクト取得
CConfigBlock* CDataBlock::GetBlock(LPCSTR szKey)
{
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_BLOCK ){

			if(pBlockItem->m_sKey == szKey ){
				return (CConfigBlock*)pBlockItem;
			}
		}

	}
	return NULL;
}

CConfigBlock* CDataBlock::AddBlock( LPCSTR szKey)
{
	CConfigBlock* pBlock=GetBlock( szKey );

	//ブロックを追加
	std::vector<CConfigBlock*>& vBlockItems = GetDataBlockItems();
	pBlock = new CDataBlock();
	pBlock->SetName(szKey);
	vBlockItems.push_back(pBlock);


	return pBlock;
}
int CDataBlock::RemoveBlock( UINT iItem )
{
	std::vector<CConfigBlock*>& vBlockItems = GetDataBlockItems();
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

//ブロック内のVALUE型のKey文字列取得
LPCSTR CDataBlock::GetValueKey(UINT idx)
{
	if( idx>=GetValueCount()){
		return NULL;
	}
	int iBlock=0;
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_VALUE ){

			if(idx == iBlock ){
				return ((CConfigBlock*)pBlockItem)->GetName();
			}
			iBlock++;
		}

	}
	return NULL;

}
//ブロック内のVALUE形データの値取得
LPCSTR CDataBlock::GetValue(UINT idx, LPCSTR szDefault/*=NULL*/)
{
	if( idx>=GetValueCount()){
		return NULL;
	}
	int iBlock=0;
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_VALUE ){

			if(idx == iBlock ){
				return ((CConfigBlock*)pBlockItem)->GetName();
			}
			iBlock++;
		}

	}
	if( szDefault ){
		return szDefault;
	}
	return NULL;

}
//ブロック内のVALUE形データの値取得
LPCSTR CDataBlock::GetValue(LPCSTR szKey, LPCSTR szDefault/*=NULL*/)
{
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_VALUE ){

			if(pBlockItem->m_sKey == szKey ){
				return ((CDataBlockValue*)pBlockItem)->GetValue();
			}
		}

	}
	if( szDefault ){
		return szDefault;
	}
	
	return NULL;

}


int CDataBlock::SetValue(UINT idx, LPCSTR szValue)
{
	if( idx>=GetValueCount()){
		return -1;
	}
	int iBlock=0;
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_VALUE ){

			if(idx == iBlock ){

				CDataBlockValue* pValueItem = (CDataBlockValue*)pBlockItem;
				pValueItem->UpdateValue( szValue );
			}
			iBlock++;
		}

	}
	return 0;
}
int CDataBlock::SetValue(LPCSTR szKey, LPCSTR szValue)
{
	for( UINT i=0; i<GetItemCount(); i++){
		
		CConfigBlock* pBlockItem = GetItem(i);
		if( pBlockItem->m_blockItemtype == BLOCKITEM_TYPE_VALUE ){

			if(pBlockItem->m_sKey == szKey ){
				CDataBlockValue* pValueItem = (CDataBlockValue*)pBlockItem;
				pValueItem->UpdateValue( szValue );
				return 0;
			}
		}

	}
	//キー文字に該当するものがなければ追加
	CConfigBlock* pBlockValue = new CDataBlockValue();
	((CDataBlockValue*)pBlockValue)->SetValue( szKey,szValue,"", "");
	m_vDataItems.push_back( pBlockValue );
	return 0;
}


/////////////////////////////////////////////////////////////////////
// CDataBlockValue インプリメンテーション
/////////////////////////////////////////////////////////////////////
CDataBlockValue::CDataBlockValue(void)
{
	m_blockItemtype = BLOCKITEM_TYPE_VALUE;
}
CDataBlockValue::~CDataBlockValue(void)
{
}

void CDataBlockValue::SetValue(LPCWSTR szKey, LPCWSTR szValue, LPCWSTR szSpaceToComment, LPCWSTR szComment)
{
	std::wstring_convert<std::codecvt_utf8_utf16<wchar_t>> cv;

	std::string s = wide_to_multi(szValue);

	SetValue(	wide_to_multi(szKey).c_str(), 
				wide_to_multi(szValue).c_str(), 
				wide_to_multi(szSpaceToComment).c_str(), 				
				wide_to_multi( szComment).c_str()
				);
		
}
void CDataBlockValue::SetValue(LPCSTR szKey, LPCSTR szValue, LPCSTR szSpaceToComment, LPCSTR szComment)
{
	m_blockItemtype = BLOCKITEM_TYPE_VALUE;
	m_sKey = szKey;
	m_sValue = szValue;
	m_sSpaceToComment = szSpaceToComment;
	m_sComment = szComment;

	trim(m_sKey);
	trim(m_sValue);
}
void CDataBlockValue::UpdateValue( LPCSTR szValue)
{
	m_sValue = szValue;
}

void CDataBlockValue::SetComment(LPCWSTR sComment, LPCWSTR sSpaceToComment/*=L""*/)
{
	SetComment(wide_to_multi(sComment).c_str(),  wide_to_multi(sSpaceToComment).c_str());
}

void CDataBlockValue::SetComment(LPCSTR sComment, LPCSTR sSpaceToComment/*=""*/)
{
	m_blockItemtype = BLOCKITEM_TYPE_COMMENT;
	m_sKey = "";
	m_sValue = "";
	m_sSpaceToComment=sSpaceToComment;
	m_sComment = sComment;
}


int CDataBlockValue::WriteFile( FILE *fp, int layer )
{
	std::string sTab="";
	for( int i=0; i<layer; i++){
		sTab += '\t';
	}

	switch( m_blockItemtype){
	case BLOCKITEM_TYPE_COMMENT:
		fprintf_s(fp, "%s%s\n", sTab.c_str(), GetComment());
		break;
	case BLOCKITEM_TYPE_VALUE:
		std::string sComment = GetComment();
		std::string sSpaceToComment = GetSpaceToComment();
		if( sComment.size()==0){
			fprintf_s(fp, "%s%s=%s\n", sTab.c_str(), GetKey(), GetValue());
		}
		else{
			fprintf_s(fp, "%s%s=%s%s%s\n", sTab.c_str(), GetKey(), GetValue(),sSpaceToComment.c_str(), sComment.c_str());
		}
		break;
	}
	return 0;
}

