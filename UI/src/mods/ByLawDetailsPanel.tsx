import { Button, DropdownItem, DropdownToggle, Scrollable } from "cs2/ui"
import styles from './mainpanel.module.scss';
import { selectedByLawData$, selectedByLawName$, setByLawData, setByLawName } from "./bindings";
import { useValue } from "cs2/api";
import { ChangeEvent, ChangeEventHandler, useEffect, useRef, useState } from "react";
import { ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1 } from "cs2/bindings";
import { Dropdown } from "cs2/ui";
import { FOCUS_AUTO, InputContext } from "cs2/input";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const Bounds1Field = (props : {bounds?: Bounds1, name: string, onChange?: (name: string, newValue: Bounds1) => void}) => {
    let [localBounds, setLocalBounds] = useState(props.bounds);
    let minRef = useRef<HTMLInputElement>(null);
    let maxRef = useRef<HTMLInputElement>(null);
    useEffect(() => {
        console.log("Bounds: " + props.bounds?.min);
        setLocalBounds(props.bounds);
    }, [props.bounds, minRef, maxRef]);    

    let onInputChange = (e: any) => {
        let min = Number(minRef.current?.value);
        let max = Number(maxRef.current?.value);
        if (isNaN(min) || isNaN(max)) {
            return;
        }        
        let nBounds = {min, max};
        console.log(nBounds);
        setLocalBounds(nBounds);
        if (props.onChange) {
            props.onChange(props.name, nBounds);
        }
    }

    return (
        <div className={styles.bounds1Field}>        
            <div>
                <label>Min</label>
                <input type="number" ref={minRef} value={localBounds?.min} onChange={onInputChange} />
            </div>
            <div>
                <label>Max</label>
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

export const ByLawDetailsPanel = (props: {selectedRowIndex: number}) => {    
    let byLawData = useValue(selectedByLawData$);    
    let byLawName = useValue(selectedByLawName$);
    let [newByLawData, updateNewByLawData] = useState<ByLawZoneComponent>();
    let [newByLawName, updateNewByLawName] = useState<string>();

    useEffect(() => {                
        updateNewByLawData(byLawData);
    }, [byLawData]);
    useEffect(() => {
        updateNewByLawName(byLawName);
    }, [byLawName]);

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
    }

    return (
        <Scrollable className={styles.bylawDetails}>   
            <div style={{display: props.selectedRowIndex == -1? 'none': 'block'}}>
                <div className={styles.byLawDetailsTable}>
                    <tr>
                        <th>Name</th>
                        <td><input type="text" value={newByLawName} onChange={onNameChange}/></td>
                    </tr>
                    <tr>
                        <th>Permitted Uses</th>
                        <td><EnumField<ByLawZoneType> enum={newByLawData != undefined? newByLawData!.zoneType : byLawData? byLawData.zoneType : 0} onChange={onUpdateZoneType} /> </td>
                    </tr>
                    <tr>
                        <th>Height Constraints</th>
                        <td><Bounds1Field bounds={newByLawData?.height} name='height' onChange={onUpdateBounds} /></td>
                    </tr>
                    <tr>
                        <th>Lot Frontage Constraints</th>
                        <td><Bounds1Field bounds={newByLawData?.frontage} name='frontage' onChange={onUpdateBounds} /></td>
                    </tr>
                </div>
                <div>
                    <Button onClick={onSave} variant="flat">Save</Button>
                </div>                
            </div>            
        </Scrollable>
    )
}