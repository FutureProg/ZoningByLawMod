import { ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types"
import { Bounds1Field } from "../Bounds1Field";
import { ByLawItemBounds1Editor } from "./ByLawItemBounds1Editor";
import { Bounds1 } from "cs2/bindings";
import { useState } from "react";
import ByLawItemEnumEditor from "./ByLawItemEnumEditor";
import { getMeasurementString } from "mods/utils";
import { UnitSystem, useLocalization } from "cs2/l10n";

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
    if (!isOpen) {
        return (<></>);
    }

    let {constraintType: constraintType, byLawItemType: itemType} = byLawItem;

    // let [localByLawItem, updateLocalByLawItem] = useState(byLawItem); 
    
    if (constraintType == ByLawConstraintType.Length || constraintType == ByLawConstraintType.Count) {
        const measurementSuffix = getMeasurementString(byLawItem.byLawItemType, byLawItem.constraintType);
        let onChange = (name: string, newValue: Bounds1) => {            
            // Handle conversions to metric
            if (measurementSuffix.indexOf("units") >= 0) {
                newValue = {
                    max: newValue.max * 8,
                    min: newValue.min * 8
                }
            } else if (['m', 'ft'].includes(measurementSuffix) && localization.unitSettings.unitSystem == 1){
                newValue = {
                    max: newValue.max / 3,
                    min: newValue.min / 3
                }
            } 
            let nItemVal = {
                ...byLawItem,
                valueBounds1: newValue                
            };
            onChangeCallback && onChangeCallback(nItemVal);
        }
        let boundsValue = byLawItem.valueBounds1;
        if (measurementSuffix.indexOf("units") >= 0) {
            boundsValue = {
                max: boundsValue.max / 8,
                min: boundsValue.min / 8
            }
        } else if (['m', 'ft'].includes(measurementSuffix) && localization.unitSettings.unitSystem == 1){
            boundsValue = {
                max: boundsValue.max * 3,
                min: boundsValue.min * 3
            }
        } 
        return ByLawItemBounds1Editor({
            name: ByLawItemType[byLawItem.byLawItemType], 
            bounds: boundsValue,   
            onChange
        });
    }

    if (constraintType == ByLawConstraintType.MultiSelect || constraintType == ByLawConstraintType.SingleSelect) {
        let onChange = (nValue: number) => {
            let nItemVal = {
                ...byLawItem,
                valueByteFlag: nValue
            }
            // updateLocalByLawItem(nItemVal);            
            onChangeCallback && onChangeCallback(nItemVal);
        }

        return ByLawItemEnumEditor({
            constraintType: byLawItem.constraintType,
            itemType: byLawItem.byLawItemType,
            itemValue: byLawItem.valueByteFlag,
            onChange
        });
    }
    return (<></>);
}
