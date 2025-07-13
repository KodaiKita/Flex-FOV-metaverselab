package net.id107.flexfov.projection;

import net.id107.flexfov.Reader;
import net.minecraft.client.MinecraftClient;
import net.minecraft.client.util.math.MatrixStack;
import net.minecraft.util.math.Matrix4f;
import net.minecraft.util.math.Quaternion;
import net.minecraft.util.math.Vec3f;

public class MetaverseLab extends Projection {
    MinecraftClient mc;

    //Constructor
    public MetaverseLab() {
        mc = MinecraftClient.getInstance();
    }

    @Override
    public void rotateCamera(MatrixStack matrixStack) {
        Matrix4f matrix;
        float playerPitch = mc.player.pitch;
        System.out.println("playerPitch: " + playerPitch);

        // ピッチをリセットする回転（X?軸周りの回転）

        // POSITIVE_Z -> LEFT テクスチャの固定
        // POSITIVE_X -> TOP, BOTTOM, FRONT テクスチャの固定
        Quaternion resetPitchRotation;
        if (renderPass == 2) {
            resetPitchRotation = Vec3f.POSITIVE_Z.getDegreesQuaternion(-playerPitch);
        }else if (renderPass == 0 || renderPass == 3 || renderPass == 4) {
            // 0, 3, 4 の場合は X 軸周りの回転
            resetPitchRotation = Vec3f.POSITIVE_X.getDegreesQuaternion(-playerPitch);
        } else if (renderPass == 5) {
            resetPitchRotation = Vec3f.NEGATIVE_X.getDegreesQuaternion(-playerPitch);
        } else { // renderPass == 1
            resetPitchRotation = Vec3f.NEGATIVE_Z.getDegreesQuaternion(-playerPitch);
        }
        // 最終的に適用する回転を保持するクォータニオン
        Quaternion finalRotation = new Quaternion(resetPitchRotation);

        // 方向に応じた回転
        if (renderPass > 0) {
            Quaternion directionRotation;
            switch (renderPass) {
                case 1: // 右
                    directionRotation = new Quaternion(0, 0.707106781f, 0, 0.707106781f);
                    break;
                case 2: // 左
                    directionRotation = new Quaternion(0, -0.707106781f, 0, 0.707106781f);
                    break;
                case 3: // 下
                    directionRotation = new Quaternion(0.707106781f, 0, 0, 0.707106781f);
                    break;
                case 4: // 上
                    directionRotation = new Quaternion(-0.707106781f, 0, 0, 0.707106781f);
                    break;
                case 5: // 後ろ
                    directionRotation = new Quaternion(0, -1, 0, 0);
                    break;
                default:
                    directionRotation = new Quaternion(0, 0, 0, 1); // 単位クォータニオン
                    break;
            }
            // 回転の方向を合成
            finalRotation.hamiltonProduct(directionRotation);
        }

        // 最終的な回転を適用
        matrixStack.peek().getModel().multiply(new Matrix4f(finalRotation));
    }

    @Override
    public String getFragmentShader() {
        return Reader.read("flexfov:shaders/metaverselab.fsh");
    }

    @Override
    public float[] getBackgroundColor(boolean ignored) {
        return super.getBackgroundColor(skyBackground);
    }

    @Override
    public double getFovX() {
        return 360;
    }

    @Override
    public double getFovY() {
        return 180;
    }
}
