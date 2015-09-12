#version 400
#extension GL_ARB_bindless_texture : require

struct BindlessTextureHandle
{
	sampler2D Texture;
	float Slice;
	uint Index;
};

struct SentanceBlock 
{
	BindlessTextureHandle Handle;
	vec4 Color;
	mat4 Transform;
};

layout(binding = 0, std430) buffer CB0
{
	SentanceBlock sentances[];
};

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoords;
layout(location = 2) in uint in_drawId;

flat varying uint materialIndex;
smooth out vec4 blendColor;
varying vec2 uv;

void main(void)
{
	uv = in_texCoords;
	materialIndex = in_drawId;
	blendColor = sentances[in_drawId].Color;
	//sentances[materialIndex].Transform * 
	gl_Position = vec4(in_position, 0, 1);
}


