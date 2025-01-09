#version 110

/* This comes interpolated from the vertex shader */
varying vec2 texcoord;

/* The 4 textures to be rendered */
uniform sampler2D texFront;
// uniform sampler2D texBack;
uniform sampler2D texLeft;
uniform sampler2D texRight;
// uniform sampler2D texTop;
uniform sampler2D texBottom;

uniform int antialiasing;

uniform vec2 pixelOffset[16];

uniform vec4 backgroundColor;

uniform vec2 cursorPos;

uniform bool drawCursor;

float normalizeCoordinate(float value, vec2 range) {
    float min = range.x;
    float max = range.y;
    return (value - min) / (max - min);
}


void main(void) {
	//Anti-aliasing
	vec4 colorN[16];
	float heightRatio = 2.714;
    float widthRatio  = 5.841;
    float depthRatio  = 7.298;

	for (int loop = 0; loop < 16; loop++) {
		vec2 coord = texcoord+pixelOffset[loop];

//         // make -1.0 point green
//         if (-0.95 > coord.x && coord.x > -1.0 &&
//             -0.95 > coord.y && coord.y > -1.0 ) {
//             gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
//             return;
//         }
//
//         // make 1.0 point blue
//         if (0.95 < coord.x && coord.x < 1.0 &&
//             0.95 < coord.y && coord.y < 1.0 ) {
//             gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
//             return;
//         }

        // Left, Front, Right
        vec2 yRange = vec2(1.0 - (2.0*heightRatio/(heightRatio+depthRatio)), 1.0);

        vec2 xRangeBottom = vec2(-1.0 + depthRatio*2.0/(depthRatio*2.0+widthRatio), -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));
        vec2 yRangeBottom = vec2(yRange[0] - 2.0*depthRatio/(heightRatio+depthRatio) , yRange[0]);
        if (yRange[0] <= coord.y && coord.y <= yRange[1]) {
            // Left
            vec2 xRange = vec2(-1.0,
                               -1.0 + depthRatio*2.0/(depthRatio*2.0+widthRatio));
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {

                colorN[loop] = vec4(texture2D(texLeft, vec2(normalizeCoordinate(coord.x, xRange),
                                                            normalizeCoordinate(coord.y, yRange)
                                   )).rgb, 1.0);
            }
            // Front
            xRange = vec2(xRange[1],
                          -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {

                colorN[loop] = vec4(texture2D(texFront, vec2(normalizeCoordinate(coord.x, xRange),
                                                             normalizeCoordinate(coord.y, yRange)
                                   )).rgb, 1.0);
            }
            // Right
            xRange = vec2(xRange[1], 1.0);
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {

                colorN[loop] = vec4(texture2D(texRight, vec2(normalizeCoordinate(coord.x, xRange),
                                                             normalizeCoordinate(coord.y, yRange)
                                   )).rgb, 1.0);
            }
        }else if (yRangeBottom[0] <= coord.y && coord.y <= yRangeBottom[1] && xRangeBottom[0] <= coord.x && coord.x <= xRangeBottom[1]) {
            colorN[loop] = vec4(texture2D(texBottom, vec2(normalizeCoordinate(coord.x, xRangeBottom),
                                                         normalizeCoordinate(coord.y, yRangeBottom)
                               )).rgb, 1.0);
        }else {
            gl_FragColor = backgroundColor;
            return;
        }


		if (drawCursor) {
			if (coord.x*2.0 + 0.006 >= cursorPos.x-1.0 && coord.x*2.0 - 0.006 < cursorPos.x-1.0 &&
				coord.y*3.0 + 0.012 >= cursorPos.y*2.0-1.0 && coord.y*3.0 - 0.012 < cursorPos.y*2.0-1.0) {
					colorN[loop] = vec4(1.0, 1.0, 1.0, 1.0);
			}
		}


	}

	// if (antialiasing == 16)
	vec4 corner[4];
	corner[0] = mix(mix(colorN[0], colorN[1], 2.0/3.0), mix(colorN[4], colorN[5], 3.0/5.0), 5.0/8.0);
	corner[1] = mix(mix(colorN[3], colorN[2], 2.0/3.0), mix(colorN[7], colorN[6], 3.0/5.0), 5.0/8.0);
	corner[2] = mix(mix(colorN[12], colorN[13], 2.0/3.0), mix(colorN[8], colorN[9], 3.0/5.0), 5.0/8.0);
	corner[3] = mix(mix(colorN[15], colorN[14], 2.0/3.0), mix(colorN[11], colorN[10], 3.0/5.0), 5.0/8.0);
	gl_FragColor = mix(mix(corner[0], corner[1], 0.5), mix(corner[2], corner[3], 0.5), 0.5);
}
