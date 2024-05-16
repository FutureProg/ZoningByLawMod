import { Button, Panel, Scrollable } from "cs2/ui";
import styles from './mainpanel.module.scss';
import { useValue } from "cs2/api";
import { byLawZoneList$ } from "./bindings";
import { ByLawZoneListItem } from "./types";
import { useState } from "react";
import { ByLawDetailsPanel } from "./ByLawDetailsPanel";

export const MainPanel = () => {
    // This is a void component that does not output anynthing.
    // Cities: Skylines 2 UI is built with React and mods support outputting standard
    // React JSX elements!
    const onClose = () => {}
    let [selectedListItem, setSelectedListItem] = useState(-1);
    let byLawZoneList = useValue(byLawZoneList$);    
    const testZoneList : ByLawZoneListItem[] = [...Array(20)].map((_, idx) => {return {name: (idx+1) + ": Zone", entity: {index: idx, version: -1}}})
    const listItems = testZoneList.map((item : ByLawZoneListItem, idx) => 
        <div 
        className={styles.bylawListItem + " " + (selectedListItem == item.entity.index? styles.selected : "")} 
        key={item.entity.index}
        onClick={() => {setSelectedListItem(item.entity.index)}}
        >
            {item.name}
        </div>
    );

    return (
        <Panel className={styles.mainPanel} draggable={true} header={"Zoning ByLaws"} onClose={onClose} contentClassName={styles.mainPanelContentContainer}>            
            <div className={styles.mainPanelContent}>
                <div className={styles.mainPanelTopBar}>
                    <Button>Add New ByLaw</Button>
                </div>
                <div className={styles.mainPanelSections}>
                    <Scrollable className={styles.bylawList}>
                        {listItems}
                    </Scrollable>
                    <ByLawDetailsPanel/>
                </div>
                <div className={styles.mainPanelBottomBar}>
                    <Button>Close</Button>
                </div>
            </div>                            
        </Panel>
    );
}