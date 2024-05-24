import { useValue } from "cs2/api"
import { isConfigPanelOpen$, setConfigPanelOpen } from "./bindings"
import { Button } from "cs2/ui";
import { ModuleRegistryExtend } from "cs2/modding";

import styles from './configpaneltogglebutton.module.scss';
import { VanillaComponentResolver } from "vanillacomponentresolver";

export const ConfigPanelToggleButton : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps } = props || {};
        let isPanelOpen = useValue(isConfigPanelOpen$);
        let onClick = () => {
            setConfigPanelOpen(!isPanelOpen);
        }
        return (
        <>            
            <VanillaComponentResolver.instance.Section>
                <Button className={styles.button + ' ' + VanillaComponentResolver.instance.toolButtonTheme.button} variant="flat" onClick={onClick} >Zoning ByLaw Editor</Button>            
            </VanillaComponentResolver.instance.Section>
            <Component {...otherProps}>                            
                {children}
            </Component>                        
        </>            
        )
    }    
}