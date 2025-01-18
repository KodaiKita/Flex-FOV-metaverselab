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

// uniform int antialiasing;

uniform vec2 pixelOffset[16];

uniform vec4 backgroundColor;

uniform vec2 cursorPos;

uniform bool drawCursor;

// Range 内のvalue を 0.0 ~ 1.0 に丸める
float normalizeCoordinate(float value, vec2 range) {
    float min = range.x;
    float max = range.y;
    return (value - min) / (max - min);
}

// 0.0 ~ 1.0 の value を Range 内に丸める
float customizeCoordinate(float value, vec2 range) {
    float min = range.x;
    float max = range.y;
    return value * (max - min) + min;
}


void main(void) {
	//Anti-aliasing
	vec4 colorN[16];

	// The ratios of the sides of the cube
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

        // Minecraft の画面内で, 左下が coord = (-1, -1), 右上が coord = (1, 1) になる
        // texture2D


        vec2 yRange = vec2(1.0 - (2.0*heightRatio/(heightRatio+depthRatio)), 1.0);

        vec2 xRangeBottom = vec2(-1.0 + depthRatio*2.0/(depthRatio*2.0+widthRatio), -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));
        vec2 yRangeBottom = vec2(yRange[0] - 2.0*depthRatio/(heightRatio+depthRatio) , yRange[0]);
        // Left, Front, Right
        if (yRange[0] <= coord.y && coord.y <= yRange[1]) {


            vec2 xRange = vec2(-1.0,
                               -1.0 + depthRatio*2.0/(depthRatio*2.0+widthRatio));
            // Left
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                float normalX = normalizeCoordinate(coord.x, xRange);
                float normalY = normalizeCoordinate(coord.y, yRange);

                vec3 destination3D = vec3(customizeCoordinate(normalX, vec2(-depthRatio/2, depthRatio/2)),
                                          -widthRatio/2,
                                          customizeCoordinate(normalY, vec2(-heightRatio/2, heightRatio/2)));
                colorN[loop] = vec4(texture2D(texLeft, vec2(normalX, normalY)).rgb, 1.0);
            }

            xRange = vec2(xRange[1],
                          -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));
            // Front
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                float normalX = normalizeCoordinate(coord.x, xRange);
                float normalY = normalizeCoordinate(coord.y, yRange);

                vec3 destination3D = vec3(depthRatio/2,
                                          customizeCoordinate(normalX, vec2(-widthRatio/2, widthRatio/2)),
                                          customizeCoordinate(normalY, vec2(-heightRatio/2, heightRatio/2)));
                // (0, 0, 0) から destination3D までの線分が
                // 面 Front x = height/2, -height/2 <= y <= height/2 , -height/2 <= z <= height/2 と交差するかどうかを判定する

                // x成分からパラメータを計算
                float t = (height/2) / destination3D.x;

                // パラメータが 0 <= t <= 1 ならば交差する
                if (0 <= t && t <= 1) {
                    // 交差した点の3次元座標を計算
                    vec3 intersect = vec3(height/2, destination3D.y * t, destination3D.z * t);
                    // 交差した点が面の範囲内にあるかどうかを判定
                    if (-height/2 <= intersect.y && intersect.y <= height/2 &&
                        -height/2 <= intersect.z && intersect.z <= height/2) {
                        // 交差した点の色をテクスチャ座標に変換
                        normalY = normalizeCoordinate(intersect.z, vec2(-height/2, height/2));
                        normalX = normalizeCoordinate(intersect.y, vec2(-height/2, height/2));

                    }
                }

                colorN[loop] = vec4(texture2D(texFront, vec2(normalX, normalY)).rgb, 1.0);
            }
            // Right
            xRange = vec2(xRange[1], 1.0);
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                float normalX = normalizeCoordinate(coord.x, xRange);
                float normalY = normalizeCoordinate(coord.y, yRange);

                vec3 destination3D = vec3(customizeCoordinate(normalX, vec2(-depthRatio/2, depthRatio/2)),
                                          widthRatio/2,
                                          customizeCoordinate(normalY, vec2(-heightRatio/2, heightRatio/2)));
                colorN[loop] = vec4(texture2D(texRight, vec2(normalX, normalY)).rgb, 1.0);
            }
        // Bottom
        }else if (yRangeBottom[0] <= coord.y && coord.y <= yRangeBottom[1] && xRangeBottom[0] <= coord.x && coord.x <= xRangeBottom[1]) {
            float normalX = normalizeCoordinate(coord.x, xRangeBottom);
            float normalY = normalizeCoordinate(coord.y, yRangeBottom);

            vec3 destination3D = vec3(customizeCoordinate(normalY, vec2(-depthRatio/2, depthRatio/2)),
                                      customizeCoordinate(normalX, vec2(-widthRatio/2, widthRatio/2)),
                                      heightRatio/2);

            colorN[loop] = vec4(texture2D(texBottom, vec2(normalX, normalY)).rgb, 1.0);
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
