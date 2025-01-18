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

uniform vec2 pixelOffset[16];

uniform vec4 backgroundColor;

uniform vec2 cursorPos;

uniform bool drawCursor;

const float RATIO_HEIGHT = 2.714;
const float RATIO_WIDTH  = 5.841;
const float RATIO_DEPTH  = 7.298;

const int ID_FRONT = 0;
const int ID_LEFT = 1;
const int ID_RIGHT = 2;
const int ID_ROOF = 3;
const int ID_FLOOR = 4;
const int ID_BACK = 5;


// Range 内のvalue を 0.0 ~ 1.0 に丸める
float normalizeCoordinate(float value, vec2 range) {
    float min = range.x;
    float max = range.y;
    return (value - min) / (max - min);
}

// vec2 を Range 内の値を 0.0 ~ 1.0 に丸める
vec2 normalizeCoordinate(vec2 target, vec2 rangeX, vec2 rangeY) {
    return vec2(normalizeCoordinate(target.x, rangeX), normalizeCoordinate(target.y, rangeY));
}
vec2 normalizeCoordinate(vec2 target, vec2 range) {
    return normalizeCoordinate(target, range, range);
}

// 0.0 ~ 1.0 の value を Range 内に丸める
float customizeCoordinate(float value, vec2 range) {
    float min = range.x;
    float max = range.y;
    return value * (max - min) + min;
}

// 3D 空間は 左手系
// Cube の中心は (0,0,0) で辺の長さは RATIO_HEIGHT
// Cube の 各面と, (0,0,0) から destination3D までの線分 の交差点を返す
// 交差しない場合は (0, 0, 0) を返す

// TODO: 交差しない場合、交差しない情報だけでなく、どちらに交差点がずれているかも返す
vec3 getIntersectionPoint(int targetFace, vec3 destination3D) {
    vec3 intersection;
    vec3 nullVec = vec3(0.0, 0.0, 0.0);
    switch(targetFace) {
        case ID_FRONT:
            float x = height/2;
            float t = x / destination3D.x;
            if (t < 0.0 || t > 1.0) return nullVec;
            float y = destination3D.y * t;
            float z = destination3D.z * t;
            if (-height/2 <= y && y <= height/2 && -height/2 <= z && z <= height/2) return vec3(x, y, z);
        case ID_LEFT:
            float y = -width/2;
            float t = y / destination3D.y;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float z = destination3D.z * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= z && z <= height/2) return vec3(x, y, z);
        case ID_RIGHT:
            float y = width/2;
            float t = y / destination3D.y;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float z = destination3D.z * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= z && z <= height/2) return vec3(x, y, z);
        case ID_ROOF:
            float z = height/2;
            float t = z / destination3D.z;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float y = destination3D.y * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= y && y <= height/2) return vec3(x, y, z);
        case ID_FLOOR:
            float z = -height/2;
            float t = z / destination3D.z;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float y = destination3D.y * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= y && y <= height/2) return vec3(x, y, z);
        case ID_BACK:
            float x = -height/2;
            float t = x / destination3D.x;
            if (t < 0.0 || t > 1.0) return nullVec;
            float y = destination3D.y * t;
            float z = destination3D.z * t;
            if (-height/2 <= y && y <= height/2 && -height/2 <= z && z <= height/2) return vec3(x, y, z);
    }
    return nullVec;
}

void main(void) {
    
    vec2 xRangeLeft = vec2(-1.0, -1.0 + RATIO_DEPTH*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH));
    vec2 xRangeFront = vec2(xRange[1], -1.0 + (RATIO_DEPTH+RATIO_WIDTH)*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH));
    vec2 xRangeRight = vec2(xRange[1], 1.0);

    vec2 yRangeTop = vec2(1.0 - (2.0*RATIO_HEIGHT/(RATIO_HEIGHT+RATIO_DEPTH)), 1.0);

    vec2 xRangeBottom = vec2(-1.0 + RATIO_DEPTH*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH), -1.0 + (RATIO_DEPTH+RATIO_WIDTH)*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH));
    vec2 yRangeBottom = vec2(yRange[0] - 2.0*RATIO_DEPTH/(RATIO_HEIGHT+RATIO_DEPTH) , yRange[0]);

    // 現在のピクセルはMetaVerseLab のどの面か計算する
    int destinationFace;

    // 各面の左上 0,0 右下 1,1 として丸めた座標
    vec2 normalCoord;

    // 3D空間 での座標
    vec3 destination3D;
    
    if (yRangeTop[0] <= texcoord.y && texcoord.y <= yRangeTop[1]) {
        // マイクラ画面上で Left, Front, Right のいずれか
        if (xRangeLeft[0] < texcoord.x && texcoord.x <= xRangeLeft[1]) {
            // Left
            destinationFace = ID_LEFT;
            normalCoord = vec2(normalizeCoordinate(texcoord, xRangeLeft, yRangeTop));
            destination3D = vec3(customizeCoordinate(normalCoord.y, -RATIO_DEPTH/2.0, RATIO_DEPTH/2.0),
                                 -RATIO_WIDTH,
                                 customizeCoordinate(normalCoord.x, -RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0));
        }else if(xRangeFront[0] < texcoord.x && texcoord.x <= xRangeFront[1]){
            // Front
            destinationFace = ID_FRONT;
            normalCoord = vec2(normalizeCoordinate(texcoord, xRangeFront, yRangeTop));
            destination3D = vec3(RATIO_DEPTH/2.0,
                                 customizeCoordinate(normalCoord.x, -RATIO_WIDTH/2.0, RATIO_WIDTH/2.0),
                                 customizeCoordinate(normalCoord.y, -RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0));
        }else{
            // Right
            destinationFace = ID_RIGHT;
            normalCoord = vec2(normalizeCoordinate(texcoord, xRangeRight, yRangeTop));
            destination3D = vec3(customizeCoordinate(1.0-normalCoord.y, -RATIO_DEPTH/2.0, RATIO_DEPTH/2.0),
                                 RATIO_WIDTH/2.0,
                                 customizeCoordinate(normalCoord.x, -RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0));
        }
    }else if(yRangeBottom[0] <= texcoord.y && texcoord.y <= yRangeBottom[1] && xRangeBottom[0] <= texcoord.x && texcoord.x <= xRangeBottom[1]){
        // マイクラ画面下で Bottom
        destinationFace = ID_FLOOR;
        normalCoord = vec2(normalizeCoordinate(texcoord, xRangeBottom, yRangeBottom));
        destination3D = vec3(customizeCoordinate(1.0-normalCoord.y, -RATIO_DEPTH/2.0, RATIO_DEPTH/2.0),
                             customizeCoordinate(normalCoord.x, -RATIO_WIDTH/2.0, RATIO_WIDTH/2.0),
                             -RATIO_HEIGHT/2.0);
    }else{
        // 範囲外
        gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
        return;
    }

    // Cube のどの面のピクセルを持ってくるか計算する

    // とりあえず同じ面との交差点を求める
    vec3 intersectionWithSameFace = getIntersectionPoint(destinationFace, destination3D);
    // 交差しない場合は相対的に右の面を試す
    if (intersectionWithSameFace == vec3(0.0, 0.0, 0.0)) {
    }
}