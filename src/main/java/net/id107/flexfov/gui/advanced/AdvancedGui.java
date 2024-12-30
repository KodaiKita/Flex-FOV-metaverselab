package net.id107.flexfov.gui.advanced;

import net.id107.flexfov.ConfigManager;
import net.id107.flexfov.gui.SettingsGui;
import net.id107.flexfov.projection.Projection;
import net.minecraft.client.gui.screen.Screen;
import net.minecraft.client.gui.widget.ButtonWidget;
import net.minecraft.client.option.DoubleOption;
import net.minecraft.text.LiteralText;

public class AdvancedGui extends SettingsGui {

	public static int currentGui = 0;
	
	public AdvancedGui(Screen parent) {
		super(parent);
	}
	
	public static AdvancedGui getGui(Screen parent) {
		return new CubicGui(parent);
	}
	
	@Override
	protected void init() {
		super.init();
		
		ButtonWidget button = new ButtonWidget(width / 2 - 180, height / 6 + 12, 100, 20,
				new LiteralText("Cubic"), (buttonWidget) -> {
					currentGui = 0;
					client.openScreen(new CubicGui(parentScreen));
		});
		if (this instanceof CubicGui) {
			button.active = false;
		}
		addButton(button);
		
		if (!(this instanceof CubicGui)) {
			DoubleOption zoom = new DoubleOption("zoom", -2, 2, 0.05f,
					(gameOptions) -> {return (double) Projection.zoom;},
					(gameOptions, number) -> {Projection.zoom = (float)(double)number; ConfigManager.saveConfig();},
					(gameOptions, doubleOption) -> {return new LiteralText(String.format("Zoom: %.2f", Projection.zoom));});
			addButton(zoom.createButton(client.options, width / 2 + 5, height / 6 + 84, 150));
		}

		// Resize GUI Button
		addButton(new ButtonWidget(width / 2 + 5, height / 6 + 108, 150, 20,
				new LiteralText("Resize Gui: " + (Projection.resizeGui ? "ON" : "OFF")), (buttonWidget) -> {
					Projection.resizeGui = !Projection.resizeGui;
					buttonWidget.setMessage(new LiteralText("Resize Gui: " + (Projection.resizeGui ? "ON" : "OFF")));
				}));
	}
}
