#version 400
#extension GL_ARB_bindless_texture : require

struct TextureHandle
{
	sampler2D Texture;
	uint Index;
	float Slice;
};

struct SentanceBlock 
{
	TextureHandle Handle;
	vec4 Colour;
	mat4 Transform;
};

layout(binding = 0, std430) buffer CB0
{
	SentanceBlock sentances[];
};

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec2 in_texCoords;
layout(location = 2) in uint in_drawId;

flat varying uint materialIndex;
smooth out vec4 blendColor;
varying vec2 uv;

void main(void)
{
	uv = in_texCoords;
	materialIndex = in_drawId;

//	vec4 image = texture(sentances[materialIndex].Handle.Texture, uv);
	blendColor = sentances[in_drawId].Colour;
	gl_Position =  sentances[materialIndex].Transform * vec4(in_position, 1);
}


