package net.id107.flexfov.projection;

import net.id107.flexfov.BufferManager;
import net.id107.flexfov.Reader;
import net.minecraft.client.MinecraftClient;
import net.minecraft.client.util.Window;
import net.minecraft.client.util.math.MatrixStack;
import net.minecraft.util.math.Matrix4f;
import net.minecraft.util.math.Quaternion;
import org.lwjgl.opengl.GL11;
import org.lwjgl.opengl.GL20;

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

	public void loadUniforms(float tickDelta) {
		MinecraftClient mc = MinecraftClient.getInstance();
		int shaderProgram = shader.getShaderProgram();
		int displayWidth = MinecraftClient.getInstance().getWindow().getWidth();
		int displayHeight = MinecraftClient.getInstance().getWindow().getHeight();
		GL20.glUseProgram(shaderProgram);

		int aaUniform = GL20.glGetUniformLocation(shaderProgram, "antialiasing");
		GL20.glUniform1i(aaUniform, getAntialiasing());
		int pixelOffestUniform;
		System.out.println("[ANTI-ALIASING] " + getAntialiasing());

		float left = (-1f+0.25f)/displayWidth;
		float top = (-1f+0.25f)/displayHeight;
		float right = 0.5f/displayWidth;
		float bottom = 0.5f/displayHeight;
		for (int y = 0; y < 4; y++) {
			for (int x = 0; x < 4; x++) {
				pixelOffestUniform = GL20.glGetUniformLocation(shaderProgram, "pixelOffset[" + (y*4+x) + "]");
				GL20.glUniform2f(pixelOffestUniform, left + right*x, top + bottom*y);
			}
		}

		int texUniform;

		texUniform = GL20.glGetUniformLocation(shaderProgram, "texFront");
		GL20.glUniform1i(texUniform, 0);

		texUniform = GL20.glGetUniformLocation(shaderProgram, "texLeft");
		GL20.glUniform1i(texUniform, 2);

		texUniform = GL20.glGetUniformLocation(shaderProgram, "texRight");
		GL20.glUniform1i(texUniform, 1);

		texUniform = GL20.glGetUniformLocation(shaderProgram, "texBottom");
		GL20.glUniform1i(texUniform, 3);

		int fovxUniform = GL20.glGetUniformLocation(shaderProgram, "fovx");
		GL20.glUniform1f(fovxUniform, (float) getFovX());

		int fovyUniform = GL20.glGetUniformLocation(shaderProgram, "fovy");
		GL20.glUniform1f(fovyUniform, (float) getFovY());

		int backgroundUniform = GL20.glGetUniformLocation(shaderProgram, "backgroundColor");

		// Black background
		GL20.glUniform4f(backgroundUniform, 0, 0, 0, 1);

		int zoomUniform = GL20.glGetUniformLocation(shaderProgram, "zoom");
		GL20.glUniform1f(zoomUniform, (float)Math.pow(2, -zoom));

		int drawCursorUniform = GL20.glGetUniformLocation(shaderProgram, "drawCursor");
		GL20.glUniform1i(drawCursorUniform, (getResizeGui() && mc.currentScreen != null) ? 1 : 0);
		int cursorPosUniform = GL20.glGetUniformLocation(shaderProgram, "cursorPos");
		Window window = mc.getWindow();
		float mouseX = (float)mc.mouse.getX() / (float)window.getWidth();
		float mouseY = (float)mc.mouse.getY() / (float)window.getHeight();
		mouseX = (mouseX - 0.5f) * window.getWidth() / (float)window.getHeight() + 0.5f;
		mouseX = Math.max(0, Math.min(1, mouseX));
		GL20.glUniform2f(cursorPosUniform, mouseX, 1-mouseY);
	}
}
