import { FOCUS_DISABLED } from "cs2/input"
import { Button } from "cs2/ui"
import { toggleTool } from "./bindings"

export const MainModToggle = () => {
    return (
        <Button 
            onSelect={toggleTool}
            focusKey={FOCUS_DISABLED}            
            variant="floating">
                <img src="coui://trejak_zbl/config-icon.svg" />
        </Button>
    );
}