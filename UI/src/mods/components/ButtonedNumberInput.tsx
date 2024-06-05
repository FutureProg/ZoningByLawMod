import { ChangeEvent, ForwardedRef, MutableRefObject, RefObject, createRef, forwardRef, useCallback, useEffect, useImperativeHandle, useMemo, useRef, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver"
const couiStandard =                         "coui://uil/Standard/";
const arrowDownSrc =         couiStandard +  "ArrowDownThickStroke.svg";
const arrowUpSrc =           couiStandard +  "ArrowUpThickStroke.svg";

type _Props = {
    onChange?: (newValue: number) => void;
    defaultValue?: number;    
    limit?: {min?: number, max?: number};
    step?: number;
}

export interface ButtonedNumberInputRef {
    setValue: (value: number) => boolean;
    getValue: () => number | undefined;
}

export const ButtonedNumberInput = forwardRef(({onChange, defaultValue, limit, step} : _Props, ref: ForwardedRef<ButtonedNumberInputRef>) => {

    let defaultVal = defaultValue || 0;
    let _ref = useRef<HTMLInputElement>(null); 
    let [value, setValue] = useState(defaultVal);
    let _step = step || 1;

    
    const textInputTheme = VanillaComponentResolver.instance.textInputTheme;
    const toolButtonTheme = VanillaComponentResolver.instance.toolButtonTheme;

    let handleChange = useCallback((e: ChangeEvent<HTMLInputElement>) => {      
        console.log(e.target.value);  
        if (Number.isNaN(e.target.value)) {
            return;
        }
        let nValue = Number(e.target.value);
        if (limit?.max && limit.max < nValue) {
            return;
        }        
        if (limit?.min && limit.min > nValue) {
            return;
        }
        setValue(nValue);
        onChange?.apply(null, [nValue]);
    }, [onChange, limit, value]);
    
    useImperativeHandle(ref, () => {
        return {
            setValue: (value: number) => {
                if (limit?.max && limit.max < value) {
                    return false;
                }        
                if (limit?.min && limit.min > value) {
                    return false;
                }                    
                setValue(value);
                onChange?.apply(null, [value]);
                return true;
            },
            getValue: () => {
                return value;
            }
        }        
    }, [limit, ref, onChange, setValue, value]);      
    
    let changeValueByButton = (multiplier: number) => useCallback(() => {
        console.log(multiplier);
        let nValue = value + (multiplier * _step);
        if (limit?.max && limit.max < nValue) {
            return;
        }        
        if (limit?.min && limit.min > nValue) {
            return;
        }
        setValue(nValue);
        onChange?.apply(null, [nValue]);
    }, [_step, onChange, limit, value]);

    return (
        <>
            <VanillaComponentResolver.instance.ToolButton
                className={VanillaComponentResolver.instance.mouseToolOptionsTheme.startButton}       
                src={arrowDownSrc}
                onSelect={changeValueByButton(-1)}
                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}                                
            ></VanillaComponentResolver.instance.ToolButton>
            <input className={textInputTheme.input} type="number" ref={_ref} value={value} onChange={handleChange}/>
            <VanillaComponentResolver.instance.ToolButton
                className={VanillaComponentResolver.instance.mouseToolOptionsTheme.endButton}       
                src={arrowUpSrc}
                onSelect={changeValueByButton(1)}
                focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}
            ></VanillaComponentResolver.instance.ToolButton>
        </>
    )
});