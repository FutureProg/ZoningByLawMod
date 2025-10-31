import { ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types";
import { ByLawItemBounds1Editor } from "./ByLawItemBounds1Editor";
import { Bounds1 } from "cs2/bindings";
import ByLawItemEnumEditor from "./ByLawItemEnumEditor";
import { getMeasurementString } from "mods/utils";
import { useLocalization } from "cs2/l10n";
import ByLawItemAssetPackEditor from "./ByLawItemAssetPackEditor";
import { CheckboxFieldData, FieldDataBase, RadioFieldData, RangeFieldData, setByLawItemValue } from "mods/bindings";

type Props = {
    fieldData: FieldDataBase;
    byLawItem: ByLawItem;
    isOpen: boolean;
    onChange?: (newItemValue: ByLawItem) => void;
};

/**
 * Responsible for choosing which editor to display based on the property type
 */
export default (
    { byLawItem, isOpen, onChange: onChangeCallback, fieldData }: Props,
): JSX.Element => {
    let localization = useLocalization();
    const measurementSuffix = getMeasurementString(
        byLawItem.byLawItemType,
        byLawItem.constraintType,
    );
    let isCellMeasurement = measurementSuffix.indexOf("cells") >= 0;
    if (!isOpen) {
        return <></>;
    }

    let { constraintType: constraintType, byLawItemType: itemType } = byLawItem;

    if (
        fieldData.fieldType === "range"
    ) {
        let onChange = (name: string, newValue: Bounds1) => {
            // Handle conversions to metric
            if (isCellMeasurement) {
                newValue = {
                    min: newValue.min * 8,
                    max: newValue.max * 8,
                };
            } else if (
                ["m", "ft"].includes(measurementSuffix) &&
                localization.unitSettings.unitSystem == 1
            ) {
                newValue = {
                    min: Math.round(newValue.min / 3),
                    max: Math.round(newValue.max / 3),
                };
            }
            let nItemVal = {
                ...byLawItem,
                valueBounds1: newValue,
            };
            // onChangeCallback && onChangeCallback(nItemVal);
            // Error Here: invalid cast on the C# side from here (guessing it's the whole "object" thing?)
            setByLawItemValue(itemType.toString(), [newValue.min, newValue.max]);
        };
        const rangeFieldData = fieldData as RangeFieldData;
        let boundsValue = byLawItem.valueBounds1;//{min: rangeFieldData.value[0], max: rangeFieldData.value[1]};
        let step = 1;
        if (isCellMeasurement) {
            boundsValue = {
                max: boundsValue.max / 8,
                min: boundsValue.min / 8,
            };
        } else if (
            ["m", "ft"].includes(measurementSuffix) &&
            localization.unitSettings.unitSystem == 1
        ) {
            boundsValue = {
                max: boundsValue.max * 3,
                min: boundsValue.min * 3,
            };
            step = 3;
        }
        return (
            <ByLawItemBounds1Editor
                name={ByLawItemType[byLawItem.byLawItemType]}
                bounds={boundsValue}
                step={step}
                onChange={onChange}
            />
        );
    }    
    if (
        ["radio", "checkbox"].includes(fieldData.fieldType)
    ) {
        let onChange = (nValue: number[]) => {
            setByLawItemValue(itemType.toString(), nValue);
            // onChangeCallback && onChangeCallback(nItemVal);
        };
        console.log('Field Data Type:', fieldData.fieldType);
        return (
            <ByLawItemEnumEditor
                constraintType={fieldData.fieldType === "checkbox"? ByLawConstraintType.MultiSelect : ByLawConstraintType.SingleSelect}
                itemType={byLawItem.byLawItemType}
                fieldData={fieldData as RadioFieldData | CheckboxFieldData}
                onChange={onChange}
            />
        );
    }
    return <></>;
};
