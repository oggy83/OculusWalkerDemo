/**
 * @file  PS_Minimap.fx
 * @brief pixel shader for test
*/

#include "PS_Common.h"

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
	float3 specLight = pow(saturate(dot(Normal, halfVec)), 100);
	float3 specCol = specLight * 0.3f;// highlight is white 

	Out.Color.rgb = diffCol.rgb + specCol;
	Out.Color.a = diffCol.a;

	float2 min = float2(0.2, 0.3);
	float2 max = float2(0.8, 0.9);
	float width = max - min;
	
	if (min.x < In.UV1.x && In.UV1.x < max.x && min.y < In.UV1.y && In.UV1.y < max.y)
	{
		Out.Color.b *= 2;
	}

	return Out;
}

