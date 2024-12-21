#pragma once
#ifdef CSHARP_WRAPPER

#include "ConfigReaderIF.h"

using namespace System;
using namespace System::Runtime::InteropServices;

namespace ConfigReader
{
	ref class CConfigBlockWrapper;

	public enum class BlockItemType
	{
		None = BLOCKITEM_TYPE_NONE,
		Comment = BLOCKITEM_TYPE_COMMENT,
		Value = BLOCKITEM_TYPE_VALUE,
		Block = BLOCKITEM_TYPE_BLOCK
	};

	public ref class CConfigReaderIFWrapper
	{
	private:
		CConfigReaderIF* m_pIF;
	public:
		CConfigReaderIFWrapper();
		~CConfigReaderIFWrapper();
		!CConfigReaderIFWrapper();

	public:
		//定義ファイル読み込み
		int ReadConfigFile(String^ configFilePath);
		//定義ファイル書き出し
		int WriteConfigFile(String^ configFilePath);
		
		//ルートブロック数取得
		int GetBlockCount();

		//ルートブロックオブジェクト取得
		CConfigBlockWrapper^ GetBlock(int idx);
		CConfigBlockWrapper^ GetBlock(String^ keyName);

		CConfigBlockWrapper^ AddBlock(String^ keyName);
		int SetValue(String^ szKey, String^ szValue);
		int RemoveBlock(int idx);
	};

	public ref class CConfigBlockWrapper
	{
	private:
		CConfigBlock* m_pBlock;
	public:
		CConfigBlockWrapper(CConfigBlock* pBlock);
		~CConfigBlockWrapper();
		!CConfigBlockWrapper();

	public:
		//ブロック名取得・設定
		String^ GetName();
		String^ GetComment();
		int SetName(String^ szKey);
		int SetComment(String^ szComment);

		//ブロック内のアイテム項目数取得
		//(削除) int GetBlockItemCount();
		int GetItemCount(BlockItemType type, String^ keyName);
		int GetItemCount() { return GetItemCount(BlockItemType::None, nullptr); }
		//ブロック内のアイテムの登録先Index列情報取得
		int GetItemIndexes([Out]int% pIndexes, int maxSize, BlockItemType type, String^ keyName);
		int GetItemIndexes([Out]int% pIndexes, int maxSize, BlockItemType type) { return GetItemIndexes(pIndexes, maxSize, type, nullptr); };
		int GetItemIndexes([Out]int% pIndexes, int maxSize) { return GetItemIndexes(pIndexes, maxSize, BlockItemType::None, nullptr); };

		//ブロック内のアイテムオブジェクト取得
		//(削除) CConfigBlockWrapper^ GetBlockItem(int idx);
		//(削除) CConfigBlockWrapper^ GetBlockItem(String^ szkey);
		CConfigBlockWrapper^ GetItem(int idx);
		CConfigBlockWrapper^ GetItem(int idxByKey, String^ keyName);
		CConfigBlockWrapper^ GetItem(String^ szkey);

		//ブロック内のBLOCK型アイテム項目数取得
		int GetBlockCount(String^ keyName);
		int GetBlockCount() { return GetBlockCount(nullptr); }
		int GetBlockIndexes(String^ keyName, [Out]int% indexes, int size);
		//ブロック内のBLOCK型アイテムオブジェクト取得
		CConfigBlockWrapper^ GetBlock(int idx);
		CConfigBlockWrapper^ GetBlock(int idxByKey, String^ keyName);
		CConfigBlockWrapper^ GetBlock(String^ keyName);
		//ブロックの追加
		CConfigBlockWrapper^ AddBlock(String^ szKey);

		//ブロック内のVALUE型項目数取得
		int GetValueCount();
		//ブロック内のVALUE型のKey文字列取得
		String^ GeValueKey(int idx);
		//ブロック内のVALUE形データの値取得
		String^ GetValue(int idx);
		String^ GetValue(int idx, String^ szDefault);
		String^ GetValue(String^ szkey);
		String^ GetValue(String^ szkey, String^ szDefault);

		//ブロック内のVALUE形データの値設定
		int SetValue(String^ szkey, String^ szValue);


		//Reserve Function
		//以下はCDataBlockValueオブジェクト用です。
		String^ GetValue();
	};
}

#endif
