import { ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1, Color } from "cs2/bindings";

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