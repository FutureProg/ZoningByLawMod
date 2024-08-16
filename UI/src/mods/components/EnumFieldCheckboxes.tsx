import { ByLawZoneType } from "mods/types";
import { useEffect, useMemo, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import styles from './EnumFieldCheckboxes.module.scss';

export interface EnumFieldCheckboxesProps {
    enum: number, 
    enumEntries: [any, any][];
    type: 'multi' | 'single',
    onChange?: (enumValue: number) => any    
    showZero?: boolean
}
export default <T,>(props: EnumFieldCheckboxesProps) => {                
    let preEntries = props.enumEntries;  
    let entries : {[key: string]: number} = Object.fromEntries(
        preEntries.filter(([v, k], idx) => isNaN(Number(k)) && (props.showZero? true: Number(v) != 0)).map(([k,v]) => [v,k])
    )        
    
    let checked = useMemo(() => {
        let nState : Record<string, boolean> = {};    
        Object.entries(entries).forEach(([k,v]) => nState[k] = (nState[k] = v == 0 && props.enum == 0 && props.type == 'single') || (v & (props.enum as number)) !== 0);
        return nState;        
    }, [props.enum]);
    
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
    };
    
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
