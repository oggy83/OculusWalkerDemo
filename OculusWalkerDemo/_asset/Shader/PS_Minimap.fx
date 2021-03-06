/**
 * @file  PS_Minimap.fx
 * @brief pixel shader for test
*/

#include "PS_Common.h"

#define MAX_MAP_LENGTH 576	// 24x24

// material special textures
Texture2D g_MinimapRouteTex : register(t1);
SamplerState g_MinimapRouteTexSampler : register(s1);

cbuffer cbModel_Minimap : register(b3)
{
	int minimap_width;	// map width
	int minimap_height;	// map height
	float2 _dummy;
	int4 minimap_map[MAX_MAP_LENGTH / 4];	// map table (unfortunately, int array is not packed)
};

struct PS_INPUT
{
	float4 Position : SV_POSITION;		// position in screen space
	float4 WorldPosition : POSITION;	// position in world space
	float2 UV1 : TEXCOORD0;				// texture uv
	float3 Normal : NORMAL;				// normal in world space
};

struct PS_OUTPUT
{
	float4 Color : SV_Target;
};

PS_OUTPUT main(PS_INPUT In)
{
	PS_OUTPUT Out;

	float3 Light1Dir = normalize(-g_light1Dir.xyz);
	float3 EyeDir = normalize(g_cameraPos - In.WorldPosition);

	// Calc Normal
	float3 Normal = normalize(In.Normal);

	// Calc Diffuse Term
	float4 diffLight = g_ambientCol + max(0, dot(Normal, Light1Dir)) * g_lightCol1;
	float4 diffCol = diffLight * g_Diffuse1Tex.Sample(g_Diffuse1Sampler, _ScalingUV(In.UV1, g_uvScale1));

	// Blinn-Phong Model
	float3 halfVec = normalize(EyeDir + Light1Dir);
	float3 specLight = pow(saturate(dot(Normal, halfVec)), 30);
	float3 specCol = specLight * 0.1f;// highlight is white 

	Out.Color.rgb = diffCol.rgb + specCol;
	Out.Color.a = diffCol.a;

	// these params defines a minimap rectangle over map model
	float2 min = float2(0.15, 0.25);
	float2 max = float2(0.85, 0.95);
	float2 width = max - min;
	
	if (min.x < In.UV1.x && In.UV1.x < max.x && min.y < In.UV1.y && In.UV1.y < max.y)
	{
		float fx = ((In.UV1.x - min.x) / width.x * minimap_width);
		float fy = ((In.UV1.y - min.y) / width.y * minimap_height);
		int x = (int)fx;
		int y = (int)fy;
		float2 uv = float2(fx - x, fy - y);
		int index = y * minimap_width + x;
		int mapId = ((int[4])(minimap_map[index / 4]))[index % 4];
		if (mapId == 1)
		{
			Out.Color = g_MinimapRouteTex.Sample(g_MinimapRouteTexSampler, uv);
			Out.Color.b = 1.0f;
		}
		else if (mapId == 2)
		{
			Out.Color = g_MinimapRouteTex.Sample(g_MinimapRouteTexSampler, uv);
			Out.Color.b = 0.8f;
		}
		else if (mapId == 3)
		{
			Out.Color = float4(1, 1, 1, 1);
		}
		
	}

	return Out;
}

