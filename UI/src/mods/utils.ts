import { LocalizedNumber, UnitSystem, useLocalization } from "cs2/l10n";
import { BlockType, ByLawConstraintType, ByLawItem, ByLawItemCategory, ByLawItemType, ByLawPropertyOperator, ByLawZoneComponent, ByLawZoneType, LogicOperation, ZoningByLawBinding } from "./types";
import { Bounds1, Color, Unit } from "cs2/bindings";
import { useCallback, useMemo } from "react";
import hexToRgba from 'hex-to-rgba';

export const GetDefaultByLawComponent = () : ByLawZoneComponent => {return {
    frontage: {max: -1, min: -1},
    height: {max: -1, min: -1},
    lotSize: {max: -1, min: -1},
    parking: {max: -1, min: -1},
    zoneType: ByLawZoneType.None    
}};

export const GetDefaultByLawItem = () : ByLawItem => ({    
    byLawItemType: ByLawItemType.None,
    constraintType: ByLawConstraintType.None,
    itemCategory: ByLawItemCategory.None,
    propertyOperator: ByLawPropertyOperator.None,
    valueBounds1: {max: 0, min: 0} as Bounds1,
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
            byLawItemType: ByLawItemType.LandUse,
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


export const hexToRGBA = (hex: string) : Color => {
    let pieces = hexToRgba(hex).replaceAll("rgba(", "").replaceAll(")", "").split(",").map(Number);
    return {
        r: pieces[0]/255,
        g: pieces[1]/255,
        b: pieces[2]/255,
        a: pieces[3]
    }
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
        case ByLawItemType.LandUse:        
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
        case ByLawItemType.LandUse:
            if (itemType == ByLawItemType.LandUse) {
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

export const getOperationTypes = (byLawItemType: ByLawItemType) : ByLawPropertyOperator[] => {
    let re : ByLawPropertyOperator[] = [];
    switch(byLawItemType) {
        case ByLawItemType.LandUse:
            re.push(ByLawPropertyOperator.AtLeastOne);
            re.push(ByLawPropertyOperator.OnlyOneOf);
            re.push(ByLawPropertyOperator.IsNot);
            return re;
        case ByLawItemType.Height:
        case ByLawItemType.LotWidth:
        case ByLawItemType.LotSize:
        case ByLawItemType.Parking:
        case ByLawItemType.FrontSetback:
        case ByLawItemType.LeftSetback:
        case ByLawItemType.RightSetback:
        case ByLawItemType.RearSetback:                
            re.push(ByLawPropertyOperator.Is); 
            re.push(ByLawPropertyOperator.IsNot);                                   
            return re;
        case ByLawItemType.NoisePollutionLevel:
        case ByLawItemType.GroundPollutionLevel:
        case ByLawItemType.AirPollutionLevel:
            re.push(ByLawPropertyOperator.AtMost);
            re.push(ByLawPropertyOperator.AtLeast);
            re.push(ByLawPropertyOperator.Is);
            re.push(ByLawPropertyOperator.IsNot);
            return re;
        case ByLawItemType.None:
        default:
            re.push(ByLawPropertyOperator.None);
    }
    return re;
}

export const getDefaultPropertyOperator = (byLawItemType: ByLawItemType) : ByLawPropertyOperator => getOperationTypes(byLawItemType)[0];

export const getConstraintTypes = (byLawItemType: ByLawItemType) : ByLawConstraintType[] => {
    let re : ByLawConstraintType[] = [];
    switch(byLawItemType) {
        case ByLawItemType.LandUse:
            re.push(ByLawConstraintType.MultiSelect);
            break;
        case ByLawItemType.Height:
        case ByLawItemType.LotWidth:
        case ByLawItemType.LotSize:        
        case ByLawItemType.FrontSetback:
        case ByLawItemType.LeftSetback:
        case ByLawItemType.RightSetback:
        case ByLawItemType.RearSetback:                
            re.push(ByLawConstraintType.Length);                                   
            break;
        case ByLawItemType.Parking:
            re.push(ByLawConstraintType.Count);
            break;
        case ByLawItemType.NoisePollutionLevel:
        case ByLawItemType.GroundPollutionLevel:
        case ByLawItemType.AirPollutionLevel:
            re.push(ByLawConstraintType.SingleSelect);
            break;
        case ByLawItemType.None:
        default:
            re.push(ByLawConstraintType.None);
            break;
    }
    return re;
};

export const getItemCategories = (itemType: ByLawItemType) : ByLawItemCategory => {
    switch (itemType)
    {
        case ByLawItemType.LandUse:
        case ByLawItemType.LotWidth:
        case ByLawItemType.LotSize:
        case ByLawItemType.Parking:
            return ByLawItemCategory.Lot;
    
        case ByLawItemType.Height:               
        case ByLawItemType.FrontSetback:
        case ByLawItemType.LeftSetback:
        case ByLawItemType.RightSetback:
        case ByLawItemType.RearSetback:
            return ByLawItemCategory.Building;
    
        case ByLawItemType.NoisePollutionLevel:
        case ByLawItemType.GroundPollutionLevel:
        case ByLawItemType.AirPollutionLevel:
            return ByLawItemCategory.Pollution;
    
        case ByLawItemType.None:
        default:
            return ByLawItemCategory.None;
    }
}

export const deepCopy = <T,>(obj: T) => JSON.parse(JSON.stringify(obj)) as T;   