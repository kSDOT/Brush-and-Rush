extern "C" {
	#include <stdio.h>
	class Color {
	public:
		float r;
		float g;
		float b;
		float a;
	};
	// __device__ void multiply_int_color(int int_val, Color* color_val){
	// 	color_val->r = color_val->r * (float)int_val;
	// 	color_val->g = color_val->g * (float)int_val;
	// 	color_val->b = color_val->b * (float)int_val;
	// }
	__device__ void add_multiply_color_intcolor(Color* result, int int_val, Color* color_val){
		result->r+= color_val->r * ((float)int_val);
		result->g+= color_val->g * ((float)int_val);
		result->b+= color_val->b * ((float)int_val);
	}

	__device__ void div_color_int(Color* result, int int_val){
		result->r/=(float)int_val;
		result->g/=(float)int_val;
		result->b/=(float)int_val;
	}
	// Foreach pixel, perform convolution with filter, output is put back in input array
	__global__ void convolution(Color* pic1Pixels, 
								int columns, int rows,
								int* filter, int fColumns, int fRows,
								int lengthWithWeights) {
		int index = blockDim.x * blockIdx.x + threadIdx.x ; 
		 if(index < columns * rows){	
			Color accum = {0.0f, 0.0f, 0.0f, 0.0f};
			for(int fR = 0; fR < fRows; ++fR){
				for(int fC = 0; fC < fColumns; ++fC){
					// Convolution
					add_multiply_color_intcolor(&accum, filter[fR * fColumns + fC],
											    &pic1Pixels[index + fR*columns + fC ] );
				}
			}
			// Normalize differences
 			div_color_int(&accum, lengthWithWeights);
			pic1Pixels[index] = accum;
		}
	}

	// __global__ void buffer(Color* imgPixels, Color* bufImgPixels, 
	// 							int  columns, int rows, int pixel, Color color,
	// 							int imgWidth, int imgHeight){

	// 	int index = blockDim.x * blockIdx.x + threadIdx.x;
	// 	if(index < columns * rows){
	// 			int temp = threadIdx.x - blockDim.x * blockIdx.x;

	// 			int block_i = sqrt((float)blockDim.x);
	// 			int blockRow = (int)(threadIdx.x % block_i);
	// 			int blockColumn = (int)(threadIdx.x / block_i);

	// 			int grid_length = sqrt((float)gridDim.x);
	// 			int gridRow = (int)(blockIdx.x % grid_length);
	// 			int gridColumn = (int)(blockIdx.x / (float)grid_length);

	// 		    if ((gridRow == 0 ) &&true)//(blockRow == 0 || blockColumn ==  block_i-1))
    //             {
    //                 bufImgPixels[index] =  Color{0,0,1,1};
    //             }
    //             else if (blockRow >= imgHeight + pixel || blockColumn >= imgWidth + pixel)
    //             {
    //                 bufImgPixels[index] = Color{0, 1,0,1};
    //             }
    //             else {
    //                 //bufImgPixels[index] = imgPixels[index-pixel];
    //                 bufImgPixels[index] = Color{1, 0,0,1};

    //             }
	// 	}
	// }
	
	__device__ void color_abs(Color* color_val){
		color_val->r = abs(color_val->r);
		color_val->g = abs(color_val->g);
		color_val->b = abs(color_val->b);
		color_val->a = abs(color_val->a);
	}

	__device__ void getDistance(Color* color_val, float* output){
		*output = sqrt(color_val->r*color_val->r + color_val->g*color_val->g + color_val->b*color_val->b);
	}


	__device__ float lerp(const float a, const float b,  const float w, float* output)
	{
    	*output =  a + w*(b-a);
	}
	__device__ float lerp_vec(const Color* a,const Color* b,const float w, Color* output)
	{
		lerp(a->r, b->r, w, &output->r);
		lerp(a->g, b->g, w, &output->g);
		lerp(a->b, b->b, w, &output->b);
	}

	// foreach pixl, calculate pixelwise difference between the two arrays and output the modified difference in the first array
	__global__ void diff(Color* pic1Pixels, Color* pic2Pixels, 
								int  columns, int rows, float Threshold, float* diffAccumulator){
		int index = blockDim.x * blockIdx.x + threadIdx.x;

		if(index < columns * rows){
				//if pixel is in padded area, ignore it
				if(pic1Pixels[index].a == 1.0f){
                	pic1Pixels[index]=  Color{0.0f, 0.0f, 0.0f, 0.0f};
				}
				else{  
					// Absolute Component-wise difference
					Color diff = Color{0.0f, 0.0f, 0.0f, 1.0f};
					diff.r =  pic1Pixels[index].r - pic2Pixels[index].r;
					diff.g =  pic1Pixels[index].g - pic2Pixels[index].g;
					diff.b =  pic1Pixels[index].b - pic2Pixels[index].b;
					diff.a =  pic1Pixels[index].a - pic2Pixels[index].a;
					color_abs(&diff);
					// Distance of color from 0
                	float diffNumber=0;
					getDistance(&diff, &diffNumber);
					// If difference between threshold, ignore it
                	if (diffNumber < Threshold)
                	{
                	    diff = Color{0.0f, 0.0f, 0.0f, 0.0f};
						diffNumber = 0;
                	}
                	else {
                	    Color temp = diff;

						// From threshold to maxdistance, how far is our color distance?
                	    float t1 =0;

						lerp((float)Threshold, 1.732f, (float)diffNumber, &t1);
						Color diff = Color{0.0, 0.0, 0.0, 1.0};
						Color black = Color{0.0, 0.0, 0.0, 0.0};

						// Based on how far our distance is, interpolate our color from minimum to maximum
                	    lerp_vec(&black, &temp, t1, &diff);
                	}
                	pic1Pixels[index] = diff;
					// Add modified difference
					getDistance(&diff, &diffNumber);
                	*diffAccumulator += diffNumber;
				}
		}

	}

	//foreach pixel
	__global__ void overlay(Color* colors, int  columns, int rows, float Threshold){
		int index = blockDim.x * blockIdx.x + threadIdx.x;

		if(index < columns * rows){
			
                float remappedDistance = 0.0f;
				getDistance(&colors[index], &remappedDistance);
				// remap from [0.7, 1] to [0, 1]
                remappedDistance = (remappedDistance - Threshold) / (1.0f - Threshold);
                colors[index] = Color{1.0f, 0.0f, 0.0f, remappedDistance};
		}

	}

	
	int main(){}
}