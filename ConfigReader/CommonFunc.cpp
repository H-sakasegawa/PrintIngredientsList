#include "stdafx.h"
#include <vector>
#include <string>
#include <sstream>
#include "Const.h"

std::vector<std::string> StringSplit( const std::string &str, char sep)
{

	std::vector<std::string> v;
	std::stringstream ss(str);

	std::string buffer;

	while( std::getline(ss, buffer,sep)){
		v.push_back( buffer );
	}

	return v;

}

std::string wide_to_multi( std::wstring const &src)
{
	auto const dest_size = ::WideCharToMultiByte(CP_ACP, 0U, src.data(), -1, nullptr, 0, nullptr, nullptr);
	std::vector<char> dest(dest_size, '\0');
	if( ::WideCharToMultiByte( CP_ACP, 0U, src.data(), -1, dest.data(), dest.size(), nullptr, nullptr)==0){
		return "";
	}
	dest.resize( std::char_traits<char>::length(dest.data()));
	dest.shrink_to_fit();
	return std::string(dest.begin(), dest.end());

}


//文字列データの前のtrimCharを除外
void LTrim(std::string& str, LPCSTR trimChar = REMOVE_CHAR)
{
	int num = (int)strlen(trimChar);
	int c = 0;
	while (c < (int)str.size()) {

		BOOL bFind = FALSE;
		for (int i = 0; i < num; i++) {
			if (str.at(c) == trimChar[i]) {
				str.erase(c, 1);
				bFind = TRUE;
				break;
			}
		}
		if (!bFind) {
			//c++;
			break;
		}
	}
}
//文字列データの前のtrimCharを除外
void LTrim(std::wstring& str, LPCWSTR trimChar = REMOVE_WCHAR)
{

	int num = (int)wcslen(trimChar);
	int c = 0;
	while (c < (int)str.size()) {

		BOOL bFind = FALSE;
		for (int i = 0; i < num; i++) {
			if (str.at(c) == trimChar[i]) {
				str.erase(c, 1);
				bFind = TRUE;
				break;
			}
		}
		if (!bFind) {
			//c++;
			break;
		}
	}
}

//文字列データの後ろのtrimCharを除外
void RTrim(std::string& str, LPCSTR trimChar = REMOVE_CHAR)
{

	int num = (int)strlen(trimChar);
	int c = (int)str.size() - 1;
	while (c >= 0) {

		BOOL bFind = FALSE;
		for (int i = 0; i < num; i++) {
			if (str.at(c) == trimChar[i]) {
				str.erase(c, 1);
				bFind = TRUE;
				c--;
				break;
			}
		}
		if (!bFind) {
			//c--;
			break;
		}
	}
}
//文字列データの後ろのtrimCharを除外
void RTrim(std::wstring& str, LPCWSTR trimChar = REMOVE_WCHAR)
{

	int num = (int)wcslen(trimChar);
	int c = (int)str.size() - 1;
	while (c >= 0) {

		BOOL bFind = FALSE;
		for (int i = 0; i < num; i++) {
			if (str.at(c) == trimChar[i]) {
				str.erase(c, 1);
				bFind = TRUE;
				c--;
				break;
			}
		}
		if (!bFind) {
			//c--;
			break;
		}
	}
}

//文字列データの前と後ろのtrimCharを除外
void trim(std::string& string, LPCSTR trimChar = REMOVE_CHAR)
{

	LTrim(string, trimChar);
	RTrim(string, trimChar);

}
//文字列データの前と後ろのtrimCharを除外
void trim(std::wstring& string, LPCWSTR trimChar = REMOVE_WCHAR)
{
	LTrim(string, trimChar);
	RTrim(string, trimChar);
}

void RemoveComment(std::wstring& string, std::wstring& sComment, std::wstring& sSpaceToComment)
{
	// "XXXX=100\t\t//aiueo"
	size_t c = string.find(L"//");
	if (c != std::string::npos) {

		sComment = string.substr(c);// "//aiueo"
		string = string.substr(0, c);// "XXXX=100\t\t"

		//設定値からコメントまでのタブ、スペースを取得
		std::wstring removeChar = REMOVE_WCHAR;

		for (int i = (int)string.length() - 1; i >= 0; i--)
		{
			std::wstring tmp = string.substr(i, 1);

			//int x =  removeChar.find( string.substr(i,1) );

			if (removeChar.find(string.substr(i, 1)) == std::wstring::npos) {
				sSpaceToComment = string.substr(i + 1);	// "\t\t"
				string = string.substr(0, i + 1); ;// "XXXX=100"
				break;
			}
		}

	}
}
