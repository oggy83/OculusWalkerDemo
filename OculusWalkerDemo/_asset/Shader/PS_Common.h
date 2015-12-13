/**
 * @file  PS_Common.fx
 * @brief common
*/

#define MAX_INSTANCE_COUNT 32

cbuffer cbMain : register(b0)
{
	float4 g_instanceCol[MAX_INSTANCE_COUNT];
};

cbuffer cbModel : register(b1)
{
	float2 g_uvScale1;
	float2 g_uvScale2;
};

cbuffer cbWorld : register(b2)
{
	float4 g_ambientCol;
	float4 g_fogCol;
	float4 g_lightCol1;		// light1 color
	float4 g_cameraPos;		// camera position in world coords
	float4 g_light1Dir;		// light1 direction in world coords
};

// diffuse texture1
Texture2D g_Diffuse1Tex : register(t0);
SamplerState g_Diffuse1Sampler : register(s0);


float2 _ScalingUV(float2 uv, float2 scaleUV)
{
	return uv * scaleUV;
}