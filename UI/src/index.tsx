import { ModRegistrar } from "cs2/modding";
import { MainPanel } from "mods/MainPanel";

const register: ModRegistrar = (moduleRegistry) => {

    moduleRegistry.append('Game', MainPanel);

}

export default register;