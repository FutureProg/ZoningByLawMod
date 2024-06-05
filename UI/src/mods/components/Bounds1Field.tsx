import { Bounds1 } from "cs2/bindings";
import { Button } from "cs2/ui";
import { MutableRefObject, useCallback, useEffect, useRef, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver";

import styles from './bounds1-field.module.scss';
import {ButtonedNumberInput, ButtonedNumberInputRef} from "./ButtonedNumberInput";

const couiStandard =                         "coui://uil/Standard/";

const resetSrc =            couiStandard + "Reset.svg";

export const Bounds1Field = (props : {bounds?: Bounds1, name: string, onChange?: (name: string, newValue: Bounds1) => void}) => {
    let [localBounds, setLocalBounds] = useState({min: String(props.bounds?.min), max: String(props.bounds?.max)});
    let minRef = useRef<ButtonedNumberInputRef>(null);
    let maxRef = useRef<ButtonedNumberInputRef>(null);
    useEffect(() => {
        setLocalBounds({min: String(props.bounds?.min), max: String(props.bounds?.max)});
        if (props.bounds) {
            minRef.current?.setValue(props.bounds.min);
            maxRef.current?.setValue(props.bounds.max);
        }        
    }, [props.bounds, minRef, maxRef]);    

    let onInputChange = (e: any) => {
        let minS = minRef.current?.getValue();
        let maxS = maxRef.current?.getValue();
        setLocalBounds({min: String(minS), max: String(maxS)});
        
        let max = Number(maxS);
        let min = Number(minS);
        if (isNaN(min) || isNaN(max)) {
            return;
        }
        let nBounds = {min, max};
        if (nBounds.min > nBounds.max && nBounds.max > -1) {
            return;
        }
        if (props.onChange) {
            props.onChange(props.name, nBounds);
        }
    }

    const onClickUnset = (sender: 'min' | 'max') => useCallback(() => {
        var nBoundsText = localBounds;
        console.log(minRef.current);
        if (sender == 'min' && minRef.current) {        
            minRef.current.setValue(-1);
        }
        // nBoundsText[sender] = "-1";
        // setLocalBounds(nBoundsText);
        // if (isNaN(Number(nBoundsText.min)) || isNaN(Number(nBoundsText.max))) {
        //     return;
        // }
        // let nBounds = {min: Number(nBoundsText.min), max: Number(nBoundsText.max)};
        // if (props.onChange) {
        //     props.onChange(props.name, nBounds);
        // }
    }, [minRef.current]);
    const textInputTheme = VanillaComponentResolver.instance.textInputTheme;
    const toolButtonTheme = VanillaComponentResolver.instance.toolButtonTheme;

    return (
        <div className={styles.bounds1Field}>        
            <div>
                {/* <div style={{display: "flex", justifyContent: "space-between"}}>
                    <label className={textInputTheme.label}>Min</label>
                    <Button className={styles.unsetButton} onClick={onClickUnset("min")}>Unset</Button>
                </div>                 */}
                <label className={textInputTheme.label}>Min</label>
                <VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.ToolButton 
                                className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                                tooltip={"Unset"} 
                                onSelect={onClickUnset("min")} 
                                src={resetSrc}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    ></VanillaComponentResolver.instance.ToolButton>  
                    <ButtonedNumberInput defaultValue={-1} ref={minRef} limit={{min: -1}} />
                </VanillaComponentResolver.instance.Section>                
            </div>
            <div>
                <label className={textInputTheme.label}>Max</label>
                <VanillaComponentResolver.instance.Section>
                    <VanillaComponentResolver.instance.ToolButton 
                                className={VanillaComponentResolver.instance.toolButtonTheme.button} 
                                tooltip={"Unset"} 
                                onSelect={onClickUnset("max")} 
                                src={resetSrc}
                                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
                    ></VanillaComponentResolver.instance.ToolButton>  
                    <ButtonedNumberInput defaultValue={-1} ref={maxRef} limit={{min: -1}} />
                </VanillaComponentResolver.instance.Section> 
            </div>
            {/* <div className={textInputTheme.container}>
                <div style={{display: "flex", justifyContent: "space-between"}}>
                    
                    <Button  className={styles.unsetButton} onClick={onClickUnset("max")}>Unset</Button>
                </div>                                                    
                <input className={textInputTheme.input} type="number" ref={maxRef} value={localBounds?.max} onChange={onInputChange} />                            
            </div>         */}
        </div>
    )
}