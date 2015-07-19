/**
 * @file  VS_Std.fx
 * @brief vertex shader for test
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
	//bool g_isEnableSkinning;
};

cbuffer cbWorld : register(b2)
{
	float4x4 g_vpMat;		// view projection matrix (row major)
};


struct VS_INPUT
{
	float4 Position : POSITION;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
	int4 BoneIndex : BONEINDEX;
	float4 BoneWeight : BONEWEIGHT;
	uint InstanceId : SV_InstanceID;   
};

struct VS_OUTPUT
{
	float4 Position : SV_POSITION;
	float4 WorldPosition : POSITION;
	float2 UV1 : TEXCOORD0;
	float3 Normal : NORMAL;
	uint InstanceId : ID;
};

VS_OUTPUT main(VS_INPUT In)
{
	VS_OUTPUT Out;

	float4x4 worldMat = g_worldMat[In.InstanceId];
	float4x4 wvpMat = mul(worldMat, g_vpMat);

	if (true/*g_isEnableSkinning*/)
	{
		float4x4 skinningMat
			= g_boneMatrices[In.BoneIndex.x] * In.BoneWeight.x
			+ g_boneMatrices[In.BoneIndex.y] * In.BoneWeight.y
			+ g_boneMatrices[In.BoneIndex.z] * In.BoneWeight.z
			+ g_boneMatrices[In.BoneIndex.w] * In.BoneWeight.w;
		float4 skinedPosition = mul(In.Position, skinningMat);

		Out.Position = mul(skinedPosition, wvpMat);
		Out.WorldPosition = mul(skinedPosition, worldMat);
		Out.Normal = mul(mul(In.Normal, skinningMat), worldMat);
	}
	else
	{
		Out.Position = mul(In.Position, wvpMat);
		Out.WorldPosition = mul(In.Position, worldMat);
		Out.Normal = mul(In.Normal, worldMat);
	}
	

	Out.UV1 = In.UV1;
	Out.InstanceId = In.InstanceId;
	
	return Out;
}

