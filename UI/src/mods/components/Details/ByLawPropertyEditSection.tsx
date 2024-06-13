import { ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types"
import { Bounds1Field } from "../Bounds1Field";
import { ByLawItemBounds1Editor } from "./ByLawItemBounds1Editor";
import { Bounds1 } from "cs2/bindings";
import { useState } from "react";

type Props = {
    byLawItem : ByLawItem;  
    isOpen: boolean; 
    onChange?: (newItemValue: ByLawItem) => void;
};

export default ({byLawItem, isOpen, onChange: onChangeCallback}: Props) : JSX.Element => {     
    if (!isOpen) {
        return (<></>);
    }

    let {byLawConstraintType: constraintType, byLawItemType: itemType} = byLawItem;

    let [localByLawItem, updateLocalByLawItem] = useState(byLawItem); 

    if (constraintType == ByLawConstraintType.Length || constraintType == ByLawConstraintType.Count) {
        let onChange = (name: string, newValue: Bounds1) => {
            updateLocalByLawItem({
                ...localByLawItem,
                valueBounds1: newValue                
            });
            onChangeCallback?.call(null, localByLawItem);
        }
        return ByLawItemBounds1Editor({
            name: ByLawItemType[byLawItem.byLawItemType], 
            bounds: byLawItem.valueBounds1,   
            onChange: onChange
        });
    }

    if (constraintType == ByLawConstraintType.MultiSelect) {
        return (<></>)
    }
    return (<></>);
}
