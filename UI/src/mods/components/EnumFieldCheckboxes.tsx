import { useMemo } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import styles from './EnumFieldCheckboxes.module.scss';
import { useLocalization } from "cs2/l10n";
import { FieldDataOption } from "mods/bindings";

export interface EnumFieldCheckboxesProps {
    value: number[], 
    options: FieldDataOption[],
    // enumEntries: [any, any][];
    type: 'multi' | 'single',
    onChange?: (enumValue: number[]) => any    
    showZero?: boolean
}
export default (props: EnumFieldCheckboxesProps) => {      
    let {translate} = useLocalization();          
    // let preEntries = props.enumEntries;  
    // let entries : {[key: string]: number} = Object.fromEntries(
    //     preEntries.filter(([v, k], idx) => isNaN(Number(k)) && (props.showZero? true: Number(v) != 0)).map(([k,v]) => [v,k])
    // )        
    console.log('Rendering EnumFieldCheckboxes', props);
    // let checked = useMemo(() => {
    //     let nState : Record<string, boolean> = {};    
    //     Object.entries(entries).forEach(([k,v]) => nState[k] = (nState[k] = v == 0 && props.enum == 0 && props.type == 'single') || (v & (props.enum as number)) !== 0);
    //     return nState;        
    // }, [props.enum]);
    
    const onCheckboxChange = (option: FieldDataOption) => (e: any) => {
        let nState = [...props.value];
        if (props.type == 'single') {
            nState = [];
        }
        if (option.value in nState) {
            nState.splice(nState.indexOf(option.value), 1);
        } else {
            nState.push(option.value);
        }        
        props.onChange?.call(null, nState);
    };
    
    const list = props.options.map((option, idx) => 
        <div key={option.value}>
            <label>{translate(option.label, option.label)}</label>
            <VanillaComponentResolver.instance.Checkbox 
                checked={option.value in props.value}
                onChange={onCheckboxChange(option)} 
                theme={VanillaComponentResolver.instance.checkboxTheme}/>
            { option.image? <img src={option.image} className={styles.optionImage}/>: null }
        </div>
    );
    return (
        <div className={styles.enumField}>
            {list}
        </div>   
    )
}
