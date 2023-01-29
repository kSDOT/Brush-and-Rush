extern "C" {
	#include <stdio.h>
	class Vector3 {
	public:
		float x;
        float y;
        float z;
	};


	// foreach pixl, calculate pixelwise difference between the two arrays and output the modified difference in the first array
	__global__ void carve(int width, int height, int depth, int x, int y, int z, float* voxels){
        int new_x = x+threadIdx.x - 1;
        int new_y = y+threadIdx.y - 1;
        int new_z = z+threadIdx.z - 1;

		if( new_x < width  && new_x>0 &&  new_y < height  && new_y>0 &&  new_z < depth  && new_z>0){
            voxels[new_z * width * height + new_y * width + new_x] = -1;
		}

	}

	int main(){}
}