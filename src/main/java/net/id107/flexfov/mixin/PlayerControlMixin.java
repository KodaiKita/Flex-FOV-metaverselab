package net.id107.flexfov.mixin;

import net.minecraft.client.MinecraftClient;
import net.minecraft.client.network.ClientPlayerEntity;
import org.spongepowered.asm.mixin.Final;
import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.Shadow;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

import java.util.Objects;

@Mixin(ClientPlayerEntity.class)
public class PlayerControlMixin {

    @Shadow @Final protected MinecraftClient client;

    @Inject(
            method = "tickMovement",
            at = @At(
                    value = "INVOKE",
                    target = "Lnet/minecraft/client/input/Input;tick(Z)V",
                    shift = At.Shift.AFTER
            )
    )
    private void overrideTickMovement(CallbackInfo ci) {
        ClientPlayerEntity player = (ClientPlayerEntity) (Object) this;

        // ここで player.input を更新すれば自動操作が可能

        // 前方に移動し続ける例
//        player.input.pressingForward = true;
//        player.input.movementForward = 1.0f;
        // ダッシュする
//        this.client.options.keySprint.setPressed(true);
    }
}
