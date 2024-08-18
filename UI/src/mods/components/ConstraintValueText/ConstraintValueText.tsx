import { BOUNDS_VALUE_DISABLED, ByLawConstraintType, ByLawItem, ByLawItemType, ByLawZoneType, PollutionValues } from "mods/types";

export default (props: {className?: string, item?: ByLawItem}) => {
    let text = "";
    switch(props.item?.constraintType) {
        case ByLawConstraintType.Length:
        case ByLawConstraintType.Count: {
            let value = props.item.valueBounds1;
            let measurement = props.item?.constraintType == ByLawConstraintType.Length? 'm' : '';
            let minText = value.min > BOUNDS_VALUE_DISABLED? `${value.min}${measurement}` : "";
            let maxText = value.max > BOUNDS_VALUE_DISABLED? `${value.max}${measurement}` : "";            
            text = `${minText}${minText && maxText? " to " : ""}${maxText}`;
            break; 
        }
        case ByLawConstraintType.MultiSelect: {            
            switch(props.item!.byLawItemType) {
                default: {
                    let value = props.item.valueByteFlag;
                    let count = Object.keys(ByLawZoneType)
                        .filter(key => !isNaN(Number(key)))                        
                        .map((key, _) => ((Number(key) & value) != 0? 1 : 0) as number)
                        .reduce((prevValue, currentValue) => prevValue + currentValue, 0);
                    text = `${count} item(s)`;
                    break;
                }
            }
            break;
        }
        case ByLawConstraintType.SingleSelect: {
            let value = props.item.valueByteFlag;
            switch(props.item!.byLawItemType) {
                case ByLawItemType.AirPollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.NoisePollutionLevel:
                    text = PollutionValues[value];
                    break;
            }
        }
    }
    return (
        <div className={props.className||""}>
            {text}
        </div>
    )
}