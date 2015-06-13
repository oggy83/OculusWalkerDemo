/**
 * @file  PS_Debug.fx
 * @brief vertex shader for debug (using vertex color only)
*/

#define MAX_INSTANCE_COUNT 32

cbuffer cbMain : register(b0)
{
	float4 g_instanceCol[MAX_INSTANCE_COUNT];	
};

struct PS_INPUT
{
	float4 Position : SV_POSITION;
	float4 Color    : COLOR;
};

struct PS_OUTPUT
{
	float4 Color : SV_Target;
};


PS_OUTPUT main(PS_INPUT In)
{
	PS_OUTPUT Out;
	Out.Color = In.Color;
	return Out;
}

