import { useValue } from "cs2/api"
import { isConfigPanelOpen$, setConfigPanelOpen } from "./bindings"
import { Button } from "cs2/ui";

export const ConfigPanelToggleButton = () => {
    let isPanelOpen = useValue(isConfigPanelOpen$);
    let onClick = () => {
        setConfigPanelOpen(!isPanelOpen);
    }
    return (<Button onClick={onClick} variant="floating">ZBL</Button>)
}