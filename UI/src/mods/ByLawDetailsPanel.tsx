import { Button, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED, Scrollable } from "cs2/ui"
import styles from './ByLawDetailsPanel.module.scss';
import { ZONE_BORDER_IDX, ZONE_COLOR_IDX, defaultColor, deleteByLaw, selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor, toggleByLawRenderPreview } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, ChangeEventHandler, forwardRef, useEffect, useImperativeHandle, useRef, useState } from "react";
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
export interface DetailsPanelRef {
    saveChanges: () => void;
}
export const ByLawDetailsPanel = forwardRef<DetailsPanelRef,_Props>((props, ref) => {    
    let byLawData = useValue(selectedByLawData$);    
    let byLawName = useValue(selectedByLawName$);
    let byLawColour = useValue(selectedByLawColor$);    
    let [newByLawData, updateNewByLawData] = useState<ZoningByLawBinding>();
    let [newByLawName, updateNewByLawName] = useState<string>();
    let [newByLawColor, updateNewByLawColor] = useState<Color>(defaultColor);
    let [newByLawBorder, updatenewByLawBorder] = useState<Color>(defaultColor);

    useImperativeHandle(ref, () => {  
        console.log("Update ref with new bylaw data");
        console.log(newByLawData);      
        return {
            saveChanges: () => {
                if (newByLawData != undefined) {
                    console.log("Will save the following");
                    console.log(newByLawData);
                    setByLawData(newByLawData);            
                }
                if (newByLawName != undefined && newByLawName !== byLawName && byLawName.length > 0) {
                    setByLawName(newByLawName!);
                }
                if (newByLawColor != undefined && (newByLawColor != byLawColour[0] || newByLawBorder != byLawColour[1])) {
                    setByLawZoneColor(newByLawColor, newByLawBorder);
                }
            }
        };
    }, [newByLawData, newByLawName, newByLawColor, newByLawBorder]);

    useEffect(() => {                
        updateNewByLawData(byLawData);
    }, [byLawData]);
    useEffect(() => {
        updateNewByLawName(byLawName);
    }, [byLawName]);
    useEffect(() => {
        updateNewByLawColor(byLawColour[ZONE_COLOR_IDX]);
        updatenewByLawBorder(byLawColour[ZONE_BORDER_IDX]);
    }, [byLawColour]);

    const onUpdateByLawColor = (idx: number) => (color: Color) => {
        if (idx == ZONE_COLOR_IDX) {
            updateNewByLawColor(color);
        }
        else if (idx == ZONE_BORDER_IDX) {
            updatenewByLawBorder(color);
        }        
    }

    const onUpdateZoneType = (newType: number) => {        
        updateNewByLawData({
            ...newByLawData!,
            // zoneType: newType
        });
    }
    
    const onUpdateBounds = (name: string, newValue: Bounds1) => {
        let newData = {...newByLawData} as any;
        newData[name] = newValue;
        updateNewByLawData(newData);
    }

    const onNameChange = (e: ChangeEvent<HTMLInputElement>) => {
        updateNewByLawName(e.target.value);        
    }
    
    const onDelete = () => {
        deleteByLaw();
        if (props.onDelete) {
            props.onDelete();
        }
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
        if (newByLawData) {
            let nData = {...newByLawData}
            if (nData.blocks.length == 0) {
                nData.blocks = [...GetDefaultZoningByLawBinding().blocks];
                nData.blocks[0].itemData = [];
            }
            nData!.blocks[0].itemData.push({...GetDefaultByLawItem()});         
            updateNewByLawData({...nData});
        }        
    }

    const onDeleteProperty = (index: number) => () => {
        if (newByLawData) {
            let nData = {...newByLawData}
            nData!.blocks[0].itemData = nData?.blocks[0].itemData.filter((_, idx, _1) => idx != index);
            updateNewByLawData(nData);
        }
    }

    let onPropertyViewChange = (blockIndex: number, itemIndex: number) => (item: ByLawItem) => {
        if (newByLawData) {
            let oldItem = newByLawData.blocks[blockIndex].itemData[itemIndex];            
            let newItem = {
                ...item
            };
            if (oldItem.byLawItemType !== item.byLawItemType) {
                newItem = {
                    ...GetDefaultByLawItem(),
                    ...item,
                    constraintType: getConstraintTypes(item)[0]
                };                
                // newItem.valueBounds1 = {min: 0, max: 0};
                // newItem.valueByteFlag = 0;
                // newItem.valueNumber = 0;
                // newItem.propertyOperator = ByLawPropertyOperator.AtLeastOne;
            }
            let newData = {
                ...newByLawData,                          
            } as ZoningByLawBinding;
            newData.blocks[blockIndex].itemData[itemIndex] = newItem; 
            updateNewByLawData(newData);
            console.log("Property view changed data:");
            console.log(newData);
        }
    }
    console.log(newByLawData);
    
    let propertyViews = newByLawData && newByLawData.blocks? newByLawData?.blocks[0]?.itemData.map((item, index) => {
        return (
            <ByLawPropertyView byLawItem={item} key={index} onDelete={onDeleteProperty(index)} onChange={onPropertyViewChange(0, index)} />
        )
    }) : <></>;
    
    return (
        <div className={styles.bylawDetails}>
            <Scrollable>   
                <div style={{display: props.selectedRowIndex == -1? 'none': 'block'}}>
                    <div className={styles.byLawDetailsTable}>
                        <tr>                            
                            <td><VanillaComponentResolver.instance.EllipsisTextInput 
                                value={newByLawName} 
                                onChange={onNameChange} 
                                maxLength={84} 
                                placeholder="ByLaw Name" 
                                vkTitle="ByLaw Name"
                                className={styles.nameInput}
                                focusKey={FOCUS_AUTO}/>                                
                            </td>                            
                            {/* <td><input type="text" value={newByLawName} onChange={onNameChange}/></td> */}
                        </tr>
                        <VanillaComponentResolver.instance.ColorField value={newByLawColor} onChange={onUpdateByLawColor(ZONE_COLOR_IDX)}/>                        
                        <h2>Constraints</h2>
                        {propertyViews}
                        <Button focusKey={FOCUS_AUTO} variant="flat" onSelect={onAddProperty} theme={addPropertyTheme} >Add Constraint</Button>                        
                        {/* <tr>
                            <Button onClick={toggleByLawRenderPreview}>Preview (very much WIP)</Button>
                        </tr> */}                                      
                    </div>                               
                </div>            
            </Scrollable>
            {/* <div style={{display: props.selectedRowIndex == -1? 'none': 'block', marginTop: '8rem'}}>
                <Button onClick={onSave} variant="flat" style={{marginBottom: '16rem'}}>Save</Button>                
                <Button onClick={onDelete} variant="flat" style={{backgroundColor: 'red'}}>Delete</Button>
            </div>             */}
        </div>
    )
});