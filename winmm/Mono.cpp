#include "Mono.h"
#include <tchar.h>
#include <Shlwapi.h>
#include <list>
#pragma comment(lib, "shlwapi.lib")

MONO_STRUCT mono;

bool mono_struct::init()
{
	if (hModule != nullptr)
		return true;

	hModule = GetModuleHandle("mono.dll");
	if (hModule == nullptr)
		return false;

	*(FARPROC*)&mono_get_root_domain = GetProcAddress(hModule, "mono_get_root_domain");
	*(FARPROC*)&mono_thread_attach = GetProcAddress(hModule, "mono_thread_attach");
	*(FARPROC*)&mono_thread_detach = GetProcAddress(hModule, "mono_thread_detach");

	*(FARPROC*)&mono_object_get_class = GetProcAddress(hModule, "mono_object_get_class");

	*(FARPROC*)&mono_domain_foreach = GetProcAddress(hModule, "mono_domain_foreach");
	*(FARPROC*)&mono_domain_set = GetProcAddress(hModule, "mono_domain_set");
	*(FARPROC*)&mono_assembly_foreach = GetProcAddress(hModule, "mono_assembly_foreach");
	*(FARPROC*)&mono_assembly_get_image = GetProcAddress(hModule, "mono_assembly_get_image");
	*(FARPROC*)&mono_image_get_assembly = GetProcAddress(hModule, "mono_image_get_assembly");

	*(FARPROC*)&mono_image_get_name = GetProcAddress(hModule, "mono_image_get_name");
	*(FARPROC*)&mono_image_get_table_info = GetProcAddress(hModule, "mono_image_get_table_info");
	*(FARPROC*)&mono_image_rva_map = GetProcAddress(hModule, "mono_image_rva_map");

	*(FARPROC*)&mono_table_info_get_rows = GetProcAddress(hModule, "mono_table_info_get_rows");
	*(FARPROC*)&mono_metadata_decode_row_col = GetProcAddress(hModule, "mono_metadata_decode_row_col");
	*(FARPROC*)&mono_metadata_string_heap = GetProcAddress(hModule, "mono_metadata_string_heap");


	*(FARPROC*)&mono_class_get = GetProcAddress(hModule, "mono_class_get");
	*(FARPROC*)&mono_class_from_name_case = GetProcAddress(hModule, "mono_class_from_name_case");
	*(FARPROC*)&mono_class_get_name = GetProcAddress(hModule, "mono_class_get_name");
	*(FARPROC*)&mono_class_get_namespace = GetProcAddress(hModule, "mono_class_get_namespace");
	*(FARPROC*)&mono_class_get_methods = GetProcAddress(hModule, "mono_class_get_methods");
	*(FARPROC*)&mono_class_get_method_from_name = GetProcAddress(hModule, "mono_class_get_method_from_name");
	*(FARPROC*)&mono_class_get_fields = GetProcAddress(hModule, "mono_class_get_fields");
	*(FARPROC*)&mono_class_get_parent = GetProcAddress(hModule, "mono_class_get_parent");
	*(FARPROC*)&mono_class_vtable = GetProcAddress(hModule, "mono_class_vtable");
	*(FARPROC*)&mono_class_from_mono_type = GetProcAddress(hModule, "mono_class_from_mono_type");
	*(FARPROC*)&mono_class_get_element_class = GetProcAddress(hModule, "mono_class_get_element_class");

	*(FARPROC*)&mono_class_num_fields = GetProcAddress(hModule, "mono_class_num_fields");
	*(FARPROC*)&mono_class_num_methods = GetProcAddress(hModule, "mono_class_num_methods");


	*(FARPROC*)&mono_field_get_name = GetProcAddress(hModule, "mono_field_get_name");
	*(FARPROC*)&mono_field_get_type = GetProcAddress(hModule, "mono_field_get_type");
	*(FARPROC*)&mono_field_get_parent = GetProcAddress(hModule, "mono_field_get_parent");
	*(FARPROC*)&mono_field_get_offset = GetProcAddress(hModule, "mono_field_get_offset");
	*(FARPROC*)&mono_field_get_flags = GetProcAddress(hModule, "mono_field_get_flags");

	*(FARPROC*)&mono_type_get_name = GetProcAddress(hModule, "mono_type_get_name");
	*(FARPROC*)&mono_type_get_type = GetProcAddress(hModule, "mono_type_get_type");
	*(FARPROC*)&mono_type_get_name_full = GetProcAddress(hModule, "mono_type_get_name_full");

	*(FARPROC*)&mono_method_get_name = GetProcAddress(hModule, "mono_method_get_name");
	*(FARPROC*)&mono_method_get_class = GetProcAddress(hModule, "mono_method_get_class");
	*(FARPROC*)&mono_method_get_header = GetProcAddress(hModule, "mono_method_get_header");
	*(FARPROC*)&mono_method_signature = GetProcAddress(hModule, "mono_method_signature");
	*(FARPROC*)&mono_method_get_param_names = GetProcAddress(hModule, "mono_method_get_param_names");
	
	*(FARPROC*)&mono_signature_get_desc = GetProcAddress(hModule, "mono_signature_get_desc");
	*(FARPROC*)&mono_signature_get_param_count = GetProcAddress(hModule, "mono_signature_get_param_count");
	*(FARPROC*)&mono_signature_get_return_type = GetProcAddress(hModule, "mono_signature_get_return_type");
	
	*(FARPROC*)&mono_compile_method = GetProcAddress(hModule, "mono_compile_method");
	*(FARPROC*)&mono_free_method = GetProcAddress(hModule, "mono_free_method");
	*(FARPROC*)&mono_jit_info_table_find = GetProcAddress(hModule, "mono_jit_info_table_find");
	*(FARPROC*)&mono_jit_info_get_method = GetProcAddress(hModule, "mono_jit_info_get_method");
	*(FARPROC*)&mono_jit_info_get_code_start = GetProcAddress(hModule, "mono_jit_info_get_code_start");
	*(FARPROC*)&mono_jit_info_get_code_size = GetProcAddress(hModule, "mono_jit_info_get_code_size");

	*(FARPROC*)&mono_method_header_get_code = GetProcAddress(hModule, "mono_method_header_get_code");
	*(FARPROC*)&mono_disasm_code = GetProcAddress(hModule, "mono_disasm_code");

	*(FARPROC*)&mono_vtable_get_static_field_data = GetProcAddress(hModule, "mono_vtable_get_static_field_data");

	*(FARPROC*)&mono_method_desc_new = GetProcAddress(hModule, "mono_method_desc_new");;
	*(FARPROC*)&mono_method_desc_from_method = GetProcAddress(hModule, "mono_method_desc_from_method");;
	*(FARPROC*)&mono_method_desc_free = GetProcAddress(hModule, "mono_method_desc_free");;

	*(FARPROC*)&mono_string_new = GetProcAddress(hModule, "mono_string_new");
	*(FARPROC*)&mono_string_to_utf8 = GetProcAddress(hModule, "mono_string_to_utf8");
	*(FARPROC*)&mono_array_new = GetProcAddress(hModule, "mono_array_new");
	*(FARPROC*)&mono_value_box = GetProcAddress(hModule, "mono_value_box");
	*(FARPROC*)&mono_object_unbox = GetProcAddress(hModule, "mono_object_unbox");
	*(FARPROC*)&mono_class_get_type = GetProcAddress(hModule, "mono_class_get_type");

	*(FARPROC*)&mono_method_desc_search_in_image = GetProcAddress(hModule, "mono_method_desc_search_in_image");
	*(FARPROC*)&mono_runtime_invoke = GetProcAddress(hModule, "mono_runtime_invoke");

	*(FARPROC*)&mono_assembly_name_new = GetProcAddress(hModule, "mono_assembly_name_new");
	*(FARPROC*)&mono_assembly_load_from = GetProcAddress(hModule, "mono_assembly_load_from");
	*(FARPROC*)&mono_assembly_loaded = GetProcAddress(hModule, "mono_assembly_loaded");
	*(FARPROC*)&mono_assembly_open = GetProcAddress(hModule, "mono_assembly_open");
	*(FARPROC*)&mono_image_open = GetProcAddress(hModule, "mono_image_open");
	*(FARPROC*)&mono_image_close = GetProcAddress(hModule, "mono_image_close");	


	return true;
}

static HANDLE hConsoleThread;

// Trim whitespace before and after a string
static TCHAR *Trim(TCHAR*&p) {
	if (p == nullptr) return nullptr;
	while (_istspace(*p)) *p++ = 0;
	TCHAR *e = p + _tcslen(p) - 1;
	while (e > p && _istspace(*e)) *e-- = 0;
	return p;
}

bool mono_init()
{
	return mono.init();	
}

struct ConsoleStartup
{
	CHAR LoadClasses[4096];
	INT CodePage;
};
static ConsoleStartup k_startup;
static std::list<MonoAssembly*> k_loaded_images;

DWORD WINAPI ConsoleThreadEntry(LPVOID lpThreadParameter)
{
	if (mono.hModule == nullptr)
		return 0;

	SetConsoleTitle("Unity Debug Console");

	if (k_startup.CodePage > 0)
	{
		SetConsoleCP(k_startup.CodePage);
		SetConsoleOutputCP(k_startup.CodePage);
	}

	MonoThread* thread = nullptr;
	try
	{
		MonoDomain* domain = mono.mono_get_root_domain();
		if (domain == nullptr)
			return 0;

		thread = mono.mono_thread_attach(domain);
		if (thread == nullptr)
			return 0;

		char *next_token = nullptr;
		// this heavily manipulates the buffer is is therefore single use
		for (char *klassname = strtok_s(k_startup.LoadClasses, ";", &next_token)
			; klassname != nullptr
			; klassname = strtok_s(nullptr, ";", &next_token)
			)
		{
			Trim(klassname);
			
			CHAR clsname[MAX_PATH];
			CHAR nspace[MAX_PATH];
			CHAR imgname[MAX_PATH];
			clsname[0] = nspace[0] = imgname[0] = 0;
			if ( LPSTR ptr = strchr(klassname, ',') )
			{
				*ptr++ = 0;
				strcpy(imgname, ptr);
			}
			if (LPSTR ptr = strrchr(klassname, '.'))
			{
				*ptr++ = 0;
				strcpy(nspace, klassname);
				strcpy(clsname, ptr);
			}
			else
			{
				strcpy(clsname, klassname);
			}

			for (std::list<MonoAssembly*>::iterator itr = k_loaded_images.begin(); itr != k_loaded_images.end(); ++itr) {
				MonoImage* image = mono.mono_assembly_get_image(*itr);
				if (imgname[0] != 0) {
					if (stricmp(mono.mono_image_get_name(image), imgname) != 0)
						continue;
				}
				if ( MonoClass* klass = mono.mono_class_from_name_case(image, nspace, clsname) )
				{
					MonoMethod *method = mono.mono_class_get_method_from_name(klass, "Main", 0);
					if (method != nullptr)
					{
						void *methodsignature = mono.mono_method_signature(method);
						//int paramcount = mono.mono_signature_get_param_count(methodsignature) : 0;
						if (MonoObject* result = mono.mono_runtime_invoke(method, nullptr, nullptr, nullptr)) {
							//mono.mono_free(result);
						}
						//mono.mono_free_method(method);
					}					
				}
			}
		}
		//for (std::list<MonoImage*>::iterator itr = k_loaded_images.begin(); itr != k_loaded_images.end(); ++itr) {
		//	mono.mono_image_close(*itr);
		//}
	}
	catch(...)
	{
	}

	try
	{
		if (thread == nullptr) {
			mono.mono_thread_detach(thread);
			thread = nullptr;
		}
	}
	catch (...)
	{
		
	}

	
	FreeConsole();

	hConsoleThread = nullptr;

	return 0;
}

bool mono_load_images(LPCSTR loadImages)
{
	MonoThread* thread = nullptr;
	try
	{
		//MonoDomain* domain = mono.mono_get_root_domain();
		//if (domain == nullptr)
		//	return 0;

		//thread = mono.mono_thread_attach(domain);
		//if (thread == nullptr)
		//	return 0;

		CHAR root[MAX_PATH];
		GetModuleFileName(nullptr, root, MAX_PATH);
		PathRemoveFileSpec(root);
		PathAddBackslash(root);

		int loadImagesLen = strlen(loadImages) + 1;
		CHAR* tokenLoadImages = (CHAR*)alloca(loadImagesLen);
		lstrcpyn(tokenLoadImages, loadImages, loadImagesLen);
		char *next_token = nullptr;
		for (char *file = strtok_s(tokenLoadImages, ";", &next_token)
			; file != nullptr
			; file = strtok_s(nullptr, ";", &next_token)
			)
		{
			Trim(file);
			CHAR path[MAX_PATH];
			CHAR search[MAX_PATH];
			CHAR buffer[MAX_PATH];
			PathCombine(search, root, file);
			PathCanonicalize(path, search);
			PathRemoveFileSpec(path);
			PathAddBackslash(path);

			WIN32_FIND_DATA FindFileData;
			ZeroMemory(&FindFileData, sizeof(FindFileData));
			HANDLE hFind = FindFirstFile(file, &FindFileData);
			if (hFind != INVALID_HANDLE_VALUE) {
				do {
					if (FindFileData.cFileName[0] == '.' || (FindFileData.dwFileAttributes & (FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM)))
						continue;
					else if (FindFileData.dwFileAttributes & (FILE_ATTRIBUTE_DIRECTORY | FILE_ATTRIBUTE_REPARSE_POINT)) {
						continue;
					}
					else {
						PathCombine(buffer, path, FindFileData.cFileName);
						GetLongPathName(buffer, buffer, MAX_PATH);
						if (PathFileExists(buffer)) {
							MonoImageOpenStatus status;
							MonoAssembly* ass = mono.mono_assembly_open(buffer, &status);
							if (ass != nullptr) {
								k_loaded_images.push_back(ass);
							}
						}
					}
				} while (FindNextFile(hFind, &FindFileData));

				FindClose(hFind);
			}
		}
	}
	catch (...)
	{
	}
}

bool mono_launch(LPCSTR loadClasses, int codepage)
{
	if (hConsoleThread != nullptr)
		return false;
	k_startup.LoadClasses[0] = 0;
	k_startup.CodePage = codepage;
	if (loadClasses != nullptr)
		lstrcpyn(k_startup.LoadClasses, loadClasses, sizeof(k_startup.LoadClasses));
	hConsoleThread = CreateThread(nullptr, 0, ConsoleThreadEntry, &k_startup, 0, nullptr);
	return true;
}

