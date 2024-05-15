import { Button, Panel, Scrollable } from "cs2/ui";
import styles from './mainpanel.module.scss';

export const MainPanel = () => {
    // This is a void component that does not output anynthing.
    // Cities: Skylines 2 UI is built with React and mods support outputting standard
    // React JSX elements!
    const onClose = () => {}

    return (
        <Panel className={styles.mainPanel} draggable={true} header={"Zoning ByLaws"} onClose={onClose} contentClassName={styles.mainPanelContentContainer}>            
            <div className={styles.mainPanelContent}>
                <div className={styles.mainPanelTopBar}>
                    <Button>Add New ByLaw</Button>
                </div>
                <div className={styles.mainPanelSections}>
                    <Scrollable className={styles.bylawList}>
                        ByLaw List
                    </Scrollable>
                    <Scrollable className={styles.bylawDetails}>   
                        ByLaw Details                     
                    </Scrollable>
                </div>
                <div className={styles.mainPanelBottomBar}>
                    <Button>Close</Button>
                </div>
            </div>                            
        </Panel>
    );
}