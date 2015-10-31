/**
 * @file  VS_Debug.fx
 * @brief vertex shader for debug (using vertex color only)
*/

#include "VS_Common.h"


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

