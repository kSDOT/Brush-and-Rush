#define __NV_CUBIN_HANDLE_STORAGE__ static
#if !defined(__CUDA_INCLUDE_COMPILER_INTERNAL_HEADERS__)
#define __CUDA_INCLUDE_COMPILER_INTERNAL_HEADERS__
#endif
#include "crt/host_runtime.h"
#include "convolution.fatbin.c"
extern void __device_stub__Z11convolutionP5ColoriiPiiii(struct Color *, int, int, int *, int, int, int);
extern void __device_stub__Z4diffP5ColorS0_iifPf(struct Color *, struct Color *, int, int, float, float *);
extern void __device_stub__Z7overlayP5Coloriif(struct Color *, int, int, float);
static void __nv_cudaEntityRegisterCallback(void **);
static void __sti____cudaRegisterAll(void);
#pragma section(".CRT$XCT",read)
__declspec(allocate(".CRT$XCT"))static void (*__dummy_static_init__sti____cudaRegisterAll[])(void) = {__sti____cudaRegisterAll};
void __device_stub__Z11convolutionP5ColoriiPiiii(
struct Color *__par0, 
int __par1, 
int __par2, 
int *__par3, 
int __par4, 
int __par5, 
int __par6)
{
__cudaLaunchPrologue(7);
__cudaSetupArgSimple(__par0, 0Ui64);
__cudaSetupArgSimple(__par1, 8Ui64);
__cudaSetupArgSimple(__par2, 12Ui64);
__cudaSetupArgSimple(__par3, 16Ui64);
__cudaSetupArgSimple(__par4, 24Ui64);
__cudaSetupArgSimple(__par5, 28Ui64);
__cudaSetupArgSimple(__par6, 32Ui64);
__cudaLaunch(((char *)((void ( *)(struct Color *, int, int, int *, int, int, int))convolution)));
}
void convolution( struct Color *__cuda_0,int __cuda_1,int __cuda_2,int *__cuda_3,int __cuda_4,int __cuda_5,int __cuda_6)
{__device_stub__Z11convolutionP5ColoriiPiiii( __cuda_0,__cuda_1,__cuda_2,__cuda_3,__cuda_4,__cuda_5,__cuda_6);
}
#line 1 "convolution.cudafe1.stub.c"
void __device_stub__Z4diffP5ColorS0_iifPf(
struct Color *__par0, 
struct Color *__par1, 
int __par2, 
int __par3, 
float __par4, 
float *__par5)
{
__cudaLaunchPrologue(6);
__cudaSetupArgSimple(__par0, 0Ui64);
__cudaSetupArgSimple(__par1, 8Ui64);
__cudaSetupArgSimple(__par2, 16Ui64);
__cudaSetupArgSimple(__par3, 20Ui64);
__cudaSetupArgSimple(__par4, 24Ui64);
__cudaSetupArgSimple(__par5, 32Ui64);
__cudaLaunch(((char *)((void ( *)(struct Color *, struct Color *, int, int, float, float *))diff)));
}
void diff( struct Color *__cuda_0,struct Color *__cuda_1,int __cuda_2,int __cuda_3,float __cuda_4,float *__cuda_5)
{__device_stub__Z4diffP5ColorS0_iifPf( __cuda_0,__cuda_1,__cuda_2,__cuda_3,__cuda_4,__cuda_5);
}
#line 1 "convolution.cudafe1.stub.c"
void __device_stub__Z7overlayP5Coloriif(
struct Color *__par0, 
int __par1, 
int __par2, 
float __par3)
{
__cudaLaunchPrologue(4);
__cudaSetupArgSimple(__par0, 0Ui64);
__cudaSetupArgSimple(__par1, 8Ui64);
__cudaSetupArgSimple(__par2, 12Ui64);
__cudaSetupArgSimple(__par3, 16Ui64);
__cudaLaunch(((char *)((void ( *)(struct Color *, int, int, float))overlay)));
}
void overlay( struct Color *__cuda_0,int __cuda_1,int __cuda_2,float __cuda_3)
{__device_stub__Z7overlayP5Coloriif( __cuda_0,__cuda_1,__cuda_2,__cuda_3);
}
#line 1 "convolution.cudafe1.stub.c"
static void __nv_cudaEntityRegisterCallback(
void **__T3)
{
__nv_dummy_param_ref(__T3);
__nv_save_fatbinhandle_for_managed_rt(__T3);
__cudaRegisterEntry(__T3, ((void ( *)(struct Color *, int, int, float))overlay), overlay, (-1));
__cudaRegisterEntry(__T3, ((void ( *)(struct Color *, struct Color *, int, int, float, float *))diff), diff, (-1));
__cudaRegisterEntry(__T3, ((void ( *)(struct Color *, int, int, int *, int, int, int))convolution), convolution, (-1));
}
static void __sti____cudaRegisterAll(void)
{
__cudaRegisterBinary(__nv_cudaEntityRegisterCallback);
}
