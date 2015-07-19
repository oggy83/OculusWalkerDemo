/**
 * @file  VS_Debug.fx
 * @brief vertex shader for debug (using vertex color only)
*/

#define MAX_INSTANCE_COUNT 32

cbuffer cbMain : register(b0)
{
	float4x4 g_worldMat[MAX_INSTANCE_COUNT];	// word matrix (row major)
};

cbuffer cbWorld : register(b2)
{
	float4x4 g_vpMat;		// view projection matrix (row major)
};

struct VS_INPUT
{
	float4 Position : POSITION;
	float4 Color    : COLOR;
	uint InstanceId : SV_InstanceID;   
};

struct VS_OUTPUT
{
	float4 Position : SV_POSITION;
	float4 Color    : COLOR;
};

VS_OUTPUT main(VS_INPUT In)
{
	VS_OUTPUT Out;
	float4x4 worldMat = g_worldMat[In.InstanceId];
	float4x4 wvpMat = mul(worldMat, g_vpMat);

	Out.Position = mul(In.Position, wvpMat);
	Out.Color = In.Color;
	return Out;
}

