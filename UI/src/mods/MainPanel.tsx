import { Button, FOCUS_AUTO, Panel, Scrollable } from "cs2/ui";
import styles from './mainpanel.module.scss';
import { useValue } from "cs2/api";
import { byLawZoneList$, createNewByLaw, isConfigPanelOpen$, setActiveByLaw, setConfigPanelOpen } from "./bindings";
import { ByLawZoneListItem } from "./types";
import { useEffect, useRef, useState } from "react";
import { ByLawDetailsPanel, DetailsPanelRef } from "./ByLawDetailsPanel";
import { Entity, toolbar } from "cs2/bindings";
import { GetDefaultByLawComponent, GetDefaultZoningByLawBinding } from "./utils";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import ImageLabelButton from "./atoms/ImageLabelButton";

export const MainPanel = () => {   
    let detailPanelRef = useRef<DetailsPanelRef>(null);

    const onClose = () => {
        setActiveByLaw({index: 0, version: 0});        
        setSelectedListItem(-1);        
        setConfigPanelOpen(false);
    }    
    const selectedAsset = useValue(toolbar.selectedAsset$);    
    let [prevSelectedAsset, setPrevSelectedAsset] = useState<Entity>({index: 0, version: 0});       
    useEffect(() => {
        if (selectedAsset.index > 0) {
            setPrevSelectedAsset(selectedAsset);
            console.log("New Asset Selected, id: " + selectedAsset.index);        
        }        
    }, [selectedAsset]) 

    const isPanelOpen = useValue(isConfigPanelOpen$);
    useEffect(() => {
        if (isPanelOpen) {
            toolbar.clearAssetSelection();
        } else {          
            toolbar.selectAsset(prevSelectedAsset);
            setPrevSelectedAsset({index: 0, version: 0});           
        }
    }, [isPanelOpen]);    
    

    let [selectedListItem, setSelectedListItem] = useState(-1);
    const listItemOnClick = (entity: Entity) => () => {
        setSelectedListItem(entity.index);
        setActiveByLaw(entity);
    }

    let byLawZoneList = useValue(byLawZoneList$);    
    const testZoneList : ByLawZoneListItem[] = [...Array(20)].map((_, idx) => {return {name: (idx+1) + ": Zone", entity: {index: idx, version: -1}}})
    const listItems = byLawZoneList.map((item : ByLawZoneListItem, idx) => 
        <div 
        className={styles.bylawListItem + " " + (selectedListItem == item.entity.index? styles.selected : "")} 
        key={item.entity.index}
        onClick={listItemOnClick(item.entity)}
        >
            {item.name}
        </div>
    );

    const onCreateNewByLaw = () => {
        const baseByLaw = GetDefaultZoningByLawBinding();
        createNewByLaw(baseByLaw);
    }

    const onDeleteByLaw = () => {
        setSelectedListItem(-1);
        toolbar.selectAsset({index: 0, version: 0});
    }

    let [x, setX] = useState<string | undefined | null>("");

    let topRightSection = (
        <>            
            <Button 
                onSelect={detailPanelRef.current?.saveChanges}
                className={styles.saveButton} 
                focusKey={FOCUS_AUTO} 
                src="Media/Glyphs/Save.svg"
                variant="icon"/>
        </>
    )

    return !isPanelOpen? <></> : (
        <Panel className={styles.mainPanel} draggable={false} header={"Zoning ByLaws"} onClose={onClose} contentClassName={styles.mainPanelContentContainer}>            
            <div className={styles.mainPanelContent}>
                <div className={styles.mainPanelTopBar}>
                    <div className={styles.topLeftSection}>
                        <ImageLabelButton 
                            src="coui://uil/Standard/Plus.svg"
                            onClick={onCreateNewByLaw}>                                                
                            Add New ByLaw
                        </ImageLabelButton>                                        
                    </div>   
                    <div className={styles.topRightSection}>
                        { selectedListItem >= 0? topRightSection: <></>}
                        
                    </div>                 
                </div>
                <div className={styles.mainPanelSections}>
                    <Scrollable className={styles.bylawList}>
                        {listItems}
                    </Scrollable>
                    <ByLawDetailsPanel ref={detailPanelRef} selectedRowIndex={selectedListItem} onDelete={onDeleteByLaw}/>
                </div>
                <div className={styles.mainPanelBottomBar}>
                </div>
            </div>                                        
        </Panel>
    );
}