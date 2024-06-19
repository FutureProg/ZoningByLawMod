import { ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types"
import { Bounds1Field } from "../Bounds1Field";
import { ByLawItemBounds1Editor } from "./ByLawItemBounds1Editor";
import { Bounds1 } from "cs2/bindings";
import { useState } from "react";
import ByLawItemEnumEditor from "./ByLawItemEnumEditor";

type Props = {
    byLawItem : ByLawItem;  
    isOpen: boolean; 
    onChange?: (newItemValue: ByLawItem) => void;
};

/**
 * Responsible for choosing which editor to display based on the property type
 */
export default ({byLawItem, isOpen, onChange: onChangeCallback}: Props) : JSX.Element => {     
    if (!isOpen) {
        return (<></>);
    }

    let {constraintType: constraintType, byLawItemType: itemType} = byLawItem;

    // let [localByLawItem, updateLocalByLawItem] = useState(byLawItem); 
    
    if (constraintType == ByLawConstraintType.Length || constraintType == ByLawConstraintType.Count) {
        let onChange = (name: string, newValue: Bounds1) => {
            let nItemVal = {
                ...byLawItem,
                valueBounds1: newValue                
            };
            // updateLocalByLawItem(nItemVal);            
            onChangeCallback?.call(null, nItemVal);
        }
        return ByLawItemBounds1Editor({
            name: ByLawItemType[byLawItem.byLawItemType], 
            bounds: byLawItem.valueBounds1,   
            onChange: onChange
        });
    }

    if (constraintType == ByLawConstraintType.MultiSelect || constraintType == ByLawConstraintType.SingleSelect) {
        let onChange = (nValue: number) => {
            let nItemVal = {
                ...byLawItem,
                valueByteFlag: nValue
            }
            // updateLocalByLawItem(nItemVal);            
            onChangeCallback?.call(null, nItemVal);
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
