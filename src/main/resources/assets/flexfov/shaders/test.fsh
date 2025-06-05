#version 110

/* This comes interpolated from the vertex shader */
varying vec2 texcoord;

/* The 6 textures to be rendered */
uniform sampler2D texFront;
uniform sampler2D texBack;
uniform sampler2D texLeft;
uniform sampler2D texRight;
uniform sampler2D texTop;
uniform sampler2D texBottom;

uniform int antialiasing;

uniform vec2 pixelOffset[16];

uniform vec4 backgroundColor;

uniform vec2 cursorPos;

uniform bool drawCursor;


float distance = 0.0;
float width = 0.0;
float height = 0.0;


void main(void) {
    vec2 coord = texcoord;
    float heightRatio = 2.714;
    float widthRatio  = 5.841;
    float depthRatio  = 7.298;

        // faceLeft, faceFront, faceRight の y座標範囲
        vec2 yRange = vec2(1.0 - (2.0*heightRatio/(heightRatio+depthRatio)), 1.0);

        // faceBottom の x座標範囲
        vec2 xRangeBottom = vec2(-1.0 + depthRatio*2.0/(depthRatio*2.0+widthRatio), -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));

        // faceBottom の y座標範囲
        vec2 yRangeBottom = vec2(yRange[0] - 2.0*depthRatio/(heightRatio+depthRatio) , yRange[0]);

        // y座標が faceLeft, faceFront faceRight の範囲内
        if (yRange[0] <= coord.y && coord.y <= yRange[1]) {

            vec2 xRange = vec2(-1.0,
            -1.0 + depthRatio*2.0/(depthRatio*2.0+widthRatio));
            // faceLeft
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                // return red;
                gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
                return;
            }
            // faceFront
            xRange = vec2(xRange[1],
            -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                // debug return green
                gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
                return;
            }

            xRange = vec2(xRange[1], 1.0);
            // faceRight
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                // return blue;
                gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
                return;
            }
        // faceBottom
        }else if (yRangeBottom[0] <= coord.y && coord.y <= yRangeBottom[1] && xRangeBottom[0] <= coord.x && coord.x <= xRangeBottom[1]) {
            // return yellow;
            gl_FragColor = vec4(1.0, 1.0, 0.0, 1.0);
            return;
        }else {
            gl_FragColor = backgroundColor;
            return;
        }
}
