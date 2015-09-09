/**
 * @file  PS_Test.fx
 * @brief pixel shader for test
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


struct PS_INPUT
{
	float4 Position : SV_POSITION;		// position in screen space
	float4 WorldPosition : POSITION;	// position in world space
	float2 UV1 : TEXCOORD0;				// texture uv
	float3 Normal : NORMAL;				// normal in world space
	uint InstanceId : ID;	  
};

struct PS_OUTPUT
{
	float4 Color : SV_Target;
};

// diffuse texture1
Texture2D g_Diffuse1Tex : register(t0);
SamplerState g_Diffuse1Sampler : register(s0);

float2 _ScalingUV(float2 uv, float2 scaleUV)
{
	return uv * scaleUV;
}

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

	// Fog
	float startFogDistance = 10.0f;
	float endFogDistance = 50;
	float3 fogColor = g_fogCol.rgb;
	float fogFactor = clamp((In.Position.w - startFogDistance) / (endFogDistance - startFogDistance), 0, 1);
	Out.Color.rgb = lerp(Out.Color.rgb, fogColor, fogFactor);

	return Out;
}

