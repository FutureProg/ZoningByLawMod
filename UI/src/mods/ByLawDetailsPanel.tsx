import { Button, FOCUS_AUTO, FOCUS_DISABLED, Scrollable } from "cs2/ui";
import styles from './ByLawDetailsPanel.module.scss';
import { selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, useEffect, useState } from "react";
import { ByLawItem, ZoningByLawBinding } from "./types";
import { Color } from "cs2/bindings";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { GetDefaultByLawItem, GetDefaultZoningByLawBinding, deepCopy, getConstraintTypes } from "./utils";
import ByLawPropertyView from "./components/Details/ByLawPropertyView";


interface _Props {
    selectedRowIndex: number;
}
export const ByLawDetailsPanel = (props: _Props) => {    
    let byLawData = useValue(selectedByLawData$);        
    let byLawName = useValue(selectedByLawName$);
    let byLawColour = useValue(selectedByLawColor$); 
    let itemData = byLawData.blocks?.length > 0 && byLawData.blocks[0].itemData? [...byLawData.blocks[0].itemData] : [];          

    let onUpdateByLawColor = (color: Color) => {        
        setByLawZoneColor(color, color);
    }
    let onNameChange = (e: ChangeEvent<HTMLInputElement>) => {
        setByLawName(e.target.value);
    }
    
    let ellipseTextInputTheme = VanillaComponentResolver.instance.ellipsesTextInputTheme;
    let nameInputTheme = {                
        ...ellipseTextInputTheme,
        ellipsesTextInput: ellipseTextInputTheme.ellipsesTextInput + ' ' + styles.nameInput
    };

    const addPropertyTheme = {
        button: styles.addPropertyButton
    }
    
    let onAddProperty = () => {
        if (byLawData) {
            let nData = deepCopy<ZoningByLawBinding>(byLawData);
            if (nData.blocks.length == 0) {
                nData.blocks = [...GetDefaultZoningByLawBinding().blocks];
                nData.blocks[0].itemData = [];
            }
            nData!.blocks[0].itemData.push({...GetDefaultByLawItem()});         
            setByLawData({...nData});
        }        
    }

    let onDeleteProperty = (index: number) => () => {                
        if (byLawData) {
            let nData = deepCopy(byLawData);
            nData.blocks = [...nData.blocks];            
            nData!.blocks[0].itemData = [...nData!.blocks[0].itemData.filter((_, idx) => idx != index)];
            setByLawData({...nData});
        }
    }

    let onPropertyViewChange = (blockIndex: number, itemIndex: number) => (item: ByLawItem) => {
        if (byLawData) {
            let oldItem = byLawData.blocks[blockIndex].itemData[itemIndex];            
            let newItem = deepCopy(item);
            if (oldItem.byLawItemType !== item.byLawItemType) {
                newItem = {
                    ...GetDefaultByLawItem(),
                    ...item,
                    constraintType: getConstraintTypes(item.byLawItemType)[0]
                };                
            }
            let newData = deepCopy<ZoningByLawBinding>(byLawData);
            newData.blocks[blockIndex].itemData[itemIndex] = newItem; 
            setByLawData(newData);   
        }
    }
    
    let propertyViews = itemData.map((item, index) => (        
        <ByLawPropertyView byLawItem={item} key={index} onDelete={onDeleteProperty(index)} onChange={onPropertyViewChange(0, index)} />
    ));  
    
    return (
        <div className={styles.bylawDetails}>
            <Scrollable>   
                <div style={{display: props.selectedRowIndex == -1? 'none': 'block'}}>
                    <div className={styles.byLawDetailsTable}>                        
                        <VanillaComponentResolver.instance.EllipsisTextInput 
                            value={byLawName} 
                            onChange={onNameChange} 
                            maxLength={84} 
                            placeholder="ByLaw Name" 
                            vkTitle="ByLaw Name"
                            className={styles.nameInput}
                            focusKey={FOCUS_DISABLED}/>                                                                                                             
                        <VanillaComponentResolver.instance.ColorField value={byLawColour[0]} onChange={onUpdateByLawColor}/>                        
                        <h2>Constraints</h2>
                        {propertyViews}
                        <Button focusKey={FOCUS_AUTO} variant="flat" onSelect={onAddProperty} theme={addPropertyTheme} >Add Constraint</Button>                                                          
                    </div>                               
                </div>            
            </Scrollable>
        </div>
    )
};