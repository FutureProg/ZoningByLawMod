import { Button, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED, Scrollable } from "cs2/ui"
import styles from './ByLawDetailsPanel.module.scss';
import { ZONE_BORDER_IDX, ZONE_COLOR_IDX, defaultColor, deleteByLaw, selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor, toggleByLawRenderPreview } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, ChangeEventHandler, forwardRef, useCallback, useEffect, useImperativeHandle, useMemo, useRef, useState } from "react";
import { ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator, ByLawZoneComponent, ByLawZoneType, ZoningByLawBinding } from "./types";
import { Bounds1, Color } from "cs2/bindings";
import { Dropdown } from "cs2/ui";
import { ColorHSV, VanillaComponentResolver } from "vanillacomponentresolver";
import { GetDefaultByLawItem, GetDefaultZoningByLawBinding, getConstraintTypes, rgbaToHex } from "./utils";
import { Bounds1Field } from "./components/Bounds1Field";
import ByLawPropertyView from "./components/Details/ByLawPropertyView";

interface _Props {
    selectedRowIndex: number;
    onDelete?: () => void;
}
export const ByLawDetailsPanel = (props: _Props) => {    
    let byLawData = useValue(selectedByLawData$);    
    let byLawName = useValue(selectedByLawName$);
    let byLawColour = useValue(selectedByLawColor$);    

    useEffect(() => {
        console.log("Updated ByLawData: ", byLawData, props.selectedRowIndex);
    }, [byLawData]);

    const onUpdateByLawColor = (color: Color) => {        
        setByLawZoneColor(color, color);
    }
    const onNameChange = (e: ChangeEvent<HTMLInputElement>) => {
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
    
    const onAddProperty = () => {
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

    const onDeleteProperty = (index: number) => () => {
        if (byLawData) {
            let nData = {...byLawData}
            nData!.blocks[0].itemData = nData?.blocks[0].itemData.filter((_, idx, _1) => idx != index);
            setByLawData(nData);
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

    let propertyViews = byLawData && byLawData.blocks? byLawData?.blocks[0]?.itemData.map((item, index) => {
        console.log("RENDER");
        return (
            <ByLawPropertyView byLawItem={item} key={index} onDelete={onDeleteProperty(index)} onChange={onPropertyViewChange(0, index)} />
        )
    }) : <></>;
    
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