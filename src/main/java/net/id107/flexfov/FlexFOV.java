package net.id107.flexfov;

import net.fabricmc.api.ClientModInitializer;
import net.id107.flexfov.command.FlexFovCommand;

public class FlexFOV implements ClientModInitializer {

	@Override
	public void onInitializeClient() {
		ConfigManager.loadConfig();
		FlexFovCommand.register();
		System.out.println("FlexFOV loaded");
	}
}
