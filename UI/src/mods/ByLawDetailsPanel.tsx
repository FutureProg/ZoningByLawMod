import { Button, DropdownItem, DropdownToggle, FOCUS_AUTO, FOCUS_DISABLED, Scrollable } from "cs2/ui"
import styles from './ByLawDetailsPanel.module.scss';
import { ZONE_BORDER_IDX, ZONE_COLOR_IDX, defaultColor, deleteByLaw, selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor, toggleByLawRenderPreview } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, ChangeEventHandler, useEffect, useRef, useState } from "react";
import { ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1, Color } from "cs2/bindings";
import { Dropdown } from "cs2/ui";
import { ColorHSV, VanillaComponentResolver } from "vanillacomponentresolver";
import { rgbaToHex } from "./utils";
import { Bounds1Field } from "./components/Bounds1Field";
import ByLawPropertyView from "./components/Details/ByLawPropertyView";

export const ByLawDetailsPanel = (props: {selectedRowIndex: number, onDelete?: () => void}) => {    
    let byLawData = useValue(selectedByLawData$);    
    let byLawName = useValue(selectedByLawName$);
    let byLawColour = useValue(selectedByLawColor$);    
    let [newByLawData, updateNewByLawData] = useState<ByLawZoneComponent>();
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
            zoneType: newType
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

    let testPropertyItem : ByLawItem = {
        byLawConstraintType: ByLawConstraintType.Length,
        byLawItemType: ByLawItemType.Height,
        itemCategory: ByLawItemCategory.Building,
        valueBounds1: {max: 0, min: 0},
        valueByteFlag: 0
    };
    let testPropertyItem2 : ByLawItem = {
        byLawConstraintType: ByLawConstraintType.MultiSelect,
        byLawItemType: ByLawItemType.Uses,
        itemCategory: ByLawItemCategory.Lot,
        valueBounds1: {max: 0, min: 0},
        valueByteFlag: ByLawZoneType.Commercial | ByLawZoneType.Residential
    };
    
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
                        {/* <tr>
                            <Button onClick={toggleByLawRenderPreview}>Preview (very much WIP)</Button>
                        </tr> */}
                        {/* <tr>
                            <th>Zone Colour</th>
                            <td>
                                
                                <input type="text" readOnly={true} value={rgbaToHex(newByLawColor)} />
                            </td>                        
                        </tr> */}
                        {/* <tr>
                            <th>Zone Border Colour</th>
                            <td>
                                <VanillaComponentResolver.instance.ColorField value={newByLawBorder} onChange={onUpdateByLawColor(ZONE_BORDER_IDX)}/>
                                <input type="text" readOnly={true} value={rgbaToHex(newByLawBorder)} />
                            </td>                        
                        </tr> */}
                        {/* <tr>
                            <th>Permitted Uses</th>
                            <td><EnumField<ByLawZoneType> enum={newByLawData != undefined? newByLawData!.zoneType : byLawData? byLawData.zoneType : 0} onChange={onUpdateZoneType} /> </td>
                        </tr>
                        <tr>
                            <th>Height Constraints (metres)</th>
                            <td><Bounds1Field bounds={newByLawData?.height} name='height' onChange={onUpdateBounds} /></td>
                        </tr>
                        <tr>
                            <th>Lot Frontage Constraints (metres)</th>
                            <td><Bounds1Field bounds={newByLawData?.frontage} name='frontage' onChange={onUpdateBounds} /></td>
                        </tr>
                        <tr>
                            <th>Lot Size Constraints (metres)</th>
                            <td><Bounds1Field bounds={newByLawData?.lotSize} name='lotSize' onChange={onUpdateBounds} /></td>
                        </tr>
                        <tr>
                            <th>Parking Constraints (count)</th>
                            <td><Bounds1Field bounds={newByLawData?.parking} name='parking' onChange={onUpdateBounds} /></td>
                        </tr> */}
                        <h2>Properties</h2>
                        <ByLawPropertyView byLawItem={testPropertyItem}/>
                        <ByLawPropertyView byLawItem={testPropertyItem2}/>
                        <Button focusKey={FOCUS_AUTO} variant="flat" theme={addPropertyTheme} >Add Constraint</Button>                        
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