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
    IsNot = 2,
    AtLeastOne = 3,
    OnlyOneOf = 4    
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

export enum ByLawConstraintType {
    None = 0,        
    Length,
    Count,
    MultiSelect,
    SingleSelect
}

export enum ByLawItemCategory {
    None = 0,
    Building = 1,
    Lot,
    Pollution
}

export interface ByLawItem {
    byLawItemType: ByLawItemType;
    byLawConstraintType: ByLawConstraintType;
    itemCategory: ByLawItemCategory;
    operator: ByLawPropertyOperator;

    valueBounds1: Bounds1;
    valueByteFlag: number;
    valueNumber: number;
}

export enum BlockType
{
    None = 0,
    Instruction = 1,
    Logic = 2
}

export enum LogicOperation
{
    None = 0
}

export interface ByLawBlock
{
    blockType: BlockType ;
    logicOperation: LogicOperation;
    items: ByLawItem[];
}

export interface ByLawBlockBinding {
    blockData: ByLawBlock;
    itemData: ByLawItem[];
}

export interface ZoningByLawBinding {    
    blocks: ByLawBlockBinding[];
    deleted: boolean;
}