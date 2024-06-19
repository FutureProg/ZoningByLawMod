import { ByLawZoneType } from "mods/types";
import { useEffect, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import styles from './EnumFieldCheckboxes.module.scss';

export interface EnumFieldCheckboxesProps {
    enum: number, 
    enumEntries: [any, any][];
    type: 'multi' | 'single',
    onChange?: (enumValue: number) => any    
}
export default <T,>(props: EnumFieldCheckboxesProps) => {            
    type x = keyof T;  
    let preEntries = props.enumEntries;  
    // let preEntries : [any, any][] = Object.entries(typeof (props.enum as {[k: string] : string | T}));
    let entries : {[key: string]: number} = Object.fromEntries(
        preEntries.filter((value, idx) => idx < preEntries.length/2 && Number(value[0]) != 0).map(([k,v]) => [v,k])
    )        
    let defaultState : Record<string, boolean> = {};    
    Object.entries(entries).forEach(([k,v]) => defaultState[k] = (v & (props.enum as number)) !== 0);        
    const onCheckboxChange = (key: string) => (e: any) => {
        let nState = {...checked};
        if (props.type == 'single') {
            Object.entries(nState).forEach(([k, v], idx) => {
                nState[k] = false;
            });
        }
        nState[key] = (e as any) as boolean;                 
        let nEnum = 0;        
        Object.entries(nState).forEach(([k, v], idx) => {
            if (v) {
                nEnum |= entries[k];
            }
        });
        props.onChange?.call(null, nEnum);
        setChecked(nState);
    };

    let [checked, setChecked] = useState(defaultState);
    useEffect(() => {
        Object.entries(entries).forEach(([k,v]) => defaultState[k] = (v & props.enum) !== 0);        
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
