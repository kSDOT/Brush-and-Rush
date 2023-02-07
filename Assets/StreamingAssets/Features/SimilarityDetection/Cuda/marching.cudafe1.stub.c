#define __NV_CUBIN_HANDLE_STORAGE__ static
#if !defined(__CUDA_INCLUDE_COMPILER_INTERNAL_HEADERS__)
#define __CUDA_INCLUDE_COMPILER_INTERNAL_HEADERS__
#endif
#include "crt/host_runtime.h"
#include "marching.fatbin.c"
extern void __device_stub__Z5carveiiiiiiPf(int, int, int, int, int, int, float *);
extern void __device_stub__Z9GetNormaliiiiP7Vector3PfS0_(int, int, int, int, struct Vector3 *, float *, struct Vector3 *);
static void __nv_cudaEntityRegisterCallback(void **);
static void __sti____cudaRegisterAll(void);
#pragma section(".CRT$XCT",read)
__declspec(allocate(".CRT$XCT"))static void (*__dummy_static_init__sti____cudaRegisterAll[])(void) = {__sti____cudaRegisterAll};
void __device_stub__Z5carveiiiiiiPf(
int __par0, 
int __par1, 
int __par2, 
int __par3, 
int __par4, 
int __par5, 
float *__par6)
{
__cudaLaunchPrologue(7);
__cudaSetupArgSimple(__par0, 0Ui64);
__cudaSetupArgSimple(__par1, 4Ui64);
__cudaSetupArgSimple(__par2, 8Ui64);
__cudaSetupArgSimple(__par3, 12Ui64);
__cudaSetupArgSimple(__par4, 16Ui64);
__cudaSetupArgSimple(__par5, 20Ui64);
__cudaSetupArgSimple(__par6, 24Ui64);
__cudaLaunch(((char *)((void ( *)(int, int, int, int, int, int, float *))carve)));
}
void carve( int __cuda_0,int __cuda_1,int __cuda_2,int __cuda_3,int __cuda_4,int __cuda_5,float *__cuda_6)
{__device_stub__Z5carveiiiiiiPf( __cuda_0,__cuda_1,__cuda_2,__cuda_3,__cuda_4,__cuda_5,__cuda_6);
}
#line 1 "marching.cudafe1.stub.c"
void __device_stub__Z9GetNormaliiiiP7Vector3PfS0_(
int __par0, 
int __par1, 
int __par2, 
int __par3, 
struct Vector3 *__par4, 
float *__par5, 
struct Vector3 *__par6)
{
__cudaLaunchPrologue(7);
__cudaSetupArgSimple(__par0, 0Ui64);
__cudaSetupArgSimple(__par1, 4Ui64);
__cudaSetupArgSimple(__par2, 8Ui64);
__cudaSetupArgSimple(__par3, 12Ui64);
__cudaSetupArgSimple(__par4, 16Ui64);
__cudaSetupArgSimple(__par5, 24Ui64);
__cudaSetupArgSimple(__par6, 32Ui64);
__cudaLaunch(((char *)((void ( *)(int, int, int, int, struct Vector3 *, float *, struct Vector3 *))GetNormal)));
}
void GetNormal( int __cuda_0,int __cuda_1,int __cuda_2,int __cuda_3,struct Vector3 *__cuda_4,float *__cuda_5,struct Vector3 *__cuda_6)
{__device_stub__Z9GetNormaliiiiP7Vector3PfS0_( __cuda_0,__cuda_1,__cuda_2,__cuda_3,__cuda_4,__cuda_5,__cuda_6);
}
#line 1 "marching.cudafe1.stub.c"
static void __nv_cudaEntityRegisterCallback(
void **__T0)
{
__nv_dummy_param_ref(__T0);
__nv_save_fatbinhandle_for_managed_rt(__T0);
__cudaRegisterEntry(__T0, ((void ( *)(int, int, int, int, struct Vector3 *, float *, struct Vector3 *))GetNormal), GetNormal, (-1));
__cudaRegisterEntry(__T0, ((void ( *)(int, int, int, int, int, int, float *))carve), carve, (-1));
}
static void __sti____cudaRegisterAll(void)
{
__cudaRegisterBinary(__nv_cudaEntityRegisterCallback);
}
