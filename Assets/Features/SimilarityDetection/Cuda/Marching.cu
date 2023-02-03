extern "C" {
	#include <stdio.h>
    #include <stdlib.h>
	class Vector3 {
	public:
		float x;
        float y;
        float z;
	};

	__global__ void carve(int width, int height, int depth, int x, int y, int z, float* voxels){
        int new_x = x+threadIdx.x - 1;
        int new_y = y+threadIdx.y - 1;
        int new_z = z+threadIdx.z - 1;

		if( new_x < width  && new_x>0 &&  new_y < height  && new_y>0 &&  new_z < depth  && new_z>0){
            voxels[new_x * height * depth + new_y * depth + new_z] = -1;
		}

	}
    __device__ Vector3* normalize_inversed(Vector3* vec){
        float length = sqrt(pow(vec->x, 2) + pow(vec->y, 2) + pow(vec->z, 2));
        vec->x = (vec->x / length) * -1;
        vec->y = (vec->y / length) * -1; 
        vec->z = (vec->z / length) * -1;
        return vec;
    }
    __device__ float Lerp(float v0, float v1, float t)
    {
        return v0 + (v1 - v0) * t;
    }

     __device__ float BLerp(float v00, float v10, float v01, float v11, float tx, float ty)
    {
        return Lerp(Lerp(v00, v10, tx), Lerp(v01, v11, tx), ty);
    }

    __device__ float clamp(float d, float min, float max){
        const float t = d < min ? min : d;
        return t > max ? max : t;
    }

    __device__ float GetVoxel_int(int x, int y, int z, int Width, int Height, int Depth, float* Voxels){
        x = clamp(x, 0, Width - 1);
        y = clamp(y, 0, Height - 1);
        z = clamp(z, 0, Depth - 1);
        return Voxels[x * Height * Depth + y * Depth + z];
    }



    __device__ float GetVoxel(float u, float v, float w, int Width, int Height, int Depth, float* Voxels)
    {
        float x = u * (Width - 1);
        float y = v * (Height - 1);
        float z = w * (Depth - 1);

        int xi = (int)floor(x);
        int yi = (int)floor(y);
        int zi = (int)floor(z);

        float v000 = GetVoxel_int(xi    , yi    , zi    , Width, Height, Depth, Voxels);
        float v100 = GetVoxel_int(xi + 1, yi    , zi    , Width, Height, Depth, Voxels);
        float v010 = GetVoxel_int(xi    , yi + 1, zi    , Width, Height, Depth, Voxels);
        float v110 = GetVoxel_int(xi + 1, yi + 1, zi    , Width, Height, Depth, Voxels);

        float v001 = GetVoxel_int(xi    , yi    , zi + 1        , Width, Height, Depth, Voxels);
        float v101 = GetVoxel_int(xi + 1, yi    , zi + 1    , Width, Height, Depth, Voxels);
        float v011 = GetVoxel_int(xi    , yi + 1, zi + 1    , Width, Height, Depth, Voxels);
        float v111 = GetVoxel_int(xi + 1, yi + 1, zi + 1, Width, Height, Depth, Voxels);

        float tx = clamp(x - xi, 0 ,1);
        float ty = clamp(y - yi, 0 ,1);
        float tz = clamp(z - zi, 0, 1);

        //use bilinear interpolation the find these values.
        float v0 = BLerp(v000, v100, v010, v110, tx, ty);
        float v1 = BLerp(v001, v101, v011, v111, tx, ty);

        return Lerp(v0, v1, tz);
    }

    __global__ void GetNormal(int N, int Width, int Height, int Depth, Vector3* verts, float* Voxels, Vector3* output)
    {
		int index = blockDim.x * blockIdx.x + threadIdx.x; 
        if(index < N){
            float u = verts[index].x / (Width - 1.0f);
            float v = verts[index].y / (Height - 1.0f);
            float w = verts[index].z / (Depth - 1.0f);

            const float h = 0.005f;
            const float hh = h * 0.5f;
            const float ih = 1.0f / h;

            float dx_p1 = GetVoxel(u + hh, v     , w     , Width, Height, Depth, Voxels);
            float dy_p1 = GetVoxel(u     , v + hh, w     , Width, Height, Depth, Voxels);
            float dz_p1 = GetVoxel(u     , v     , w + hh, Width, Height, Depth, Voxels);

            float dx_m1 = GetVoxel(u - hh, v     , w     , Width, Height, Depth, Voxels);
            float dy_m1 = GetVoxel(u     , v - hh, w     , Width, Height, Depth, Voxels);
            float dz_m1 = GetVoxel(u     , v     , w - hh, Width, Height, Depth, Voxels);

            float dx = (dx_p1 - dx_m1) * ih;
            float dy = (dy_p1 - dy_m1) * ih;
            float dz = (dz_p1 - dz_m1) * ih;
            Vector3 out_temp =  Vector3{dx, dy, dz};
            output[index] = *normalize_inversed(&out_temp);
            //output[index] = Vector3{0.57, 0.57, 0.57};
        }
    }


	int main(){}
}