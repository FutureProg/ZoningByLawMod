import { useValue } from "cs2/api"
import { isConfigPanelOpen$, setConfigPanelOpen } from "./bindings"
import { Button, FOCUS_AUTO, FOCUS_DISABLED } from "cs2/ui";
import { ModuleRegistryExtend } from "cs2/modding";
import { tool, toolbar } from "cs2/bindings";

import styles from './configpaneltogglebutton.module.scss';
import { VanillaComponentResolver } from "vanillacomponentresolver";

export const ConfigPanelToggleButton : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps } = props || {};
        let activeTool = useValue(tool.activeTool$);
        let isPanelOpen = useValue(isConfigPanelOpen$);
        let onClick = () => {
            setConfigPanelOpen(!isPanelOpen);
        }
        let button = activeTool.id != tool.ZONE_TOOL? <></> : (
            <VanillaComponentResolver.instance.Section>
                <Button className={styles.button + ' ' + VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={FOCUS_DISABLED} variant="flat" onClick={onClick}>
                    <img src="coui://trejak_zbl/config-icon.svg" />
                    Zoning ByLaw Editor
                </Button>            
            </VanillaComponentResolver.instance.Section>
        );                
        return (
        <>            
            {button}
            <Component {...otherProps}>                            
                {children}
            </Component>                        
        </>            
        )
    }    
}