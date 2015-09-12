#version 400
#extension GL_ARB_bindless_texture : require

struct BindlessTextureHandle
{
	sampler2D Handle;
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

flat varying uint materialIndex;
smooth in vec4 blendColor;
varying vec2 uv;

out vec4 fragColor;

void main()
{
	//vec4 image = texture(sentances[materialIndex].Handle, uv);
	fragColor = blendColor;
}