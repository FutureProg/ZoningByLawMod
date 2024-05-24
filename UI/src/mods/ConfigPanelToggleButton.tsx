import { useValue } from "cs2/api"
import { isConfigPanelOpen$, setConfigPanelOpen } from "./bindings"
import { Button } from "cs2/ui";
import { ModuleRegistryExtend } from "cs2/modding";

import styles from './configpaneltogglebutton.module.scss';

export const ConfigPanelToggleButton : ModuleRegistryExtend = (Component) => {
    return (props) => {
        const { children, ...otherProps } = props || {};
        let isPanelOpen = useValue(isConfigPanelOpen$);
        let onClick = () => {
            setConfigPanelOpen(!isPanelOpen);
        }
        return (
        <>
            <Component {...otherProps}>{children}</Component>
            <div className={styles.container}>
                <Button className={styles.button} variant="flat" onClick={onClick} >Zoning ByLaw Editor</Button>            
            </div>            
        </>            
        )
    }    
}