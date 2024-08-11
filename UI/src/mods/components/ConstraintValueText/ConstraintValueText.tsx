import { BOUNDS_VALUE_DISABLED, ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types";

export default (props: {className?: string, item?: ByLawItem}) => {
    let text = "";
    switch(props.item?.constraintType) {
        case ByLawConstraintType.Length: {
            let value = props.item.valueBounds1;
            let minText = value.min > BOUNDS_VALUE_DISABLED? `${value.min} metres` : "";
            let maxText = value.max > BOUNDS_VALUE_DISABLED? `${value.max} metres` : "";            
            text = `${minText}${minText && maxText? " to " : ""}${maxText}`;
            break; 
        }            
    }
    return (
        <div className={props.className||""}>
            {text}
        </div>
    )
}