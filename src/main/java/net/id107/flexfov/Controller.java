package net.id107.flexfov;

import net.id107.flexfov.mixin.PlayerControlMixin;
import net.minecraft.client.MinecraftClient;
import net.minecraft.client.input.Input;
import net.minecraft.client.network.ClientPlayerEntity;
import org.lwjgl.glfw.GLFW;
import org.lwjgl.glfw.GLFWGamepadState;
import org.spongepowered.asm.mixin.Unique;

import java.util.Objects;

public class Controller {
    private GLFWGamepadState gamepadState;

    float movementForward = 0.0f;
    float movementSideways = 0.0f;
    boolean isSprinting = true;

    public float getMovementForward() {
        return movementForward;
    }
    public float getMovementSideways() {
        return movementSideways;
    }
    public boolean isSprinting() {
        return isSprinting;
    }

    public void initialize() {
        if (!GLFW.glfwInit()) {
            throw new IllegalStateException("Unable to initialize GLFW");
        }
        gamepadState = GLFWGamepadState.create();
    }

    public boolean checkControllerValid(){
        // 2. コントローラーが接続されているか確認
        if(!GLFW.glfwJoystickPresent(GLFW.GLFW_JOYSTICK_1)) {
            System.out.println("コントローラーが接続されていません");
            return false;
        }

        // 3. ゲームパッドとして認識されているか確認
        if(!GLFW.glfwJoystickIsGamepad(GLFW.GLFW_JOYSTICK_1)) {
            System.out.println("ゲームパッドとして認識されていません");
            return false;
        }

        // 4. 現在の状態を取得
        if(!GLFW.glfwGetGamepadState(GLFW.GLFW_JOYSTICK_1, gamepadState)) {
            System.out.println("状態取得に失敗");
            return false;
        }
        return true;
    }

    public void pollInput() {
        // 2. コントローラーが接続されているか確認
        if(!GLFW.glfwJoystickPresent(GLFW.GLFW_JOYSTICK_1)) {
            System.out.println("コントローラーが接続されていません");
            return;
        }

        // 3. ゲームパッドとして認識されているか確認
        if(!GLFW.glfwJoystickIsGamepad(GLFW.GLFW_JOYSTICK_1)) {
            System.out.println("ゲームパッドとして認識されていません");
            return;
        }

        // 4. 現在の状態を取得
        if(!GLFW.glfwGetGamepadState(GLFW.GLFW_JOYSTICK_1, gamepadState)) {
            System.out.println("状態取得に失敗");
            return;
        }

        // 5. ボタン状態を読み取る
        for(int i = 0; i <= GLFW.GLFW_GAMEPAD_BUTTON_LAST; i++) {
            if(gamepadState.buttons(i) == GLFW.GLFW_PRESS) {
                System.out.println("Button " + i + " pressed");
            }
        }

        // 6. アナログスティックの値を読み取る
        float leftX = gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_LEFT_X);
        float leftY = gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_LEFT_Y);
        System.out.println("Left Stick: X=" + leftX + ", Y=" + leftY);

        // 7. トリガーの値を読み取る（-1.0～1.0を0.0～1.0に変換）
        float leftTrigger = (gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_LEFT_TRIGGER) + 1.0f) / 2.0f;
        float rightTrigger = (gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_RIGHT_TRIGGER) + 1.0f) / 2.0f;
        System.out.println("Triggers: L=" + leftTrigger + ", R=" + rightTrigger);
    }

    public void controllerTick(MinecraftClient client){
        handleControllerLook(client);
        handleControllerMovement(client);
//        printInputValues(client);
//        pollInput();
        if (Objects.isNull(client.player)) {
            return;
        }
        // Example: Always move forward
        client.player.input.pressingForward = true;
        client.player.input.movementForward = 1.0f;
    }

    void handleControllerMovement(MinecraftClient client) {
        ClientPlayerEntity player = client.player;

        if (Objects.isNull(player)) {
            return;
        }


        // --- ここでコントローラーからの入力を取得 ---
        if(!GLFW.glfwGetGamepadState(GLFW.GLFW_JOYSTICK_1, gamepadState)) {
            System.out.println("状態取得に失敗");
            return;
        }
        // 例として、すでに入力値が変数に格納されていると仮定します
        // 左スティックの入力を -1.0f から 1.0f の範囲で取得すると仮定
        float rightStickX = gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_RIGHT_X); // 左右
        float rightStickY = -gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_RIGHT_Y); // 前後 (Y軸は上下が逆の場合があるので -1 を掛ける)

        System.out.println("RightStick: X=" + rightStickX + ", Y=" + rightStickY);
        // --- 入力値をプレイヤーの移動に反映 ---
        final float DEAD_ZONE = 0.2f;

        // 左右移動 (-1.0f: 左, 1.0f: 右)
        if (Math.abs(rightStickY) > DEAD_ZONE) {
            this.movementForward = rightStickY;
        } else {
            this.movementForward = 0.0f;
        }

        // 前後移動 (-1.0f: 後, 1.0f: 前)
        if (Math.abs(rightStickX) > DEAD_ZONE) {
            this.movementSideways = -rightStickX;
        } else {
            this.movementSideways = 0.0f;
        }
        

        // ジャンプ
//        player.input.jumping = jumpButtonPressed;

        // (任意) スニーク
        // player.input.sneaking = isControllerSneakButtonPressed();

        // (参考) booleanフラグも更新しておくと、他のModとの互換性が高まる場合がある
        player.input.pressingForward = player.input.movementForward > 0;
        player.input.pressingBack = player.input.movementForward < 0;
        player.input.pressingLeft = player.input.movementSideways < 0; // X軸は負の値が左
        player.input.pressingRight = player.input.movementSideways > 0; // X軸は正の値が右
    }

    public void printInputValues(MinecraftClient client) {
        ClientPlayerEntity player = client.player;
        // check player null
        if (Objects.isNull(player)) {
            return;
        }
        // print all input values
        System.out.println("movementForward: " + player.input.movementForward);
        System.out.println("movementSideways: " + player.input.movementSideways);
        System.out.println("pressingForward: " + player.input.pressingForward);
        System.out.println("pressingBack: " + player.input.pressingBack);
        System.out.println("pressingLeft: " + player.input.pressingLeft);
        System.out.println("pressingRight: " + player.input.pressingRight);
        System.out.println("jumping: " + player.input.jumping);
        System.out.println("sneaking: " + player.input.sneaking);
    }

    void handleControllerLook(MinecraftClient client) {
        // プレイヤーがワールドに存在しない場合（例：タイトル画面）は何もしない
        if (client.player == null || client.world == null) {
            return;
        }

//        System.out.println("handleControllerLook called");

        // --- ここでコントローラーからの入力を取得 ---
        // 4. 現在の状態を取得
        if(!GLFW.glfwGetGamepadState(GLFW.GLFW_JOYSTICK_1, gamepadState)) {
            System.out.println("状態取得に失敗");
            return;
        }
        // 例として、すでに入力値が変数に格納されていると仮定します
        // 値の範囲は -1.0f から 1.0f とします
        float controllerX = gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_LEFT_X);// 水平方向の入力
        float controllerY = gamepadState.axes(GLFW.GLFW_GAMEPAD_AXIS_LEFT_Y); // 垂直方向の入力

//        System.out.println("ControllerX: " + controllerX + ", ControllerY: " + controllerY);


        // 感度設定（この値を調整して視点移動の速さを変える）
        final float SENSITIVITY = 2.5f;

        // デッドゾーン（スティックのわずかな傾きを無視する）
        final float DEAD_ZONE = 0.15f;

        ClientPlayerEntity player = client.player;

        // 水平方向（ヨー）の視点移動
        if (Math.abs(controllerX) > DEAD_ZONE) {
            float yawChange = controllerX * SENSITIVITY;
            player.yaw += yawChange;
        }

        // 垂直方向（ピッチ）の視点移動
        if (Math.abs(controllerY) > DEAD_ZONE) {
            float pitchChange = controllerY * SENSITIVITY;
            // setPitchは上下の角度を-90度から90度の範囲に自動でクランプ（制限）してくれる
            player.pitch += pitchChange;
        }
    }
}
