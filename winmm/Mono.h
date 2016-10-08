#pragma once
#define WIN32_LEAN_AND_MEAN
#include <windows.h>

typedef int            gboolean;
typedef int            gint;
typedef unsigned int   guint;
typedef short          gshort;
typedef unsigned short gushort;
typedef long           glong;
typedef unsigned long  gulong;
typedef void *         gpointer;
typedef const void *   gconstpointer;
typedef char           gchar;
typedef unsigned char  guchar;
typedef __int8				gint8;
typedef unsigned __int8		guint8;
typedef __int16				gint16;
typedef unsigned __int16	guint16;
typedef __int32				gint32;
typedef unsigned __int32	guint32;
typedef __int64				gint64;
typedef unsigned __int64	guint64;
typedef float				gfloat;
typedef double				gdouble;
typedef unsigned __int16	gunichar2;

typedef struct _MonoClass MonoClass;
typedef struct _MonoDomain MonoDomain;
typedef struct _MonoMethod MonoMethod;
typedef struct _MonoThread MonoThread;
typedef struct _MonoObject MonoObject;
typedef struct _MonoImage MonoImage;
typedef struct _MonoClassField MonoClassField;
typedef struct _MonoTableInfo MonoTableInfo;
typedef struct _MonoType MonoType;
typedef struct _MonoAssembly MonoAssembly;
typedef struct _MonoVTable MonoVTable;
typedef struct _MonoJitInfo MonoJitInfo;
typedef struct _MonoMethodSignature MonoMethodSignature;
typedef struct _MonoMethodDesc MonoMethodDesc;
typedef struct _MonoString MonoString;
typedef struct _MonoArray MonoArray;
typedef struct _MonoAssemblyName MonoAssemblyName;

typedef void(*MonoDomainFunc) (MonoDomain *domain, gpointer user_data);
typedef void(*GFunc)          (gpointer data, gpointer user_data);
typedef gint(*GCompareFunc)   (gconstpointer a, gconstpointer b);
typedef gint(*GCompareDataFunc) (gconstpointer a, gconstpointer b, gpointer user_data);
typedef void(*GHFunc)         (gpointer key, gpointer value, gpointer user_data);
typedef gboolean(*GHRFunc)        (gpointer key, gpointer value, gpointer user_data);
typedef void(*GDestroyNotify) (gpointer data);
typedef guint(*GHashFunc)      (gconstpointer key);
typedef gboolean(*GEqualFunc)     (gconstpointer a, gconstpointer b);
typedef void(*GFreeFunc)      (gpointer       data);

typedef enum {
	MONO_IMAGE_OK,
	MONO_IMAGE_ERROR_ERRNO,
	MONO_IMAGE_MISSING_ASSEMBLYREF,
	MONO_IMAGE_IMAGE_INVALID
} MonoImageOpenStatus;

#ifdef CDECL
#undef CDECL
#endif
#define CDECL __cdecl

typedef struct mono_struct {
	HMODULE hModule;
	bool init();
	
	void (*g_free)(void *ptr);

	MonoDomain* (CDECL *mono_get_root_domain)(void);
	MonoThread* (CDECL *mono_thread_attach)(MonoDomain* domain);
	void  (CDECL *mono_thread_detach)(MonoThread* monothread);
	MonoClass* (CDECL *mono_object_get_class)(MonoObject* object);

	void (CDECL *mono_domain_foreach)(MonoDomainFunc func, gpointer user_data);

	int (CDECL *mono_domain_set)(MonoDomain* domain, BOOL force);
	int (CDECL *mono_assembly_foreach)(GFunc func, void *user_data);
	MonoImage* (CDECL *mono_assembly_get_image)(MonoAssembly *assembly);
	MonoAssembly* (CDECL *mono_assembly_open)(const char *fname, MonoImageOpenStatus *status);
	MonoAssembly* (CDECL *mono_image_get_assembly)(MonoImage* image);
	char* (CDECL *mono_image_get_name)(MonoImage* image);
	MonoImage* (CDECL *mono_image_open)(const char *fname, MonoImageOpenStatus *status);
	void(CDECL *mono_image_close)(MonoImage *image);

	const MonoTableInfo * (CDECL *mono_image_get_table_info)(MonoImage* image, int table_id);
	int(CDECL *mono_table_info_get_rows)(MonoTableInfo *tableinfo);
	int(CDECL *mono_metadata_decode_row_col)(MonoTableInfo *tableinfo, int idx, unsigned int col);
	char* (CDECL *mono_metadata_string_heap)(MonoImage* image, guint32 index);

	MonoClass* (CDECL *mono_class_from_name_case)(MonoImage* image, const char *name_space, const char *name);
	char* (CDECL *mono_class_get_name)(MonoClass* klass);
	char* (CDECL *mono_class_get_namespace)(MonoClass* klass);
	MonoClass* (CDECL *mono_class_get)(MonoImage* image, guint32 tokenindex);
	MonoMethod* (CDECL *mono_class_get_methods)(MonoClass* klass, gpointer *iter);
	MonoMethod* (CDECL *mono_class_get_method_from_name)(MonoClass* klass, const char *methodname, int paramcount);
	MonoClassField* (CDECL *mono_class_get_fields)(MonoClass* klass, gpointer *iter);
	MonoClass* (CDECL *mono_class_get_parent)(MonoClass* klass);
	MonoVTable* (CDECL *mono_class_vtable)(MonoDomain* domain, MonoClass* klass);
	MonoClass* (CDECL *mono_class_from_mono_type)(MonoType *type);
	MonoClass* (CDECL *mono_class_get_element_class)(MonoClass* klass);
	
	int (CDECL *mono_class_num_fields)(MonoClass* klass);
	int (CDECL *mono_class_num_methods)(MonoClass* klass);

	char* (CDECL *mono_field_get_name)(MonoClassField* field);
	MonoType* (CDECL *mono_field_get_type)(MonoClassField* field);
	MonoClass* (CDECL *mono_field_get_parent)(MonoClassField* field);
	int   (CDECL *mono_field_get_offset)(MonoClassField* field);

	char* (CDECL *mono_type_get_name)(MonoType* type);
	int   (CDECL *mono_type_get_type)(MonoType* type);
	char* (CDECL *mono_type_get_name_full)(MonoType* type, int format);
	int   (CDECL *mono_field_get_flags)(MonoType* type);
	
	char* (CDECL *mono_method_get_name)(MonoMethod *method);
	void* (CDECL *mono_compile_method)(MonoMethod *method);
	void  (CDECL *mono_free_method)(MonoMethod *method);

	void* (CDECL *mono_jit_info_table_find)(MonoDomain* domain, void *addr);

	MonoMethod* (CDECL *mono_jit_info_get_method)(MonoJitInfo *jitinfo);
	gpointer (CDECL *mono_jit_info_get_code_start)(MonoJitInfo *jitinfo);
	int   (CDECL *mono_jit_info_get_code_size)(MonoJitInfo *jitinfo);

	void* (CDECL *mono_method_get_header)(MonoMethod *method);
	MonoClass* (CDECL *mono_method_get_class)(MonoMethod *method);
	MonoMethodSignature* (CDECL *mono_method_signature)(MonoMethod *method);
	void (CDECL *mono_method_get_param_names)(MonoMethod *method, const char **names);
	
	void* (CDECL *mono_method_header_get_code)(MonoMethod *methodheader, guint32 *code_size, guint32 *max_stack);
	char* (CDECL *mono_disasm_code)(void *dishelper, MonoMethod *method, const guchar *ip, const guchar *end);

	char* (CDECL *mono_signature_get_desc)(MonoMethodSignature* signature, int include_namespace);
	int(CDECL *mono_signature_get_param_count)(MonoMethodSignature* signature);
	void* (CDECL *mono_signature_get_return_type)(MonoMethodSignature* signature);


	void* (CDECL *mono_image_rva_map)(MonoImage* image, guint32 addr);
	void* (CDECL *mono_vtable_get_static_field_data)(MonoVTable *vtable);

	MonoMethodDesc* (CDECL *mono_method_desc_new)(const char *name, int include_namespace);
	MonoMethodDesc* (CDECL *mono_method_desc_from_method)(MonoMethod *method);
	void(CDECL *mono_method_desc_free)(MonoMethodDesc *desc);

	MonoAssembly* (CDECL *mono_assembly_name_new)(const char *name);
	MonoAssembly* (CDECL *mono_assembly_load_from)(MonoImage *image, const char *fname, MonoImageOpenStatus *status);
	MonoAssembly* (CDECL *mono_assembly_loaded)(const char *name);
	MonoImage* (CDECL *mono_image_loaded)(const char *name);

	MonoString* (CDECL *mono_string_new)(MonoDomain* domain, const char *text);
	char* (CDECL *mono_string_to_utf8)(MonoString*);
	MonoArray* (CDECL *mono_array_new)(MonoDomain* domain, MonoClass *eclass, uintptr_t n);
	MonoString* (CDECL *mono_object_to_string)(MonoObject* object, void **exc);
	void (CDECL *mono_free)(void*);

	MonoMethod* (CDECL *mono_method_desc_search_in_image)(MonoMethodDesc *desc, MonoImage* image);
	MonoObject* (CDECL *mono_runtime_invoke)(MonoMethod *method, void *obj, void **params, void **exc);
	MonoObject* (CDECL *mono_runtime_invoke_array)(MonoMethod *method, MonoObject *obj, void *params, void **exc);
	MonoObject * (CDECL *mono_value_box)(MonoDomain* domain, MonoClass* klass, gpointer val);
	gpointer (CDECL *mono_object_unbox)(MonoObject *obj);
	MonoType* (CDECL *mono_class_get_type)(MonoClass* klass);

} MONO_STRUCT;

extern MONO_STRUCT mono;