import { Button, ButtonProps, IconButtonProps } from "cs2/ui"
import styles from './ImageLabelButton.module.scss';
import { VanillaComponentResolver } from "vanillacomponentresolver";

export default (props: ButtonProps & Partial<IconButtonProps>) => {
    return (
        <Button 
            {...props} 
            variant="icon" 
            className={styles.button + ' ' + VanillaComponentResolver.instance.toolButtonTheme.button}>            
            {props.children}
        </Button>
    )
}