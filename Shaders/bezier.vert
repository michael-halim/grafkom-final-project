#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor; // unused if you don't use the 6 params

out vec4 vertexColor; // unused if you don't use the 6 params

void main(void){

	gl_Position = vec4(aPosition, 1.0f);
	vertexColor = vec4(aColor, 1.0); // unused if you don't use the 6 params
}
