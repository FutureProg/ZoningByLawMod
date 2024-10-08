import { ChangeEvent, ForwardedRef, MutableRefObject, RefObject, createRef, forwardRef, useCallback, useEffect, useImperativeHandle, useMemo, useRef, useState } from "react";
import { VanillaComponentResolver } from "vanillacomponentresolver"
import styles from './ButtonedNumberInput.module.scss';

const couiStandard =                         "coui://uil/Standard/";
const arrowDownSrc =         couiStandard +  "ArrowDownThickStroke.svg";
const arrowUpSrc =           couiStandard +  "ArrowUpThickStroke.svg";

type _Props = {
    onChange?: (newValue: number) => void;
    value: number;    
    limit?: {min?: number, max?: number};
    step?: number;
}

export interface ButtonedNumberInputRef {
    setValue: (value: number, silent?: boolean) => boolean;
    getValue: () => number | undefined;
}

export const ButtonedNumberInput = forwardRef(({onChange, value, limit, step} : _Props, ref: ForwardedRef<ButtonedNumberInputRef>) => {
    
    let _ref = useRef<HTMLInputElement>(null); 
    let _step = step || 1;

    
    const textInputTheme = VanillaComponentResolver.instance.textInputTheme;
    const toolButtonTheme = VanillaComponentResolver.instance.toolButtonTheme;

    let handleChange = useCallback((e: ChangeEvent<HTMLInputElement>) => {      
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
        onChange && onChange(nValue);
    }, [onChange, limit, value]);
    
    useImperativeHandle(ref, () => {
        return {
            setValue: (value: number, silent: boolean = false) => {
                if (limit?.max && limit.max < value) {
                    return false;
                }        
                if (limit?.min && limit.min > value) {
                    return false;
                }                                    
                !silent && onChange && onChange(value);
                return true;
            },
            getValue: () => {
                return value;
            }
        }        
    }, [limit, ref, onChange, value]);      
    
    let changeValueByButton = (multiplier: number) => useCallback(() => {
        let nValue = value + (multiplier * _step);
        if (nValue % _step !== 0) {
            nValue += (nValue % _step) * multiplier;           
        }        
        if (limit?.max && limit.max < nValue) {
            nValue = limit.max;
        }        
        if (limit?.min && limit.min > nValue) {
            nValue = limit.min;
        }        
        onChange && onChange(nValue);
    }, [_step, onChange, limit, value]);

    return (
        <div className={styles.container}>
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
        </div>
    )
});