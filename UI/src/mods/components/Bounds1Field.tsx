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

export const Bounds1Field = (props: Bounds1FieldProps) => {
    let minRef = useRef<ButtonedNumberInputRef>(null);
    let maxRef = useRef<ButtonedNumberInputRef>(null);

    let onInputChange = (field: keyof Bounds1) => (value: number) => {
        if (isNaN(value)) {
            return;
        }
        let nBounds: Bounds1 = {
            ...props.bounds,
            [field]: value
        };
        let {min, max} = nBounds;
        if (max != BOUNDS_VALUE_DISABLED && min != BOUNDS_VALUE_DISABLED) {
            if (field == 'min' && min > max) {
                max = min;
            } else if (field == 'max' && max < min) {
                min = max;
            }
        }        
        nBounds = {min, max};
        if (props.onChange) {
            props.onChange(props.name, nBounds);
        }
    }

    const onToggleEnable = (sender: keyof Bounds1) => useCallback(() => {
        let nValue = props.bounds[sender] == BOUNDS_VALUE_DISABLED ? 0 : BOUNDS_VALUE_DISABLED;
        let {min, max} = props.bounds;
        if (nValue != BOUNDS_VALUE_DISABLED) {
            if (sender == 'min' && nValue > max && max != BOUNDS_VALUE_DISABLED) {
                nValue = max;
            } else if (sender == 'max' && nValue < min && min != BOUNDS_VALUE_DISABLED) {
                nValue = min;
            }
        }
        onInputChange(sender)(nValue);
    }, [onInputChange]);

    let isMinEnabled = props.bounds.min != BOUNDS_VALUE_DISABLED;
    let isMaxEnabled = props.bounds.max != BOUNDS_VALUE_DISABLED;
    return (
        <div className={styles.bounds1Field}>
            <div className={styles.inputContainer}>
                <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={onToggleEnable("min")}
                    checked={isMinEnabled}
                />
                <div className={styles.label}>Minimum</div>
                {isMinEnabled ? <ButtonedNumberInput value={props.bounds.min} onChange={onInputChange('min')} ref={minRef} limit={{ min: -1 }} /> : <div></div>}
            </div>
            <div className={styles.inputContainer}>
                <VanillaComponentResolver.instance.Checkbox
                    theme={checkboxTheme}
                    onChange={onToggleEnable("max")}
                    checked={isMaxEnabled}
                />
                <div className={styles.label}>Maximum</div>
                {isMaxEnabled ? <ButtonedNumberInput onChange={onInputChange('max')} value={props.bounds.max} ref={maxRef} limit={{ min: -1 }} /> : <div></div>}
            </div>
        </div>
    )
}