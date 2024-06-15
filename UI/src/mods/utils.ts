import { LocalizedNumber, UnitSystem, useLocalization } from "cs2/l10n";
import { BlockType, ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator, ByLawZoneComponent, ByLawZoneType, LogicOperation, ZoningByLawBinding } from "./types";
import { Bounds1, Color, Unit } from "cs2/bindings";
import { useCallback, useEffect, useMemo, useState } from "react";

export const GetDefaultByLawComponent = () : ByLawZoneComponent => {return {
    frontage: {max: -1, min: -1},
    height: {max: -1, min: -1},
    lotSize: {max: -1, min: -1},
    parking: {max: -1, min: -1},
    zoneType: ByLawZoneType.None    
}};

export const GetDefaultByLawItem = () : ByLawItem => ({
    constraintType: ByLawConstraintType.None,
    byLawItemType: ByLawItemType.None,
    itemCategory: ByLawItemCategory.None,
    propertyOperator: ByLawPropertyOperator.None,
    valueBounds1: {max: 0, min: 0},
    valueByteFlag: 0,
    valueNumber: 0
});

export const GetDefaultZoningByLawBinding = () : ZoningByLawBinding => ({
    blocks: [{
        blockData: {
            blockType: BlockType.Instruction,
            logicOperation: LogicOperation.None            
        },
        itemData: [{
            ...GetDefaultByLawItem(),
            byLawItemType: ByLawItemType.Uses,
            constraintType: ByLawConstraintType.MultiSelect,
            propertyOperator: ByLawPropertyOperator.AtLeastOne,
            itemCategory: ByLawItemCategory.Lot,
            valueByteFlag: 0                 
        }]
    }],
    deleted: false
});

export const rgbaToHex = (color: Color) : string => {
    return Object.values(color).reduce((accum, curr) => {
        if (!isNaN(curr)) {
            let curr255 = Math.round((curr as number) * 255);
            return accum + curr255.toString(16).toUpperCase();
        }        
        return accum;
    }, '#');
}

export const useLocalizedMeasurement = (value: number) => {
    let localization = useLocalization();    
    let { unitSettings } = localization;

    let measurementText = useMemo(() => {                            
        let props = {value, signed: true, unit: Unit.NetElevation};
        return LocalizedNumber.renderString(localization, props); // NetElevation will use Metres and Feet, for some reason Length uses Yards.                   
    }, [value, unitSettings.unitSystem]);

    /**
     * "Common.VALUE_FOOT": "{SIGN}{VALUE} ft",
     */
    
    let valueLocalized = useMemo(() => {
        return unitSettings.unitSystem == UnitSystem.Metric? value : value * 3; // in the UI code it multiplies the value by 3 instead of 3.28.
    }, [value, unitSettings.unitSystem]);    

    let convertToMetric = useCallback((num: number) => {
        if (unitSettings.unitSystem == UnitSystem.Metric) {
            return num;
        } else {
            return num / 3;
        }
    }, [value, unitSettings.unitSystem]);

    return { measurementText, valueLocalized, convertToMetric };
}

export const getMeasurementString = (itemType: ByLawItemType, constraintType: ByLawConstraintType) => {
    switch(itemType) {        
        case ByLawItemType.Height:
            return "m";
        case ByLawItemType.Uses:        
        case ByLawItemType.LotWidth:
        case ByLawItemType.LotSize:
        case ByLawItemType.Parking:
        case ByLawItemType.FrontSetback:
        case ByLawItemType.LeftSetback:
        case ByLawItemType.RightSetback:
        case ByLawItemType.RearSetback:
        case ByLawItemType.AirPollutionLevel:
        case ByLawItemType.GroundPollutionLevel:
        case ByLawItemType.NoisePollutionLevel:
        case ByLawItemType.None:
        default:
            return "";            
    }    
}

export const flagToStringArr = (flag: number, itemType: ByLawItemType) => {
    let entries : [any, any][] = [];
    switch(itemType) {
        case ByLawItemType.Uses:
            if (itemType == ByLawItemType.Uses) {
                entries = Object.entries(ByLawZoneType);                            
            }
            break;
        case ByLawItemType.None:        
        case ByLawItemType.Height:
        case ByLawItemType.LotWidth:
        case ByLawItemType.LotSize:
        case ByLawItemType.Parking:
        case ByLawItemType.FrontSetback:
        case ByLawItemType.LeftSetback:
        case ByLawItemType.RightSetback:
        case ByLawItemType.RearSetback:
        case ByLawItemType.AirPollutionLevel:
        case ByLawItemType.GroundPollutionLevel:
        case ByLawItemType.NoisePollutionLevel:
            return [];
    }
    return entries
        .filter((value, idx) => idx < entries.length/2 && Number(value[0]) != 0).map(([k,v]) => [v,k])
        .filter(([k, v], idx) => {            
            return (v & flag) != 0;
        })
        .map(([k, v], idx) => k);
}