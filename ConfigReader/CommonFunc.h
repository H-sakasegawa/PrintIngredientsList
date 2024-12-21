#include <vector>
#include <string>
#include "Const.h"

std::vector<std::string> StringSplit( const std::string &str, char sep);
std::string wide_to_multi( std::wstring const &src);

void LTrim(std::string& str, LPCSTR trimChar = REMOVE_CHAR);
void LTrim(std::wstring& str, LPCWSTR trimChar = REMOVE_WCHAR);
void RTrim(std::string& str, LPCSTR trimChar = REMOVE_CHAR);
void RTrim(std::wstring& str, LPCWSTR trimChar = REMOVE_WCHAR);
void trim(std::string& string, LPCSTR trimChar = REMOVE_CHAR);
//文字列データの前と後ろのtrimCharを除外
void trim(std::wstring& string, LPCWSTR trimChar = REMOVE_WCHAR);
void RemoveComment(std::wstring& string, std::wstring& sComment, std::wstring& sSpaceToComment);