/**
 * @file  VS_Common.fx
 * @brief common
*/

#define MAX_INSTANCE_COUNT 32
#define MAX_BONE_MATRICES 32

cbuffer cbMain : register(b0)
{
	float4x4 g_worldMat[MAX_INSTANCE_COUNT];	// word matrix (row major)
};

cbuffer cbBone : register(b1)
{
	matrix g_boneMatrices[MAX_BONE_MATRICES];
};

cbuffer cbModel : register(b2)
{
	bool g_isEnableSkinning;
	float3 _dummy;
};

cbuffer cbWorld : register(b3)
{
	float4x4 g_vpMat;		// view projection matrix (row major)
};
