#version 110

varying vec2 texcoord;

uniform sampler2D texFront;
uniform sampler2D texBack;
uniform sampler2D texLeft;
uniform sampler2D texRight;
uniform sampler2D texTop;
uniform sampler2D texBottom;

// 新しいパラメータ
uniform float fovH; // 水平視野角（ラジアン）
uniform float fovV; // 垂直視野角（ラジアン）
uniform vec3 viewDirection; // 視線方向ベクトル（正規化済み）
uniform vec3 upVector; // 上方向ベクトル（正規化済み）

vec4 sampleCubemap(vec3 direction) {
    vec3 absDir = abs(direction);

    if (absDir.x >= absDir.y && absDir.x >= absDir.z) {
        // X軸が最大
        if (direction.x > 0.0) {
            // Right face
            vec2 uv = vec2(-direction.z / direction.x, direction.y / direction.x) * 0.5 + 0.5;
            return texture2D(texRight, uv);
        } else {
            // Left face
            vec2 uv = vec2(direction.z / -direction.x, direction.y / -direction.x) * 0.5 + 0.5;
            return texture2D(texLeft, uv);
        }
    } else if (absDir.y >= absDir.z) {
        // Y軸が最大
        if (direction.y > 0.0) {
            // Top face
            vec2 uv = vec2(direction.x / direction.y, -direction.z / direction.y) * 0.5 + 0.5;
            return texture2D(texTop, uv);
        } else {
            // Bottom face
            vec2 uv = vec2(direction.x / -direction.y, direction.z / -direction.y) * 0.5 + 0.5;
            return texture2D(texBottom, uv);
        }
    } else {
        // Z軸が最大
        if (direction.z > 0.0) {
            // Front face
            vec2 uv = vec2(direction.x / direction.z, direction.y / direction.z) * 0.5 + 0.5;
            return texture2D(texFront, uv);
        } else {
            // Back face
            vec2 uv = vec2(-direction.x / -direction.z, direction.y / -direction.z) * 0.5 + 0.5;
            return texture2D(texBack, uv);
        }
    }
}

void main(void) {
    // 正規化されたスクリーン座標 (-1 to 1)
    vec2 screenCoord = texcoord * 2.0 - 1.0;

    // アスペクト比を考慮した座標変換
    float aspectRatio = tan(fovH * 0.5) / tan(fovV * 0.5);
    screenCoord.x *= aspectRatio;

    // スクリーン座標から3D方向ベクトルを計算
    float tanHalfFovV = tan(fovV * 0.5);
    float tanHalfFovH = tan(fovH * 0.5);

    vec3 forward = normalize(viewDirection);
    vec3 right = normalize(cross(forward, upVector));
    vec3 up = cross(right, forward);

    // レイの方向を計算
    vec3 rayDir = forward +
    right * screenCoord.x * tanHalfFovH +
    up * screenCoord.y * tanHalfFovV;
    rayDir = normalize(rayDir);

    // キューブマップからサンプリング
    gl_FragColor = sampleCubemap(rayDir);
}