#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <cstdlib>
#include <tchar.h>
#include "Hook.h"

EXTERN_C DWORD(WINAPI *pHookTimeGetTime)(DWORD);

static CHAR szIniFileName[MAX_PATH];
static int EnableHook = 0;
static DWORD MaxScanTime = 500;

static int EnableSpeedHooks = 0;
static double fMultIncr = 1;
static int DecreaseSpeedKey = VK_SUBTRACT;//'7';
static int IncreaseSpeedKey = VK_MULTIPLY;//'8';
static int ResetSpeedKey = VK_DIVIDE;  //'9';

static int EnableConsoleHook = 0;
static DWORD ConsoleHookKey = VK_CONTROL|VK_OEM_3;
static DWORD ConsoleActionDelay = 5000;

static int EnableMonoHook = 0;
static int ShowConsoleOnStart = 0;
static int ShowOnce = 0;
static CHAR szLoadImages[4096];
static CHAR szLoadImagesSection[4096];
static CHAR szLoadClasses[4096];
static int CodePage = 65001;
static AssemblyRef aAssemblyRefs[20];

extern bool mono_init();
extern bool mono_load_images(LPCSTR loadImages);
extern bool mono_launch(LPCSTR loadClasses, int codepage);


typedef struct EnumLookupType {
	BYTE value;
	const char *name;
} EnumLookupType;

EnumLookupType keyTable[] = {
	{ VK_LBUTTON,	"VK_LBUTTON" },
	{ VK_RBUTTON,	"VK_RBUTTON" },
	{ VK_CANCEL,	"VK_CANCEL" },
	{ VK_MBUTTON,	"VK_MBUTTON" },
	{ VK_XBUTTON1,	"VK_XBUTTON1" },
	{ VK_XBUTTON2,	"VK_XBUTTON2" },
	{ VK_BACK,	"VK_BACK" },
	{ VK_TAB,	"VK_TAB" },
	{ VK_CLEAR,	"VK_CLEAR" },
	{ VK_RETURN,	"VK_RETURN" },
	{ VK_SHIFT,	"VK_SHIFT" },
	{ VK_CONTROL,	"VK_CONTROL" },
	{ VK_MENU,	"VK_MENU" },
	{ VK_PAUSE,	"VK_PAUSE" },
	{ VK_CAPITAL,	"VK_CAPITAL" },
	{ VK_KANA,	"VK_KANA" },
	{ VK_HANGEUL,	"VK_HANGEUL" },
	{ VK_HANGUL,	"VK_HANGUL" },
	{ VK_JUNJA,	"VK_JUNJA" },
	{ VK_FINAL,	"VK_FINAL" },
	{ VK_HANJA,	"VK_HANJA" },
	{ VK_KANJI,	"VK_KANJI" },
	{ VK_ESCAPE,	"VK_ESCAPE" },
	{ VK_CONVERT,	"VK_CONVERT" },
	{ VK_NONCONVERT,	"VK_NONCONVERT" },
	{ VK_ACCEPT,	"VK_ACCEPT" },
	{ VK_MODECHANGE,	"VK_MODECHANGE" },
	{ VK_SPACE,	"VK_SPACE" },
	{ VK_PRIOR,	"VK_PRIOR" },
	{ VK_NEXT,	"VK_NEXT" },
	{ VK_END,	"VK_END" },
	{ VK_HOME,	"VK_HOME" },
	{ VK_LEFT,	"VK_LEFT" },
	{ VK_UP,	"VK_UP" },
	{ VK_RIGHT,	"VK_RIGHT" },
	{ VK_DOWN,	"VK_DOWN" },
	{ VK_SELECT,	"VK_SELECT" },
	{ VK_PRINT,	"VK_PRINT" },
	{ VK_EXECUTE,	"VK_EXECUTE" },
	{ VK_SNAPSHOT,	"VK_SNAPSHOT" },
	{ VK_INSERT,	"VK_INSERT" },
	{ VK_DELETE,	"VK_DELETE" },
	{ VK_HELP,	"VK_HELP" },
	{ 0x30, "VK_0" },
	{ 0x31, "VK_1" },
	{ 0x32, "VK_2" },
	{ 0x33, "VK_3" },
	{ 0x34, "VK_4" },
	{ 0x35, "VK_5" },
	{ 0x36, "VK_6" },
	{ 0x37, "VK_7" },
	{ 0x38, "VK_8" },
	{ 0x39, "VK_9" },
	{ 0x41, "VK_A" },
	{ 0x42, "VK_B" },
	{ 0x43, "VK_C" },
	{ 0x44, "VK_D" },
	{ 0x45, "VK_E" },
	{ 0x46, "VK_F" },
	{ 0x47, "VK_G" },
	{ 0x48, "VK_H" },
	{ 0x49, "VK_I" },
	{ 0x4A, "VK_J" },
	{ 0x4B, "VK_K" },
	{ 0x4C, "VK_L" },
	{ 0x4D, "VK_M" },
	{ 0x4E, "VK_N" },
	{ 0x4F, "VK_O" },
	{ 0x50, "VK_P" },
	{ 0x51, "VK_Q" },
	{ 0x52, "VK_R" },
	{ 0x53, "VK_S" },
	{ 0x54, "VK_T" },
	{ 0x55, "VK_U" },
	{ 0x56, "VK_V" },
	{ 0x57, "VK_W" },
	{ 0x58, "VK_X" },
	{ 0x59, "VK_Y" },
	{ 0x5A, "VK_Z" },
	{ VK_LWIN,	"VK_LWIN" },
	{ VK_RWIN,	"VK_RWIN" },
	{ VK_APPS,	"VK_APPS" },
	{ VK_SLEEP,	"VK_SLEEP" },
	{ VK_NUMPAD0,	"VK_NUMPAD0" },
	{ VK_NUMPAD1,	"VK_NUMPAD1" },
	{ VK_NUMPAD2,	"VK_NUMPAD2" },
	{ VK_NUMPAD3,	"VK_NUMPAD3" },
	{ VK_NUMPAD4,	"VK_NUMPAD4" },
	{ VK_NUMPAD5,	"VK_NUMPAD5" },
	{ VK_NUMPAD6,	"VK_NUMPAD6" },
	{ VK_NUMPAD7,	"VK_NUMPAD7" },
	{ VK_NUMPAD8,	"VK_NUMPAD8" },
	{ VK_NUMPAD9,	"VK_NUMPAD9" },
	{ VK_MULTIPLY,	"VK_MULTIPLY" },
	{ VK_ADD,	"VK_ADD" },
	{ VK_SEPARATOR,	"VK_SEPARATOR" },
	{ VK_SUBTRACT,	"VK_SUBTRACT" },
	{ VK_DECIMAL,	"VK_DECIMAL" },
	{ VK_DIVIDE,	"VK_DIVIDE" },
	{ VK_F1,	"VK_F1" },
	{ VK_F2,	"VK_F2" },
	{ VK_F3,	"VK_F3" },
	{ VK_F4,	"VK_F4" },
	{ VK_F5,	"VK_F5" },
	{ VK_F6,	"VK_F6" },
	{ VK_F7,	"VK_F7" },
	{ VK_F8,	"VK_F8" },
	{ VK_F9,	"VK_F9" },
	{ VK_F10,	"VK_F10" },
	{ VK_F11,	"VK_F11" },
	{ VK_F12,	"VK_F12" },
	{ VK_F13,	"VK_F13" },
	{ VK_F14,	"VK_F14" },
	{ VK_F15,	"VK_F15" },
	{ VK_F16,	"VK_F16" },
	{ VK_F17,	"VK_F17" },
	{ VK_F18,	"VK_F18" },
	{ VK_F19,	"VK_F19" },
	{ VK_F20,	"VK_F20" },
	{ VK_F21,	"VK_F21" },
	{ VK_F22,	"VK_F22" },
	{ VK_F23,	"VK_F23" },
	{ VK_F24,	"VK_F24" },
	{ VK_NUMLOCK,	"VK_NUMLOCK" },
	{ VK_SCROLL,	"VK_SCROLL" },
	{ VK_OEM_NEC_EQUAL,	"VK_OEM_NEC_EQUAL" },
	{ VK_OEM_FJ_JISHO,	"VK_OEM_FJ_JISHO" },
	{ VK_OEM_FJ_MASSHOU,	"VK_OEM_FJ_MASSHOU" },
	{ VK_OEM_FJ_TOUROKU,	"VK_OEM_FJ_TOUROKU" },
	{ VK_OEM_FJ_LOYA,	"VK_OEM_FJ_LOYA" },
	{ VK_OEM_FJ_ROYA,	"VK_OEM_FJ_ROYA" },
	{ VK_LSHIFT,	"VK_LSHIFT" },
	{ VK_RSHIFT,	"VK_RSHIFT" },
	{ VK_LCONTROL,	"VK_LCONTROL" },
	{ VK_RCONTROL,	"VK_RCONTROL" },
	{ VK_LMENU,	"VK_LMENU" },
	{ VK_RMENU,	"VK_RMENU" },
	{ VK_BROWSER_BACK,	"VK_BROWSER_BACK" },
	{ VK_BROWSER_FORWARD,	"VK_BROWSER_FORWARD" },
	{ VK_BROWSER_REFRESH,	"VK_BROWSER_REFRESH" },
	{ VK_BROWSER_STOP,	"VK_BROWSER_STOP" },
	{ VK_BROWSER_SEARCH,	"VK_BROWSER_SEARCH" },
	{ VK_BROWSER_FAVORITES,	"VK_BROWSER_FAVORITES" },
	{ VK_BROWSER_HOME,	"VK_BROWSER_HOME" },
	{ VK_VOLUME_MUTE,	"VK_VOLUME_MUTE" },
	{ VK_VOLUME_DOWN,	"VK_VOLUME_DOWN" },
	{ VK_VOLUME_UP,	"VK_VOLUME_UP" },
	{ VK_MEDIA_NEXT_TRACK,	"VK_MEDIA_NEXT_TRACK" },
	{ VK_MEDIA_PREV_TRACK,	"VK_MEDIA_PREV_TRACK" },
	{ VK_MEDIA_STOP,	"VK_MEDIA_STOP" },
	{ VK_MEDIA_PLAY_PAUSE,	"VK_MEDIA_PLAY_PAUSE" },
	{ VK_LAUNCH_MAIL,	"VK_LAUNCH_MAIL" },
	{ VK_LAUNCH_MEDIA_SELECT,	"VK_LAUNCH_MEDIA_SELECT" },
	{ VK_LAUNCH_APP1,	"VK_LAUNCH_APP1" },
	{ VK_LAUNCH_APP2,	"VK_LAUNCH_APP2" },
	{ VK_OEM_1,	"VK_OEM_1" },
	{ VK_OEM_PLUS,	"VK_OEM_PLUS" },
	{ VK_OEM_COMMA,	"VK_OEM_COMMA" },
	{ VK_OEM_MINUS,	"VK_OEM_MINUS" },
	{ VK_OEM_PERIOD,	"VK_OEM_PERIOD" },
	{ VK_OEM_2,	"VK_OEM_2" },
	{ VK_OEM_3,	"VK_OEM_3" },
	{ VK_OEM_4,	"VK_OEM_4" },
	{ VK_OEM_5,	"VK_OEM_5" },
	{ VK_OEM_6,	"VK_OEM_6" },
	{ VK_OEM_7,	"VK_OEM_7" },
	{ VK_OEM_8,	"VK_OEM_8" },
	{ VK_OEM_AX,	"VK_OEM_AX" },
	{ VK_OEM_102,	"VK_OEM_102" },
	{ VK_ICO_HELP,	"VK_ICO_HELP" },
	{ VK_ICO_00,	"VK_ICO_00" },
	{ VK_PROCESSKEY,	"VK_PROCESSKEY" },
	{ VK_ICO_CLEAR,	"VK_ICO_CLEAR" },
	{ VK_PACKET,	"VK_PACKET" },
	{ VK_OEM_RESET,	"VK_OEM_RESET" },
	{ VK_OEM_JUMP,	"VK_OEM_JUMP" },
	{ VK_OEM_PA1,	"VK_OEM_PA1" },
	{ VK_OEM_PA2,	"VK_OEM_PA2" },
	{ VK_OEM_PA3,	"VK_OEM_PA3" },
	{ VK_OEM_WSCTRL,	"VK_OEM_WSCTRL" },
	{ VK_OEM_CUSEL,	"VK_OEM_CUSEL" },
	{ VK_OEM_ATTN,	"VK_OEM_ATTN" },
	{ VK_OEM_FINISH,	"VK_OEM_FINISH" },
	{ VK_OEM_COPY,	"VK_OEM_COPY" },
	{ VK_OEM_AUTO,	"VK_OEM_AUTO" },
	{ VK_OEM_ENLW,	"VK_OEM_ENLW" },
	{ VK_OEM_BACKTAB,	"VK_OEM_BACKTAB" },
	{ VK_ATTN,	"VK_ATTN" },
	{ VK_CRSEL,	"VK_CRSEL" },
	{ VK_EXSEL,	"VK_EXSEL" },
	{ VK_EREOF,	"VK_EREOF" },
	{ VK_PLAY,	"VK_PLAY" },
	{ VK_ZOOM,	"VK_ZOOM" },
	{ VK_NONAME,	"VK_NONAME" },
	{ VK_PA1,	"VK_PA1" },
	{ VK_OEM_CLEAR,	"VK_OEM_CLEAR" },
};


// Trim whitespace before and after a string
static TCHAR *Trim(TCHAR*&p) {
	if (p == nullptr) return nullptr;
	while (_istspace(*p)) *p++ = 0;
	TCHAR *e = p + _tcslen(p) - 1;
	while (e > p && _istspace(*e)) *e-- = 0;
	return p;
}

long StringToEnum(LPTSTR value, const EnumLookupType *table) {
	Trim(value);
	if (value == nullptr || value[0] == 0)
		return 0;
	for (const EnumLookupType *itr = table; itr->name != nullptr; ++itr) {
		if (0 == _tcsicmp(value, itr->name)) return itr->value;
	}
	char *end = nullptr;
	return strtol(value, &end, 0);
}

int KeysToInt(LPTSTR value, const EnumLookupType *table) {
	int retval = 0;
	Trim(value);
	if (value == nullptr) return 0;
	LPTSTR start = value;
	LPTSTR end = value + _tcslen(value);
	while (start < end) {
		LPCTSTR bar = _tcschr(start, '|');
		int len = (bar != nullptr) ? bar - start : end - start;
		start[len] = 0;
		int slen = _tcslen(Trim(start));
		if (slen == 1)
		{
			short key = VkKeyScan(start[0]);
			int val = key & 0xFF;
			if (!val) return 0;

			if ((key & 0x0100) == 0x0100)
				retval = (retval << 8) | VK_SHIFT;
			if ((key & 0x0200) == 0x0200)
				retval = (retval << 8) | VK_CONTROL;
			if ((key & 0x0400) == 0x0400)
				retval = (retval << 8) | VK_MENU;
			//if ((key & 0x0400) == 0x0400) // Hankaku
			//	retval = (retval << 8) | VK_????;
			retval = (retval << 8) | (key & 0xFF);
		}
		else
		{
			int val = StringToEnum(start, table);
			if (!val) return 0;
			retval = (retval << 8) | (val & 0xFF);
		}
		start += (len + 1);
	}
	return retval;
}

static void LoadSettings(void)
{
	CHAR buffer[256];

	MaxScanTime = GetPrivateProfileInt("System", "ScanFrequency", MaxScanTime, szIniFileName);

	EnableSpeedHooks = GetPrivateProfileInt("Speed", "Enable", 0, szIniFileName);
	if (0 < GetPrivateProfileString("Speed", "DecreaseKey", "VK_SUBTRACT", buffer, sizeof(buffer), szIniFileName))
		DecreaseSpeedKey = KeysToInt(buffer, keyTable);
	if (0 < GetPrivateProfileString("Speed", "IncreaseKey", "VK_SUBTRACT", buffer, sizeof(buffer), szIniFileName))
		IncreaseSpeedKey = KeysToInt(buffer, keyTable);
	if (0 < GetPrivateProfileString("Speed", "ResetKey", "VK_SUBTRACT", buffer, sizeof(buffer), szIniFileName))
		ResetSpeedKey = KeysToInt(buffer, keyTable);
	if (0 < GetPrivateProfileString("Speed", "IncrementalMultiplierChange", "1", buffer, sizeof(buffer), szIniFileName)) {
		fMultIncr = atof(buffer);
		if (fMultIncr <= 0.0)
			fMultIncr = 1.0;
	}
	EnableConsoleHook = GetPrivateProfileInt("Console", "Enable", 0, szIniFileName);
	if (0 < GetPrivateProfileString("Console", "ShowKey", "VK_CONTROL|`", buffer, sizeof(buffer), szIniFileName))
		ConsoleHookKey = KeysToInt(buffer, keyTable);

	ConsoleActionDelay = GetPrivateProfileInt("Console", "ActionDelay", MaxScanTime, szIniFileName);
	ShowOnce = GetPrivateProfileInt("Console", "ShowOnce", 0, szIniFileName);
	CodePage = GetPrivateProfileInt("Console", "CodePage", 65001, szIniFileName);
	ShowConsoleOnStart = GetPrivateProfileInt("Console", "ShowConsoleOnStart", 0, szIniFileName);
	GetPrivateProfileString("Console", "RunConsole", "", szLoadClasses, sizeof(szLoadClasses), szIniFileName);

	EnableMonoHook = GetPrivateProfileInt("Mono", "Enable", 0, szIniFileName);
	//GetPrivateProfileString("Mono", "Loadimages", "", szLoadImages, sizeof(szLoadImages), szIniFileName);
	
	if ( 0 < GetPrivateProfileSection("Mono.Images", szLoadImagesSection, sizeof(szLoadImagesSection), szIniFileName) )
	{
		LPSTR ptr = nullptr, next = nullptr;
		int len = 0;
		for ( ptr = szLoadImagesSection, next = ptr + strlen(ptr) + 1
			; *ptr != 0
			; ptr = next, next = ptr + strlen(ptr) + 1)
		{
			Trim(ptr);
			if (*ptr != 0 && *ptr != ';' && *ptr != '#')
			{
				if (szLoadImages[0] != 0)
					strcat(szLoadImages, ";");
				strcat(szLoadImages, ptr);				
			}
		}
	}
}

BOOL CheckKeyState(DWORD value)
{
	BOOL state = FALSE;
	DWORD key = value & 0xFF;
	if (GetAsyncKeyState(key))
	{
		key = (value & 0xFF00) >> 8;
		if (!key)
			state = TRUE;
		else
		{
			if (GetAsyncKeyState(key))
			{
				key = (value & 0xFF0000) >> 16;
				if (!key)
					state = TRUE;
				else
				{
					if (GetAsyncKeyState(key))
					{
						key = (value & 0xFF000000) >> 24;
						if (!key)
							state = TRUE;
						else
						{
							if (GetAsyncKeyState(key))
								state = TRUE;
						}
					}
				}
			}
		}
	}
	return state;
}

DWORD APIENTRY HandleSpeedHook(DWORD time)
{
	static double fMult = 1;
	static DWORD dwLastTime = 0;
	static DWORD dwLastRealTime = 0;
	static DWORD dwLastCheckTime = 0;
	if (dwLastRealTime == 0)
	{
		dwLastRealTime = time;
	}
	else
	{
		DWORD curTime = time;
		// Prevent excessive checking
		if ((curTime - dwLastCheckTime) >= MaxScanTime)
		{
			dwLastCheckTime = curTime;
			if (CheckKeyState(ResetSpeedKey))
			{
				fMult = 1.;
			}
			else if (CheckKeyState(DecreaseSpeedKey))
			{
				if (fMult <= 1.)
				{
					//fMult /= 2;
				}
				else
				{
					fMult -= fMultIncr;
					fMult = max(fMult, 0.1);
				}
			}
			else if (CheckKeyState(IncreaseSpeedKey))
			{
				if (fMult < 1.)
				{
					//fMult *= 2;
				}
				else
				{
					fMult += fMultIncr;
					fMult = min(fMult, 10.);
				}
			}
		}
		dwLastTime = DWORD(double(curTime - dwLastRealTime) * fMult) + dwLastTime;
		dwLastRealTime = curTime;
	}
	return dwLastTime;
}

void HandleMonoInit()
{
	//MessageBox(nullptr, "About to load", "Loading", MB_OK);
	if (nullptr == GetConsoleWindow())
	{
		HWND h_foreground = GetForegroundWindow();
		HWND h_active_hwnd = GetActiveWindow();
		HWND h_focus_hwnd = GetFocus();

		AllocConsole();

		mono_init();
		mono_load_images(szLoadImages);
		mono_launch(szLoadClasses, CodePage);

		if (h_foreground != nullptr)
			SetForegroundWindow(h_foreground);
		if (h_active_hwnd != nullptr)
			SetActiveWindow(h_active_hwnd);
		if (h_focus_hwnd != nullptr)
			SetFocus(h_focus_hwnd);
	}
}

void HandleConsoleHook(DWORD time)
{
	// prevent repeated actions
	static int lastScanTime = 0;
	static int startTime = 0;
	if (startTime == 0)
		startTime = time;

	if (EnableMonoHook && ShowConsoleOnStart > 0)
	{
		if ((time - startTime) > ShowConsoleOnStart)
		{
			ShowConsoleOnStart = 0;
			HandleMonoInit();
			if (ShowOnce)
				EnableConsoleHook = 0;
			return;
		}
	}

	if ((time - startTime) > ConsoleActionDelay)
	{
		if (CheckKeyState(ConsoleHookKey))
		{
			if (EnableMonoHook)
				HandleMonoInit();
			if (ShowOnce)
				EnableConsoleHook = 0;
			lastScanTime = time;
		}
	}
}

DWORD APIENTRY HookTimeGetTime(DWORD time)
{
	DWORD result = time;
	if (EnableSpeedHooks)
		result = HandleSpeedHook(time);
	if (EnableConsoleHook)
		HandleConsoleHook(time);
	//if (EnableMonoHook)
	//	MonoHook(time);
	return result;
}

void EnableHooks()
{
	EnableHook = GetPrivateProfileInt("SYSTEM", "Enable", 0, szIniFileName);
	if (EnableHook)
	{
		LoadSettings();
	}
	else
	{
		EnableSpeedHooks = 0;
		EnableConsoleHook = 0;
		EnableMonoHook = 0;
	}	
}


EXTERN_C
BOOL APIENTRY HookDllMain(HANDLE hModule, DWORD  ul_reason_for_call)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
	{
		if ( 0 < GetModuleFileName(HMODULE(hModule), szIniFileName, MAX_PATH) )
		{
			if (LPSTR szIniName = strrchr(szIniFileName, '\\'))
				strcpy(szIniName + 1, "Console.ini");
			EnableHooks();
		}
		pHookTimeGetTime = HookTimeGetTime;
	}
	return TRUE;
}