import { LocalizedNumber, UnitSystem, useLocalization } from "cs2/l10n";
import { ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1, Color, Unit } from "cs2/bindings";
import { useCallback, useEffect, useMemo, useState } from "react";

export const GetDefaultByLawComponent = () : ByLawZoneComponent => {return {
    frontage: {max: -1, min: -1},
    height: {max: -1, min: -1},
    lotSize: {max: -1, min: -1},
    parking: {max: -1, min: -1},
    zoneType: ByLawZoneType.None    
}};

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