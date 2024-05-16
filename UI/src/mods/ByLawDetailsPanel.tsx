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

const EnumField = <T,>(props: {enum? : number}) => {        
    if (!props.enum) {
        props.enum = ByLawZoneType.None;
    }    
    let preEntries : [any, any][] = Object.entries(ByLawZoneType);
    let entries : [string: number] = Object.fromEntries(
        preEntries.filter((value, idx) => idx < preEntries.length/2).map(([k,v]) => [v,k])
    )        
    let defaultState : Record<string, boolean> = {};
    Object.keys(entries).forEach((k) => defaultState[k] =  false);    
    let [checked, setChecked] = useState(defaultState);
    useEffect(() => {console.log(checked);}, [checked]);
    const list = Object.entries(entries).map(([key, value], idx) => 
        <div key={key}>
            <label>{key}</label>
            <VanillaComponentResolver.instance.Checkbox 
                checked={checked[key]}
                onChange={(e) => {console.log("Change " + e + " " + typeof e); let x = {...checked}; x[key] = (e as any) as boolean; setChecked(x);}} 
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
        updateNewByLawData(byLawData);
    }, [byLawData])    

    return (
        <Scrollable className={styles.bylawDetails}>   
            <div>
                <div className={styles.byLawDetailsTable}>
                    <tr>
                        <th>Permitted Uses</th>
                        <td><EnumField<ByLawZoneType> enum={newByLawData?.zoneType} /> </td>
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