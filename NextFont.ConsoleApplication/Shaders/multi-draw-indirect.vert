#version 420

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec3 in_colour;
layout(location = 2) in uint in_drawId;

varying vec3 blendColour;
flat varying uint materialIndex;

void main()
{
	blendColour = in_colour;
	materialIndex = in_drawId;
	gl_Position = vec4(in_position, 0, 1);
}