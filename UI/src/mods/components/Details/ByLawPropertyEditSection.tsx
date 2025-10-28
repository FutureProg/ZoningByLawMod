import { ByLawConstraintType, ByLawItem, ByLawItemType } from "mods/types";
import { ByLawItemBounds1Editor } from "./ByLawItemBounds1Editor";
import { Bounds1 } from "cs2/bindings";
import ByLawItemEnumEditor from "./ByLawItemEnumEditor";
import { getMeasurementString } from "mods/utils";
import { useLocalization } from "cs2/l10n";
import ByLawItemAssetPackEditor from "./ByLawItemAssetPackEditor";
import { FieldDataBase, RangeFieldData, setByLawItemValue } from "mods/bindings";

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
            setByLawItemValue(itemType.toString(), [newValue.min, newValue.max]);
        };
        const rangeFieldData = fieldData as RangeFieldData;
        let boundsValue = {min: rangeFieldData.value[0], max: rangeFieldData.value[1]};
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
        fieldData.fieldType in ["radio", "checkbox"]
    ) {
        let onChange = (nValue: number) => {
            let nItemVal = {
                ...byLawItem,
                valueByteFlag: nValue,
            };
            setByLawItemValue(itemType.toString(), nValue);
            // onChangeCallback && onChangeCallback(nItemVal);
        };

        return (
            <ByLawItemEnumEditor
                constraintType={fieldData.fieldType === "checkbox"? ByLawConstraintType.MultiSelect : ByLawConstraintType.SingleSelect}
                itemType={byLawItem.byLawItemType}
                itemValue={fieldData.value}
                onChange={onChange}
            />
        );
    }
    return <></>;
};
