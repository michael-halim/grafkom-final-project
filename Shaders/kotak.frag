#version 330

out vec4 outputColor;
in vec4 vertexColor; // unused if you don't use the 6 params

// this also works

// uniform vec4 ourColor;  //set in onrenderframe
void main(){
	//outputColor = vertexColor;
	outputColor = vec4(0.47, 0.235, 0, 1.0);
}