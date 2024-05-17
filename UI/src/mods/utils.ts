import { ByLawZoneComponent, ByLawZoneType } from "./types";
import { Bounds1 } from "cs2/bindings";

export const GetDefaultByLawComponent = () : ByLawZoneComponent => {return {
    frontage: {max: -1, min: -1},
    height: {max: -1, min: -1},
    lotSize: {max: -1, min: -1},
    parking: {max: -1, min: -1},
    zoneType: ByLawZoneType.None    
}};