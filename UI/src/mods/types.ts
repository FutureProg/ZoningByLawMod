import { Bounds1, Entity } from "cs2/bindings";

export interface ByLawZoneListItem {
    entity: Entity;
    name: string;
}

export interface ByLawZoneData extends ByLawZoneComponent {
    entity: Entity;
}
export interface ByLawZoneComponent {
    zoneType: ByLawZoneType;
    height: Bounds1;
    lotSize: Bounds1;
    frontage: Bounds1;
    parking: Bounds1;
}

export enum ByLawZoneType {
    None = 0,
    Residential = 1,
    Commercial = 2,
    Industrial = 4,
    Office = 8
}

export enum ByLawPropertyOperator {
    None = 0,
    Is = 1,
    IsNot = 2
}

export enum ByLawItemType {
    None = 0,
    Uses,
    Height,
    LotWidth,
    LotSize,
    Parking,
    FrontSetback,
    LeftSetback,
    RightSetback,
    RearSetback,
    AirPollutionLevel,
    GroundPollutionLevel,
    NoisePollutionLevel
}