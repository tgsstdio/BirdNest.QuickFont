#version 150

varying vec3 blendColour;
flat varying uint materialIndex;

out vec4 fragColor;

void main()
{
	fragColor = vec4(0, 0, 1, 1.0f);
}