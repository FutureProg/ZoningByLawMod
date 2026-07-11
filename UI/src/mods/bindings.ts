import { bindValue, trigger } from "cs2/api";
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
    operatorOptions: OperatorOption[];
    selectedOperator: ByLawPropertyOperator;
}

export type OperatorOption = {
    label: string;
    value: ByLawPropertyOperator;
    image: string | undefined;
}

export type FieldDataOption = {
    image?: string;
    label: string;
    value: number;
}

export type CheckboxFieldData = FieldDataBase & {
    fieldType: "checkbox";
    value?: number; // array of selected values
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

type SetItemValuePayload = {
    id: string;
    value: any;
}

export const setByLawItemValue = (id: string, value: any) => { 
    console.log("Setting ByLaw Item Value:", id, value);
    console.log("Value Type:", typeof value, Array.isArray(value) ? "Array" : "Not Array");    
    if (Array.isArray(value)) {
        console.log("Array Element Type:", typeof value[0], value[0]);
    }
    const payload = {
        id: id,
        value: value
    } as SetItemValuePayload;
    if (Array.isArray(value) && (value.length === 0 || typeof value[0] === 'number')) {
        // Every array-valued field here (range bounds, multi-select selections) is an int
        // array; an empty array (e.g. deselecting the last option) has no element to check
        // the type of but is still a valid, meaningful value that must be sent through.
        console.log("Calling SetByLawItemValueIntArr");
        trigger(mod.fullname, "SetByLawItemValueIntArr", id, payload);
    }
    else if (typeof value === 'number') {
        console.log("Calling SetByLawItemValueInt");
        trigger(mod.fullname, "SetByLawItemValueInt", id, payload);    
    } 
    else if (typeof value === 'string') {
        console.log("Calling SetByLawItemValueString");
        trigger(mod.fullname, "SetByLawItemValueString", id, payload);
    }
    else {
        console.warn("Unsupported value type for SetByLawItemValue:", typeof value);
    }    
}

export const setByLawItemPropertyOperator = (id: string, operator: number) => {
    trigger(mod.fullname, "SetByLawItemPropertyOperator", id, operator);
}

export const toggleByLawItemEnabled = (itemType: ByLawItemType) => {
    trigger(mod.fullname, "ToggleItemEnabled", itemType);
}