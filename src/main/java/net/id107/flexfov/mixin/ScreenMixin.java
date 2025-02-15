package net.id107.flexfov.mixin;

import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.Inject;
import org.spongepowered.asm.mixin.injection.callback.CallbackInfo;

import net.id107.flexfov.projection.Projection;
import net.minecraft.client.MinecraftClient;
import net.minecraft.client.gui.screen.Screen;

@Mixin(Screen.class)
public class ScreenMixin {

	@Inject(method = "renderBackground(Lnet/minecraft/client/util/math/MatrixStack;)V", at = @At(value = "HEAD"), cancellable = true)
	private void render(CallbackInfo callbackInfo) {
		// Esc でポーズしたとき、ワールド読み込み時, Options設定時画面(曖昧) 　などで繰り返し呼ばれる
//		System.out.println("ScreenMixin render called");
		if (Projection.getProjection().getResizeGui() && MinecraftClient.getInstance().world != null) {
			callbackInfo.cancel();
		}
	}
}
