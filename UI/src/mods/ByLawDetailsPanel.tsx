import { DropdownItem, DropdownToggle, Scrollable } from "cs2/ui"
import styles from './mainpanel.module.scss';
import { selectedByLawData$ } from "./bindings";
import { useValue } from "cs2/api";
import { useEffect, useState } from "react";
import { ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1 } from "cs2/bindings";
import { Dropdown } from "cs2/ui";
import { FOCUS_AUTO } from "cs2/input";
import { VanillaComponentResolver } from "vanillacomponentresolver";

const Bounds1Field = (props : {bounds?: Bounds1}) => {
    return (
        <div className={styles.bounds1Field}>        
            <div>
                <label>Min</label>
                <input type="number" value={props.bounds?.min} />
            </div>
            <div>
                <label>Max</label>
                <input type="number" value={props.bounds?.max} />
            </div>        
        </div>
    )
}

const EnumField = <T,>(props: {enum : ByLawZoneType, onChange?: (enumValue: number) => any}) => {            
    type x = keyof T;
    let preEntries : [any, any][] = Object.entries(ByLawZoneType);
    let entries : {[key: string]: number} = Object.fromEntries(
        preEntries.filter((value, idx) => idx < preEntries.length/2 && Number(value[0]) != 0).map(([k,v]) => [v,k])
    )        
    let defaultState : Record<string, boolean> = {};    
    Object.entries(entries).forEach(([k,v],idx,arr) => defaultState[k] = (v & props.enum!) !== 0);    
    
    const onCheckboxChange = (key: string) => (e: any) => {
        let nState = {...checked};
        nState[key] = (e as any) as boolean;         
        let nEnum = 0;        
        Object.entries(nState).forEach(([k, v], idx) => {
            if (v) {
                nEnum |= entries[k];
            }
        });
        props.onChange? props.onChange(nEnum) : undefined;
        setChecked(nState);
    };

    let [checked, setChecked] = useState(defaultState);
    
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

export const ByLawDetailsPanel = () => {    
    let byLawData = useValue(selectedByLawData$);    
    let [newByLawData, updateNewByLawData] = useState<ByLawZoneComponent>();

    useEffect(() => {        
        console.log(byLawData);
        updateNewByLawData(byLawData);
    }, [byLawData]);    

    return (
        <Scrollable className={styles.bylawDetails}>   
            <div>
                <div className={styles.byLawDetailsTable}>
                    <tr>
                        <th>Permitted Uses</th>
                        <td><EnumField<ByLawZoneType> enum={byLawData? byLawData.zoneType : 1} onChange={(nVal) => console.log(nVal)} /> </td>
                    </tr>
                    <tr>
                        <th>Height Constraints</th>
                        <td><Bounds1Field bounds={newByLawData?.height} /></td>
                    </tr>
                    <tr>
                        <th>Lot Frontage Constraints</th>
                        <td><Bounds1Field bounds={newByLawData?.frontage} /></td>
                    </tr>
                </div>
            </div>            
        </Scrollable>
    )
}