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
		//��`�t�@�C���ǂݍ���
		int ReadConfigFile(String^ configFilePath);
		//��`�t�@�C�������o��
		int WriteConfigFile(String^ configFilePath);
		
		//���[�g�u���b�N���擾
		int GetBlockCount();

		//���[�g�u���b�N�I�u�W�F�N�g�擾
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
		//�u���b�N���擾�E�ݒ�
		String^ GetName();
		String^ GetComment();
		int SetName(String^ szKey);
		int SetComment(String^ szComment);

		//�u���b�N���̃A�C�e�����ڐ��擾
		//(�폜) int GetBlockItemCount();
		int GetItemCount(BlockItemType type, String^ keyName);
		int GetItemCount() { return GetItemCount(BlockItemType::None, nullptr); }
		//�u���b�N���̃A�C�e���̓o�^��Index����擾
		int GetItemIndexes([Out]int% pIndexes, int maxSize, BlockItemType type, String^ keyName);
		int GetItemIndexes([Out]int% pIndexes, int maxSize, BlockItemType type) { return GetItemIndexes(pIndexes, maxSize, type, nullptr); };
		int GetItemIndexes([Out]int% pIndexes, int maxSize) { return GetItemIndexes(pIndexes, maxSize, BlockItemType::None, nullptr); };

		//�u���b�N���̃A�C�e���I�u�W�F�N�g�擾
		//(�폜) CConfigBlockWrapper^ GetBlockItem(int idx);
		//(�폜) CConfigBlockWrapper^ GetBlockItem(String^ szkey);
		CConfigBlockWrapper^ GetItem(int idx);
		CConfigBlockWrapper^ GetItem(int idxByKey, String^ keyName);
		CConfigBlockWrapper^ GetItem(String^ szkey);

		//�u���b�N����BLOCK�^�A�C�e�����ڐ��擾
		int GetBlockCount(String^ keyName);
		int GetBlockCount() { return GetBlockCount(nullptr); }
		int GetBlockIndexes(String^ keyName, [Out]int% indexes, int size);
		//�u���b�N����BLOCK�^�A�C�e���I�u�W�F�N�g�擾
		CConfigBlockWrapper^ GetBlock(int idx);
		CConfigBlockWrapper^ GetBlock(int idxByKey, String^ keyName);
		CConfigBlockWrapper^ GetBlock(String^ keyName);
		//�u���b�N�̒ǉ�
		CConfigBlockWrapper^ AddBlock(String^ szKey);

		//�u���b�N����VALUE�^���ڐ��擾
		int GetValueCount();
		//�u���b�N����VALUE�^��Key������擾
		String^ GeValueKey(int idx);
		//�u���b�N����VALUE�`�f�[�^�̒l�擾
		String^ GetValue(int idx);
		String^ GetValue(int idx, String^ szDefault);
		String^ GetValue(String^ szkey);
		String^ GetValue(String^ szkey, String^ szDefault);

		//�u���b�N����VALUE�`�f�[�^�̒l�ݒ�
		int SetValue(String^ szkey, String^ szValue);


		//Reserve Function
		//�ȉ���CDataBlockValue�I�u�W�F�N�g�p�ł��B
		String^ GetValue();
	};
}

#endif
