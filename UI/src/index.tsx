import { ModRegistrar } from "cs2/modding";
import { MainPanel } from "mods/MainPanel";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    moduleRegistry.append('Game', MainPanel);

}

export default register;