import { bindValue, trigger } from "cs2/api";
import mod from '../../mod.json';
import { ByLawZoneComponent, ByLawZoneData, ByLawZoneListItem } from "./types";
import { Color, Entity } from "cs2/bindings";

export const ZONE_COLOR_IDX = 0;
export const ZONE_BORDER_IDX = 1;

export const byLawZoneList$ = bindValue<ByLawZoneListItem[]>(mod.fullname, "ByLawZoneList", []);
export const selectedByLawData$ = bindValue<ByLawZoneComponent>(mod.fullname, "SelectedByLawData");
export const isConfigPanelOpen$ = bindValue<boolean>(mod.fullname, "IsConfigPanelOpen");
export const selectedByLawName$ = bindValue<string>(mod.fullname, "SelectedByLawName", "");
export const defaultColor = {r: 1, g: 1, b: 1, a: 1};
export const selectedByLawColor$ = bindValue<Color[]>(mod.fullname, "SelectedByLawColour", [defaultColor, defaultColor]);

export const setConfigPanelOpen = (open : boolean) => {
    trigger(mod.fullname, "SetConfigPanelOpen", open);
}

export const setActiveByLaw = (entity: Entity) => {
    trigger(mod.fullname, "SetActiveByLaw", entity);
}

export const setByLawData = (byLawData: ByLawZoneComponent) => {
    trigger(mod.fullname, "SetByLawData", byLawData);
}

export const createNewByLaw = (byLawData : ByLawZoneComponent) => {
    trigger(mod.fullname, "CreateNewByLaw", byLawData);
}

export const deleteByLaw = (entity: Entity) => {
    trigger(mod.fullname, "DeleteByLaw", entity);
}

export const setByLawName = (name: string) => {
    trigger(mod.fullname, "SetByLawName", name);
}

export const setByLawZoneColor = (zoneColor: Color, borderColor: Color) => {
    trigger(mod.fullname, "SetByLawZoneColour", zoneColor, borderColor);
}