import { Button, FOCUS_AUTO, Scrollable } from "cs2/ui";
import styles from './ByLawDetailsPanel.module.scss';
import { selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor } from "./bindings";
import { trigger, useValue } from "cs2/api";
import { ChangeEvent, useEffect } from "react";
import { ByLawItem, ZoningByLawBinding } from "./types";
import { Color } from "cs2/bindings";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import { GetDefaultByLawItem, GetDefaultZoningByLawBinding, getConstraintTypes } from "./utils";
import ByLawPropertyView from "./components/Details/ByLawPropertyView";

import mod from '../../mod.json';

interface _Props {
    selectedRowIndex: number;
}
export const ByLawDetailsPanel = (props: _Props) => {    
    let byLawData = useValue(selectedByLawData$);    
    let byLawName = useValue(selectedByLawName$);
    let byLawColour = useValue(selectedByLawColor$);    

    
    console.log("Updated ByLawData: ", byLawData, props.selectedRowIndex);
    

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
        // let nItemData = [newByLawData?.blocks[0].itemData];
        if (byLawData) {
            let nData = {...byLawData}
            if (nData.blocks.length == 0) {
                nData.blocks = [...GetDefaultZoningByLawBinding().blocks];
                nData.blocks[0].itemData = [];
            }
            nData!.blocks[0].itemData.push({...GetDefaultByLawItem()});         
            setByLawData({...nData});            
        }        
    }

    let onDeleteProperty = (index: number) => () => {        
        console.log("Delete", byLawData);
        if (byLawData) {
            let nData = {...byLawData}
            nData.blocks = [...nData.blocks];            
            nData!.blocks[0].itemData = [...nData!.blocks[0].itemData.filter((_, idx) => idx != index)];
            trigger(mod.fullname, "SetByLawData", {...nData});
            console.log(byLawData);
        }
    }

    let onPropertyViewChange = (blockIndex: number, itemIndex: number) => (item: ByLawItem) => {
        if (byLawData) {
            let oldItem = byLawData.blocks[blockIndex].itemData[itemIndex];            
            let newItem = {
                ...item
            };
            if (oldItem.byLawItemType !== item.byLawItemType) {
                newItem = {
                    ...GetDefaultByLawItem(),
                    ...item,
                    constraintType: getConstraintTypes(item)[0]
                };                
            }
            let newData = {
                ...byLawData,                          
            } as ZoningByLawBinding;
            newData.blocks[blockIndex].itemData[itemIndex] = newItem; 
            setByLawData(newData);
            console.log("Property view changed data:", newData);            
        }
    }

    let propertyViews = [<></>];
    if (byLawData.blocks?.length > 0) {
        propertyViews = byLawData.blocks[0].itemData.map((item, index) => (        
            <ByLawPropertyView byLawItem={item} key={index + " " + item.byLawItemType} onDelete={onDeleteProperty(index)} onChange={onPropertyViewChange(0, index)} />
        ));
    }     
    
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
                            focusKey={FOCUS_AUTO}/>                                                                                                             
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