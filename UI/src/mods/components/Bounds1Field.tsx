import { Bounds1 } from "cs2/bindings";
import { useCallback, useRef } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";
import checkboxTheme from '../themes/RoundCheckboxTheme.module.scss';
import styles from './bounds1-field.module.scss';
import { ButtonedNumberInput, ButtonedNumberInputRef } from "./ButtonedNumberInput";
import { BOUNDS_VALUE_DISABLED } from "mods/types";

export interface Bounds1FieldProps {
    bounds: Bounds1, 
    name: string, 
    onChange?: (name: string, newValue: Bounds1) => void
};

export const Bounds1Field = (props : Bounds1FieldProps) => {
    let minRef = useRef<ButtonedNumberInputRef>(null);
    let maxRef = useRef<ButtonedNumberInputRef>(null);

    let onInputChange = (field: keyof Bounds1) => (value: number) => {                
        if (isNaN(value)) {
            return;
        }
        let nBounds : Bounds1 = {
            ...props.bounds,
            [field]: value
        };
        if (nBounds.min > nBounds.max && nBounds.max > -1) {
            return;
        }
        if (props.onChange) {
            props.onChange(props.name, nBounds);
        }
    }

    const onToggleEnable = (sender: keyof Bounds1) => useCallback(() => {        
        onInputChange(sender)(props.bounds[sender] == BOUNDS_VALUE_DISABLED? 0 : -1);
    }, [onInputChange]);
    const textInputTheme = VanillaComponentResolver.instance.textInputTheme;
    const toolButtonTheme = VanillaComponentResolver.instance.toolButtonTheme;

    return (
        <div className={styles.bounds1Field}>        
            <div className={styles.inputContainer}>           
            <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={onToggleEnable("min")}
                    checked={props.bounds.min != BOUNDS_VALUE_DISABLED}
                />     
                <div className={styles.label}>Minimum</div>     
                <ButtonedNumberInput value={props.bounds.min} onChange={onInputChange('min')} ref={minRef} limit={{min: -1}} />                        
            </div>
            <div className={styles.inputContainer}>
                <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={onToggleEnable("max")}
                    checked={props.bounds.max != BOUNDS_VALUE_DISABLED}
                />
                <div className={styles.label}>Maximum</div>                
                <ButtonedNumberInput onChange={onInputChange('max')} value={props.bounds.max} ref={maxRef} limit={{min: -1}} />                
            </div>
        </div>
    )
}