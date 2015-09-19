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

flat varying uint materialIndex;
smooth in vec4 blendColor;
varying vec2 uv;

out vec4 fragColor;

void main()
{
	vec4 finalColour = blendColor;
	uvec2 handleCheck = uvec2(sentances[materialIndex].Handle.Texture);

	if (any(notEqual(uvec2(0,0), handleCheck)))
	{
		vec4 image = texture(sentances[materialIndex].Handle.Texture, uv);
		finalColour = image * finalColour;
	}
	fragColor = finalColour;
}