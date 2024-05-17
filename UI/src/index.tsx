import { ModRegistrar } from "cs2/modding";
import { ConfigPanelToggleButton } from "mods/ConfigPanelToggleButton";
import { MainPanel } from "mods/MainPanel";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    moduleRegistry.append('Game', MainPanel);
    moduleRegistry.append('GameTopRight', ConfigPanelToggleButton);
}

export default register;