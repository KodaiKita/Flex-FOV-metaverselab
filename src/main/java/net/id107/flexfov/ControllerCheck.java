package net.id107.flexfov;

import net.fabricmc.api.ClientModInitializer;
import net.fabricmc.fabric.api.client.event.lifecycle.v1.ClientTickEvents;

public class ControllerCheck implements ClientModInitializer {
    private Controller controller;

    @Override
    public void onInitializeClient() {
        controller = new Controller();
        controller.initialize();
        System.out.println("ControllerCheck initialized");

        // クライアントtickごとに呼ばれるイベントに登録
        ClientTickEvents.END_CLIENT_TICK.register(client -> {
            controller.controllerTick(client);
        });
    }
}
