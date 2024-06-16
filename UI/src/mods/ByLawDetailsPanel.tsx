import { Button, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED, Scrollable } from "cs2/ui"
import styles from './ByLawDetailsPanel.module.scss';
import { ZONE_BORDER_IDX, ZONE_COLOR_IDX, defaultColor, deleteByLaw, selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor, toggleByLawRenderPreview } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, ChangeEventHandler, useEffect, useRef, useState } from "react";
import { ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator, ByLawZoneComponent, ByLawZoneType, ZoningByLawBinding } from "./types";
import { Bounds1, Color } from "cs2/bindings";
import { Dropdown } from "cs2/ui";
import { ColorHSV, VanillaComponentResolver } from "vanillacomponentresolver";
import { GetDefaultByLawItem, rgbaToHex } from "./utils";
import { Bounds1Field } from "./components/Bounds1Field";
import ByLawPropertyView from "./components/Details/ByLawPropertyView";

export const ByLawDetailsPanel = (props: {selectedRowIndex: number, onDelete?: () => void}) => {    
    let byLawData = useValue(selectedByLawData$);    
    let byLawName = useValue(selectedByLawName$);
    let byLawColour = useValue(selectedByLawColor$);    
    let [newByLawData, updateNewByLawData] = useState<ZoningByLawBinding>();
    let [newByLawName, updateNewByLawName] = useState<string>();
    let [newByLawColor, updateNewByLawColor] = useState<Color>(defaultColor);
    let [newByLawBorder, updatenewByLawBorder] = useState<Color>(defaultColor);

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

    const onSave = () => {
        if (newByLawData != undefined) {
            setByLawData(newByLawData!);
        }
        if (newByLawName != undefined && newByLawName !== byLawName && byLawName.length > 0) {
            setByLawName(newByLawName!);
        }
        if (newByLawColor != undefined && (newByLawColor != byLawColour[0] || newByLawBorder != byLawColour[1])) {
            setByLawZoneColor(newByLawColor, newByLawBorder);
        }
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
        newByLawData?.blocks[0].itemData.push(GetDefaultByLawItem());
        updateNewByLawData(newByLawData);
    }

    let onPropertyViewChange = (item: ByLawItem) => {
        
    }

    let propertyViews = newByLawData?.blocks[0].itemData.map((item, index) => {
        return (
            <ByLawPropertyView byLawItem={item} key={index} onChange={onPropertyViewChange} />
        )
    });
    
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
                        <h2>Properties</h2>
                        {propertyViews}
                        <Button focusKey={FOCUS_AUTO} variant="flat" theme={addPropertyTheme} >Add Constraint</Button>                        
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
}