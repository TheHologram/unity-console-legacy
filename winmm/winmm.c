//#define WIN32_LEAN_AND_MEAN
#define DLLEXPORT __declspec(dllexport) 
#define DLLIMPORT __declspec(dllimport) 

#pragma warning( disable: 4273)

#include <windows.h>
#include <stdlib.h>

#undef PlaySound

#include "winmm.h"

//#pragma comment(linker, "/OPT:NOWIN98")

static WINMM_STRUCT winmm;


static BOOL DelayLoadWinmm()
{
	TCHAR curpath[_MAX_PATH];

	GetSystemDirectory(curpath, sizeof(curpath));
	lstrcat(curpath, "\\winmm.dll");
	winmm.hModule = LoadLibrary(curpath);

	if (!winmm.hModule)
	{
		TCHAR *filepart = NULL;
		if (SearchPath(getenv("PATH"), "winmm.dll", NULL, sizeof(curpath), curpath, &filepart))
			winmm.hModule = LoadLibrary(curpath);
	}
	if (!winmm.hModule)
		return FALSE;

	*(FARPROC*)&winmm.mciExecute = GetProcAddress(winmm.hModule, "mciExecute");
	*(FARPROC*)&winmm.mciGetErrorStringA = GetProcAddress(winmm.hModule, "mciGetErrorStringA");
	*(FARPROC*)&winmm.mciGetErrorStringW = GetProcAddress(winmm.hModule, "mciGetErrorStringW");
	*(FARPROC*)&winmm.mciSetYieldProc = GetProcAddress(winmm.hModule, "mciSetYieldProc");
	*(FARPROC*)&winmm.PlaySound = GetProcAddress(winmm.hModule, "PlaySound");
	*(FARPROC*)&winmm.PlaySoundA = GetProcAddress(winmm.hModule, "PlaySoundA");
	*(FARPROC*)&winmm.PlaySoundW = GetProcAddress(winmm.hModule, "PlaySoundW");
	*(FARPROC*)&winmm.sndPlaySoundA = GetProcAddress(winmm.hModule, "sndPlaySoundA");
	*(FARPROC*)&winmm.sndPlaySoundW = GetProcAddress(winmm.hModule, "sndPlaySoundW");
	*(FARPROC*)&winmm.mixerMessage = GetProcAddress(winmm.hModule, "mixerMessage");
	*(FARPROC*)&winmm.timeGetTime = GetProcAddress(winmm.hModule, "timeGetTime");
	*(FARPROC*)&winmm.mmioStringToFOURCCA = GetProcAddress(winmm.hModule, "mmioStringToFOURCCA");
	*(FARPROC*)&winmm.mmioStringToFOURCCW = GetProcAddress(winmm.hModule, "mmioStringToFOURCCW");
	*(FARPROC*)&winmm.OpenDriver = GetProcAddress(winmm.hModule, "OpenDriver");
	*(FARPROC*)&winmm.mmioOpenA = GetProcAddress(winmm.hModule, "mmioOpenA");
	*(FARPROC*)&winmm.mmioOpenW = GetProcAddress(winmm.hModule, "mmioOpenW");
	*(FARPROC*)&winmm.DrvGetModuleHandle = GetProcAddress(winmm.hModule, "DrvGetModuleHandle");
	*(FARPROC*)&winmm.GetDriverModuleHandle = GetProcAddress(winmm.hModule, "GetDriverModuleHandle");
	*(FARPROC*)&winmm.mciGetCreatorTask = GetProcAddress(winmm.hModule, "mciGetCreatorTask");
	*(FARPROC*)&winmm.mmioRead = GetProcAddress(winmm.hModule, "mmioRead");
	*(FARPROC*)&winmm.mmioSeek = GetProcAddress(winmm.hModule, "mmioSeek");
	*(FARPROC*)&winmm.mmioWrite = GetProcAddress(winmm.hModule, "mmioWrite");
	*(FARPROC*)&winmm.mmioInstallIOProcA = GetProcAddress(winmm.hModule, "mmioInstallIOProcA");
	*(FARPROC*)&winmm.mmioInstallIOProcW = GetProcAddress(winmm.hModule, "mmioInstallIOProcW");
	*(FARPROC*)&winmm.CloseDriver = GetProcAddress(winmm.hModule, "CloseDriver");
	*(FARPROC*)&winmm.DefDriverProc = GetProcAddress(winmm.hModule, "DefDriverProc");
	*(FARPROC*)&winmm.DrvDefDriverProc = GetProcAddress(winmm.hModule, "DrvDefDriverProc");
	*(FARPROC*)&winmm.SendDriverMessage = GetProcAddress(winmm.hModule, "SendDriverMessage");
	*(FARPROC*)&winmm.mmioSendMessage = GetProcAddress(winmm.hModule, "mmioSendMessage");
	*(FARPROC*)&winmm.mciGetDeviceIDA = GetProcAddress(winmm.hModule, "mciGetDeviceIDA");
	*(FARPROC*)&winmm.mciGetDeviceIDFromElementIDA = GetProcAddress(winmm.hModule, "mciGetDeviceIDFromElementIDA");
	*(FARPROC*)&winmm.mciGetDeviceIDFromElementIDW = GetProcAddress(winmm.hModule, "mciGetDeviceIDFromElementIDW");
	*(FARPROC*)&winmm.mciGetDeviceIDW = GetProcAddress(winmm.hModule, "mciGetDeviceIDW");
	*(FARPROC*)&winmm.mciSendStringA = GetProcAddress(winmm.hModule, "mciSendStringA");
	*(FARPROC*)&winmm.mciSendStringW = GetProcAddress(winmm.hModule, "mciSendStringW");
	*(FARPROC*)&winmm.mciSendCommandA = GetProcAddress(winmm.hModule, "mciSendCommandA");
	*(FARPROC*)&winmm.mciSendCommandW = GetProcAddress(winmm.hModule, "mciSendCommandW");
	*(FARPROC*)&winmm.auxGetDevCapsA = GetProcAddress(winmm.hModule, "auxGetDevCapsA");
	*(FARPROC*)&winmm.auxGetDevCapsW = GetProcAddress(winmm.hModule, "auxGetDevCapsW");
	*(FARPROC*)&winmm.auxGetVolume = GetProcAddress(winmm.hModule, "auxGetVolume");
	*(FARPROC*)&winmm.auxOutMessage = GetProcAddress(winmm.hModule, "auxOutMessage");
	*(FARPROC*)&winmm.auxSetVolume = GetProcAddress(winmm.hModule, "auxSetVolume");
	*(FARPROC*)&winmm.joyGetDevCapsA = GetProcAddress(winmm.hModule, "joyGetDevCapsA");
	*(FARPROC*)&winmm.joyGetDevCapsW = GetProcAddress(winmm.hModule, "joyGetDevCapsW");
	*(FARPROC*)&winmm.joyGetPos = GetProcAddress(winmm.hModule, "joyGetPos");
	*(FARPROC*)&winmm.joyGetPosEx = GetProcAddress(winmm.hModule, "joyGetPosEx");
	*(FARPROC*)&winmm.joyGetThreshold = GetProcAddress(winmm.hModule, "joyGetThreshold");
	*(FARPROC*)&winmm.joyReleaseCapture = GetProcAddress(winmm.hModule, "joyReleaseCapture");
	*(FARPROC*)&winmm.joySetCapture = GetProcAddress(winmm.hModule, "joySetCapture");
	*(FARPROC*)&winmm.joySetThreshold = GetProcAddress(winmm.hModule, "joySetThreshold");
	*(FARPROC*)&winmm.midiConnect = GetProcAddress(winmm.hModule, "midiConnect");
	*(FARPROC*)&winmm.midiDisconnect = GetProcAddress(winmm.hModule, "midiDisconnect");
	*(FARPROC*)&winmm.midiInAddBuffer = GetProcAddress(winmm.hModule, "midiInAddBuffer");
	*(FARPROC*)&winmm.midiInClose = GetProcAddress(winmm.hModule, "midiInClose");
	*(FARPROC*)&winmm.midiInGetDevCapsA = GetProcAddress(winmm.hModule, "midiInGetDevCapsA");
	*(FARPROC*)&winmm.midiInGetDevCapsW = GetProcAddress(winmm.hModule, "midiInGetDevCapsW");
	*(FARPROC*)&winmm.midiInGetErrorTextA = GetProcAddress(winmm.hModule, "midiInGetErrorTextA");
	*(FARPROC*)&winmm.midiInGetErrorTextW = GetProcAddress(winmm.hModule, "midiInGetErrorTextW");
	*(FARPROC*)&winmm.midiInGetID = GetProcAddress(winmm.hModule, "midiInGetID");
	*(FARPROC*)&winmm.midiInMessage = GetProcAddress(winmm.hModule, "midiInMessage");
	*(FARPROC*)&winmm.midiInOpen = GetProcAddress(winmm.hModule, "midiInOpen");
	*(FARPROC*)&winmm.midiInPrepareHeader = GetProcAddress(winmm.hModule, "midiInPrepareHeader");
	*(FARPROC*)&winmm.midiInReset = GetProcAddress(winmm.hModule, "midiInReset");
	*(FARPROC*)&winmm.midiInStart = GetProcAddress(winmm.hModule, "midiInStart");
	*(FARPROC*)&winmm.midiInStop = GetProcAddress(winmm.hModule, "midiInStop");
	*(FARPROC*)&winmm.midiInUnprepareHeader = GetProcAddress(winmm.hModule, "midiInUnprepareHeader");
	*(FARPROC*)&winmm.midiOutCacheDrumPatches = GetProcAddress(winmm.hModule, "midiOutCacheDrumPatches");
	*(FARPROC*)&winmm.midiOutCachePatches = GetProcAddress(winmm.hModule, "midiOutCachePatches");
	*(FARPROC*)&winmm.midiOutClose = GetProcAddress(winmm.hModule, "midiOutClose");
	*(FARPROC*)&winmm.midiOutGetDevCapsA = GetProcAddress(winmm.hModule, "midiOutGetDevCapsA");
	*(FARPROC*)&winmm.midiOutGetDevCapsW = GetProcAddress(winmm.hModule, "midiOutGetDevCapsW");
	*(FARPROC*)&winmm.midiOutGetErrorTextA = GetProcAddress(winmm.hModule, "midiOutGetErrorTextA");
	*(FARPROC*)&winmm.midiOutGetErrorTextW = GetProcAddress(winmm.hModule, "midiOutGetErrorTextW");
	*(FARPROC*)&winmm.midiOutGetID = GetProcAddress(winmm.hModule, "midiOutGetID");
	*(FARPROC*)&winmm.midiOutGetVolume = GetProcAddress(winmm.hModule, "midiOutGetVolume");
	*(FARPROC*)&winmm.midiOutLongMsg = GetProcAddress(winmm.hModule, "midiOutLongMsg");
	*(FARPROC*)&winmm.midiOutMessage = GetProcAddress(winmm.hModule, "midiOutMessage");
	*(FARPROC*)&winmm.midiOutOpen = GetProcAddress(winmm.hModule, "midiOutOpen");
	*(FARPROC*)&winmm.midiOutPrepareHeader = GetProcAddress(winmm.hModule, "midiOutPrepareHeader");
	*(FARPROC*)&winmm.midiOutReset = GetProcAddress(winmm.hModule, "midiOutReset");
	*(FARPROC*)&winmm.midiOutSetVolume = GetProcAddress(winmm.hModule, "midiOutSetVolume");
	*(FARPROC*)&winmm.midiOutShortMsg = GetProcAddress(winmm.hModule, "midiOutShortMsg");
	*(FARPROC*)&winmm.midiOutUnprepareHeader = GetProcAddress(winmm.hModule, "midiOutUnprepareHeader");
	*(FARPROC*)&winmm.midiStreamClose = GetProcAddress(winmm.hModule, "midiStreamClose");
	*(FARPROC*)&winmm.midiStreamOpen = GetProcAddress(winmm.hModule, "midiStreamOpen");
	*(FARPROC*)&winmm.midiStreamOut = GetProcAddress(winmm.hModule, "midiStreamOut");
	*(FARPROC*)&winmm.midiStreamPause = GetProcAddress(winmm.hModule, "midiStreamPause");
	*(FARPROC*)&winmm.midiStreamPosition = GetProcAddress(winmm.hModule, "midiStreamPosition");
	*(FARPROC*)&winmm.midiStreamProperty = GetProcAddress(winmm.hModule, "midiStreamProperty");
	*(FARPROC*)&winmm.midiStreamRestart = GetProcAddress(winmm.hModule, "midiStreamRestart");
	*(FARPROC*)&winmm.midiStreamStop = GetProcAddress(winmm.hModule, "midiStreamStop");
	*(FARPROC*)&winmm.mixerClose = GetProcAddress(winmm.hModule, "mixerClose");
	*(FARPROC*)&winmm.mixerGetControlDetailsA = GetProcAddress(winmm.hModule, "mixerGetControlDetailsA");
	*(FARPROC*)&winmm.mixerGetControlDetailsW = GetProcAddress(winmm.hModule, "mixerGetControlDetailsW");
	*(FARPROC*)&winmm.mixerGetDevCapsA = GetProcAddress(winmm.hModule, "mixerGetDevCapsA");
	*(FARPROC*)&winmm.mixerGetDevCapsW = GetProcAddress(winmm.hModule, "mixerGetDevCapsW");
	*(FARPROC*)&winmm.mixerGetID = GetProcAddress(winmm.hModule, "mixerGetID");
	*(FARPROC*)&winmm.mixerGetLineControlsA = GetProcAddress(winmm.hModule, "mixerGetLineControlsA");
	*(FARPROC*)&winmm.mixerGetLineControlsW = GetProcAddress(winmm.hModule, "mixerGetLineControlsW");
	*(FARPROC*)&winmm.mixerGetLineInfoA = GetProcAddress(winmm.hModule, "mixerGetLineInfoA");
	*(FARPROC*)&winmm.mixerGetLineInfoW = GetProcAddress(winmm.hModule, "mixerGetLineInfoW");
	*(FARPROC*)&winmm.mixerOpen = GetProcAddress(winmm.hModule, "mixerOpen");
	*(FARPROC*)&winmm.mixerSetControlDetails = GetProcAddress(winmm.hModule, "mixerSetControlDetails");
	*(FARPROC*)&winmm.mmioAdvance = GetProcAddress(winmm.hModule, "mmioAdvance");
	*(FARPROC*)&winmm.mmioAscend = GetProcAddress(winmm.hModule, "mmioAscend");
	*(FARPROC*)&winmm.mmioClose = GetProcAddress(winmm.hModule, "mmioClose");
	*(FARPROC*)&winmm.mmioCreateChunk = GetProcAddress(winmm.hModule, "mmioCreateChunk");
	*(FARPROC*)&winmm.mmioDescend = GetProcAddress(winmm.hModule, "mmioDescend");
	*(FARPROC*)&winmm.mmioFlush = GetProcAddress(winmm.hModule, "mmioFlush");
	*(FARPROC*)&winmm.mmioGetInfo = GetProcAddress(winmm.hModule, "mmioGetInfo");
	*(FARPROC*)&winmm.mmioRenameA = GetProcAddress(winmm.hModule, "mmioRenameA");
	*(FARPROC*)&winmm.mmioRenameW = GetProcAddress(winmm.hModule, "mmioRenameW");
	*(FARPROC*)&winmm.mmioSetBuffer = GetProcAddress(winmm.hModule, "mmioSetBuffer");
	*(FARPROC*)&winmm.mmioSetInfo = GetProcAddress(winmm.hModule, "mmioSetInfo");
	*(FARPROC*)&winmm.timeBeginPeriod = GetProcAddress(winmm.hModule, "timeBeginPeriod");
	*(FARPROC*)&winmm.timeEndPeriod = GetProcAddress(winmm.hModule, "timeEndPeriod");
	*(FARPROC*)&winmm.timeGetDevCaps = GetProcAddress(winmm.hModule, "timeGetDevCaps");
	*(FARPROC*)&winmm.timeGetSystemTime = GetProcAddress(winmm.hModule, "timeGetSystemTime");
	*(FARPROC*)&winmm.timeKillEvent = GetProcAddress(winmm.hModule, "timeKillEvent");
	*(FARPROC*)&winmm.timeSetEvent = GetProcAddress(winmm.hModule, "timeSetEvent");
	*(FARPROC*)&winmm.waveInAddBuffer = GetProcAddress(winmm.hModule, "waveInAddBuffer");
	*(FARPROC*)&winmm.waveInClose = GetProcAddress(winmm.hModule, "waveInClose");
	*(FARPROC*)&winmm.waveInGetDevCapsA = GetProcAddress(winmm.hModule, "waveInGetDevCapsA");
	*(FARPROC*)&winmm.waveInGetDevCapsW = GetProcAddress(winmm.hModule, "waveInGetDevCapsW");
	*(FARPROC*)&winmm.waveInGetErrorTextA = GetProcAddress(winmm.hModule, "waveInGetErrorTextA");
	*(FARPROC*)&winmm.waveInGetErrorTextW = GetProcAddress(winmm.hModule, "waveInGetErrorTextW");
	*(FARPROC*)&winmm.waveInGetID = GetProcAddress(winmm.hModule, "waveInGetID");
	*(FARPROC*)&winmm.waveInGetPosition = GetProcAddress(winmm.hModule, "waveInGetPosition");
	*(FARPROC*)&winmm.waveInMessage = GetProcAddress(winmm.hModule, "waveInMessage");
	*(FARPROC*)&winmm.waveInOpen = GetProcAddress(winmm.hModule, "waveInOpen");
	*(FARPROC*)&winmm.waveInPrepareHeader = GetProcAddress(winmm.hModule, "waveInPrepareHeader");
	*(FARPROC*)&winmm.waveInReset = GetProcAddress(winmm.hModule, "waveInReset");
	*(FARPROC*)&winmm.waveInStart = GetProcAddress(winmm.hModule, "waveInStart");
	*(FARPROC*)&winmm.waveInStop = GetProcAddress(winmm.hModule, "waveInStop");
	*(FARPROC*)&winmm.waveInUnprepareHeader = GetProcAddress(winmm.hModule, "waveInUnprepareHeader");
	*(FARPROC*)&winmm.waveOutBreakLoop = GetProcAddress(winmm.hModule, "waveOutBreakLoop");
	*(FARPROC*)&winmm.waveOutClose = GetProcAddress(winmm.hModule, "waveOutClose");
	*(FARPROC*)&winmm.waveOutGetDevCapsA = GetProcAddress(winmm.hModule, "waveOutGetDevCapsA");
	*(FARPROC*)&winmm.waveOutGetDevCapsW = GetProcAddress(winmm.hModule, "waveOutGetDevCapsW");
	*(FARPROC*)&winmm.waveOutGetErrorTextA = GetProcAddress(winmm.hModule, "waveOutGetErrorTextA");
	*(FARPROC*)&winmm.waveOutGetErrorTextW = GetProcAddress(winmm.hModule, "waveOutGetErrorTextW");
	*(FARPROC*)&winmm.waveOutGetID = GetProcAddress(winmm.hModule, "waveOutGetID");
	*(FARPROC*)&winmm.waveOutGetPitch = GetProcAddress(winmm.hModule, "waveOutGetPitch");
	*(FARPROC*)&winmm.waveOutGetPlaybackRate = GetProcAddress(winmm.hModule, "waveOutGetPlaybackRate");
	*(FARPROC*)&winmm.waveOutGetPosition = GetProcAddress(winmm.hModule, "waveOutGetPosition");
	*(FARPROC*)&winmm.waveOutGetVolume = GetProcAddress(winmm.hModule, "waveOutGetVolume");
	*(FARPROC*)&winmm.waveOutMessage = GetProcAddress(winmm.hModule, "waveOutMessage");
	*(FARPROC*)&winmm.waveOutOpen = GetProcAddress(winmm.hModule, "waveOutOpen");
	*(FARPROC*)&winmm.waveOutPause = GetProcAddress(winmm.hModule, "waveOutPause");
	*(FARPROC*)&winmm.waveOutPrepareHeader = GetProcAddress(winmm.hModule, "waveOutPrepareHeader");
	*(FARPROC*)&winmm.waveOutReset = GetProcAddress(winmm.hModule, "waveOutReset");
	*(FARPROC*)&winmm.waveOutRestart = GetProcAddress(winmm.hModule, "waveOutRestart");
	*(FARPROC*)&winmm.waveOutSetPitch = GetProcAddress(winmm.hModule, "waveOutSetPitch");
	*(FARPROC*)&winmm.waveOutSetPlaybackRate = GetProcAddress(winmm.hModule, "waveOutSetPlaybackRate");
	*(FARPROC*)&winmm.waveOutSetVolume = GetProcAddress(winmm.hModule, "waveOutSetVolume");
	*(FARPROC*)&winmm.waveOutUnprepareHeader = GetProcAddress(winmm.hModule, "waveOutUnprepareHeader");
	*(FARPROC*)&winmm.waveOutWrite = GetProcAddress(winmm.hModule, "waveOutWrite");
	*(FARPROC*)&winmm.auxGetNumDevs = GetProcAddress(winmm.hModule, "auxGetNumDevs");
	*(FARPROC*)&winmm.joyGetNumDevs = GetProcAddress(winmm.hModule, "joyGetNumDevs");
	*(FARPROC*)&winmm.midiInGetNumDevs = GetProcAddress(winmm.hModule, "midiInGetNumDevs");
	*(FARPROC*)&winmm.midiOutGetNumDevs = GetProcAddress(winmm.hModule, "midiOutGetNumDevs");
	*(FARPROC*)&winmm.mixerGetNumDevs = GetProcAddress(winmm.hModule, "mixerGetNumDevs");
	*(FARPROC*)&winmm.mmsystemGetVersion = GetProcAddress(winmm.hModule, "mmsystemGetVersion");
	*(FARPROC*)&winmm.waveInGetNumDevs = GetProcAddress(winmm.hModule, "waveInGetNumDevs");
	*(FARPROC*)&winmm.waveOutGetNumDevs = GetProcAddress(winmm.hModule, "waveOutGetNumDevs");
	*(FARPROC*)&winmm.mciGetYieldProc = GetProcAddress(winmm.hModule, "mciGetYieldProc");
	*(FARPROC*)&winmm.DriverCallback = GetProcAddress(winmm.hModule, "DriverCallback");
		
	return TRUE;
}

EXTERN_C BOOL APIENTRY HookDllMain(HANDLE hModule, DWORD  ul_reason_for_call);
EXTERN_C DWORD (WINAPI *pHookTimeGetTime)(DWORD) = 0;

BOOL APIENTRY DllMain( HANDLE hModule, 
                       DWORD  ul_reason_for_call, 
                       LPVOID lpReserved
					 )
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
	{
		if (!DelayLoadWinmm())
			return FALSE;
	}
	else if (ul_reason_for_call == DLL_PROCESS_DETACH)
	{
		if (winmm.hModule) FreeLibrary(winmm.hModule);
		winmm.hModule = NULL;
	}
	HookDllMain(hModule, ul_reason_for_call);
	return TRUE;
}

DLLEXPORT DWORD  WINAPI timeGetTime(void)
{
	DWORD result = winmm.timeGetTime();
	if (pHookTimeGetTime)
		result = pHookTimeGetTime(result);
	return result;
}

DLLEXPORT BOOL  WINAPI mciExecute(LPCSTR pszCommand)
{
    return winmm.mciExecute( pszCommand);
}

DLLEXPORT BOOL  WINAPI mciGetErrorStringA( IN MCIERROR mcierr, OUT LPSTR pszText, IN UINT cchText)
{
    return winmm.mciGetErrorStringA(mcierr,pszText,cchText);
}

DLLEXPORT BOOL  WINAPI mciGetErrorStringW( IN MCIERROR mcierr, OUT LPWSTR pszText, IN UINT cchText)
{
    return winmm.mciGetErrorStringW(mcierr,pszText,cchText);
}

DLLEXPORT BOOL  WINAPI mciSetYieldProc( IN MCIDEVICEID mciId, IN YIELDPROC fpYieldProc, IN DWORD dwYieldData)
{
    return winmm.mciSetYieldProc(mciId,fpYieldProc,dwYieldData);
}

DLLEXPORT BOOL  WINAPI PlaySound( IN LPCSTR pszSound, IN HMODULE hmod, IN DWORD fdwSound)
{
    return winmm.PlaySound(pszSound,hmod,fdwSound);
}

DLLEXPORT BOOL  WINAPI PlaySoundA( IN LPCSTR pszSound, IN HMODULE hmod, IN DWORD fdwSound)
{
    return winmm.PlaySoundA(pszSound,hmod,fdwSound);
}

DLLEXPORT BOOL  WINAPI PlaySoundW( IN LPCWSTR pszSound, IN HMODULE hmod, IN DWORD fdwSound)
{
    return winmm.PlaySoundW(pszSound,hmod,fdwSound);
}

DLLEXPORT BOOL  WINAPI sndPlaySoundA( IN LPCSTR pszSound, IN UINT fuSound)
{
    return winmm.sndPlaySoundA(pszSound,fuSound);
}

DLLEXPORT BOOL  WINAPI sndPlaySoundW( IN LPCWSTR pszSound, IN UINT fuSound)
{
    return winmm.sndPlaySoundW(pszSound,fuSound);
}

DLLEXPORT DWORD  WINAPI mixerMessage( IN HMIXER hmx, IN UINT uMsg, IN DWORD_PTR dwParam1, IN DWORD_PTR dwParam2)
{
    return winmm.mixerMessage(hmx,uMsg,dwParam1,dwParam2);
}

DLLEXPORT FOURCC  WINAPI mmioStringToFOURCCA( IN LPCSTR sz, IN UINT uFlags)
{
    return winmm.mmioStringToFOURCCA(sz,uFlags);
}

DLLEXPORT FOURCC  WINAPI mmioStringToFOURCCW( IN LPCWSTR sz, IN UINT uFlags)
{
    return winmm.mmioStringToFOURCCW(sz,uFlags);
}

DLLEXPORT HDRVR      WINAPI OpenDriver( IN LPCWSTR szDriverName, IN LPCWSTR szSectionName, IN LPARAM lParam2)
{
    return winmm.OpenDriver(szDriverName,szSectionName,lParam2);
}

DLLEXPORT HMMIO  WINAPI mmioOpenA( IN OUT LPSTR pszFileName, IN OUT LPMMIOINFO pmmioinfo, IN DWORD fdwOpen)
{
    return winmm.mmioOpenA(pszFileName,pmmioinfo,fdwOpen);
}

DLLEXPORT HMMIO  WINAPI mmioOpenW( IN OUT LPWSTR pszFileName, IN OUT LPMMIOINFO pmmioinfo, IN DWORD fdwOpen)
{
    return winmm.mmioOpenW(pszFileName,pmmioinfo,fdwOpen);
}

DLLEXPORT HMODULE    WINAPI DrvGetModuleHandle( IN HDRVR hDriver)
{
    return winmm.DrvGetModuleHandle(hDriver);
}

DLLEXPORT HMODULE    WINAPI GetDriverModuleHandle( IN HDRVR hDriver)
{
    return winmm.GetDriverModuleHandle(hDriver);
}

DLLEXPORT HTASK  WINAPI mciGetCreatorTask( IN MCIDEVICEID mciId)
{
    return winmm.mciGetCreatorTask(mciId);
}

DLLEXPORT LONG  WINAPI mmioRead( IN HMMIO hmmio, OUT HPSTR pch, IN LONG cch)
{
    return winmm.mmioRead(hmmio,pch,cch);
}

DLLEXPORT LONG  WINAPI mmioSeek( IN HMMIO hmmio, IN LONG lOffset, IN int iOrigin)
{
    return winmm.mmioSeek(hmmio,lOffset,iOrigin);
}

DLLEXPORT LONG  WINAPI mmioWrite( IN HMMIO hmmio, IN const char _huge* pch, IN LONG cch)
{
    return winmm.mmioWrite(hmmio,pch,cch);
}

DLLEXPORT LPMMIOPROC  WINAPI mmioInstallIOProcA( IN FOURCC fccIOProc, IN LPMMIOPROC pIOProc, IN DWORD dwFlags)
{
    return winmm.mmioInstallIOProcA(fccIOProc,pIOProc,dwFlags);
}

DLLEXPORT LPMMIOPROC  WINAPI mmioInstallIOProcW( IN FOURCC fccIOProc, IN LPMMIOPROC pIOProc, IN DWORD dwFlags)
{
    return winmm.mmioInstallIOProcW(fccIOProc,pIOProc,dwFlags);
}

DLLEXPORT LRESULT    WINAPI CloseDriver( IN HDRVR hDriver, IN LPARAM lParam1, IN LPARAM lParam2)
{
    return winmm.CloseDriver(hDriver,lParam1,lParam2);
}

DLLEXPORT LRESULT    WINAPI DefDriverProc( IN DWORD_PTR dwDriverIdentifier, IN HDRVR hdrvr, IN UINT uMsg, IN LPARAM lParam1, IN LPARAM lParam2)
{
    return winmm.DefDriverProc(dwDriverIdentifier,hdrvr,uMsg,lParam1,lParam2);
}

DLLEXPORT LRESULT    WINAPI DrvDefDriverProc(DWORD dwDriverIdentifier, HDRVR hdrvr, UINT uMsg, LPARAM lParam1, LPARAM lParam2)
{
    return winmm.DrvDefDriverProc(dwDriverIdentifier,hdrvr,uMsg,lParam1,lParam2);
}

DLLEXPORT LRESULT    WINAPI SendDriverMessage( IN HDRVR hDriver, IN UINT message, IN LPARAM lParam1, IN LPARAM lParam2)
{
    return winmm.SendDriverMessage(hDriver,message,lParam1,lParam2);
}

DLLEXPORT LRESULT  WINAPI mmioSendMessage( IN HMMIO hmmio, IN UINT uMsg, IN LPARAM lParam1, IN LPARAM lParam2)
{
    return winmm.mmioSendMessage(hmmio,uMsg,lParam1,lParam2);
}

DLLEXPORT MCIDEVICEID  WINAPI mciGetDeviceIDA( IN LPCSTR pszDevice)
{
    return winmm.mciGetDeviceIDA(pszDevice);
}

DLLEXPORT MCIDEVICEID  WINAPI mciGetDeviceIDFromElementIDA( IN DWORD dwElementID, IN LPCSTR lpstrType )
{
    return winmm.mciGetDeviceIDFromElementIDA(dwElementID, lpstrType );
}

DLLEXPORT MCIDEVICEID  WINAPI mciGetDeviceIDFromElementIDW( IN DWORD dwElementID, IN LPCWSTR lpstrType )
{
    return winmm.mciGetDeviceIDFromElementIDW(dwElementID, lpstrType );
}

DLLEXPORT MCIDEVICEID  WINAPI mciGetDeviceIDW( IN LPCWSTR pszDevice)
{
    return winmm.mciGetDeviceIDW(pszDevice);
}

DLLEXPORT MCIERROR   WINAPI mciSendStringA( IN LPCSTR lpstrCommand, OUT LPSTR lpstrReturnString, IN UINT uReturnLength, IN HWND hwndCallback)
{
    return winmm.mciSendStringA(lpstrCommand,lpstrReturnString,uReturnLength,hwndCallback);
}

DLLEXPORT MCIERROR   WINAPI mciSendStringW( IN LPCWSTR lpstrCommand, OUT LPWSTR lpstrReturnString, IN UINT uReturnLength, IN HWND hwndCallback)
{
    return winmm.mciSendStringW(lpstrCommand,lpstrReturnString,uReturnLength,hwndCallback);
}

DLLEXPORT MCIERROR  WINAPI mciSendCommandA( IN MCIDEVICEID mciId, IN UINT uMsg, IN DWORD_PTR dwParam1, IN DWORD_PTR dwParam2)
{
    return winmm.mciSendCommandA(mciId,uMsg,dwParam1,dwParam2);
}

DLLEXPORT MCIERROR  WINAPI mciSendCommandW( IN MCIDEVICEID mciId, IN UINT uMsg, IN DWORD_PTR dwParam1, IN DWORD_PTR dwParam2)
{
    return winmm.mciSendCommandW(mciId,uMsg,dwParam1,dwParam2);
}

DLLEXPORT MMRESULT  WINAPI auxGetDevCapsA( IN UINT_PTR uDeviceID, OUT LPAUXCAPSA pac, IN UINT cbac)
{
    return winmm.auxGetDevCapsA(uDeviceID,pac,cbac);
}

DLLEXPORT MMRESULT  WINAPI auxGetDevCapsW( IN UINT_PTR uDeviceID, OUT LPAUXCAPSW pac, IN UINT cbac)
{
    return winmm.auxGetDevCapsW(uDeviceID,pac,cbac);
}

DLLEXPORT MMRESULT  WINAPI auxGetVolume( IN UINT uDeviceID, OUT LPDWORD pdwVolume)
{
    return winmm.auxGetVolume(uDeviceID,pdwVolume);
}

DLLEXPORT MMRESULT  WINAPI auxOutMessage( IN UINT uDeviceID, IN UINT uMsg, IN DWORD_PTR dw1, IN DWORD_PTR dw2)
{
    return winmm.auxOutMessage(uDeviceID,uMsg,dw1,dw2);
}

DLLEXPORT MMRESULT  WINAPI auxSetVolume( IN UINT uDeviceID, IN DWORD dwVolume)
{
    return winmm.auxSetVolume(uDeviceID,dwVolume);
}

DLLEXPORT MMRESULT  WINAPI joyGetDevCapsA( IN UINT_PTR uJoyID, OUT LPJOYCAPSA pjc, IN UINT cbjc)
{
    return winmm.joyGetDevCapsA(uJoyID,pjc,cbjc);
}

DLLEXPORT MMRESULT  WINAPI joyGetDevCapsW( IN UINT_PTR uJoyID, OUT LPJOYCAPSW pjc, IN UINT cbjc)
{
    return winmm.joyGetDevCapsW(uJoyID,pjc,cbjc);
}

DLLEXPORT MMRESULT  WINAPI joyGetPos( IN UINT uJoyID, OUT LPJOYINFO pji)
{
    return winmm.joyGetPos(uJoyID,pji);
}

DLLEXPORT MMRESULT  WINAPI joyGetPosEx( IN UINT uJoyID, OUT LPJOYINFOEX pji)
{
    return winmm.joyGetPosEx(uJoyID,pji);
}

DLLEXPORT MMRESULT  WINAPI joyGetThreshold( IN UINT uJoyID, OUT LPUINT puThreshold)
{
    return winmm.joyGetThreshold(uJoyID,puThreshold);
}

DLLEXPORT MMRESULT  WINAPI joyReleaseCapture( IN UINT uJoyID)
{
    return winmm.joyReleaseCapture(uJoyID);
}

DLLEXPORT MMRESULT  WINAPI joySetCapture( IN HWND hwnd, IN UINT uJoyID, IN UINT uPeriod, IN BOOL fChanged)
{
    return winmm.joySetCapture(hwnd,uJoyID,uPeriod,fChanged);
}

DLLEXPORT MMRESULT  WINAPI joySetThreshold( IN UINT uJoyID, IN UINT uThreshold)
{
    return winmm.joySetThreshold(uJoyID,uThreshold);
}

DLLEXPORT MMRESULT  WINAPI midiConnect( IN HMIDI hmi, IN HMIDIOUT hmo, IN LPVOID pReserved)
{
    return winmm.midiConnect(hmi,hmo,pReserved);
}

DLLEXPORT MMRESULT  WINAPI midiDisconnect( IN HMIDI hmi, IN HMIDIOUT hmo, IN LPVOID pReserved)
{
    return winmm.midiDisconnect(hmi,hmo,pReserved);
}

DLLEXPORT MMRESULT  WINAPI midiInAddBuffer( IN HMIDIIN hmi, IN LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiInAddBuffer(hmi,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiInClose( IN OUT HMIDIIN hmi)
{
    return winmm.midiInClose(hmi);
}

DLLEXPORT MMRESULT  WINAPI midiInGetDevCapsA( IN UINT_PTR uDeviceID, OUT LPMIDIINCAPSA pmic, IN UINT cbmic)
{
    return winmm.midiInGetDevCapsA(uDeviceID,pmic,cbmic);
}

DLLEXPORT MMRESULT  WINAPI midiInGetDevCapsW( IN UINT_PTR uDeviceID, OUT LPMIDIINCAPSW pmic, IN UINT cbmic)
{
    return winmm.midiInGetDevCapsW(uDeviceID,pmic,cbmic);
}

DLLEXPORT MMRESULT  WINAPI midiInGetErrorTextA( IN MMRESULT mmrError, OUT LPSTR pszText, IN UINT cchText)
{
    return winmm.midiInGetErrorTextA(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI midiInGetErrorTextW( IN MMRESULT mmrError, OUT LPWSTR pszText, IN UINT cchText)
{
    return winmm.midiInGetErrorTextW(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI midiInGetID( IN HMIDIIN hmi, OUT LPUINT puDeviceID)
{
    return winmm.midiInGetID(hmi,puDeviceID);
}

DLLEXPORT MMRESULT  WINAPI midiInMessage( IN HMIDIIN hmi, IN UINT uMsg, IN DWORD_PTR dw1, IN DWORD_PTR dw2)
{
    return winmm.midiInMessage(hmi,uMsg,dw1,dw2);
}

DLLEXPORT MMRESULT  WINAPI midiInOpen( OUT LPHMIDIIN phmi, IN UINT uDeviceID, IN DWORD_PTR dwCallback, IN DWORD_PTR dwInstance, IN DWORD fdwOpen)
{
    return winmm.midiInOpen(phmi,uDeviceID,dwCallback,dwInstance,fdwOpen);
}

DLLEXPORT MMRESULT  WINAPI midiInPrepareHeader( IN HMIDIIN hmi, IN OUT LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiInPrepareHeader(hmi,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiInReset( IN HMIDIIN hmi)
{
    return winmm.midiInReset(hmi);
}

DLLEXPORT MMRESULT  WINAPI midiInStart( IN HMIDIIN hmi)
{
    return winmm.midiInStart(hmi);
}

DLLEXPORT MMRESULT  WINAPI midiInStop( IN HMIDIIN hmi)
{
    return winmm.midiInStop(hmi);
}

DLLEXPORT MMRESULT  WINAPI midiInUnprepareHeader( IN HMIDIIN hmi, IN OUT LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiInUnprepareHeader(hmi,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiOutCacheDrumPatches( IN HMIDIOUT hmo, IN UINT uPatch, OUT LPWORD pwkya, IN UINT fuCache)
{
    return winmm.midiOutCacheDrumPatches(hmo,uPatch,pwkya,fuCache);
}

DLLEXPORT MMRESULT  WINAPI midiOutCachePatches( IN HMIDIOUT hmo, IN UINT uBank, OUT LPWORD pwpa, IN UINT fuCache)
{
    return winmm.midiOutCachePatches(hmo,uBank,pwpa,fuCache);
}

DLLEXPORT MMRESULT  WINAPI midiOutClose( IN OUT HMIDIOUT hmo)
{
    return winmm.midiOutClose(hmo);
}

DLLEXPORT MMRESULT  WINAPI midiOutGetDevCapsA( IN UINT_PTR uDeviceID, OUT LPMIDIOUTCAPSA pmoc, IN UINT cbmoc)
{
    return winmm.midiOutGetDevCapsA(uDeviceID,pmoc,cbmoc);
}

DLLEXPORT MMRESULT  WINAPI midiOutGetDevCapsW( IN UINT_PTR uDeviceID, OUT LPMIDIOUTCAPSW pmoc, IN UINT cbmoc)
{
    return winmm.midiOutGetDevCapsW(uDeviceID,pmoc,cbmoc);
}

DLLEXPORT MMRESULT  WINAPI midiOutGetErrorTextA( IN MMRESULT mmrError, OUT LPSTR pszText, IN UINT cchText)
{
    return winmm.midiOutGetErrorTextA(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI midiOutGetErrorTextW( IN MMRESULT mmrError, OUT LPWSTR pszText, IN UINT cchText)
{
    return winmm.midiOutGetErrorTextW(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI midiOutGetID( IN HMIDIOUT hmo, OUT LPUINT puDeviceID)
{
    return winmm.midiOutGetID(hmo,puDeviceID);
}

DLLEXPORT MMRESULT  WINAPI midiOutGetVolume( IN HMIDIOUT hmo, OUT LPDWORD pdwVolume)
{
    return winmm.midiOutGetVolume(hmo,pdwVolume);
}

DLLEXPORT MMRESULT  WINAPI midiOutLongMsg(IN HMIDIOUT hmo, IN LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiOutLongMsg(hmo,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiOutMessage( IN HMIDIOUT hmo, IN UINT uMsg, IN DWORD_PTR dw1, IN DWORD_PTR dw2)
{
    return winmm.midiOutMessage(hmo,uMsg,dw1,dw2);
}

DLLEXPORT MMRESULT  WINAPI midiOutOpen( OUT LPHMIDIOUT phmo, IN UINT uDeviceID, IN DWORD_PTR dwCallback, IN DWORD_PTR dwInstance, IN DWORD fdwOpen)
{
    return winmm.midiOutOpen(phmo,uDeviceID,dwCallback,dwInstance,fdwOpen);
}

DLLEXPORT MMRESULT  WINAPI midiOutPrepareHeader( IN HMIDIOUT hmo, IN OUT LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiOutPrepareHeader(hmo,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiOutReset( IN HMIDIOUT hmo)
{
    return winmm.midiOutReset(hmo);
}

DLLEXPORT MMRESULT  WINAPI midiOutSetVolume( IN HMIDIOUT hmo, IN DWORD dwVolume)
{
    return winmm.midiOutSetVolume(hmo,dwVolume);
}

DLLEXPORT MMRESULT  WINAPI midiOutShortMsg( IN HMIDIOUT hmo, IN DWORD dwMsg)
{
    return winmm.midiOutShortMsg(hmo,dwMsg);
}

DLLEXPORT MMRESULT  WINAPI midiOutUnprepareHeader(IN HMIDIOUT hmo, IN OUT LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiOutUnprepareHeader(hmo,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiStreamClose( IN HMIDISTRM hms)
{
    return winmm.midiStreamClose(hms);
}

DLLEXPORT MMRESULT  WINAPI midiStreamOpen( OUT LPHMIDISTRM phms, IN LPUINT puDeviceID, IN DWORD cMidi, IN DWORD_PTR dwCallback, IN DWORD_PTR dwInstance, IN DWORD fdwOpen)
{
    return winmm.midiStreamOpen(phms,puDeviceID,cMidi,dwCallback,dwInstance,fdwOpen);
}

DLLEXPORT MMRESULT  WINAPI midiStreamOut( IN HMIDISTRM hms, IN LPMIDIHDR pmh, IN UINT cbmh)
{
    return winmm.midiStreamOut(hms,pmh,cbmh);
}

DLLEXPORT MMRESULT  WINAPI midiStreamPause( IN HMIDISTRM hms)
{
    return winmm.midiStreamPause(hms);
}

DLLEXPORT MMRESULT  WINAPI midiStreamPosition( IN HMIDISTRM hms, OUT LPMMTIME lpmmt, IN UINT cbmmt)
{
    return winmm.midiStreamPosition(hms,lpmmt,cbmmt);
}

DLLEXPORT MMRESULT  WINAPI midiStreamProperty( IN HMIDISTRM hms, OUT LPBYTE lppropdata, IN DWORD dwProperty)
{
    return winmm.midiStreamProperty(hms,lppropdata,dwProperty);
}

DLLEXPORT MMRESULT  WINAPI midiStreamRestart( IN HMIDISTRM hms)
{
    return winmm.midiStreamRestart(hms);
}

DLLEXPORT MMRESULT  WINAPI midiStreamStop( IN HMIDISTRM hms)
{
    return winmm.midiStreamStop(hms);
}

DLLEXPORT MMRESULT  WINAPI mixerClose( IN OUT HMIXER hmx)
{
    return winmm.mixerClose(hmx);
}

DLLEXPORT MMRESULT  WINAPI mixerGetControlDetailsA( IN HMIXEROBJ hmxobj, IN OUT LPMIXERCONTROLDETAILS pmxcd, IN DWORD fdwDetails)
{
    return winmm.mixerGetControlDetailsA(hmxobj,pmxcd,fdwDetails);
}

DLLEXPORT MMRESULT  WINAPI mixerGetControlDetailsW( IN HMIXEROBJ hmxobj, IN OUT LPMIXERCONTROLDETAILS pmxcd, IN DWORD fdwDetails)
{
    return winmm.mixerGetControlDetailsW(hmxobj,pmxcd,fdwDetails);
}

DLLEXPORT MMRESULT  WINAPI mixerGetDevCapsA( IN UINT_PTR uMxId, OUT LPMIXERCAPSA pmxcaps, IN UINT cbmxcaps)
{
    return winmm.mixerGetDevCapsA(uMxId,pmxcaps,cbmxcaps);
}

DLLEXPORT MMRESULT  WINAPI mixerGetDevCapsW( IN UINT_PTR uMxId, OUT LPMIXERCAPSW pmxcaps, IN UINT cbmxcaps)
{
    return winmm.mixerGetDevCapsW(uMxId,pmxcaps,cbmxcaps);
}

DLLEXPORT MMRESULT  WINAPI mixerGetID( IN HMIXEROBJ hmxobj, OUT UINT FAR *puMxId, IN DWORD fdwId)
{
    return winmm.mixerGetID(hmxobj, puMxId, fdwId);
}

DLLEXPORT MMRESULT  WINAPI mixerGetLineControlsA( IN HMIXEROBJ hmxobj, IN OUT LPMIXERLINECONTROLSA pmxlc, IN DWORD fdwControls)
{
    return winmm.mixerGetLineControlsA(hmxobj,pmxlc,fdwControls);
}

DLLEXPORT MMRESULT  WINAPI mixerGetLineControlsW( IN HMIXEROBJ hmxobj, IN OUT LPMIXERLINECONTROLSW pmxlc, IN DWORD fdwControls)
{
    return winmm.mixerGetLineControlsW(hmxobj,pmxlc,fdwControls);
}

DLLEXPORT MMRESULT  WINAPI mixerGetLineInfoA( IN HMIXEROBJ hmxobj, OUT LPMIXERLINEA pmxl, IN DWORD fdwInfo)
{
    return winmm.mixerGetLineInfoA(hmxobj,pmxl,fdwInfo);
}

DLLEXPORT MMRESULT  WINAPI mixerGetLineInfoW( IN HMIXEROBJ hmxobj, OUT LPMIXERLINEW pmxl, IN DWORD fdwInfo)
{
    return winmm.mixerGetLineInfoW(hmxobj,pmxl,fdwInfo);
}

DLLEXPORT MMRESULT  WINAPI mixerOpen( OUT LPHMIXER phmx, IN UINT uMxId, IN DWORD_PTR dwCallback, IN DWORD_PTR dwInstance, IN DWORD fdwOpen)
{
    return winmm.mixerOpen(phmx,uMxId,dwCallback,dwInstance,fdwOpen);
}

DLLEXPORT MMRESULT  WINAPI mixerSetControlDetails( IN HMIXEROBJ hmxobj, IN LPMIXERCONTROLDETAILS pmxcd, IN DWORD fdwDetails)
{
    return winmm.mixerSetControlDetails(hmxobj,pmxcd,fdwDetails);
}

DLLEXPORT MMRESULT  WINAPI mmioAdvance( IN HMMIO hmmio, IN OUT LPMMIOINFO pmmioinfo, IN UINT fuAdvance)
{
    return winmm.mmioAdvance(hmmio,pmmioinfo,fuAdvance);
}

DLLEXPORT MMRESULT  WINAPI mmioAscend( IN HMMIO hmmio, IN LPMMCKINFO pmmcki, IN UINT fuAscend)
{
    return winmm.mmioAscend(hmmio,pmmcki,fuAscend);
}

DLLEXPORT MMRESULT  WINAPI mmioClose( IN HMMIO hmmio, IN UINT fuClose)
{
    return winmm.mmioClose(hmmio,fuClose);
}

DLLEXPORT MMRESULT  WINAPI mmioCreateChunk(IN HMMIO hmmio, IN LPMMCKINFO pmmcki, IN UINT fuCreate)
{
    return winmm.mmioCreateChunk(hmmio,pmmcki,fuCreate);
}

DLLEXPORT MMRESULT  WINAPI mmioDescend( IN HMMIO hmmio, IN OUT LPMMCKINFO pmmcki, IN const MMCKINFO FAR* pmmckiParent, IN UINT fuDescend)
{
    return winmm.mmioDescend(hmmio,pmmcki,pmmckiParent,fuDescend);
}

DLLEXPORT MMRESULT  WINAPI mmioFlush( IN HMMIO hmmio, IN UINT fuFlush)
{
    return winmm.mmioFlush(hmmio,fuFlush);
}

DLLEXPORT MMRESULT  WINAPI mmioGetInfo( IN HMMIO hmmio, OUT LPMMIOINFO pmmioinfo, IN UINT fuInfo)
{
    return winmm.mmioGetInfo(hmmio,pmmioinfo,fuInfo);
}

DLLEXPORT MMRESULT  WINAPI mmioRenameA( IN LPCSTR pszFileName, IN LPCSTR pszNewFileName, IN LPCMMIOINFO pmmioinfo, IN DWORD fdwRename)
{
    return winmm.mmioRenameA(pszFileName,pszNewFileName,pmmioinfo,fdwRename);
}

DLLEXPORT MMRESULT  WINAPI mmioRenameW( IN LPCWSTR pszFileName, IN LPCWSTR pszNewFileName, IN LPCMMIOINFO pmmioinfo, IN DWORD fdwRename)
{
    return winmm.mmioRenameW(pszFileName,pszNewFileName,pmmioinfo,fdwRename);
}

DLLEXPORT MMRESULT  WINAPI mmioSetBuffer( IN HMMIO hmmio, IN LPSTR pchBuffer, IN LONG cchBuffer, IN UINT fuBuffer)
{
    return winmm.mmioSetBuffer(hmmio,pchBuffer,cchBuffer,fuBuffer);
}

DLLEXPORT MMRESULT  WINAPI mmioSetInfo( IN HMMIO hmmio, IN LPCMMIOINFO pmmioinfo, IN UINT fuInfo)
{
    return winmm.mmioSetInfo(hmmio,pmmioinfo,fuInfo);
}

DLLEXPORT MMRESULT  WINAPI timeBeginPeriod( IN UINT uPeriod)
{
    return winmm.timeBeginPeriod(uPeriod);
}

DLLEXPORT MMRESULT  WINAPI timeEndPeriod( IN UINT uPeriod)
{
    return winmm.timeEndPeriod(uPeriod);
}

DLLEXPORT MMRESULT  WINAPI timeGetDevCaps( OUT LPTIMECAPS ptc, IN UINT cbtc)
{
    return winmm.timeGetDevCaps(ptc,cbtc);
}

DLLEXPORT MMRESULT  WINAPI timeGetSystemTime( OUT LPMMTIME pmmt, IN UINT cbmmt)
{
    return winmm.timeGetSystemTime(pmmt,cbmmt);
}

DLLEXPORT MMRESULT  WINAPI timeKillEvent( IN UINT uTimerID)
{
    return winmm.timeKillEvent(uTimerID);
}

DLLEXPORT MMRESULT  WINAPI timeSetEvent( IN UINT uDelay, IN UINT uResolution, IN LPTIMECALLBACK fptc, IN DWORD_PTR dwUser, IN UINT fuEvent)
{
    return winmm.timeSetEvent(uDelay,uResolution,fptc,dwUser,fuEvent);
}

DLLEXPORT MMRESULT  WINAPI waveInAddBuffer( IN HWAVEIN hwi, IN OUT LPWAVEHDR pwh, IN UINT cbwh)
{
    return winmm.waveInAddBuffer(hwi,pwh,cbwh);
}

DLLEXPORT MMRESULT  WINAPI waveInClose( IN OUT HWAVEIN hwi)
{
    return winmm.waveInClose(hwi);
}

DLLEXPORT MMRESULT  WINAPI waveInGetDevCapsA( IN UINT_PTR uDeviceID, OUT LPWAVEINCAPSA pwic, IN UINT cbwic)
{
    return winmm.waveInGetDevCapsA(uDeviceID,pwic,cbwic);
}

DLLEXPORT MMRESULT  WINAPI waveInGetDevCapsW( IN UINT_PTR uDeviceID, OUT LPWAVEINCAPSW pwic, IN UINT cbwic)
{
    return winmm.waveInGetDevCapsW(uDeviceID,pwic,cbwic);
}

DLLEXPORT MMRESULT  WINAPI waveInGetErrorTextA(IN MMRESULT mmrError, OUT LPSTR pszText, IN UINT cchText)
{
    return winmm.waveInGetErrorTextA(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI waveInGetErrorTextW(IN MMRESULT mmrError, OUT LPWSTR pszText, IN UINT cchText)
{
    return winmm.waveInGetErrorTextW(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI waveInGetID( IN HWAVEIN hwi, OUT LPUINT puDeviceID)
{
    return winmm.waveInGetID(hwi,puDeviceID);
}

DLLEXPORT MMRESULT  WINAPI waveInGetPosition( IN HWAVEIN hwi, IN OUT LPMMTIME pmmt, IN UINT cbmmt)
{
    return winmm.waveInGetPosition(hwi,pmmt,cbmmt);
}

DLLEXPORT MMRESULT  WINAPI waveInMessage( IN HWAVEIN hwi, IN UINT uMsg, IN DWORD_PTR dw1, IN DWORD_PTR dw2)
{
    return winmm.waveInMessage(hwi,uMsg,dw1,dw2);
}

DLLEXPORT MMRESULT  WINAPI waveInOpen( OUT LPHWAVEIN phwi, IN UINT uDeviceID, IN LPCWAVEFORMATEX pwfx, IN DWORD_PTR dwCallback, IN DWORD_PTR dwInstance, IN DWORD fdwOpen)
{
    return winmm.waveInOpen(phwi,uDeviceID,pwfx,dwCallback,dwInstance,fdwOpen);
}

DLLEXPORT MMRESULT  WINAPI waveInPrepareHeader( IN HWAVEIN hwi, IN OUT LPWAVEHDR pwh, IN UINT cbwh)
{
    return winmm.waveInPrepareHeader(hwi,pwh,cbwh);
}

DLLEXPORT MMRESULT  WINAPI waveInReset( IN HWAVEIN hwi)
{
    return winmm.waveInReset(hwi);
}

DLLEXPORT MMRESULT  WINAPI waveInStart( IN HWAVEIN hwi)
{
    return winmm.waveInStart(hwi);
}

DLLEXPORT MMRESULT  WINAPI waveInStop( IN HWAVEIN hwi)
{
    return winmm.waveInStop(hwi);
}

DLLEXPORT MMRESULT  WINAPI waveInUnprepareHeader( IN HWAVEIN hwi, IN OUT LPWAVEHDR pwh, UINT cbwh)
{
    return winmm.waveInUnprepareHeader(hwi,pwh,cbwh);
}

DLLEXPORT MMRESULT  WINAPI waveOutBreakLoop( IN HWAVEOUT hwo)
{
    return winmm.waveOutBreakLoop(hwo);
}

DLLEXPORT MMRESULT  WINAPI waveOutClose( IN OUT HWAVEOUT hwo)
{
    return winmm.waveOutClose(hwo);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetDevCapsA( IN UINT_PTR uDeviceID, OUT LPWAVEOUTCAPSA pwoc, IN UINT cbwoc)
{
    return winmm.waveOutGetDevCapsA(uDeviceID,pwoc,cbwoc);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetDevCapsW( IN UINT_PTR uDeviceID, OUT LPWAVEOUTCAPSW pwoc, IN UINT cbwoc)
{
    return winmm.waveOutGetDevCapsW(uDeviceID,pwoc,cbwoc);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetErrorTextA( IN MMRESULT mmrError, OUT LPSTR pszText, IN UINT cchText)
{
    return winmm.waveOutGetErrorTextA(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetErrorTextW( IN MMRESULT mmrError, OUT LPWSTR pszText, IN UINT cchText)
{
    return winmm.waveOutGetErrorTextW(mmrError,pszText,cchText);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetID( IN HWAVEOUT hwo, OUT LPUINT puDeviceID)
{
    return winmm.waveOutGetID(hwo,puDeviceID);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetPitch( IN HWAVEOUT hwo, OUT LPDWORD pdwPitch)
{
    return winmm.waveOutGetPitch(hwo,pdwPitch);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetPlaybackRate( IN HWAVEOUT hwo, OUT LPDWORD pdwRate)
{
    return winmm.waveOutGetPlaybackRate(hwo,pdwRate);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetPosition( IN HWAVEOUT hwo, IN OUT LPMMTIME pmmt, IN UINT cbmmt)
{
    return winmm.waveOutGetPosition(hwo,pmmt,cbmmt);
}

DLLEXPORT MMRESULT  WINAPI waveOutGetVolume( IN HWAVEOUT hwo, OUT LPDWORD pdwVolume)
{
    return winmm.waveOutGetVolume(hwo,pdwVolume);
}

DLLEXPORT MMRESULT  WINAPI waveOutMessage( IN HWAVEOUT hwo, IN UINT uMsg, IN DWORD_PTR dw1, IN DWORD_PTR dw2)
{
    return winmm.waveOutMessage(hwo,uMsg,dw1,dw2);
}

DLLEXPORT MMRESULT  WINAPI waveOutOpen( OUT LPHWAVEOUT phwo, IN UINT uDeviceID, IN LPCWAVEFORMATEX pwfx, IN DWORD_PTR dwCallback, IN DWORD_PTR dwInstance, IN DWORD fdwOpen)
{
    return winmm.waveOutOpen(phwo,uDeviceID,pwfx,dwCallback,dwInstance,fdwOpen);
}

DLLEXPORT MMRESULT  WINAPI waveOutPause( IN HWAVEOUT hwo)
{
    return winmm.waveOutPause(hwo);
}

DLLEXPORT MMRESULT  WINAPI waveOutPrepareHeader( IN HWAVEOUT hwo, IN OUT LPWAVEHDR pwh, IN UINT cbwh)
{
    return winmm.waveOutPrepareHeader(hwo,pwh,cbwh);
}

DLLEXPORT MMRESULT  WINAPI waveOutReset( IN HWAVEOUT hwo)
{
    return winmm.waveOutReset(hwo);
}

DLLEXPORT MMRESULT  WINAPI waveOutRestart( IN HWAVEOUT hwo)
{
    return winmm.waveOutRestart(hwo);
}

DLLEXPORT MMRESULT  WINAPI waveOutSetPitch( IN HWAVEOUT hwo, IN DWORD dwPitch)
{
    return winmm.waveOutSetPitch(hwo,dwPitch);
}

DLLEXPORT MMRESULT  WINAPI waveOutSetPlaybackRate( IN HWAVEOUT hwo, IN DWORD dwRate)
{
    return winmm.waveOutSetPlaybackRate(hwo,dwRate);
}

DLLEXPORT MMRESULT  WINAPI waveOutSetVolume( IN HWAVEOUT hwo, IN DWORD dwVolume)
{
    return winmm.waveOutSetVolume(hwo,dwVolume);
}

DLLEXPORT MMRESULT  WINAPI waveOutUnprepareHeader( IN HWAVEOUT hwo, IN OUT LPWAVEHDR pwh, IN UINT cbwh)
{
    return winmm.waveOutUnprepareHeader(hwo,pwh,cbwh);
}

DLLEXPORT MMRESULT  WINAPI waveOutWrite( IN HWAVEOUT hwo, IN OUT LPWAVEHDR pwh, IN UINT cbwh)
{
    return winmm.waveOutWrite(hwo,pwh,cbwh);
}

DLLEXPORT UINT  WINAPI auxGetNumDevs(void)
{
    return winmm.auxGetNumDevs();
}

DLLEXPORT UINT  WINAPI joyGetNumDevs(void)
{
    return winmm.joyGetNumDevs();
}

DLLEXPORT UINT  WINAPI midiInGetNumDevs(void)
{
    return winmm.midiInGetNumDevs();
}

DLLEXPORT UINT  WINAPI midiOutGetNumDevs(void)
{
    return winmm.midiOutGetNumDevs();
}

DLLEXPORT UINT  WINAPI mixerGetNumDevs(void)
{
    return winmm.mixerGetNumDevs();
}

DLLEXPORT UINT  WINAPI mmsystemGetVersion(void)
{
    return winmm.mmsystemGetVersion();
}

DLLEXPORT UINT  WINAPI waveInGetNumDevs(void)
{
    return winmm.waveInGetNumDevs();
}

DLLEXPORT UINT  WINAPI waveOutGetNumDevs(void)
{
    return winmm.waveOutGetNumDevs();
}

DLLEXPORT YIELDPROC  WINAPI mciGetYieldProc( IN MCIDEVICEID mciId, IN LPDWORD pdwYieldData)
{
    return winmm.mciGetYieldProc(mciId,pdwYieldData);
}

BOOL WINAPI DriverCallback(DWORD_PTR dwCallBack, DWORD dwFlags, HDRVR hdrvr, DWORD msg, DWORD_PTR dwUser, DWORD_PTR dwParam1, DWORD_PTR dwParam2 )
{
	return winmm.DriverCallback(dwCallBack, dwFlags, hdrvr, msg, dwUser, dwParam1, dwParam2 );
}
