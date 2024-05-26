import { Button, DropdownItem, DropdownToggle, Scrollable } from "cs2/ui"
import styles from './mainpanel.module.scss';
import { ZONE_BORDER_IDX, ZONE_COLOR_IDX, defaultColor, deleteByLaw, selectedByLawColor$, selectedByLawData$, selectedByLawName$, setByLawData, setByLawName, setByLawZoneColor, toggleByLawRenderPreview } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, ChangeEventHandler, useEffect, useRef, useState } from "react";
import { ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1, Color } from "cs2/bindings";
import { Dropdown } from "cs2/ui";
import { FOCUS_AUTO, InputContext } from "cs2/input";
import { ColorHSV, VanillaComponentResolver } from "vanillacomponentresolver";
import { rgbaToHex } from "./utils";

const Bounds1Field = (props : {bounds?: Bounds1, name: string, onChange?: (name: string, newValue: Bounds1) => void}) => {
    let [localBounds, setLocalBounds] = useState({min: String(props.bounds?.min), max: String(props.bounds?.max)});
    let minRef = useRef<HTMLInputElement>(null);
    let maxRef = useRef<HTMLInputElement>(null);
    useEffect(() => {
        setLocalBounds({min: String(props.bounds?.min), max: String(props.bounds?.max)});
    }, [props.bounds, minRef, maxRef]);    

    let onInputChange = (e: any) => {
        let minS = minRef.current?.value;        
        let maxS = maxRef.current?.value;
        setLocalBounds({min: String(minS), max: String(maxS)});
        
        let max = Number(maxS);
        let min = Number(minS);
        if (isNaN(min) || isNaN(max)) {
            return;
        }
        let nBounds = {min, max};
        if (nBounds.min > nBounds.max) {
            return;
        }
        if (props.onChange) {
            props.onChange(props.name, nBounds);
        }
    }

    const onClickUnset = (sender: 'min' | 'max') => () => {
        var nBoundsText = localBounds;
        nBoundsText[sender] = "-1";
        setLocalBounds(nBoundsText);
        if (isNaN(Number(nBoundsText.min)) || isNaN(Number(nBoundsText.max))) {
            return;
        }
        let nBounds = {min: Number(nBoundsText.min), max: Number(nBoundsText.max)};
        if (props.onChange) {
            props.onChange(props.name, nBounds);
        }
    }

    return (
        <div className={styles.bounds1Field}>        
            <div>
                <div style={{display: "flex", justifyContent: "space-between"}}>
                    <label>Min</label>
                    <Button onClick={onClickUnset("min")}>Unset</Button>
                </div>                
                <input type="number" ref={minRef} value={localBounds?.min} onChange={onInputChange} />
            </div>
            <div>
                <div style={{display: "flex", justifyContent: "space-between"}}>
                    <label>Max</label>
                    <Button onClick={onClickUnset("max")}>Unset</Button>
                </div>                
                <input type="number" ref={maxRef} value={localBounds?.max} onChange={onInputChange} />
            </div>        
        </div>
    )
}

const EnumField = <T,>(props: {enum : ByLawZoneType, onChange?: (enumValue: T) => any}) => {            
    type x = keyof T;
    let preEntries : [any, any][] = Object.entries(ByLawZoneType);
    let entries : {[key: string]: number} = Object.fromEntries(
        preEntries.filter((value, idx) => idx < preEntries.length/2 && Number(value[0]) != 0).map(([k,v]) => [v,k])
    )        
    let defaultState : Record<string, boolean> = {};    
    Object.entries(entries).forEach(([k,v]) => defaultState[k] = (v & props.enum!) !== 0);        
    const onCheckboxChange = (key: string) => (e: any) => {
        let nState = {...checked};
        nState[key] = (e as any) as boolean;         
        let nEnum = 0;        
        Object.entries(nState).forEach(([k, v], idx) => {
            if (v) {
                nEnum |= entries[k];
            }
        });
        props.onChange? props.onChange((nEnum as any) as T) : undefined;
        setChecked(nState);
    };

    let [checked, setChecked] = useState(defaultState);
    useEffect(() => {
        Object.entries(entries).forEach(([k,v]) => defaultState[k] = (v & props.enum!) !== 0);
        setChecked(defaultState);
    }, [props.enum]);
    
    const list = Object.entries(entries).map(([key, value], idx) => 
        <div key={key}>
            <label>{key}</label>
            <VanillaComponentResolver.instance.Checkbox 
                checked={checked[key]}
                onChange={onCheckboxChange(key)} 
                theme={VanillaComponentResolver.instance.checkboxTheme}/>            
        </div>
    );
    return (
        <div className={styles.enumField}>
            {list}
        </div>   
    )
}

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
    
    return (
        <div className={styles.bylawDetails}>
            <Scrollable>   
                <div style={{display: props.selectedRowIndex == -1? 'none': 'block'}}>
                    <div className={styles.byLawDetailsTable}>
                        <tr>
                            <th>Name</th>
                            <td><input type="text" value={newByLawName} onChange={onNameChange}/></td>
                        </tr>
                        <tr>
                            <Button onClick={toggleByLawRenderPreview}>Preview</Button>
                        </tr>
                        <tr>
                            <th>Zone Colour</th>
                            <td>
                                <VanillaComponentResolver.instance.ColorField value={newByLawColor} onChange={onUpdateByLawColor(ZONE_COLOR_IDX)}/>
                                <input type="text" readOnly={true} value={rgbaToHex(newByLawColor)} />
                            </td>                        
                        </tr>
                        {/* <tr>
                            <th>Zone Border Colour</th>
                            <td>
                                <VanillaComponentResolver.instance.ColorField value={newByLawBorder} onChange={onUpdateByLawColor(ZONE_BORDER_IDX)}/>
                                <input type="text" readOnly={true} value={rgbaToHex(newByLawBorder)} />
                            </td>                        
                        </tr> */}
                        <tr>
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
                    </div>                               
                </div>            
            </Scrollable>
            <div style={{display: props.selectedRowIndex == -1? 'none': 'block', marginTop: '8rem'}}>
                <Button onClick={onSave} variant="flat" style={{marginBottom: '16rem'}}>Save</Button>                
                <Button onClick={onDelete} variant="flat" style={{backgroundColor: 'red'}}>Delete</Button>
            </div>            
        </div>
    )
}