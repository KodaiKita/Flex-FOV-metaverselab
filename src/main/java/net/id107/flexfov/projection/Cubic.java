package net.id107.flexfov.projection;

import net.id107.flexfov.BufferManager;
import net.id107.flexfov.Reader;
import net.minecraft.client.MinecraftClient;
import net.minecraft.client.util.math.MatrixStack;
import net.minecraft.util.math.Matrix4f;
import net.minecraft.util.math.Quaternion;
import org.lwjgl.opengl.GL11;

public class Cubic extends Projection {

	// ratio of sides of a cube
	public static float heightRatio = 2.714f;
	public static float widthRatio = 5.841f;
	public static float depthRatio = 7.298f;

	@Override
	public String getFragmentShader() {
		return Reader.read("flexfov:shaders/cubic.fs");
	}
	
	@Override
	public double getFovX() {
		return 360;
	}
	
	@Override
	public double getFovY() {
		return 180;
	}

	@Override
	public void rotateCamera(MatrixStack matrixStack) {
		System.out.println("rotateCamera called: renderPass = " + renderPass);
		Matrix4f matrix;
		switch (renderPass) {
			case 0:
				break;
			case 1:
				matrix = new Matrix4f(new Quaternion(0, 0.707106781f, 0, 0.707106781f)); // look right
				matrixStack.peek().getModel().multiply(matrix);
				break;
			case 2:
				matrix = new Matrix4f(new Quaternion(0, -0.707106781f, 0, 0.707106781f)); // look left
				matrixStack.peek().getModel().multiply(matrix);
				break;
			case 3:
				matrix = new Matrix4f(new Quaternion(0.707106781f, 0, 0, 0.707106781f)); // look down
				matrixStack.peek().getModel().multiply(matrix);
				break;
		}
	}

	@Override
	public void saveRenderPass() {
		if (renderPass > 3) return;
		super.saveRenderPass();
	}

	@Override
	public void renderWorld(float tickDelta, long startTime, boolean tick) {
		MinecraftClient mc = MinecraftClient.getInstance();
		Projection.tickDelta = tickDelta;
		int displayWidth = mc.getWindow().getWidth();
		int displayHeight = mc.getWindow().getHeight();
		hudHidden = mc.options.hudHidden;

		if (BufferManager.getFramebuffer() == null) {
			BufferManager.createFramebuffer();
			shader.createShaderProgram(getProjection());
			screenWidth = displayWidth;
			screenHeight = displayHeight;
		}

		if (screenWidth != displayWidth || screenHeight != displayHeight) {
			shader.deleteShaderProgram();
			BufferManager.deleteFramebuffer();
			BufferManager.createFramebuffer();
			shader.createShaderProgram(getProjection());
			screenWidth = displayWidth;
			screenHeight = displayHeight;
		}
		// 正面以外のブロック?
		if (Math.max(getFovX(), getFovY()) > 90 || zoom < 0) {
			for (renderPass = 1; renderPass < 4; renderPass++) {
				GL11.glViewport(0, 0, displayWidth, displayHeight);
				mc.worldRenderer.scheduleTerrainUpdate();
				mc.gameRenderer.renderWorld(tickDelta, startTime, new MatrixStack());
				saveRenderPass();
			}
		}
		// 正面のブロック?
		renderPass = 0;
		GL11.glViewport(0, 0, displayWidth, displayHeight);
		mc.worldRenderer.scheduleTerrainUpdate();

		mc.options.hudHidden = hudHidden;
	}
}
