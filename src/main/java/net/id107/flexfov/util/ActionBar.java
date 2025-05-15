package net.id107.flexfov.util;

import net.minecraft.client.MinecraftClient;
import net.minecraft.text.Text;

public class ActionBar {
    public static void sendActionBarMessage(String message) {
        MinecraftClient client = MinecraftClient.getInstance();
        if (client.player != null) {
            client.player.sendMessage(Text.of(message), true);
        }
    }
}