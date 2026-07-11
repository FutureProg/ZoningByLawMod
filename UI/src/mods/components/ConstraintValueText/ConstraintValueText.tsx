import { UnitSystem, useLocalization } from "cs2/l10n";
import { BOUNDS_VALUE_DISABLED, ByLawConstraintType, ByLawItem, ByLawItemType, ByLawZoneType, PollutionValues } from "mods/types";
import { getMeasurementString, isDynamicNameBasedMultiSelect } from "mods/utils";


//&#160; = space character code (should improve how all of this is done tbh...)
export default (props: {className?: string, item?: ByLawItem}) => {
    let textChild = <></>;
    let {translate, unitSettings} = useLocalization();
    switch(props.item?.constraintType) {
        case ByLawConstraintType.Length:
        case ByLawConstraintType.Count: {       
            
            //TODO: Refactor next lines to find proper suffix
            //TODO: Fix how conversion is handled
            let value = props.item.valueBounds1;
            let measurementSuffix = getMeasurementString(props.item.byLawItemType, props.item.constraintType, unitSettings.unitSystem);
            let isUnits = measurementSuffix == ' cells';
            if(unitSettings.unitSystem == 1 && !isUnits) {
                value = {
                    min: value.min * 3,
                    max: value.max * 3
                }
            }            
            let measurement = props.item?.constraintType == ByLawConstraintType.Length? (unitSettings.unitSystem == 0? 'm': 'ft') : '';
            if (isUnits) {
                measurement = measurementSuffix;
            }
            let minText = value.min > BOUNDS_VALUE_DISABLED? `${isUnits? value.min/8 : value.min}${measurement}` : "";
            let maxText = value.max > BOUNDS_VALUE_DISABLED? `${isUnits? value.max/8 : value.max}${measurement}` : "";            
            let middleText = minText && maxText? " to " : "";
            if (!middleText) {
                textChild = <span>{minText}{minText? <span>&#160;&ge;</span> : <span>&le;&#160;</span>}{maxText}</span>; // gte sign : lte sign
            } else {
                textChild = <span>{minText}&#160;{translate("ZBL.ByLawValueText[BoundsTo]", "TO")}&#160;{maxText}</span>;
            }
            break; 
        }
        case ByLawConstraintType.MultiSelect: {
            // AssetPack/Theme options are a runtime-discovered list stored as valueNumberArray (a set of
            // prefab-name hashes), not a fixed-enum bitmask, so their count is just its length.
            let count = isDynamicNameBasedMultiSelect(props.item!.byLawItemType)
                ? (props.item.valueNumberArray?.length ?? 0)
                : Object.keys(ByLawZoneType)
                    .filter(key => !isNaN(Number(key)))
                    .map((key, _) => ((Number(key) & props.item!.valueByteFlag) != 0? 1 : 0) as number)
                    .reduce((prevValue, currentValue) => prevValue + currentValue, 0);
            textChild = <span>{count}&#160;{translate("ZBL.ByLawValueText[Items]", "item[s]")}</span>;
            break;
        }
        case ByLawConstraintType.SingleSelect: {
            let value = props.item.valueByteFlag;
            switch(props.item!.byLawItemType) {
                case ByLawItemType.AirPollutionLevel:
                case ByLawItemType.GroundPollutionLevel:
                case ByLawItemType.NoisePollutionLevel:
                    textChild = <span>{translate(`ZBL.FlagValues[${PollutionValues[value]}]`, PollutionValues[value])}</span>;
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