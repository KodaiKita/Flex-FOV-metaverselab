package net.id107.flexfov.mixin;

import org.spongepowered.asm.mixin.Mixin;
import org.spongepowered.asm.mixin.injection.At;
import org.spongepowered.asm.mixin.injection.ModifyVariable;

import net.id107.flexfov.projection.Projection;
import net.minecraft.client.MinecraftClient;
import net.minecraft.client.render.entity.EntityRenderer;
import net.minecraft.client.util.math.MatrixStack;
import net.minecraft.util.math.Vec3f;
import net.minecraft.entity.Entity;
import net.minecraft.util.math.Quaternion;
import net.minecraft.util.math.Vec3d;

@Mixin(EntityRenderer.class)
public class EntityRendererMixin {
	// ネームタグ関連の処理っぽいが、消してもそんなに問題なさそうで、正直何をしてるかわからん

	private Entity currentEntity;
	
	@ModifyVariable(method = "renderLabelIfPresent(Lnet/minecraft/entity/Entity;Lnet/minecraft/text/Text;Lnet/minecraft/client/util/math/MatrixStack;Lnet/minecraft/client/render/VertexConsumerProvider;I)V",
			at = @At(value = "HEAD"))
	private Entity getEntity(Entity entity) {
		// ネームタグが描画されているときに呼ばれる
//		System.out.println("getEntity called");
		currentEntity = entity;
		return entity;
	}
	
	@ModifyVariable(method = "renderLabelIfPresent(Lnet/minecraft/entity/Entity;Lnet/minecraft/text/Text;Lnet/minecraft/client/util/math/MatrixStack;Lnet/minecraft/client/render/VertexConsumerProvider;I)V",
			at = @At(value = "INVOKE", ordinal = 0,
			target = "Lnet/minecraft/client/util/math/MatrixStack;multiply(Lnet/minecraft/util/math/Quaternion;)V"))
	private MatrixStack rotateNameplatePre(MatrixStack matrixStack) {
		// ネームタグが描画されているときに呼ばれる
//		System.out.println("rotateNameplatePre called");
		if (Projection.getProjection().shouldRotateParticles()) {
			matrixStack.push();
		}
		return matrixStack;
	}
	
	@ModifyVariable(method = "renderLabelIfPresent(Lnet/minecraft/entity/Entity;Lnet/minecraft/text/Text;Lnet/minecraft/client/util/math/MatrixStack;Lnet/minecraft/client/render/VertexConsumerProvider;I)V",
			at = @At(value = "INVOKE", ordinal = 0,
			target = "Lnet/minecraft/client/util/math/MatrixStack;scale(FFF)V"))
	private MatrixStack rotateNameplate(MatrixStack matrixStack) {
		// ネームタグが描画されているときに呼ばれる
//		System.out.println("rotateNameplate called");
		if (Projection.getProjection().shouldRotateParticles()) {
			matrixStack.pop();
			MinecraftClient mc = MinecraftClient.getInstance();
			Entity camera = mc.cameraEntity;
			Vec3d cameraPos = camera.getPos().subtract(camera.prevX, camera.prevY, camera.prevZ).multiply(Projection.getTickDelta()).add(new Vec3d(camera.prevX, camera.prevY, camera.prevZ));
			Vec3d entityPos = new Vec3d(currentEntity.getX(), currentEntity.getY(), currentEntity.getZ()).subtract(new Vec3d(currentEntity.prevX, currentEntity.prevY, currentEntity.prevZ)).multiply(Projection.getTickDelta()).add(new Vec3d(currentEntity.prevX, currentEntity.prevY, currentEntity.prevZ));
			Vec3d dir = cameraPos.subtract(entityPos).normalize();
			Quaternion quaternion = new Quaternion(0, 0, 0, 1);
			quaternion.hamiltonProduct(Vec3f.POSITIVE_Y.getRadialQuaternion((float)Math.atan2(-dir.x, -dir.z)));
			quaternion.hamiltonProduct(Vec3f.POSITIVE_X.getRadialQuaternion((float)Math.asin(dir.y)));
			matrixStack.multiply(quaternion);
		}
		return matrixStack;
	}
}
