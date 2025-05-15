package net.id107.flexfov.command;

import com.mojang.brigadier.CommandDispatcher;
import com.mojang.brigadier.arguments.FloatArgumentType;
import com.mojang.brigadier.arguments.StringArgumentType;
import net.fabricmc.fabric.api.client.command.v1.ClientCommandManager;
import net.fabricmc.fabric.api.client.command.v1.FabricClientCommandSource;
import net.id107.flexfov.ConfigManager;
import net.id107.flexfov.projection.Projection;
import net.id107.flexfov.util.ActionBar;

import static net.fabricmc.fabric.api.client.command.v1.ClientCommandManager.argument;
import static net.fabricmc.fabric.api.client.command.v1.ClientCommandManager.literal;

public class FlexFovCommand {

    public static void register() {
        // v1ではClientCommandManagerを直接使用してコマンドを登録します
        registerCommands(ClientCommandManager.DISPATCHER);
    }

    private static void registerCommands(CommandDispatcher<FabricClientCommandSource> dispatcher) {
        dispatcher.register(literal("flexfov")
                .then(literal("ab")
                        .executes(context -> {
                            // LOG TO CONSOLE
                            System.out.println("Test Command");
                            ActionBar.sendActionBarMessage("Test Command");
                            return 1;
                        })));
    }
}