import { BOUNDS_VALUE_DISABLED, ByLawConstraintType, ByLawItem, ByLawItemType, ByLawZoneType, PollutionValues } from "mods/types";


//&#160; = space character code (should improve how all of this is done tbh...)
export default (props: {className?: string, item?: ByLawItem}) => {
    let textChild = <></>;
    switch(props.item?.constraintType) {
        case ByLawConstraintType.Length:
        case ByLawConstraintType.Count: {            
            let value = props.item.valueBounds1;
            let measurement = props.item?.constraintType == ByLawConstraintType.Length? 'm' : '';
            let minText = value.min > BOUNDS_VALUE_DISABLED? `${value.min}${measurement}` : "";
            let maxText = value.max > BOUNDS_VALUE_DISABLED? `${value.max}${measurement}` : "";            
            let middleText = minText && maxText? " to " : "";
            if (!middleText) {
                textChild = <span>{minText}{minText? <span>&#160;&ge;</span> : <span>&le;&#160;</span>}{maxText}</span>; // gte sign : lte sign
            } else {
                textChild = <span>{minText}&#160;to&#160;{maxText}</span>;
            }
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
                    textChild = <span>{count}&#160;item(s)</span>;
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
                    textChild = <span>{PollutionValues[value]}</span>;
                    break;
            }
        }
    }
    return (
        <div className={props.className||""}>
            {textChild}
        </div>
    )
}