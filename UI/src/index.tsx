import { ModRegistrar } from "cs2/modding";
import { MainModToggle } from "mods/MainModToggle";
import { ModView } from "mods/ModView/ModView";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    //game-ui/game/components/asset-menu/asset-category-tab-bar/asset-category-tab-bar.tsx, AssetCategoryTabBar
    //game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx
    moduleRegistry.append('Game', ModView);
    moduleRegistry.append("GameTopLeft", MainModToggle);
}

export default register;