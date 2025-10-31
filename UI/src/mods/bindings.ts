import { bindMap, bindValue, trigger } from "cs2/api";
import mod from '../../mod.json';
import { ByLawItemType, ByLawPropertyOperator, ByLawZoneListItem, ZoningByLawBinding } from "./types";
import { Color, Entity } from "cs2/bindings";

export type ByLawFieldsDict = {
    "Key": keyof typeof ByLawItemType
    "Value": FieldDataBase
}[];  

export type FieldDataBase = {
    id: string;
    label: string;
    fieldType: "checkbox" | "radio" | "select" | "text" | "number" | "range";
    options?: FieldDataOption[];
    operatorOptions: ByLawPropertyOperator[];
    selectedOperator: ByLawPropertyOperator;
    value: any;
}

export type FieldDataOption = {
    image?: string;
    label: string;
    value: number;
}

export type CheckboxFieldData = FieldDataBase & {
    fieldType: "checkbox";
    value: number[]; // array of selected values
}

export type RadioFieldData = FieldDataBase & {
    fieldType: "radio";
    value: number; // single selected value
}

export type SelectFieldData = FieldDataBase & {
    fieldType: "select";
    value: any; // single selected value
}

export type TextFieldData = FieldDataBase & {
    fieldType: "text";
    value: string;
    validationRegex?: string;
}

export type NumberFieldData = FieldDataBase & {
    fieldType: "number";
    slider?: boolean;
    value: number;
    min?: number;
    max?: number;
    step?: number;
}

export type RangeFieldData = FieldDataBase & {
    fieldType: "range";
    value: [number, number]; // [min, max]
    min?: number;
    max?: number;
    step?: number;
}

export const ZONE_COLOR_IDX = 0;
export const ZONE_BORDER_IDX = 1;

export const byLawZoneList$ = bindValue<ByLawZoneListItem[]>(mod.fullname, "ByLawZoneList", []);
export const selectedByLawData$ = bindValue<ZoningByLawBinding>(mod.fullname, "SelectedByLawData");
export const isConfigPanelOpen$ = bindValue<boolean>(mod.fullname, "IsConfigPanelOpen");
export const selectedByLawName$ = bindValue<string>(mod.fullname, "SelectedByLawName", "");
export const defaultColor = {r: 1, g: 1, b: 1, a: 1};
export const selectedByLawColor$ = bindValue<Color[]>(mod.fullname, "SelectedByLawColour", [defaultColor, defaultColor]);
export const selectedByLaw$ = bindValue<Entity>(mod.fullname, "SelectedByLaw");
export const elligibleBuildingCount$ = bindValue<number>(mod.fullname, "ElligibleBuildings", -1);
export const assetPackNameToHash$ = bindMap<string, number>(mod.fullname, "assetPackNameToHash");
export const byLawFields$ = bindValue<ByLawFieldsDict>(mod.fullname, "ByLawFields");

export const setConfigPanelOpen = (open : boolean) => {
    trigger(mod.fullname, "SetConfigPanelOpen", open);
}

export const setActiveByLaw = (entity: Entity) => {    
    trigger(mod.fullname, "SetActiveByLaw", entity);
}

export const setByLawData = (byLawData: ZoningByLawBinding) => {    
    trigger(mod.fullname, "SetByLawData", byLawData);
}

export const createNewByLaw = () => {    
    trigger(mod.fullname, "CreateNewByLaw");
}

export const deleteByLaw = () => {
    trigger(mod.fullname, "DeleteByLaw");
}

export const setByLawName = (name: string) => {
    trigger(mod.fullname, "SetByLawName", name);
}

export const setByLawZoneColor = (zoneColor: Color, borderColor: Color) => {
    trigger(mod.fullname, "SetByLawZoneColour", zoneColor, borderColor);
}

export const toggleByLawRenderPreview = () => {
    trigger(mod.fullname, "ToggleByLawRenderPreview");
}

export const toggleTool = () => {
    trigger(mod.fullname, "ToggleTool");
}

export const setByLawItemValue = (id: string, value: any) => { 
    if (Array.isArray(value) && typeof value[0] === 'number') {
        trigger(mod.fullname, "SetByLawItemValueIntArr", id, value);    
    } 
    else if (typeof value === 'number') {
        trigger(mod.fullname, "SetByLawItemValueInt", id, value);    
    } else {
        trigger(mod.fullname, "SetByLawItemValue", id, value);
    }    
}

export const setByLawItemPropertyOperator = (id: string, operator: number) => {
    trigger(mod.fullname, "SetByLawItemPropertyOperator", id, operator);
}

export const toggleByLawItemEnabled = (itemType: ByLawItemType) => {
    trigger(mod.fullname, "ToggleItemEnabled", itemType);
}