import { ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types"
import { Bounds1Field } from "../Bounds1Field";
import { ByLawItemBounds1Editor } from "./ByLawItemBounds1Editor";
import { Bounds1 } from "cs2/bindings";
import { useState } from "react";
import ByLawItemEnumEditor from "./ByLawItemEnumEditor";
import { getMeasurementString } from "mods/utils";
import { UnitSystem, useLocalization } from "cs2/l10n";
import React from "react";

type Props = {
    byLawItem : ByLawItem;  
    isOpen: boolean; 
    onChange?: (newItemValue: ByLawItem) => void;
};

/**
 * Responsible for choosing which editor to display based on the property type
 */
export default ({byLawItem, isOpen, onChange: onChangeCallback}: Props) : JSX.Element => {  
    let localization = useLocalization();  
    const measurementSuffix = getMeasurementString(byLawItem.byLawItemType, byLawItem.constraintType);
    let isCellMeasurement = measurementSuffix.indexOf("cells") >= 0;
    if (!isOpen) {
        return (<></>);
    }

    let {constraintType: constraintType, byLawItemType: itemType} = byLawItem;

    if (constraintType == ByLawConstraintType.Length || constraintType == ByLawConstraintType.Count) {
        
        let onChange = (name: string, newValue: Bounds1) => {            
            // Handle conversions to metric
            if (isCellMeasurement) {
                newValue = {
                    min: newValue.min * 8,
                    max: newValue.max * 8                    
                }
            } else if (['m', 'ft'].includes(measurementSuffix) && localization.unitSettings.unitSystem == 1){
                newValue = {
                    min: Math.round(newValue.min / 3),
                    max: Math.round(newValue.max / 3)                    
                }
            } 
            console.log(newValue);
            let nItemVal = {
                ...byLawItem,
                valueBounds1: newValue                
            };
            onChangeCallback && onChangeCallback(nItemVal);
        }
        let boundsValue = byLawItem.valueBounds1;
        let step = 1;
        if (isCellMeasurement) {
            boundsValue = {
                max: boundsValue.max / 8,
                min: boundsValue.min / 8
            }
        } else if (['m', 'ft'].includes(measurementSuffix) && localization.unitSettings.unitSystem == 1){
            boundsValue = {
                max: boundsValue.max * 3,
                min: boundsValue.min * 3
            }
            step = 3;
        } 
        return React.createElement(ByLawItemBounds1Editor, {
            name: ByLawItemType[byLawItem.byLawItemType], 
            bounds: boundsValue,   
            step: step,
            onChange
        });
    }

    if (constraintType == ByLawConstraintType.MultiSelect || constraintType == ByLawConstraintType.SingleSelect) {
        let onChange = (nValue: number) => {
            let nItemVal = {
                ...byLawItem,
                valueByteFlag: nValue
            }     
            onChangeCallback && onChangeCallback(nItemVal);
        }

        return (
            <ByLawItemEnumEditor
            constraintType={byLawItem.constraintType}
            itemType={byLawItem.byLawItemType}
            itemValue={byLawItem.valueByteFlag}
            onChange={onChange}
            />
        )
    }
    return (<></>);
}
