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
		return Reader.read("flexfov:shaders/metaverselab.fs");
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
