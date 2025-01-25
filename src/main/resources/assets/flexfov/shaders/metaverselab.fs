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

// Cube , メタバースラボ の各面のID (方向共通)
const int ID_FRONT = 0;
const int ID_LEFT  = 1;
const int ID_RIGHT = 2;
const int ID_ROOF  = 3;
const int ID_FLOOR = 4;
const int ID_BACK  = 5;

const vec2 METAVERSE_LAB_RANGE_X = vec2(-RATIO_DEPTH/2.0, RATIO_DEPTH/2.0);
const vec2 METAVERSE_LAB_RANGE_Y = vec2(-RATIO_WIDTH/2.0, RATIO_WIDTH/2.0);
const vec2 METAVERSE_LAB_RANGE_Z = vec2(-RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0);

const float METALAB_FRONT_COORD_X = RATIO_DEPTH/2.0;
const float METALAB_LEFT_COORD_Y = -RATIO_WIDTH/2.0;
const float METALAB_RIGHT_COORD_Y = RATIO_WIDTH/2.0;
const float METALAB_ROOF_COORD_Z = RATIO_HEIGHT/2.0;
const float METALAB_FLOOR_COORD_Z = -RATIO_HEIGHT/2.0;
const float METALAB_BACK_COORD_X = -RATIO_DEPTH/2.0;

const vec2 CUBE_RANGE_XYZ = vec2(-RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0);

// Range.x ~ Range.y 内のvalue を 0.0 ~ 1.0 に丸める
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

// 0.0 ~ 1.0 の value を Range.x ~ Range.y 内に丸める
float customizeCoordinate(float value, vec2 range) {
    float min = range.x;
    float max = range.y;
    return value * (max - min) + min;
}

// 3D 空間は 左手系
// Cube の中心は (0,0,0) で辺の長さは RATIO_HEIGHT
// Cube の 無限に広がる各面と, (0,0,0) から destination3D までの線分 の交差点を返す
// 交差しない場合は (NaN, 0, 0) を返す
// Cube の面と交差する場合は isIntersected を true にする

// GLSLとMinecraft ウィンドウの座標関係は,
// Minecraft の画面内で, 左下が texcoord = (-1, -1), 右上が texcoord = (1, 1) になる
// x 軸 →
// y 軸 ↑

vec3 getIntersectionPoint(int targetFace, vec3 destination3D, out bool isIntersected) {
    vec3 intersection;
    vec3 nullVec = vec3(NaN, NaN, NaN);
    isIntersected = false;
    switch(targetFace) {
        case ID_FRONT:
            float x = height/2;
            float t = x / destination3D.x;
            if (t < 0.0 || t > 1.0) return nullVec;
            float y = destination3D.y * t;
            float z = destination3D.z * t;
            if (-height/2 <= y && y <= height/2 && -height/2 <= z && z <= height/2) isIntersected = true;
            return vec3(x, y, z);
        case ID_LEFT:
            float y = -width/2;
            float t = y / destination3D.y;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float z = destination3D.z * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= z && z <= height/2) isIntersected = true;
            return vec3(x, y, z);
        case ID_RIGHT:
            float y = width/2;
            float t = y / destination3D.y;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float z = destination3D.z * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= z && z <= height/2) isIntersected = true;
            return vec3(x, y, z);
        case ID_ROOF:
            float z = height/2;
            float t = z / destination3D.z;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float y = destination3D.y * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= y && y <= height/2) isIntersected = true;
            return vec3(x, y, z);
        case ID_FLOOR:
            float z = -height/2;
            float t = z / destination3D.z;
            if (t < 0.0 || t > 1.0) return nullVec;
            float x = destination3D.x * t;
            float y = destination3D.y * t;
            if (-height/2 <= x && x <= height/2 && -height/2 <= y && y <= height/2) isIntersected = true;
            return vec3(x, y, z);
        case ID_BACK:
            float x = -height/2;
            float t = x / destination3D.x;
            if (t < 0.0 || t > 1.0) return nullVec;
            float y = destination3D.y * t;
            float z = destination3D.z * t;
            if (-height/2 <= y && y <= height/2 && -height/2 <= z && z <= height/2) isIntersected = true;
            return vec3(x, y, z);
    }
    return nullVec;
}

// メタバースラボ面のテクスチャ座標を 3D 空間の座標に変換する
vec3 convertTextureCoordinateTo3D(int targetFace, vec2 textureCoordinate) {
    switch(targetFace) {
        case ID_FRONT:
            float y = customizeCoordinate(textureCoordinate.x, METAVERSE_LAB_RANGE_X);
            float z = customizeCoordinate(textureCoordinate.y, METAVERSE_LAB_RANGE_Z);
            return vec3(METALAB_FRONT_COORD_X, y, z);
        case ID_LEFT:
            float x = customizeCoordinate(textureCoordinate.x, METAVERSE_LAB_RANGE_X);
            float z = customizeCoordinate(textureCoordinate.y, METAVERSE_LAB_RANGE_Z);
            return vec3(x, METALAB_LEFT_COORD_Y, z);
        case ID_RIGHT:
            float x = customizeCoordinate(1.0-textureCoordinate.x, METAVERSE_LAB_RANGE_X);
            float z = customizeCoordinate(textureCoordinate.y, METAVERSE_LAB_RANGE_Z);
            return vec3(x, METALAB_RIGHT_COORD_Y, z);
        case ID_FLOOR:
            float y = customizeCoordinate(textureCoordinate.x, METAVERSE_LAB_RANGE_Y);
            float x = customizeCoordinate(1.0-textureCoordinate.y, METAVERSE_LAB_RANGE_X);
            return vec3(x, y, METALAB_FLOOR_COORD_Z);
    }
    return vec3(NaN, NaN, NaN);
}

// 正しい交差点をその面のテクスチャ座標に変換する
vec2 convert3DToTextureCoordinate(int targetFace, vec3 positionOnCubeFace3D) {
    switch(targetFace) {
        case ID_FRONT:
            float x = normalizeCoordinate(positionOnCubeFace3D.y, CUBE_RANGE_XYZ);
            float y = normalizeCoordinate(positionOnCubeFace3D.z, CUBE_RANGE_XYZ);
            return vec2(x, y);
        case ID_LEFT:
            float x = normalizeCoordinate(positionOnCubeFace3D.x, CUBE_RANGE_XYZ);
            float y = normalizeCoordinate(positionOnCubeFace3D.z, CUBE_RANGE_XYZ);
            return vec2(x, y);
        case ID_RIGHT:
            float x = 1 - normalizeCoordinate(positionOnCubeFace3D.x, CUBE_RANGE_XYZ);
            float y = normalizeCoordinate(positionOnCubeFace3D.z, CUBE_RANGE_XYZ);
            return vec2(x, y);
        case ID_ROOF:
            float x = normalizeCoordinate(positionOnCubeFace3D.y, CUBE_RANGE_XYZ);
            float y = normalizeCoordinate(positionOnCubeFace3D.x, CUBE_RANGE_XYZ);
            return vec2(x, y);
        case ID_FLOOR:
            float x = normalizeCoordinate(positionOnCubeFace3D.y, CUBE_RANGE_XYZ);
            float y = 1 - normalizeCoordinate(positionOnCubeFace3D.x, CUBE_RANGE_XYZ);
            return vec2(x, y);
        case ID_BACK:
            float x = 1 - normalizeCoordinate(positionOnCubeFace3D.y, CUBE_RANGE_XYZ);
            float y = normalizeCoordinate(positionOnCubeFace3D.z, CUBE_RANGE_XYZ);
            return vec2(x, y);
    }
    return vec2(NaN, NaN);
}

void main(void) {
    
    // 各面の「Minecraftウィンドウ上の位置」をtexcoord系座標にしたもの
    // 画面上部のxの範囲
    vec2 rangeXLeft = vec2(-1.0, -1.0 + RATIO_DEPTH*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH));
    vec2 rangeXFront = vec2(xRange[1], -1.0 + (RATIO_DEPTH+RATIO_WIDTH)*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH));
    vec2 rangeXRight = vec2(xRange[1], 1.0);

    // 画面上部 (Left, Front, Right) の y の範囲
    vec2 rangeYUpperPart = vec2(1.0 - (2.0*RATIO_HEIGHT/(RATIO_HEIGHT+RATIO_DEPTH)), 1.0);

    vec2 rangeXFloor = vec2(-1.0 + RATIO_DEPTH*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH), -1.0 + (RATIO_DEPTH+RATIO_WIDTH)*2.0/(RATIO_DEPTH*2.0+RATIO_WIDTH));
    vec2 rangeYFloor = vec2(yRange[0] - 2.0*RATIO_DEPTH/(RATIO_HEIGHT+RATIO_DEPTH) , yRange[0]);

    // 現在のピクセルはMetaVerseLab のどの面か計算する
    int destinationFace;

    // 各面の左上 0,0 右下 1,1 として丸めた座標
    vec2 normalCoord;

    // 3D空間 での座標
    vec3 coordDestination3D;

    // TODO: このあたりのTexture座標から3D座標に変換する処理が間違っている気がするので確認する
    // TODO: このあたりの処理を関数にまとめる (convertTextureCoordinateTo3D)
    if (rangeYUpperPart[0] <= texcoord.y && texcoord.y <= rangeYUpperPart[1]) {// マイクラ画面上で Left, Front, Right のいずれか
        if (rangeXLeft[0] < texcoord.x && texcoord.x <= rangeXLeft[1]) {// Left
            convertTextureCoordinateTo3D(ID_LEFT, texcoord);
            destinationFace = ID_LEFT;
            normalCoord = vec2(normalizeCoordinate(texcoord, rangeXLeft, rangeYUpperPart));
            coordDestination3D = vec3(customizeCoordinate(normalCoord.y, -RATIO_DEPTH/2.0, RATIO_DEPTH/2.0),
                                 -RATIO_WIDTH,
                                 customizeCoordinate(normalCoord.x, -RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0));
        }else if(rangeXFront[0] < texcoord.x && texcoord.x <= rangeXFront[1]){// Front
            destinationFace = ID_FRONT;
            normalCoord = vec2(normalizeCoordinate(texcoord, rangeXFront, rangeYUpperPart));
            coordDestination3D = vec3(RATIO_DEPTH/2.0,
                                 customizeCoordinate(normalCoord.x, -RATIO_WIDTH/2.0, RATIO_WIDTH/2.0),
                                 customizeCoordinate(normalCoord.y, -RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0));
        }else{// Right
            destinationFace = ID_RIGHT;
            normalCoord = vec2(normalizeCoordinate(texcoord, rangeXRight, rangeYUpperPart));
            coordDestination3D = vec3(customizeCoordinate(1.0-normalCoord.y, -RATIO_DEPTH/2.0, RATIO_DEPTH/2.0),
                                 RATIO_WIDTH/2.0,
                                 customizeCoordinate(normalCoord.x, -RATIO_HEIGHT/2.0, RATIO_HEIGHT/2.0));
        }
    }else if(rangeYFloor[0] <= texcoord.y && texcoord.y <= rangeYFloor[1] &&
             rangeXFloor[0] <= texcoord.x && texcoord.x <= rangeXFloor[1]){// マイクラ画面下で Bottom
        destinationFace = ID_FLOOR;
        normalCoord = vec2(normalizeCoordinate(texcoord, rangeXFloor, rangeYFloor));
        coordDestination3D = vec3(customizeCoordinate(1.0-normalCoord.y, -RATIO_DEPTH/2.0, RATIO_DEPTH/2.0),
                             customizeCoordinate(normalCoord.x, -RATIO_WIDTH/2.0, RATIO_WIDTH/2.0),
                             -RATIO_HEIGHT/2.0);
    }else{// 範囲外
        gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
        return;
    }

    // Cube のどの面のピクセルを持ってくるか計算する

    // とりあえず同じ面との交差点を求める
    bool isIntersected;
    vec3 intersectionWithSameFace = getIntersectionPoint(destinationFace, coordDestination3D, isIntersected);

    if(isIntersected){
        // TODO: 交差点をテクスチャ座標に変換してからピクセルデータをとってくる

    }

    // 他の面を試す
    for (int i=ID_FRONT; i < ID_BACK; i++) {
        if (i == destinationFace) continue;
        bool isIntersected;
        vec3 intersection = getIntersectionPoint(i, coordDestination3D, isIntersected);

        if (isIntersected) {
            // TODO: 交差点をテクスチャ座標に変換してからピクセルデータをとってくる

            break;
        }
    }
}