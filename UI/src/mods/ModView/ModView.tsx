import { SidePanel } from 'mods/SidePanel/SidePanel';
import styles from './ModView.module.scss';
import { useValue } from 'cs2/api';
import { isConfigPanelOpen$ } from 'mods/bindings';

export const ModView = () => {
    let isModOpen = useValue(isConfigPanelOpen$);
    if (!isModOpen) {
        return (<></>);
    }
    return (
        <div className={styles.view}>
            <SidePanel />
        </div>
    )
}