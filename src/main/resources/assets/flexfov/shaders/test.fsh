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

const int ID_TOP = 0;
const int ID_BOTTOM = 1;
const int ID_LEFT = 2;
const int ID_FRONT = 3;
const int ID_RIGHT = 4;
const int ID_BACK = 5;

float heightRatio = 2.714;
float widthRatio  = 5.841;
float depthRatio  = 7.298;

// normalized coord はテクスチャ範囲内で 0.0 から 1.0 の範囲
vec3 calc3dCoord(vec2 normalizedCoord, int face_id) {
    vec3 result = vec3(0.0, 0.0, 0.0);
    switch (face_id) {
        case ID_BOTTOM:
            result.z = -heightRatio/2.0;
            result.x = normalizedCoord.x * widthRatio - widthRatio/2.0;
            result.y = normalizedCoord.y * depthRatio - depthRatio/2.0;
            break;
        case ID_LEFT:
            result.x = -widthRatio/2.0;
            result.y = normalizedCoord.x * depthRatio - depthRatio/2.0;
            result.z = normalizedCoord.y * heightRatio - heightRatio/2.0;
            break;
        case ID_FRONT:
            result.y = depthRatio/2.0;
            result.x = normalizedCoord.x * widthRatio - widthRatio/2.0;
            result.z = normalizedCoord.y * heightRatio - heightRatio/2.0;
            break;
        case ID_RIGHT:
            result.x = widthRatio/2.0;
            result.y = (1.0 - normalizedCoord.x) * depthRatio - depthRatio/2.0;
            result.z = normalizedCoord.y * heightRatio - heightRatio/2.0;
            break;
    }
    return result;
}

vec3 pickColor(vec3 normalizedViewDirection){

    int face_id;
    // normalizedViewDirection の最大の絶対値を持つ軸を見つける
    float maxDirection;
    if (abs(normalizedViewDirection.x) >= abs(normalizedViewDirection.y) && abs(normalizedViewDirection.x) >= abs(normalizedViewDirection.z)) {
        // X軸が最大
        maxDirection = normalizedViewDirection.x;
        if (normalizedViewDirection.x > 0.0) {
            face_id = ID_RIGHT;
        } else {
            face_id = ID_LEFT;
        }
    } else if (abs(normalizedViewDirection.y) >= abs(normalizedViewDirection.z)) {
        // Y軸が最大
        maxDirection = normalizedViewDirection.y;
        if (normalizedViewDirection.y > 0.0) {
            face_id = ID_FRONT;
        } else {
            face_id = ID_BACK;
        }
    } else {
        // Z軸が最大
        maxDirection = normalizedViewDirection.z;
        if (normalizedViewDirection.z > 0.0) {
            face_id = ID_TOP;
        } else {
            face_id = ID_BOTTOM;
        }
    }

    // f
    vec3 color = vec3(0.0, 0.0, 0.0);
    switch (face_id) {
        float dx, dy, dz;
        float t;
        vec2 texc;
        case ID_LEFT:
            dx = -0.5;
            t = dx / maxDirection;

            dz = normalizedViewDirection.z * t;
            dy = normalizedViewDirection.y * t;

            // テクスチャ座標(0~1)に変換
            texc = vec2(dy+0.5, dz+0.5);
            color = texture2D(texLeft, texc).rgb;
            break;
        case ID_FRONT:
            dy = 0.5;
            t = dy / maxDirection;

            dz = normalizedViewDirection.z * t;
            dx = normalizedViewDirection.x * t;

            // テクスチャ座標(0~1)に変換
            texc = vec2(dx+0.5, dz+0.5);
            color = texture2D(texFront, texc).rgb;
            break;
        case ID_RIGHT:
            dx = 0.5;
            t = dx / maxDirection;

            dz = normalizedViewDirection.z * t;
            dy = normalizedViewDirection.y * t;

            // テクスチャ座標(0~1)に変換
            texc = vec2(-dy+0.5, dz+0.5);
            color = texture2D(texRight, texc).rgb;
            break;
        case ID_BOTTOM:
            dz = -0.5;
            t = dz / maxDirection;

            dx = normalizedViewDirection.x * t;
            dy = normalizedViewDirection.y * t;

            // テクスチャ座標(0~1)に変換
            texc = vec2(dx+0.5, dy+0.5);
            color = texture2D(texBottom, texc).rgb;
            break;
        case ID_TOP:
            dz = 0.5;
            t = dz / maxDirection;

            dx = normalizedViewDirection.x * t;
            dy = normalizedViewDirection.y * t;

            // テクスチャ座標(0~1)に変換
            texc = vec2(dx+0.5, -dy+0.5);
            color = texture2D(texTop, texc).rgb;
            break;
        case ID_BACK:
            dy = -0.5;
            t = dy / maxDirection;

            dz = normalizedViewDirection.z * t;
            dx = normalizedViewDirection.x * t;

            // テクスチャ座標(0~1)に変換
            texc = vec2(-dx+0.5, dz+0.5);
            color = texture2D(texBack, texc).rgb;
            break;
    }
    // color が の各値が0.0 ~ 1.0でなければ 緑色を返す
    if (color.r < 0.0 || color.r > 1.0 || color.g < 0.0 || color.g > 1.0 || color.b < 0.0 || color.b > 1.0) {
        return vec3(0.0, 1.0, 0.0); // 緑色
    }
    return color;
}

float normalizeFloat(float value, float min, float max) {
    return (value - min) / (max - min);
}
vec2 normalizeVec2(vec2 value, vec2 min, vec2 max) {
    return vec2(normalizeFloat(value.x, min.x, max.x), normalizeFloat(value.y, min.y, max.y));
}


void main(void) {
    //Anti-aliasing
    vec4 colorN[16];

    for (int loop = 0; loop < antialiasing; loop++) {
        vec2 coord = texcoord+pixelOffset[loop];


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
                float normalizedX = normalizeFloat(coord.x, xRange[0], xRange[1]);
                float normalizedY = normalizeFloat(coord.y, yRange[0], yRange[1]);
                vec3 coord3d = calc3dCoord(vec2(normalizedX, normalizedY), ID_LEFT);
                // 視線ベクトルを正規化
                vec3 viewDirection = normalize(coord3d);

                colorN[loop] = vec4(pickColor(viewDirection), 1.0);
                continue;
            }
            // faceFront
            xRange = vec2(xRange[1],
            -1.0 + (depthRatio+widthRatio)*2.0/(depthRatio*2.0+widthRatio));
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                // debug return green
                float normalizedX = normalizeFloat(coord.x, xRange[0], xRange[1]);
                float normalizedY = normalizeFloat(coord.y, yRange[0], yRange[1]);
                vec3 coord3d = calc3dCoord(vec2(normalizedX, normalizedY), ID_FRONT);
                // 視線ベクトルを正規化
                vec3 viewDirection = normalize(coord3d);

                colorN[loop] = vec4(pickColor(viewDirection), 1.0);
                continue;
            }

            xRange = vec2(xRange[1], 1.0);
            // faceRight
            if (xRange[0] < coord.x && coord.x <= xRange[1]) {
                // return blue;
                float normalizedX = normalizeFloat(coord.x, xRange[0], xRange[1]);
                float normalizedY = normalizeFloat(coord.y, yRange[0], yRange[1]);
                vec3 coord3d = calc3dCoord(vec2(normalizedX, normalizedY), ID_RIGHT);

                vec3 viewDirection = normalize(coord3d);

                colorN[loop] = vec4(pickColor(viewDirection), 1.0);
                continue;
            }
            // faceBottom
        }else if (yRangeBottom[0] <= coord.y && coord.y <= yRangeBottom[1] && xRangeBottom[0] <= coord.x && coord.x <= xRangeBottom[1]) {
            // return yellow;
            float normalizedX = normalizeFloat(coord.x, xRangeBottom[0], xRangeBottom[1]);
            float normalizedY = normalizeFloat(coord.y, yRangeBottom[0], yRangeBottom[1]);
            vec3 coord3d = calc3dCoord(vec2(normalizedX, normalizedY), ID_BOTTOM);
            // 視線ベクトルを正規化
            vec3 viewDirection = normalize(coord3d);

            colorN[loop] = vec4(pickColor(viewDirection), 1.0);
            //            gl_FragColor = vec4(1.0, 1.0, 0.0, 1.0);
            continue;
        }else {
            // background color (gray)
            gl_FragColor = vec4(0.7, 0.7, 0.7, 1.0);
            return;
        }
    }

    // アンチエイリアス処理
    vec4 corner[4];
    corner[0] = mix(mix(colorN[0], colorN[1], 2.0/3.0), mix(colorN[4], colorN[5], 3.0/5.0), 5.0/8.0);
    corner[1] = mix(mix(colorN[3], colorN[2], 2.0/3.0), mix(colorN[7], colorN[6], 3.0/5.0), 5.0/8.0);
    corner[2] = mix(mix(colorN[12], colorN[13], 2.0/3.0), mix(colorN[8], colorN[9], 3.0/5.0), 5.0/8.0);
    corner[3] = mix(mix(colorN[15], colorN[14], 2.0/3.0), mix(colorN[11], colorN[10], 3.0/5.0), 5.0/8.0);
    gl_FragColor = mix(mix(corner[0], corner[1], 0.5), mix(corner[2], corner[3], 0.5), 0.5);
}