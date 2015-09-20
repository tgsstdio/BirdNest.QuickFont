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
	sampler2D currentSampler = sentances[materialIndex].Handle.Texture;

	uvec2 handleCheck = uvec2(currentSampler);

	// only perform texture lookup if not null
	if (any(notEqual(uvec2(0,0), handleCheck)))
	{
		vec4 image = texture(currentSampler, uv);
		if (image.a <= 0)
		{
			discard;
		}
		else
		{
			finalColour = image * finalColour;
		}
	}
	fragColor = finalColour;
}