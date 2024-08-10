import { ModRegistrar } from "cs2/modding";
import { ConfigPanelToggleButton } from "mods/ConfigPanelToggleButton";
import { ModView } from "mods/ModView/ModView";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    //game-ui/game/components/asset-menu/asset-category-tab-bar/asset-category-tab-bar.tsx, AssetCategoryTabBar
    //game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx
    moduleRegistry.extend("game-ui/game/components/tool-options/mouse-tool-options/mouse-tool-options.tsx", "MouseToolOptions", ConfigPanelToggleButton);
    moduleRegistry.append('Game', ModView);
}

export default register;