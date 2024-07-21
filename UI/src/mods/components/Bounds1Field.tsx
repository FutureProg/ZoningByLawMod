import { Bounds1 } from "cs2/bindings";
import { Button } from "cs2/ui";
import { MutableRefObject, useCallback, useEffect, useRef, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";

import styles from './bounds1-field.module.scss';
import {ButtonedNumberInput, ButtonedNumberInputRef} from "./ButtonedNumberInput";

const couiStandard =                         "coui://uil/Standard/";

const resetSrc =            couiStandard + "Reset.svg";

export interface Bounds1FieldProps {
    bounds: Bounds1, 
    name: string, 
    onChange?: (name: string, newValue: Bounds1) => void
};

export const Bounds1Field = (props : Bounds1FieldProps) => {
    // let [localBounds, setLocalBounds] = useState({min: String(props.bounds?.min), max: String(props.bounds?.max)});
    let minRef = useRef<ButtonedNumberInputRef>(null);
    let maxRef = useRef<ButtonedNumberInputRef>(null);
    // useEffect(() => {
    //     setLocalBounds({min: String(props.bounds?.min), max: String(props.bounds?.max)});
    //     if (props.bounds) {
    //         minRef.current?.setValue(props.bounds.min);
    //         maxRef.current?.setValue(props.bounds.max);
    //     }        
    // }, [props.bounds, minRef, maxRef]);        

    let onInputChange = (field: keyof Bounds1) => (value: number) => {        
        // setLocalBounds({min: String(minS), max: String(maxS)});       

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

    const onClickUnset = (sender: keyof Bounds1) => useCallback(() => {
        onInputChange(sender)(-1);
    }, [onInputChange]);
    const textInputTheme = VanillaComponentResolver.instance.textInputTheme;
    const toolButtonTheme = VanillaComponentResolver.instance.toolButtonTheme;

    return (
        <div className={styles.bounds1Field}>        
            <div>
                <VanillaComponentResolver.instance.Section title="Minimum">
                    <VanillaComponentResolver.instance.ToolButton 
                                className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                                tooltip={"Unset"} 
                                onSelect={onClickUnset("min")} 
                                src={resetSrc}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    ></VanillaComponentResolver.instance.ToolButton>  
                    <ButtonedNumberInput value={props.bounds.min} onChange={onInputChange('min')} ref={minRef} limit={{min: -1}} />
                </VanillaComponentResolver.instance.Section>                
            </div>
            <div>
                <VanillaComponentResolver.instance.Section title="Maximum">
                    <VanillaComponentResolver.instance.ToolButton 
                                className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                                tooltip={"Unset"} 
                                onSelect={onClickUnset("max")} 
                                src={resetSrc}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    ></VanillaComponentResolver.instance.ToolButton>  
                    <ButtonedNumberInput onChange={onInputChange('max')} value={props.bounds.max} ref={maxRef} limit={{min: -1}} />
                </VanillaComponentResolver.instance.Section> 
            </div>
        </div>
    )
}