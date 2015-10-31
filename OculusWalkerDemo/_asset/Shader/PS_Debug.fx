/**
 * @file  PS_Debug.fx
 * @brief vertex shader for debug (using vertex color only)
*/

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

